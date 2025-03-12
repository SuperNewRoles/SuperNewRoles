using SuperNewRoles.Modules;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using AmongUs.Data;
using System;

namespace SuperNewRoles.CustomCosmetics.UI;

public enum CostumeTabType
{
    Visor1,
    Hat1,
    Visor2,
    Hat2,
    Skin,
}

// 共通インターフェース
public interface ICosmeticData
{
    string ProdId { get; }
    bool PreviewCrewmateColor { get; }
    void SetPreview(SpriteRenderer renderer, int colorId);
    string GetItemName();
}

// 標準のCosmeticDataをラップするクラス
public class CosmeticDataWrapper : ICosmeticData
{
    private readonly CosmeticData _data;

    public CosmeticDataWrapper(CosmeticData data)
    {
        _data = data;
    }

    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => _data.PreviewCrewmateColor;

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        _data.SetPreview(renderer, colorId);
    }

    public string GetItemName()
    {
        return _data.GetItemName();
    }

    // CosmeticDataに変換するメソッド
    public CosmeticData ToCosmeticData()
    {
        return _data;
    }
}

// ModdedHatDataをラップするクラス
public class ModdedHatDataWrapper : ICosmeticData
{
    private readonly CustomCosmeticsHat _data;

    public ModdedHatDataWrapper(CustomCosmeticsHat data)
    {
        _data = data;
    }

    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => _data.options.front == HatOptionType.Adaptive; // 通常はハットはクルーメイトの色を反映する

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        // ModdedHatDataのプレビュー設定ロジック
        renderer.sprite = _data.LoadFrontSprite();
        // 必要に応じて追加の設定
    }

    public string GetItemName()
    {
        return FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.Japanese ? _data.name : _data.name_en;
    }

    // CosmeticDataに変換するメソッド（必要に応じて）
    public CosmeticData ToCosmeticData()
    {
        // ModdedHatDataからCosmeticDataへの変換ロジック
        // 実装は実際のデータ構造に依存します
        return null;
    }
}
/*
// ModdedVisorDataをラップするクラス
public class ModdedVisorDataWrapper : ICosmeticData
{
    private readonly ModdedVisorData _data;

    public ModdedVisorDataWrapper(ModdedVisorData data)
    {
        _data = data;
    }

    public string ProdId => _data.Id;
    public bool PreviewCrewmateColor => true; // 通常はバイザーはクルーメイトの色を反映する

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        // ModdedVisorDataのプレビュー設定ロジック
        renderer.sprite = _data.MainImage;
        // 必要に応じて追加の設定
    }

    public string GetItemName()
    {
        return ModTranslation.GetString(_data.TranslationKey) ?? _data.Name;
    }

    // CosmeticDataに変換するメソッド（必要に応じて）
    public CosmeticData ToCosmeticData()
    {
        // ModdedVisorDataからCosmeticDataへの変換ロジック
        // 実装は実際のデータ構造に依存します
        return null;
    }
}*/

