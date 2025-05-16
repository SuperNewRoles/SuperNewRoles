using SuperNewRoles.Modules;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using AmongUs.Data;
using System;
using Innersloth.Assets;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using Sentry.Unity.NativeUtils;

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
    string Package { get; }
    string Package_EN { get; }
    Sprite Asset { get; }
    Sprite Asset_Back { get; }
    void SetPreview(SpriteRenderer renderer, int colorId);
    string GetItemName();
    string Author { get; }
    void LoadAsync(Action onSuccess);
}
public interface ICustomCosmeticHat
{
    CustomCosmeticsHatOptions Options { get; }
    Sprite Climb { get; }
    Sprite ClimbLeft { get; }
    Sprite Front { get; }
    Sprite Back { get; }
    Sprite Flip { get; }
    Sprite FlipBack { get; }
    bool BlocksVisors { get; }
    void SetDontUnload();
    void SetDoUnload();
}
public interface ICustomCosmeticVisor
{
    CustomCosmeticsVisorOptions Options { get; }
    Sprite Climb { get; }
    Sprite Idle { get; }
    Sprite IdleLeft { get; }
    void SetDontUnload();
    void SetDoUnload();
}

// 標準のCosmeticDataをラップするクラス
public abstract class CosmeticDataWrapper : ICosmeticData
{
    protected readonly CosmeticData _data;

    public CosmeticDataWrapper(CosmeticData data)
    {
        _data = data;
    }

    public abstract string Package { get; }
    public abstract string Package_EN { get; }
    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => _data.PreviewCrewmateColor;
    public string Author => "Vanilla";
    public abstract Sprite Asset { get; }
    public abstract Sprite Asset_Back { get; }

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
    public abstract void LoadAsync(Action onSuccess);
}
public class CosmeticDataWrapperHat : CosmeticDataWrapper, ICustomCosmeticHat
{
    public CosmeticDataWrapperHat(HatData data) : base(data)
    {
        _options = new(front: data.PreviewCrewmateColor ? HatOptionType.Adaptive : HatOptionType.NoAdaptive,
        back: data.InFront ? HatOptionType.None : data.PreviewCrewmateColor ? HatOptionType.Adaptive : HatOptionType.NoAdaptive,
        flip: HatOptionType.None,
        flip_back: HatOptionType.None,
        climb: HatOptionType.None,
        hideBody: false);
    }
    public void SetDontUnload()
    {
        // なんもなくていいとおもう
    }
    public void SetDoUnload()
    {
        if (hatViewData == null) return;
        hatViewData.Unload();
    }

    public override string Package => "バニラハット";
    public override string Package_EN => "Vanilla Hats";

    public bool BlocksVisors => (_data as HatData).BlocksVisors;
    private CustomCosmeticsHatOptions _options;
    public CustomCosmeticsHatOptions Options => _options;

    public Sprite Climb => hatViewData?.GetAsset()?.ClimbImage;
    public Sprite ClimbLeft => hatViewData?.GetAsset()?.LeftClimbImage;
    public Sprite Front => hatViewData?.GetAsset()?.MainImage;
    public Sprite Back => hatViewData?.GetAsset()?.BackImage;
    public Sprite Flip => hatViewData?.GetAsset()?.LeftMainImage;
    public Sprite FlipBack => hatViewData?.GetAsset()?.LeftBackImage;

    private AddressableAsset<HatViewData> hatViewData;
    public override Sprite Asset => hatViewData?.GetAsset()?.MainImage;
    public override Sprite Asset_Back => hatViewData?.GetAsset()?.BackImage;
    public override void LoadAsync(Action onSuccess)
    {
        // Logger.Info("Wrrapperrrr!!!!!!!!!!!!!!!!!!");
        if (_data is HatData hat)
        {
            // Logger.Info("Wrrapperrr222222222222r!!!!!!!!!!!!!!!!!!");
            if (hatViewData == null)
                hatViewData = hat.CreateAddressableAsset();
            if (hatViewData != null)
                hatViewData.LoadAsync((Il2CppSystem.Action)onSuccess);
        }
    }
}
public class CosmeticDataWrapperVisor : CosmeticDataWrapper, ICustomCosmeticVisor
{
    public CosmeticDataWrapperVisor(VisorData data) : base(data)
    {
        _options = new(adaptive: data.PreviewCrewmateColor, flip: false, isSNR: false);
    }
    private AddressableAsset<VisorViewData> visorViewData;
    public override Sprite Asset => visorViewData?.GetAsset()?.IdleFrame;
    public override Sprite Asset_Back => null;
    public CustomCosmeticsVisorOptions Options => _options;
    private CustomCosmeticsVisorOptions _options;
    public Sprite Climb => visorViewData?.GetAsset()?.ClimbFrame;
    public Sprite Idle => visorViewData?.GetAsset()?.IdleFrame;
    public Sprite IdleLeft => visorViewData?.GetAsset()?.LeftIdleFrame;

