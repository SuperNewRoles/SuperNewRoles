using System.Collections.Generic;
using AmongUs.Data;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomCosmetics.UI;

public class CustomCosmeticsColorMenu : CustomCosmeticsMenuBase<CustomCosmeticsColorMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.color;

    private GameObject colorMenu;
    private List<GameObject> allUnAvailableButtons;
    private List<PassiveButton> allButtons;

    public override void Initialize()
    {
        // カラーメニューの初期化処理
        var customizationMenu = PlayerCustomizationMenu.Instance;

        // CosmeticMenuList の複製と配置設定
        colorMenu = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuList"), customizationMenu.transform);
        colorMenu.transform.localPosition = new Vector3(0.33f, -0.22f, -15f);
        colorMenu.transform.localScale = Vector3.one * 0.28f;

        Scroller scroller = colorMenu.GetComponentInChildren<Scroller>();

        // Innerコンテナの取得とカテゴリ名の設定
        Transform inner = colorMenu.transform.Find("LeftArea/Scroller/Inner");
        inner.Find("CategoryText").GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Colors);

        const int colorsPerRow = 10;
        int selectedColorId = PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.CurrentOutfit.ColorId : DataManager.Player.Customization.Color;
        List<PassiveButton> buttons = new();

        PlayerCustomizationMenu.Instance.SetItemName(Palette.GetColorName(selectedColorId));

        // ローカル関数：ボタンの位置を計算する
        Vector3 CalculateButtonPosition(int index)
        {
            float x = -16.6f + (index % colorsPerRow) * 1.732f;
            float y = 3.1f - (index / colorsPerRow) * 1.8f;
            return new Vector3(x, y, -15f);
        }

        // ローカル関数：ボタンのSelected表示を切り替える
        void SetButtonSelected(GameObject btn, bool isSelected)
        {
            var selectedObj = btn.transform.Find("Selected")?.gameObject;
            if (selectedObj != null)
            {
                selectedObj.SetActive(isSelected);
            }
        }

        allUnAvailableButtons = new();
        allButtons = new();

        // 各カラーに対してボタンを作成し、イベントリスナーを登録
        for (int i = 0; i < Palette.PlayerColors.Length; i++)
        {
            int index = i;
            GameObject button = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticColor"), inner);
            button.transform.localPosition = CalculateButtonPosition(index);
            button.transform.localScale = Vector3.one * 0.6f;
            button.GetComponent<SpriteRenderer>().color = ModHelpers.TryGetIndex<Color32>(Palette.PlayerColors, index);
            button.transform.Find("Shadow").GetComponent<SpriteRenderer>().color = ModHelpers.TryGetIndex<Color32>(Palette.ShadowColors, index);
            allUnAvailableButtons.Add(button.transform.Find("UnAvailable").gameObject);

            // PassiveButton を追加して、イベント設定を行う
            PassiveButton passiveButton = button.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
            allButtons.Add(passiveButton);

            // OnClick イベント
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                DataManager.Player.Customization.Color = (byte)index;
                if (PlayerControl.LocalPlayer != null)
                    PlayerControl.LocalPlayer.CmdCheckColor((byte)index);

                // 前回選択されたボタンの Selected 表示を解除
                PassiveButton previousButton = buttons.TryGetIndex(selectedColorId);
                if (previousButton != null)
                    SetButtonSelected(previousButton.gameObject, false);

                selectedColorId = index;
                UpdateShowingColor(index);
                SetButtonSelected(button, true);
            }));

            // OnMouseOver イベント：ホバー時に Selected 表示とプレビュー更新を行う
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                SetButtonSelected(button, true);
                UpdateShowingColor(index);
            }));

            // OnMouseOut イベント：ホバー解除時、選択状態でない場合は Selected 表示を解除
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (selectedColorId != index)
                    SetButtonSelected(button, false);
                UpdateShowingColor(selectedColorId);
            }));

            // 初期状態で選択中のボタンの場合は表示をONにする
            if (index == selectedColorId)
                SetButtonSelected(button, true);

            ControllerManager.Instance.AddSelectableUiElement(passiveButton);

            buttons.Add(passiveButton);
        }

        var selectedColorButton = buttons.TryGetIndex(selectedColorId);
        if (selectedColorButton != null)
        {
            ControllerManager.Instance.SetCurrentSelected(selectedColorButton);
            selectedColorButton.ReceiveMouseOver();
        }

        if (Palette.PlayerColors.Length <= 60)
            scroller.ContentYBounds.max = 0;
        else
            scroller.ContentYBounds.max = (((Palette.PlayerColors.Length - 60) / colorsPerRow) + 1) * 1.4f;
        scroller.DragScrollSpeed = 1.4f;
    }

    private void UpdateShowingColor(int index)
    {
        var menu = PlayerCustomizationMenu.Instance;
        string colorName = Palette.GetColorName(index);
        menu.SetItemName(colorName);
        menu.PreviewArea.SetBodyColor(index);
        menu.PreviewArea.SetPetColor(index);
        menu.PreviewArea.SetSkin(DataManager.Player.Customization.Skin, index);
        CustomCosmeticsLayer layer = CustomCosmeticsLayers.ExistsOrInitialize(menu.PreviewArea.cosmetics);
        layer.hat1.SetMaterialColor(index);
        layer.hat2.SetMaterialColor(index);
        layer.visor1.SetMaterialColor(index);
        layer.visor2.SetMaterialColor(index);
    }

    public override void Update()
    {
        // 更新処理が必要な場合はここに実装
        if (PlayerControl.LocalPlayer == null) return;
        List<int> usedColors = new();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player == PlayerControl.LocalPlayer) continue;
            usedColors.Add(player.CurrentOutfit.ColorId);
        }
        for (int i = 0; i < allUnAvailableButtons.Count; i++)
        {
            allUnAvailableButtons[i].SetActive(usedColors.Contains(i));
        }
        for (int i = 0; i < allButtons.Count; i++)
        {
            allButtons[i].enabled = !usedColors.Contains(i);
        }
    }

    public override void Hide()
    {
        if (colorMenu != null)
        {
            GameObject.Destroy(colorMenu);
        }
    }
}