public class CustomCosmeticsCostumeMenu : CustomCosmeticsMenuBase<CustomCosmeticsCostumeMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.costume;
    private GameObject kisekae;

    // ボタンの参照を保持
    private GameObject visorButton01;
    private GameObject hatButton01;
    private GameObject visorButton02;
    private GameObject hatButton02;
    private GameObject skinButton;

    private GameObject CurrentCostumeTab;
    private CostumeTabType CurrentCostumeTabType;

    public override void Initialize()
    {
        // CosmeticMenuKisekae
        var obj = GameObject.FindObjectOfType<PlayerCustomizationMenu>();
        kisekae = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuKisekae"), obj.transform);
        kisekae.transform.localPosition = new(0.31f, -0.085f, -10);
        kisekae.transform.localScale = Vector3.one * 0.28f;

        // ボタンのセットアップ
        SetupButtons(obj);

        slots = new();
        activeSlots = new();
        slotToGroupMap = new();
    }

    private void SetupButtons(PlayerCustomizationMenu obj)
    {
        // VisorButton01のセットアップ
        visorButton01 = kisekae.transform.Find("Buttons/VisorButton01").gameObject;
        SetupButton(visorButton01, "VisorButton01", GetTabName(CostumeTabType.Visor1), () =>
        {
            // VisorButton01がクリックされたときの処理
            Logger.Info("VisorButton01 clicked");
            List<ICosmeticData> combinedCosmetics = new();
            foreach (var cosmetic in FastDestroyableSingleton<HatManager>.Instance.GetUnlockedVisors())
            {
                combinedCosmetics.Add(new CosmeticDataWrapper(cosmetic));
            }
            ICosmeticData getVisor()
            {
                return new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
            }
            ShowCostumeTab(CostumeTabType.Visor1, obj, combinedCosmetics, getVisor, (cosmetic) =>
            {
                // TODO: ModdedVisorDataWrapperに後で変える
                if (cosmetic is ModdedHatDataWrapper moddedHat)
                {
                    // TODO:後で
                }
                else
                    CustomCosmeticsSaver.SetVisor1Id(cosmetic.ProdId);
            }, (cosmetic) =>
            {
                obj.PreviewArea.SetVisor(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            });
        });

        // HatButton01のセットアップ
        hatButton01 = kisekae.transform.Find("Buttons/HatButton01").gameObject;
        SetupButton(hatButton01, "HatButton01", GetTabName(CostumeTabType.Hat1), () =>
        {
            // HatButton01がクリックされたときの処理
            Logger.Info("HatButton01 clicked");
            List<ICosmeticData> combinedCosmetics = new();
            foreach (var cosmetic in FastDestroyableSingleton<HatManager>.Instance.GetUnlockedHats())
            {
                combinedCosmetics.Add(new CosmeticDataWrapper(cosmetic));
            }
            ICosmeticData getHat()
            {
                if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentHat1Id))
                    return new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat));
                else
                    return CustomCosmeticsSaver.CurrentHat1Id.StartsWith("Modded") ? new ModdedHatDataWrapper(CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.hats).First(hat => hat.ProdId == CustomCosmeticsSaver.CurrentHat1Id)) : new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetHatById(CustomCosmeticsSaver.CurrentHat1Id));
            }
            ShowCostumeTab(CostumeTabType.Hat1, obj, combinedCosmetics, getHat, (cosmetic) =>
            {
                if (cosmetic is ModdedHatDataWrapper moddedHat)
                {
                    DataManager.Player.Customization.Hat = HatData.EmptyId;
                    if (PlayerControl.LocalPlayer != null)
                        PlayerControl.LocalPlayer.RpcSetHat(HatData.EmptyId);
                    CustomCosmeticsSaver.SetHat1Id(cosmetic.ProdId);
                }
                else
                {
                    DataManager.Player.Customization.Hat = cosmetic.ProdId;
                    if (PlayerControl.LocalPlayer != null)
                        PlayerControl.LocalPlayer.RpcSetHat(cosmetic.ProdId);
                    CustomCosmeticsSaver.SetHat1Id(cosmetic.ProdId);
                }
            }, (cosmetic) =>
            {
                if (cosmetic is ModdedHatDataWrapper moddedHat)
                {
                    // TODO:後で
                }
                else
                    obj.PreviewArea.SetHat(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            });
        });

        // VisorButton02のセットアップ
        visorButton02 = kisekae.transform.Find("Buttons/VisorButton02").gameObject;
        SetupButton(visorButton02, "VisorButton02", GetTabName(CostumeTabType.Visor2), () =>
        {
            // VisorButton02がクリックされたときの処理
            Logger.Info("VisorButton02 clicked");
            List<ICosmeticData> combinedCosmetics = new();
            foreach (var cosmetic in FastDestroyableSingleton<HatManager>.Instance.GetUnlockedVisors())
            {
                combinedCosmetics.Add(new CosmeticDataWrapper(cosmetic));
            }
            ICosmeticData getVisor()
            {
                return new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
            }
            ShowCostumeTab(CostumeTabType.Visor2, obj, combinedCosmetics, getVisor, (cosmetic) =>
            {
                CustomCosmeticsSaver.SetVisor2Id(cosmetic.ProdId);
            }, (cosmetic) =>
            {
                if (cosmetic is ModdedHatDataWrapper moddedHat)
                {
                    // TODO:後で
                }
                else
                    obj.PreviewArea.SetVisor(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            });
        });

        // HatButton02のセットアップ
        hatButton02 = kisekae.transform.Find("Buttons/HatButton02").gameObject;
        SetupButton(hatButton02, "HatButton02", GetTabName(CostumeTabType.Hat2), () =>
        {
            // HatButton02がクリックされたときの処理
            Logger.Info("HatButton02 clicked");
            // ModdedデータとAmongUs標準のCosmeticDataを混合するための処理
            List<ICosmeticData> combinedCosmetics = new();

            // 標準のコスメティックを追加
            foreach (var cosmetic in FastDestroyableSingleton<HatManager>.Instance.GetUnlockedHats())
            {
                combinedCosmetics.Add(new CosmeticDataWrapper(cosmetic));
            }

            foreach (var moddedHat in CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.hats))
            {
                combinedCosmetics.Add(new ModdedHatDataWrapper(moddedHat));
            }
            ICosmeticData getHat()
            {
                if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentHat2Id))
                    return new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
                else
                    return CustomCosmeticsSaver.CurrentHat2Id.StartsWith("Modded") ? new ModdedHatDataWrapper(CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.hats).First(hat => hat.ProdId == CustomCosmeticsSaver.CurrentHat2Id)) : new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetHatById(CustomCosmeticsSaver.CurrentHat2Id));
            }
            ShowCostumeTab(CostumeTabType.Hat2, obj, combinedCosmetics, getHat, (cosmetic) =>
                {
                    CustomCosmeticsSaver.SetHat2Id(cosmetic.ProdId);
                }, (cosmetic) =>
                {
                    if (cosmetic is ModdedHatDataWrapper moddedHat)
                    {
                        // TODO:後で
                    }
                    else
                        obj.PreviewArea.SetHat(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
                });
        });

        // SkinButtonのセットアップ
        skinButton = kisekae.transform.Find("Buttons/SkinButton").gameObject;
        SetupButton(skinButton, "SkinButton", GetTabName(CostumeTabType.Skin), () =>
        {
            ShowCostumeTab(CostumeTabType.Skin, obj, FastDestroyableSingleton<HatManager>.Instance.GetUnlockedSkins().Select(skin => new CosmeticDataWrapper(skin) as ICosmeticData).ToList(), () => new CosmeticDataWrapper(FastDestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin)), (cosmetic) =>
            {
                DataManager.Player.Customization.Skin = cosmetic.ProdId;
                if (PlayerControl.LocalPlayer != null)
                    PlayerControl.LocalPlayer.RpcSetSkin(cosmetic.ProdId);
            }, (cosmetic) =>
            {
                obj.PreviewArea.SetSkin(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            });
        });
    }

    private void SetupButton(GameObject button, string buttonName, string translated, System.Action onClick)
    {
        if (button == null)
        {
            Logger.Error($"{buttonName} not found in CosmeticMenuKisekae");
            return;
        }

        // テキスト設定（もしテキストコンポーネントがあれば）
        var textComponent = button.transform.Find("Text")?.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = translated;
        }

        // ボタンイベント設定
        var passiveButton = button.AddComponent<PassiveButton>();
        if (button.GetComponent<BoxCollider2D>() != null)
        {
            passiveButton.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        }

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() => onClick()));

        // ハイライト処理
        GameObject selectedObject = button.transform.Find("Selected")?.gameObject;
        if (selectedObject != null)
        {
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                selectedObject.SetActive(true);
            }));

            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                selectedObject.SetActive(false);
            }));

            // 初期状態では非表示
            selectedObject.SetActive(false);
        }
    }

    private List<Transform> slots;
    private Scroller scroller;
    private float lastInnerY;
    private List<Transform> activeSlots;
    private void ShowCostumeTab(CostumeTabType tabType, PlayerCustomizationMenu obj, List<ICosmeticData> unlockedCosmetics, Func<ICosmeticData> currentCosmeticFunc, Action<ICosmeticData> onSet, Action<ICosmeticData> onPreview)
    {
        string currentCosmeticId = currentCosmeticFunc().ProdId;
        slots = [];
        activeSlots = [];
        slotToGroupMap = null;
        kisekae.SetActive(false);
        if (CurrentCostumeTab != null)
        {
            GameObject.Destroy(CurrentCostumeTab);
        }
        CurrentCostumeTabType = tabType;
        CurrentCostumeTab = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuList"), kisekae.transform.parent);
        CurrentCostumeTab.transform.localPosition = new(0, -0.085f, -10);
        CurrentCostumeTab.transform.localScale = Vector3.one * 0.26f;
        CurrentCostumeTab.transform.Find("LeftArea/Scroller/Inner/CategoryText").GetComponent<TextMeshPro>().text = GetTabName(tabType);

        var slotBase = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticItemSlot"), CurrentCostumeTab.transform);
        var slotBasePassive = slotBase.AddComponent<PassiveButton>();
        slotBase.SetActive(false);

        CustomCosmeticsCostumeSlot costumeSlot = slotBase.AddComponent<CustomCosmeticsCostumeSlot>();
        PassiveButton selectedButton = null;

        int itemsPerRow = 7;
        int totalItems = unlockedCosmetics.Count;
        scroller = CurrentCostumeTab.transform.Find("LeftArea/Scroller").GetComponent<Scroller>();
        Transform inner = scroller.transform.Find("Inner");

        for (int i = 0; i < totalItems; i++)
        {
            int row = i / itemsPerRow;
            int col = i % itemsPerRow;

            int index = i;

            var slot = GameObject.Instantiate(costumeSlot, inner);
            slots.Add(slot.transform);
            slot.Awake();

            var cosmeticData = unlockedCosmetics[i];

            if (cosmeticData.PreviewCrewmateColor)
            {
                slot.spriteRenderer.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(slot.spriteRenderer, false);
                PlayerMaterial.SetColors(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, slot.spriteRenderer);
            }

            slot.transform.localPosition = new(-15.78f + col * 2.63f, 2.63f - row * 2.6f, -10);
            slot.transform.localScale = Vector3.one * 0.8f;
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
                SetCosmetic(unlockedCosmetics[index], onSet);
            }));
            slot.button.OnMouseOver = new();
            slot.button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(true);
                PreviewCosmetic(unlockedCosmetics[index], obj, onPreview);
            }));
            slot.button.OnMouseOut = new();
            slot.button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (selectedButton != slot.button)
                    slot.transform.Find("Selected").gameObject.SetActive(false);
                PreviewCosmetic(currentCosmeticFunc(), obj, onPreview);
            }));
            slot.gameObject.SetActive(true);
            cosmeticData.SetPreview(slot.spriteRenderer, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            if (cosmeticData.ProdId == currentCosmeticId)
            {
                slot.button.SelectButton(true);
                selectedButton = slot.button;
                selectedButton.transform.Find("Selected").gameObject.SetActive(true);
            }
        }
        float contentYBounds = 0;
        if (totalItems > 35)
        {
            int extraRows = Mathf.CeilToInt((totalItems - 35) / (float)itemsPerRow);
            contentYBounds = extraRows * 2.6f + 0.15f;
        }
        scroller.ContentYBounds = new(0, contentYBounds);
    }

    private void PreviewCosmetic(ICosmeticData cosmetic, PlayerCustomizationMenu obj, Action<ICosmeticData> onPreview)
    {
        if (cosmetic != null)
        {
            onPreview(cosmetic);
            obj.SetItemName(cosmetic.GetItemName());
        }
    }

    private void SetCosmetic(ICosmeticData cosmetic, Action<ICosmeticData> onSet)
    {
        if (cosmetic != null)
        {
            onSet(cosmetic);
        }
    }

    private static string GetTabName(CostumeTabType tabType)
    {
        switch (tabType)
        {
            case CostumeTabType.Visor1:
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visor) + " 1";
            case CostumeTabType.Hat1:
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HatLabel) + " 1";
            case CostumeTabType.Visor2:
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Visor) + " 2";
            case CostumeTabType.Hat2:
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.HatLabel) + " 2";
            case CostumeTabType.Skin:
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SkinLabel);
            default:
                throw new System.Exception($"Invalid costume tab type: {tabType}");
        }
    }
    private Dictionary<Transform, int> slotToGroupMap;

    public override void Update()
    {
        if (scroller == null) return;

        // transform参照をローカル変数にキャッシュして毎回の参照コストを削減
        Transform innerTransform = scroller.Inner.transform;
        Transform scrollerTransform = scroller.transform;

        float currentInnerY = innerTransform.localPosition.y;
        if (currentInnerY == lastInnerY) return; // 変化がなければ処理を省略

        float deltaY = currentInnerY - lastInnerY;
        lastInnerY = currentInnerY;

        if (slots.Count <= 35) return;

        int totalSlots = slots.Count;
        int totalGroups = Mathf.CeilToInt(totalSlots / 7f);

        // 代表スロットの位置と行間隔を利用して可視範囲のグループのみを計算する
        Vector3 firstGroupPos = scrollerTransform.InverseTransformPoint(slots[0].position);
        float firstGroupY = firstGroupPos.y;
        float spacing = 2.6f; // デフォルトの間隔
        if (slots.Count >= 7)
        {
            spacing = Mathf.Abs(scrollerTransform.InverseTransformPoint(slots[7].position).y - firstGroupY);
            if (spacing < 0.001f) spacing = 2.6f;
        }

        float upperBound = 12.6f; // 可視上限
        float lowerBound = -15.3f; // 可視下限

        // 可視範囲に入るグループのインデックス範囲を算出する（バッファを少し追加）
        int visibleStartGroup = Mathf.Max(0, Mathf.FloorToInt((firstGroupY - upperBound - 2.0f) / spacing));
        int visibleEndGroup = Mathf.Min(totalGroups - 1, Mathf.CeilToInt((firstGroupY - lowerBound + 2.0f) / spacing));

        // 前回のアクティブグループ範囲を取得
        int prevFirstGroup = -1;
        int prevLastGroup = -1;

        // スロットとグループのマッピングを保持するDictionaryを使用
        // 初回実行時に初期化
        if (slotToGroupMap == null)
        {
            slotToGroupMap = new Dictionary<Transform, int>();
            for (int i = 0; i < slots.Count; i++)
            {
                slotToGroupMap[slots[i]] = i / 7;
            }
        }

        if (activeSlots.Count > 0)
        {
            // Dictionaryから直接グループ番号を取得
            prevFirstGroup = slotToGroupMap[activeSlots[0]];
            prevLastGroup = slotToGroupMap[activeSlots[activeSlots.Count - 1]];
        }

        // 更新が必要かどうかを判定
        bool needUpdate = false;
        if (activeSlots.Count == 0 || visibleStartGroup != prevFirstGroup || visibleEndGroup != prevLastGroup)
        {
            needUpdate = true;
        }

        // 更新が必要な場合のみ処理を実行
        if (needUpdate)
        {
            // 更新範囲を厳密に可視範囲のみに制限（バッファは既に追加済み）
            int updateStartGroup = visibleStartGroup;
            int updateEndGroup = visibleEndGroup;

            // 現在のアクティブスロットを非アクティブにする（可視範囲外になったもののみ）
            for (int i = activeSlots.Count - 1; i >= 0; i--)
            {
                var slot = activeSlots[i];
                int group = slotToGroupMap[slot];
                if (group < updateStartGroup || group > updateEndGroup)
                {
                    slot.gameObject.SetActive(false);
                    activeSlots.RemoveAt(i);
                }
            }

            // 新しく可視範囲に入ったグループのスロットをアクティブにする
            for (int g = updateStartGroup; g <= updateEndGroup; g++)
            {
                int startIndex = g * 7;
                int endIndex = Mathf.Min(startIndex + 7, totalSlots);

                // このグループのスロットが既にアクティブかチェック
                bool groupAlreadyActive = false;
                if (activeSlots.Count > 0)
                {
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        if (j < totalSlots && slots[j].gameObject.activeSelf)
                        {
                            groupAlreadyActive = true;
                            break;
                        }
                    }
                }

                // 既にアクティブなグループはスキップ
                if (groupAlreadyActive) continue;

                // 代表スロットで可視判定
                Vector3 repPos = scrollerTransform.InverseTransformPoint(slots[startIndex].position);
                bool groupActive = repPos.y > lowerBound && repPos.y < upperBound;

                if (groupActive)
                {
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        if (j < totalSlots)
                        {
                            var slot = slots[j];
                            slot.gameObject.SetActive(true);
                            activeSlots.Add(slot);
                        }
                    }
                }
            }
        }
    }

    public override void Hide()
    {
        GameObject.Destroy(kisekae);
        if (CurrentCostumeTab != null)
            GameObject.Destroy(CurrentCostumeTab);
    }
}
public class CustomCosmeticsCostumeSlot : MonoBehaviour
{
    public PassiveButton button;
    public SpriteRenderer spriteRenderer;
    public void Awake()
    {
        if (button == null)
            button = GetComponent<PassiveButton>();
        if (spriteRenderer == null)
            spriteRenderer = transform.Find("Slot").GetComponent<SpriteRenderer>();
    }
}