    public override string Package => "バニラバイザー";
    public override string Package_EN => "Vanilla Visors";

    public void SetDontUnload()
    {
        // なんもなくていいとおもう
    }
    public void SetDoUnload()
    {
        if (visorViewData == null) return;
        visorViewData.Unload();
    }
    public override void LoadAsync(Action onSuccess)
    {
        if (_data is VisorData visor)
        {
            if (visorViewData == null)
                visorViewData = visor.CreateAddressableAsset();
            if (visorViewData != null)
                visorViewData.LoadAsync((Il2CppSystem.Action)onSuccess);
        }
    }
}
public class CosmeticDataWrapperSkin : CosmeticDataWrapper
{
    public CosmeticDataWrapperSkin(SkinData data) : base(data)
    {
    }
    private AddressableAsset<SkinViewData> skinViewData;
    public override Sprite Asset => skinViewData?.GetAsset()?.IdleFrame;
    public override Sprite Asset_Back => null;

    public override string Package => "バニラスキン";
    public override string Package_EN => "Vanilla Skins";

    public override void LoadAsync(Action onSuccess)
    {
        if (_data is SkinData skin)
        {
            if (skinViewData == null)
                skinViewData = skin.CreateAddressableAsset();
            if (skinViewData != null)
                skinViewData.LoadAsync((Il2CppSystem.Action)onSuccess);
        }
    }
}
public class CosmeticDataWrapperNamePlate : CosmeticDataWrapper
{
    public CosmeticDataWrapperNamePlate(NamePlateData data) : base(data)
    {
    }
    private AddressableAsset<NamePlateViewData> namePlateViewData;
    public override Sprite Asset => namePlateViewData?.GetAsset()?.Image;
    public override Sprite Asset_Back => null;

    public override string Package => "バニラスキン";
    public override string Package_EN => "Vanilla Skins";

    public override void LoadAsync(Action onSuccess)
    {
        if (_data is NamePlateData namePlate)
        {
            if (namePlateViewData == null)
                namePlateViewData = namePlate.CreateAddressableAsset();
            if (namePlateViewData != null)
                namePlateViewData.LoadAsync((Il2CppSystem.Action)onSuccess);
        }
    }
}

// ModdedHatDataをラップするクラス
public class ModdedHatDataWrapper : ICosmeticData, ICustomCosmeticHat
{
    private readonly CustomCosmeticsHat _data;
    public CustomCosmeticsHatOptions Options => _data.options;
    public string Author => _data.author;

    public string Package => _data.package.name;
    public string Package_EN => _data.package.name_en;

    public ModdedHatDataWrapper(CustomCosmeticsHat data)
    {
        if (data == null)
            throw new Exception("data is null");
        _data = data;
    }
    public void SetDontUnload()
    {
        _data.LoadFrontSprite()?.DontUnload();
        _data.LoadBackSprite()?.DontUnload();
        _data.LoadClimbSprite()?.DontUnload();
        _data.LoadClimbLeftSprite()?.DontUnload();
        _data.LoadFlipSprite()?.DontUnload();
        _data.LoadFlipBackSprite()?.DontUnload();
    }
    public void SetDoUnload()
    {/*
        _data.LoadFrontSprite()?.Unload();
        _data.LoadFrontLeftSprite()?.Unload();
        _data.LoadBackSprite()?.Unload();
        _data.LoadBackLeftSprite()?.Unload();
        _data.LoadClimbSprite()?.Unload();
        _data.LoadClimbLeftSprite()?.Unload();
        _data.LoadFlipSprite()?.Unload();
        _data.LoadFlipBackSprite()?.Unload();*/
    }
    public bool BlocksVisors => Options.blockVisors;
    public Sprite Climb => _data.LoadClimbSprite()?.DontUnload();
    public Sprite ClimbLeft => _data.LoadClimbLeftSprite()?.DontUnload();
    public Sprite Front => _data.LoadFrontSprite()?.DontUnload();
    public Sprite Back => _data.LoadBackSprite()?.DontUnload();
    public Sprite Flip => _data.LoadFlipSprite()?.DontUnload();
    public Sprite FlipBack => _data.LoadFlipBackSprite()?.DontUnload();

    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => _data.options.front == HatOptionType.Adaptive; // 通常はハットはクルーメイトの色を反映する

    public Sprite Asset => _data.LoadFrontSprite();
    public Sprite Asset_Back => _data.LoadBackSprite();

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        // ModdedHatDataのプレビュー設定ロジック
        renderer.sprite = _data.LoadFrontSprite();
    }

    public string GetItemName()
    {
        return FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.Japanese ? _data.name : _data.name_en;
    }

    public void LoadAsync(Action onSuccess)
    {
        SetDontUnload();
        onSuccess();
    }
}

// ModdedVisorDataをラップするクラス
public class ModdedVisorDataWrapper : ICosmeticData, ICustomCosmeticVisor
{
    private readonly CustomCosmeticsVisor _data;
    public CustomCosmeticsVisorOptions Options => _data.options;
    public string Author => _data.author;

