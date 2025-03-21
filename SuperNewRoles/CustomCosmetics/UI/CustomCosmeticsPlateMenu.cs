using System;
using System.Collections.Generic;
using AmongUs.Data;
using Innersloth.Assets;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomCosmetics.UI;

public class CustomCosmeticsPlateMenu : CustomCosmeticsMenuBase<CustomCosmeticsPlateMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.plate;

    private List<Transform> slots = new();
    private List<GameObject> activeSlots = new();
    private Dictionary<string, List<GameObject>> slotToGroupMap = new();
    private GameObject CurrentCostumeTab;
    private Scroller scroller;
    private PlayerVoteArea playerVoteArea;

    public override void Initialize()
    {
        playerVoteArea = PlayerCustomizationMenu.Instance.nameplateMaskArea.GetComponent<PlayerVoteArea>();
        playerVoteArea.PreviewNameplate(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.NamePlateId : DataManager.Player.Customization.NamePlate);
        playerVoteArea.gameObject.SetActive(true);

        var unlockedNamePlates = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates();

        string currentCosmeticId = PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.NamePlateId : DataManager.Player.Customization.NamePlate;
        slots = [];
        activeSlots = [];
        slotToGroupMap = null;
        if (CurrentCostumeTab != null)
        {
            GameObject.Destroy(CurrentCostumeTab);
        }
        CurrentCostumeTab = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuList"), PlayerCustomizationMenu.Instance.transform);
        CurrentCostumeTab.transform.localPosition = new(0, -0.085f, -10);
        CurrentCostumeTab.transform.localScale = Vector3.one * 0.27f;
        CurrentCostumeTab.transform.Find("LeftArea/Scroller/Inner/CategoryText").GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NamePlates);

        var slotBase = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("PlateCosmeticItemSlot"), CurrentCostumeTab.transform);
        var slotBasePassive = slotBase.AddComponent<PassiveButton>();
        slotBase.SetActive(false);

        CustomCosmeticsCostumeSlot costumeSlot = slotBase.AddComponent<CustomCosmeticsCostumeSlot>();
        PassiveButton selectedButton = null;

        int itemsPerRow = 2;
        int totalItems = unlockedNamePlates.Count;
        scroller = CurrentCostumeTab.transform.Find("LeftArea/Scroller").GetComponent<Scroller>();
        Transform inner = scroller.transform.Find("Inner");

        int allI = itemsPerRow;
        const float offSetY = 2.7f;
        for (int i = 0; i < unlockedNamePlates.Count; i++)
        {
            int index = i;
            string namePlateId = unlockedNamePlates[i].ProdId;

            // 各アイテムの行と列を計算
            int col = allI % itemsPerRow;
            int itemRow = allI / itemsPerRow;

            var slot = GameObject.Instantiate(costumeSlot, inner);
            slots.Add(slot.transform);
            slot.Awake();

            var cosmeticData = unlockedNamePlates[i];

            slot.transform.localPosition = new(-12.48f + col * 9.13f, 2.63f - itemRow * 2.6f + offSetY, -10);
            slot.transform.localScale = Vector3.one * 1.2f;
            slot.button.Colliders = new Collider2D[] { slot.GetComponent<BoxCollider2D>() };
            slot.button.OnClick = new();
            slot.button.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("Slot clicked");
                if (selectedButton != null)
                {
                    selectedButton.SelectButton(false);
                    selectedButton.transform.Find("Selected").gameObject.SetActive(false);
                }
                slot.button.SelectButton(true);
                selectedButton = slot.button;
                selectedButton.transform.Find("Selected").gameObject.SetActive(true);
                DataManager.Player.Customization.NamePlate = namePlateId;
                if (PlayerControl.LocalPlayer != null)
                {
                    PlayerControl.LocalPlayer.RpcSetNamePlate(namePlateId);
                }
            }));
            slot.button.OnMouseOver = new();
            slot.button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(true);
                PreviewCosmetic(cosmeticData);
            }));
            slot.button.OnMouseOut = new();
            slot.button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(false);
                PreviewCosmetic(FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.NamePlateId : DataManager.Player.Customization.NamePlate));
            }));
            slot.gameObject.SetActive(true);
            if (!cosmeticData.IsEmpty)
            {
                slot.StartCoroutine(slot.CoLoadAssetAsync(cosmeticData.TryCast<IAddressableAssetProvider<NamePlateViewData>>(), (Action<NamePlateViewData>)delegate (NamePlateViewData viewData)
                {
                    slot.spriteRenderer.sprite = viewData?.Image;
                }));
            }
            // cosmeticData.SetPreview(slot.spriteRenderer, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            if (cosmeticData.ProdId == currentCosmeticId)
            {
                slot.button.SelectButton(true);
                selectedButton = slot.button;
                selectedButton.transform.Find("Selected").gameObject.SetActive(true);
            }
            allI++;
        }
        float contentYBounds = 0;
        if (allI > 10)
        {
            int extraRows = Mathf.CeilToInt((allI - 10) / (float)itemsPerRow);
            contentYBounds = extraRows * 2.6f + 0.45f - offSetY;
        }
        scroller.ContentYBounds = new(0, contentYBounds);
    }
    private void PreviewCosmetic(NamePlateData namePlate)
    {
        if (namePlate == null) return;
        PlayerCustomizationMenu.Instance.SetItemName(namePlate.GetItemName());
        playerVoteArea.PreviewNameplate(namePlate.ProdId);
    }
    public override void Update()
    {
    }
    public override void Hide()
    {
        PlayerCustomizationMenu.Instance.nameplateMaskArea.SetActive(false);
        GameObject.Destroy(CurrentCostumeTab);
    }
}