    public string Package => _data.package.name;
    public string Package_EN => _data.package.name_en;

    public ModdedVisorDataWrapper(CustomCosmeticsVisor data)
    {
        if (data == null)
            throw new Exception("data is null");
        _data = data;
    }
    public void SetDontUnload()
    {
        _data.LoadIdleSprite()?.DontUnload();
        _data.LoadLeftIdleSprite()?.DontUnload();
        _data.LoadClimbSprite()?.DontUnload();
    }
    public void SetDoUnload()
    {/*
        _data.LoadIdleSprite()?.Unload();
        _data.LoadLeftIdleSprite()?.Unload();
        _data.LoadClimbSprite()?.Unload();*/
    }
    public bool BlocksVisors => false;
    public Sprite Climb => _data.LoadClimbSprite()?.DontUnload();
    public Sprite ClimbLeft => null;
    public Sprite Idle => _data.LoadIdleSprite()?.DontUnload();
    public Sprite IdleLeft => _data.LoadLeftIdleSprite()?.DontUnload();

    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => _data.options.adaptive; // バイザーがクルーメイトの色を反映するかどうか

    public Sprite Asset => _data.LoadIdleSprite();
    public Sprite Asset_Back => null;

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        // ModdedVisorDataのプレビュー設定ロジック
        renderer.sprite = _data.LoadIdleSprite();
    }

    public string GetItemName()
    {
        return FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.Japanese ? _data.name : _data.name_en;
    }

    public void LoadAsync(Action onSuccess)
    {
        SetDontUnload();
        onSuccess?.Invoke();
    }
}

public class ModdedNamePlateDataWrapper : ICosmeticData
{
    private readonly CustomCosmeticsNamePlate _data;
    public string Author => _data.author;

    public string Package => _data.package.name;
    public string Package_EN => _data.package.name_en;

    public ModdedNamePlateDataWrapper(CustomCosmeticsNamePlate data)
    {
        if (data == null)
            throw new Exception("data is null");
        _data = data;
    }

    public string ProdId => _data.ProdId;
    public bool PreviewCrewmateColor => false;

    public Sprite Asset => _data.LoadSprite();
    public Sprite Asset_Back => null;

    public void SetPreview(SpriteRenderer renderer, int colorId)
    {
        renderer.sprite = Asset;
    }

    public string GetItemName()
    {
        return _data.name + "\nby " + _data.author;
    }

    public void LoadAsync(Action onSuccess)
    {
        onSuccess?.Invoke();
    }
}

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
        var obj = PlayerCustomizationMenu.Instance;
        kisekae = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuKisekae"), obj.transform);
        kisekae.transform.localPosition = new(0.31f, -0.085f, -15f);
        kisekae.transform.localScale = Vector3.one * 0.28f;

        // ボタンのセットアップ
        SetupButtons(obj);

        slots = new();
        activeSlots = new();
        slotToGroupMap = new();
    }

    private void SetupButtons(PlayerCustomizationMenu obj)
    {
        obj.SetItemName("");
        // VisorButton01のセットアップ
        visorButton01 = kisekae.transform.Find("Buttons/VisorButton01").gameObject;
        SetupButton(visorButton01, "VisorButton01", GetTabName(CostumeTabType.Visor1), () =>
        {
            // VisorButton01がクリックされたときの処理
            Logger.Info("VisorButton01 clicked");
            List<ICosmeticData> combinedCosmetics = new();
            foreach (var cosmetic in FastDestroyableSingleton<HatManager>.Instance.GetUnlockedVisors())
            {
                combinedCosmetics.Add(new CosmeticDataWrapperVisor(cosmetic));
            }
            foreach (var moddedVisor in CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.visors))
            {
                combinedCosmetics.Add(new ModdedVisorDataWrapper(moddedVisor));
            }
            ICosmeticData getVisor()
            {
                if (!DataManager.Player.Customization.Visor.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                    return new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
                var moddedVisor = CustomCosmeticsLoader.GetModdedVisor(DataManager.Player.Customization.Visor);
                if (moddedVisor != null)
                    return new ModdedVisorDataWrapper(moddedVisor);
                return new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById("visor_NoVisor"));
            }
            ShowCostumeTab(CostumeTabType.Visor1, obj, combinedCosmetics, getVisor, (cosmetic) =>
            {
                DataManager.Player.Customization.Visor = cosmetic.ProdId;
                if (PlayerControl.LocalPlayer != null)
                    PlayerControl.LocalPlayer.RpcSetVisor(cosmetic.ProdId);
                DataManager.Player.Save();

                // プレビュー画像を更新
                UpdateButtonPreview(visorButton01, cosmetic);
            }, (cosmetic) =>
            {
                CustomCosmeticsLayers.ExistsOrInitialize(obj.PreviewArea.cosmetics).visor1.SetVisor(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            }, () => new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(VisorData.EmptyId)));
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
                combinedCosmetics.Add(new CosmeticDataWrapperHat(cosmetic));
            }
            foreach (var moddedHat in CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.hats))
            {
                combinedCosmetics.Add(new ModdedHatDataWrapper(moddedHat));
            }
            ICosmeticData getHat()
            {
                if (!DataManager.Player.Customization.Hat.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                    return new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat));
                var moddedHat = CustomCosmeticsLoader.GetModdedHat(DataManager.Player.Customization.Hat);
                if (moddedHat != null)
                    return new ModdedHatDataWrapper(moddedHat);
                return new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
            }
            ShowCostumeTab(CostumeTabType.Hat1, obj, combinedCosmetics, getHat, (cosmetic) =>
            {
                DataManager.Player.Customization.Hat = cosmetic.ProdId;
                if (PlayerControl.LocalPlayer != null)
                    PlayerControl.LocalPlayer.RpcSetHat(cosmetic.ProdId);
                DataManager.Player.Save();

                // プレビュー画像を更新
                UpdateButtonPreview(hatButton01, cosmetic);
            }, (cosmetic) =>
            {
                CustomCosmeticsLayers.ExistsOrInitialize(obj.PreviewArea.cosmetics).hat1.SetHat(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            }, () => new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(HatData.EmptyId)));
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
                combinedCosmetics.Add(new CosmeticDataWrapperVisor(cosmetic));
            }
            foreach (var moddedVisor in CustomCosmeticsLoader.LoadedPackages.SelectMany(package => package.visors))
            {
                combinedCosmetics.Add(new ModdedVisorDataWrapper(moddedVisor));
            }

            ICosmeticData getVisor()
            {
                if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentVisor2Id))
                    return new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById("visor_NoVisor"));
                else
                {
                    if (CustomCosmeticsSaver.CurrentVisor2Id.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                    {
                        var moddedVisor = CustomCosmeticsLoader.GetModdedVisor(CustomCosmeticsSaver.CurrentVisor2Id);
                        if (moddedVisor != null)
                            return new ModdedVisorDataWrapper(moddedVisor);
                        return new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById("visor_NoVisor"));
                    }
                    else
                        return new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(CustomCosmeticsSaver.CurrentVisor2Id));
                }
            }
            ShowCostumeTab(CostumeTabType.Visor2, obj, combinedCosmetics, getVisor, (cosmetic) =>
            {
                CustomCosmeticsSaver.SetVisor2Id(cosmetic.ProdId);
                if (PlayerControl.LocalPlayer != null)
                    PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Visor2, cosmetic.ProdId, PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId);

                // プレビュー画像を更新
                UpdateButtonPreview(visorButton02, cosmetic);
            }, (cosmetic) =>
            {
                CustomCosmeticsLayers.ExistsOrInitialize(obj.PreviewArea.cosmetics).visor2.SetVisor(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            }, () => new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(VisorData.EmptyId)));
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
                combinedCosmetics.Add(new CosmeticDataWrapperHat(cosmetic));
            }

            foreach (var moddedHat in CustomCosmeticsLoader.moddedHats.Values)
            {
                combinedCosmetics.Add(new ModdedHatDataWrapper(moddedHat));
            }
            ICosmeticData getHat()
            {
                if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentHat2Id))
                    return new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
                else
                {
                    if (CustomCosmeticsSaver.CurrentHat2Id.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
                    {
                        var moddedHat = CustomCosmeticsLoader.GetModdedHat(CustomCosmeticsSaver.CurrentHat2Id);
                        if (moddedHat != null)
                            return new ModdedHatDataWrapper(moddedHat);
                        return new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
                    }
                    else
                        return new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(CustomCosmeticsSaver.CurrentHat2Id));
                }
            }
            ShowCostumeTab(CostumeTabType.Hat2, obj, combinedCosmetics, getHat, (cosmetic) =>
                {
                    CustomCosmeticsSaver.SetHat2Id(cosmetic.ProdId);
                    if (PlayerControl.LocalPlayer != null)
                        PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Hat2, cosmetic.ProdId, PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId);

                    // プレビュー画像を更新
                    UpdateButtonPreview(hatButton02, cosmetic);
                }, (cosmetic) =>
                {
                    CustomCosmeticsLayers.ExistsOrInitialize(obj.PreviewArea.cosmetics).hat2.SetHat(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
                }, () => new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(HatData.EmptyId)));
        });

        // SkinButtonのセットアップ
        skinButton = kisekae.transform.Find("Buttons/SkinButton").gameObject;
        SetupButton(skinButton, "SkinButton", GetTabName(CostumeTabType.Skin), () =>
        {
            ShowCostumeTab(CostumeTabType.Skin, obj, FastDestroyableSingleton<HatManager>.Instance.GetUnlockedSkins().Select(skin => new CosmeticDataWrapperSkin(skin) as ICosmeticData).ToList(), () => new CosmeticDataWrapperSkin(FastDestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin)), (cosmetic) =>
            {
                DataManager.Player.Customization.Skin = cosmetic.ProdId;
                if (PlayerControl.LocalPlayer != null)
                    PlayerControl.LocalPlayer.RpcSetSkin(cosmetic.ProdId);

                // プレビュー画像を更新
                UpdateButtonPreview(skinButton, cosmetic);
            }, (cosmetic) =>
            {
                obj.PreviewArea.SetSkin(cosmetic.ProdId, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            }, () => new CosmeticDataWrapperSkin(FastDestroyableSingleton<HatManager>.Instance.GetSkinById(SkinData.EmptyId)));
        });

        ControllerManager.Instance.SetCurrentSelected(visorButton02.GetComponent<PassiveButton>());
        visorButton02.GetComponent<PassiveButton>().ReceiveMouseOver();

        // 初期プレビューの設定
        InitializeButtonPreviews();
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

        ControllerManager.Instance.AddSelectableUiElement(passiveButton);

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
    private void ShowCostumeTab(CostumeTabType tabType, PlayerCustomizationMenu obj, List<ICosmeticData> unlockedCosmetics, Func<ICosmeticData> currentCosmeticFunc, Action<ICosmeticData> onSet, Action<ICosmeticData> onPreview, Func<ICosmeticData> emptyCosmeticFunc)
    {
        if (tabType != CostumeTabType.Skin)
        {
            CustomCosmeticsUIStart.SetFrameType(CustomCosmeticsUIStart.FrameType.Category);
            obj.PreviewArea.transform.localPosition = new(0.2f, -0.25f, -3f);
            obj.itemName.transform.localPosition = new(0.25f, -1.74f, -5f);
        }
        string currentCosmeticId = currentCosmeticFunc()?.ProdId ?? "";
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
        CurrentCostumeTab.transform.localScale = Vector3.one * 0.27f;
        CurrentCostumeTab.transform.Find("LeftArea/Scroller/Inner/CategoryText").GetComponent<TextMeshPro>().text = GetTabName(tabType);

        var categoryScroller = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CategoryScroller"), CurrentCostumeTab.transform);
        categoryScroller.transform.localPosition = new(-0.05f, -0.085f, -20);
        categoryScroller.transform.localScale = Vector3.one * 1.05f;
        var categoryScrollerscroller = categoryScroller.GetComponentInChildren<Scroller>();
        var slotBase = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticItemSlot"), CurrentCostumeTab.transform);
        var slotBasePassive = slotBase.AddComponent<PassiveButton>();
        slotBase.SetActive(false);

        CustomCosmeticsCostumeSlot costumeSlot = slotBase.AddComponent<CustomCosmeticsCostumeSlot>();
        PassiveButton selectedButton = null;

        int itemsPerRow = tabType != CostumeTabType.Skin ? 6 : 7;
        int totalItems = unlockedCosmetics.Count;
        scroller = CurrentCostumeTab.transform.Find("LeftArea/Scroller").GetComponent<Scroller>();
        Transform inner = scroller.transform.Find("Inner");

        SortedDictionary<string, List<ICosmeticData>> packagedCosmetics = new();

        foreach (var cosmetic in unlockedCosmetics)
        {
            if (packagedCosmetics.ContainsKey(cosmetic.Package))
                packagedCosmetics[cosmetic.Package].Add(cosmetic);
            else
                packagedCosmetics[cosmetic.Package] = [cosmetic];
        }
        int allI = itemsPerRow;
        float offSetY = tabType != CostumeTabType.Skin ? 1.3f : 1.1f;
        int package_i = 0;
        Dictionary<string, PassiveButton> categoryFirstCosmetics = new();
        foreach (var package in packagedCosmetics)
        {
            Logger.Info($"Package!!!: {package.Key} {package.Value.Count}");

            // パッケージ名を表示するテキストを追加
            GameObject packageText = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticPackageText"), CurrentCostumeTab.transform);
            packageText.transform.SetParent(inner);
            packageText.transform.localScale = Vector3.one * (tabType != CostumeTabType.Skin ? 0.78f : 0.7f);
            TextMeshPro textMesh = packageText.GetComponent<TextMeshPro>();
            textMesh.fontSize = 1.8f;
            textMesh.alignment = TextAlignmentOptions.TopLeft;
            textMesh.text = FastDestroyableSingleton<TranslationController>.Instance.currentLanguage.languageID == SupportedLangs.Japanese ? package.Key : package.Value.First().Package_EN;
            textMesh.color = Color.white;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.enableWordWrapping = false;
            textMesh.rectTransform.sizeDelta = new Vector2(35f, 2f);

            // パッケージテキストの位置を設定
            int row = allI / itemsPerRow;
            packageText.transform.localPosition = new(tabType != CostumeTabType.Skin ? -5.8f : -6.7f, 2.63f - row * (tabType != CostumeTabType.Skin ? 2.68f : 2.6f) + 2.4f + offSetY, -10);

            var emptyCosmetic = emptyCosmeticFunc();
            if (!package.Value.Any(x => x.ProdId == emptyCosmetic.ProdId))
            {
                // Emptyスロットを追加
                var emptySlot = GameObject.Instantiate(costumeSlot, inner);
                slots.Add(emptySlot.transform);
                emptySlot.Awake();

                // Emptyスロットの設定
                int emptyCol = allI % itemsPerRow;
                int emptyRow = allI / itemsPerRow;
                emptySlot.transform.localPosition = tabType != CostumeTabType.Skin ? new(-15.69f + emptyCol * 2.77f, 2.63f - emptyRow * 2.68f + offSetY, -10) : new(-15.78f + emptyCol * 2.63f, 2.63f - emptyRow * 2.6f + offSetY, -10);
                emptySlot.transform.localScale = Vector3.one * (tabType != CostumeTabType.Skin ? 0.85f : 0.8f);
                emptySlot.button.Colliders = new Collider2D[] { emptySlot.GetComponent<BoxCollider2D>() };
                emptySlot.button.OnClick = new();
                emptySlot.button.OnClick.AddListener((UnityAction)(() =>
                {
                    Logger.Info("Empty Slot clicked");
                    if (selectedButton != null)
                    {
                        selectedButton.SelectButton(false);
                        selectedButton.transform.Find("Selected").gameObject.SetActive(false);
                    }
                    emptySlot.button.SelectButton(true);
                    selectedButton = emptySlot.button;
                    selectedButton.transform.Find("Selected").gameObject.SetActive(true);
                    SetCosmetic(emptyCosmetic, onSet);
                    PreviewCosmetic(emptyCosmetic, obj, onPreview);
                }));
                emptySlot.button.OnMouseOver = new();
                emptySlot.button.OnMouseOver.AddListener((UnityAction)(() =>
                {
                    if (selectedButton != emptySlot.button)
                        emptySlot.transform.Find("Selected").gameObject.SetActive(true);
                    PreviewCosmetic(emptyCosmetic, obj, onPreview);
                }));
                emptySlot.button.OnMouseOut = new();
                emptySlot.button.OnMouseOut.AddListener((UnityAction)(() =>
                {
                    if (selectedButton != emptySlot.button)
                        emptySlot.transform.Find("Selected").gameObject.SetActive(false); // Empty slot preview is always empty
                    PreviewCosmetic(currentCosmeticFunc(), obj, onPreview);
                }));
                emptySlot.gameObject.SetActive(true);
                Logger.Info($"Empty cosmetic: {emptyCosmetic.ProdId}");
                // Empty cosmetic doesn't need LoadAsync or explicit preview setup
                // emptySlot.spriteRenderer.sprite = null; // Clear sprite
                // Handle translation for "None" or "Empty"
                // For now, leave sprite blank, name will be handled by GetItemName of the empty wrapper

                if (emptyCosmetic.ProdId == currentCosmeticId) // Check if the current selected item is the empty one
                {
                    emptySlot.button.SelectButton(true);
                    selectedButton = emptySlot.button;
                    selectedButton.transform.Find("Selected").gameObject.SetActive(true);
                }
                ControllerManager.Instance.AddSelectableUiElement(emptySlot.button);
                allI++;
            }
            //

            for (int i = 0; i < package.Value.Count; i++)
            {
                int index = i;

                // 各アイテムの行と列を計算
                int col = allI % itemsPerRow;
                int itemRow = allI / itemsPerRow;

                var slot = GameObject.Instantiate(costumeSlot, inner);
                slots.Add(slot.transform);
                slot.Awake();

                var cosmeticData = package.Value[i];

                if (cosmeticData.PreviewCrewmateColor)
                {
                    slot.spriteRenderer.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                    PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(slot.spriteRenderer, false);
                    PlayerMaterial.SetColors(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, slot.spriteRenderer);
                }

                slot.transform.localPosition = tabType != CostumeTabType.Skin ? new(-15.69f + col * 2.77f, 2.63f - itemRow * 2.68f + offSetY, -10) : new(-15.78f + col * 2.63f, 2.63f - itemRow * 2.6f + offSetY, -10);
                slot.transform.localScale = Vector3.one * (tabType != CostumeTabType.Skin ? 0.85f : 0.8f);
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
                    SetCosmetic(package.Value[index], onSet);
                    PreviewCosmetic(package.Value[index], obj, onPreview);
                }));
                slot.button.OnMouseOver = new();
                slot.button.OnMouseOver.AddListener((UnityAction)(() =>
                {
                    if (selectedButton != slot.button)
                        slot.transform.Find("Selected").gameObject.SetActive(true);
                    PreviewCosmetic(package.Value[index], obj, onPreview);
                }));
                slot.button.OnMouseOut = new();
                slot.button.OnMouseOut.AddListener((UnityAction)(() =>
                {
                    if (selectedButton != slot.button)
                        slot.transform.Find("Selected").gameObject.SetActive(false);
                    PreviewCosmetic(currentCosmeticFunc(), obj, onPreview);
                }));
                slot.gameObject.SetActive(true);
                cosmeticData.LoadAsync(() =>
                {
                    cosmeticData.SetPreview(slot.spriteRenderer, PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
                });
                if (cosmeticData.ProdId == currentCosmeticId)
                {
                    slot.button.SelectButton(true);
                    selectedButton = slot.button;
                    selectedButton.transform.Find("Selected").gameObject.SetActive(true);
                }
                ControllerManager.Instance.AddSelectableUiElement(slot.button);
                allI++;
                if (!categoryFirstCosmetics.ContainsKey(package.Key))
                {
                    categoryFirstCosmetics[package.Key] = slot.button;
                }
            }

            if (selectedButton != null)
            {
                ControllerManager.Instance.SetCurrentSelected(selectedButton);
                selectedButton.ReceiveMouseOver();
            }

            var category = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CategoryTubButton"), categoryScrollerscroller.Inner);
            category.transform.localScale = Vector3.one * 0.98f;
            category.transform.localPosition = new(1.95f, 4.6f - package_i * 2.6f, -10);
            category.transform.Find("Text").GetComponent<TextMeshPro>().text = package.Key;
            PassiveButton categoryButton = category.AddComponent<PassiveButton>();
            categoryButton.Colliders = new Collider2D[] { category.GetComponentInChildren<BoxCollider2D>() };
            categoryButton.OnClick = new();
            categoryButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info($"Category clicked: {package.Key}");
                scroller.Inner.transform.localPosition = new(0, -categoryFirstCosmetics[package.Key].transform.localPosition.y + 3.5f, 0);
            }));
            categoryButton.OnMouseOut = new();
            categoryButton.OnMouseOver = new();

            if (tabType == CostumeTabType.Skin)
                category.SetActive(false);

            package_i++;
            if (package_i < packagedCosmetics.Count)
            {
                // パッケージ間に余白を追加
                if (allI % itemsPerRow == 0)
                    allI += itemsPerRow; // 次の行から始める
                else
                    allI += (itemsPerRow * 2) - (allI % itemsPerRow); // 現在の行を埋めて次の行をスキップ
            }
        }
        if (packagedCosmetics.Count > 6)
        {
            categoryScrollerscroller.ContentYBounds.max = (packagedCosmetics.Count - 6) * 2.65f + 0.6f;
        }
        float contentYBounds = 0;
        if (allI > 35)
        {
            int extraRows = Mathf.CeilToInt((allI - 35) / (float)itemsPerRow);
            contentYBounds = extraRows * 2.7f + 2.45f - offSetY;
        }
        scroller.ContentYBounds = new(0, contentYBounds);
    }

    private void PreviewCosmetic(ICosmeticData cosmetic, PlayerCustomizationMenu obj, Action<ICosmeticData> onPreview)
    {
        if (cosmetic != null)
        {
            onPreview(cosmetic);
            if (cosmetic.ProdId.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
            {
                obj.SetItemName(cosmetic.GetItemName() + "\n by " + cosmetic.Author);
            }
            else
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
                throw new Exception($"Invalid costume tab type: {tabType}");
        }
    }
    private Dictionary<Transform, int> slotToGroupMap;

    public override void Update()
    {
        return;
        if (scroller == null) return;

        // スクロール位置が変化したときだけ処理
        float currentInnerY = scroller.Inner.transform.localPosition.y;
        if (Mathf.Approximately(currentInnerY, lastInnerY)) return;

        lastInnerY = currentInnerY;

        if (slots.Count <= 35) return; // 少ない場合は常に全部表示

        // 可視範囲の計算
        float viewportHeight = 10f; // ビューポートの高さ
        float upperBound = 5f; // 上側の表示境界
        float lowerBound = -5f - viewportHeight; // 下側の表示境界

        // バッファを追加（少し余裕を持たせる）
        upperBound += 5f;
        lowerBound -= 5f;

        // 各スロットごとに可視判定
        for (int i = 0; i < slots.Count; i++)
        {
            Transform slot = slots[i];
            Vector3 slotWorldPos = slot.position;
            Vector3 viewportPos = scroller.transform.InverseTransformPoint(slotWorldPos);

            bool isVisible = viewportPos.y <= upperBound && viewportPos.y >= lowerBound;

            // 表示状態を更新（現在と異なる場合のみ）
            if (isVisible != slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(isVisible);

                // アクティブスロットリストの更新
                if (isVisible)
                {
                    if (!activeSlots.Contains(slot))
                        activeSlots.Add(slot);
                }
                else
                {
                    activeSlots.Remove(slot);
                }
            }
        }
    }

    public override void Hide()
    {
        GameObject.Destroy(kisekae);
        if (CurrentCostumeTab != null)
            GameObject.Destroy(CurrentCostumeTab);
        PlayerCustomizationMenu.Instance.PreviewArea.transform.localPosition = new(0f, -0.25f, -3f);
        PlayerCustomizationMenu.Instance.itemName.transform.localPosition = new(0f, -1.74f, -5f);
    }

    // ボタンのプレビュー画像を更新するメソッド
    private void UpdateButtonPreview(GameObject button, ICosmeticData cosmetic)
    {
        var previewRenderer = button.transform.Find("Preview")?.GetComponent<SpriteRenderer>();
        var previewRendererBack = button.transform.Find("PreviewBack")?.GetComponent<SpriteRenderer>();
        Logger.Info($"UpdateButtonPreview: {button.name}, {cosmetic.ProdId}");
        if (previewRenderer != null && previewRendererBack != null && cosmetic != null)
        {
            // アセットを非同期ロード
            cosmetic.LoadAsync(() =>
            {
                // ロード完了後にスプライトを設定
                previewRenderer.sprite = cosmetic.Asset;
                previewRendererBack.sprite = cosmetic.Asset_Back;
                // 色適応が必要な場合は色を設定
                if (cosmetic.PreviewCrewmateColor)
                {
                    previewRenderer.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                    PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(previewRenderer, false);
                    PlayerMaterial.SetColors(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, previewRenderer);
                    previewRendererBack.material = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                    PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(previewRendererBack, false);
                    PlayerMaterial.SetColors(PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color, previewRendererBack);
                }
                else
                {
                    previewRenderer.material = FastDestroyableSingleton<HatManager>.Instance.DefaultShader;
                    previewRendererBack.material = FastDestroyableSingleton<HatManager>.Instance.DefaultShader;
                }
            });
        }
    }

    // 初期プレビューの設定
    private void InitializeButtonPreviews()
    {
        // Visor1のプレビュー初期化
        ICosmeticData visor1Data;
        if (DataManager.Player.Customization.Visor.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
        {
            var moddedVisor = CustomCosmeticsLoader.GetModdedVisor(DataManager.Player.Customization.Visor);
            if (moddedVisor != null)
                visor1Data = new ModdedVisorDataWrapper(moddedVisor);
            else
                visor1Data = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
        }
        else
        {
            visor1Data = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
        }
        UpdateButtonPreview(visorButton01, visor1Data);

        // Hat1のプレビュー初期化
        ICosmeticData hat1Data;
        if (DataManager.Player.Customization.Hat.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
        {
            var moddedHat = CustomCosmeticsLoader.GetModdedHat(DataManager.Player.Customization.Hat);
            if (moddedHat != null)
                hat1Data = new ModdedHatDataWrapper(moddedHat);
            else
                hat1Data = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
        }
        else
        {
            hat1Data = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat));
        }
        UpdateButtonPreview(hatButton01, hat1Data);

        // Visor2のプレビュー初期化
        ICosmeticData visor2Data;
        if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentVisor2Id))
        {
            visor2Data = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById("visor_EmptyVisor"));
        }
        else if (CustomCosmeticsSaver.CurrentVisor2Id.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
        {
            var moddedVisor = CustomCosmeticsLoader.GetModdedVisor(CustomCosmeticsSaver.CurrentVisor2Id);
            if (moddedVisor != null)
                visor2Data = new ModdedVisorDataWrapper(moddedVisor);
            else
                visor2Data = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById("visor_EmptyVisor"));
        }
        else
        {
            visor2Data = new CosmeticDataWrapperVisor(FastDestroyableSingleton<HatManager>.Instance.GetVisorById(CustomCosmeticsSaver.CurrentVisor2Id));
        }
        UpdateButtonPreview(visorButton02, visor2Data);

        // Hat2のプレビュー初期化
        ICosmeticData hat2Data;
        if (string.IsNullOrEmpty(CustomCosmeticsSaver.CurrentHat2Id))
        {
            hat2Data = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
        }
        else if (CustomCosmeticsSaver.CurrentHat2Id.StartsWith(CustomCosmeticsLoader.ModdedPrefix))
        {
            var moddedHat = CustomCosmeticsLoader.GetModdedHat(CustomCosmeticsSaver.CurrentHat2Id);
            if (moddedHat != null)
                hat2Data = new ModdedHatDataWrapper(moddedHat);
            else
                hat2Data = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById("hat_NoHat"));
        }
        else
        {
            hat2Data = new CosmeticDataWrapperHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(CustomCosmeticsSaver.CurrentHat2Id));
        }
        UpdateButtonPreview(hatButton02, hat2Data);

        // Skinのプレビュー初期化
        var skinData = new CosmeticDataWrapperSkin(FastDestroyableSingleton<HatManager>.Instance.GetSkinById(DataManager.Player.Customization.Skin));
        UpdateButtonPreview(skinButton, skinData);
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