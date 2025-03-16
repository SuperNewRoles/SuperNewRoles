using AmongUs.Data;
using HarmonyLib;
using PowerTools;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using UnityEngine;
using static CosmeticsLayer;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

public static class CosmeticsHats
{
    public static void SetHat(string hatId)
    {
        PlayerControl.LocalPlayer.RpcSetHat(hatId);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetNamePosition))]
public static class CosmeticsLayer_SetNamePosition
{
    public static void Postfix(CosmeticsLayer __instance, Vector3 newPosition)
    {
        newPosition.z = -1f;
        __instance.nameTextContainer.transform.localPosition = newPosition;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetAsLocalPlayer))]
public static class PlayerControl_Start
{
    public static void Postfix(CosmeticsLayer __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat1?.SetLocalPlayer(true);
        customCosmeticsLayer?.hat2?.SetLocalPlayer(true);
        new LateTask(() =>
        {
            PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
        }, 0.1f, "SetHat2");
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ClientInitialize))]
public static class PlayerControl_ClientInitialize
{
    public static void Postfix(PlayerControl __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Hat1, DataManager.Player.Customization.Hat, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        customCosmeticsLayer?.hat1?.SetLocalPlayer(false);
        customCosmeticsLayer?.hat2?.SetLocalPlayer(false);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetHat), [typeof(string), typeof(int)])]
public static class PoolablePlayer_SetHat_String
{
    public static void Postfix(CosmeticsLayer __instance, string hatId, int color)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat1?.SetHat(hatId, color);
        __instance.OnCosmeticSet?.Invoke(hatId, color, CosmeticKind.HAT);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetHat), [typeof(HatData), typeof(int)])]
public static class PoolablePlayer_SetHat_HatData
{
    public static void Postfix(CosmeticsLayer __instance, HatData hatData, int color)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat1?.SetHat(new CosmeticDataWrapperHat(hatData), color);
        __instance.OnCosmeticSet?.Invoke(hatData.ProdId, color, CosmeticKind.HAT);
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromDataManager))]
public static class PoolablePlayer_UpdateFromDataManager
{
    public static void Postfix(PoolablePlayer __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(CustomCosmeticsSaver.CurrentHat2Id, DataManager.Player.Customization.Color);
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
public static class PoolablePlayer_UpdateFromEitherPlayerDataOrCache
{
    public static void Postfix(PoolablePlayer __instance, NetworkedPlayerInfo pData)
    {
        if (pData.Object == null) return;
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        CustomCosmeticsLayer pcLayer = CustomCosmeticsLayers.ExistsOrInitialize(pData.Object.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(pcLayer.hat2.Hat?.ProdId ?? "", pData.DefaultOutfit.ColorId);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHat))]
public static class PlayerControl_SetHat
{
    public static void Postfix(PlayerControl __instance, string hatId, int colorId)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(hatId, colorId);
    }
}
[HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
public static class HatParent_LateUpdate
{
    public static void Postfix(HatParent __instance)
    {
        __instance.FrontLayer.enabled = false;
        __instance.BackLayer.enabled = false;
    }
}
public class CustomHatLayer : MonoBehaviour
{
    public CosmeticsLayer CosmeticLayer;
    public SpriteRenderer BackLayer;

    public SpriteRenderer FrontLayer;

    public SpriteRenderer Parent;

    private PlayerMaterial.Properties matProperties;

    private bool shouldFaceLeft;

    private const float ClimbZOffset = -0.02f;
    private SpriteAnimNodeSync spriteSyncNode;

    public ICustomCosmeticHat CustomCosmeticHat { get; set; }
    public ICosmeticData Hat => CustomCosmeticHat as ICosmeticData;

    public bool Visible
    {
        set
        {
            FrontLayer.gameObject.SetActive(value);
            BackLayer.gameObject.SetActive(value);
        }
    }

    public Color SpriteColor
    {
        set
        {
            BackLayer.color = value;
            FrontLayer.color = value;
        }
    }

    public bool FlipX
    {
        set
        {
            BackLayer.flipX = value;
            FrontLayer.flipX = value;
        }
    }

    public void OnDestroy()
    {
        UnloadAsset();
    }

    public bool HasHat()
    {
        if (Hat != null)
        {
            return Hat.ProdId != HatData.EmptyId;
        }
        return false;
    }

    public void SetHat(string hatId, int color)
    {
        Logger.Info($"SetHat: {hatId}, {color}");
        if (DestroyableSingleton<HatManager>.InstanceExists)
        {
            ICosmeticData hat;
            if (hatId.StartsWith("Modded_"))
                hat = CustomCosmeticsLoader.GetModdedHatData(hatId);
            else
                hat = new CosmeticDataWrapperHat(DestroyableSingleton<HatManager>.Instance.GetHatById(hatId));
            SetHat(hat, color);
        }
    }

    public void SetHat(ICosmeticData hat, int color)
    {
        if (hat == null || hat != Hat)
        {
            BackLayer.sprite = null;
            FrontLayer.sprite = null;
        }
        CustomCosmeticHat = hat as ICustomCosmeticHat;
        SetHat(color);
    }

    private void SetHat(int color)
    {
        if (Hat == null) return;
        SetMaterialColor(color);
        UnloadAsset();
        Hat.LoadAsync(() =>
        {
            PopulateFromViewData();
        });
    }

    public void SetIdleAnim(int colorId)
    {
        if (Hat != null)
        {
            SetHat(colorId);
            base.transform.SetLocalZ(0f);
        }
    }

    public void SetShouldFaceLeft(bool leftFacingVictim)
    {
        shouldFaceLeft = leftFacingVictim;
    }

    public void SetFloorAnim()
    {
        BackLayer.enabled = false;
        FrontLayer.enabled = true;
        FrontLayer.flipX = false;
        FrontLayer.sprite = Hat.Asset;
    }

    public void SetClimbAnim()
    {
        if (CustomCosmeticHat.Options.climb != HatOptionType.None)
        {
            base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -0.02f);
            BackLayer.enabled = false;
            FrontLayer.enabled = true;
            FrontLayer.sprite = CustomCosmeticHat.Climb;
        }
    }

    public void SetLocalPlayer(bool localPlayer)
    {
        matProperties.IsLocalPlayer = localPlayer;
        UpdateMaterial();
    }

    public void SetMaterialColor(int color)
    {
        matProperties.ColorId = color;
        UpdateMaterial();
    }

    public void SetMaskType(PlayerMaterial.MaskType maskType)
    {
        matProperties.MaskType = maskType;
        UpdateMaterial();
    }

    public void SetMaskLayer(int layer)
    {
        matProperties.MaskLayer = layer;
        UpdateMaterial();
    }

    private void UnloadAsset()
    {
        CustomCosmeticHat?.SetDoUnload();
    }
    private void DontUnloadAsset()
    {
        CustomCosmeticHat?.SetDontUnload();
    }

    private void UpdateMaterial()
    {
        PlayerMaterial.MaskType maskType = matProperties.MaskType;
        if (Hat != null && Hat.PreviewCrewmateColor)
        {
            if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
            {
                BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
                FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedPlayerMaterial;
            }
            else
            {
                BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            }
        }
        else if (maskType == PlayerMaterial.MaskType.ComplexUI || maskType == PlayerMaterial.MaskType.ScrollingUI)
        {
            BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
            FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.MaskedMaterial;
        }
        else
        {
            BackLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
            FrontLayer.sharedMaterial = DestroyableSingleton<HatManager>.Instance.DefaultShader;
        }
        switch (maskType)
        {
            case PlayerMaterial.MaskType.SimpleUI:
                BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                break;
            case PlayerMaterial.MaskType.Exile:
                BackLayer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                break;
            default:
                BackLayer.maskInteraction = SpriteMaskInteraction.None;
                FrontLayer.maskInteraction = SpriteMaskInteraction.None;
                break;
        }
        BackLayer.material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
        FrontLayer.material.SetInt(PlayerMaterial.MaskLayer, matProperties.MaskLayer);
        if (Hat != null && Hat.Asset != null && Hat.PreviewCrewmateColor)
        {
            PlayerMaterial.SetColors(matProperties.ColorId, BackLayer);
            PlayerMaterial.SetColors(matProperties.ColorId, FrontLayer);
        }
        if (matProperties.MaskLayer <= 0)
        {
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(BackLayer, matProperties.IsLocalPlayer);
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(FrontLayer, matProperties.IsLocalPlayer);
        }
    }

    private void PopulateFromViewData()
    {
        UpdateMaterial();
        if (Hat.Asset == null) return;

        SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
        if ((bool)spriteAnimNodeSync)
        {
            spriteAnimNodeSync.NodeId = !CustomCosmeticHat.Options.front.HasFlag(HatOptionType.Bounce) ? 1 : 0;
        }
        if (CustomCosmeticHat.Options.front != HatOptionType.None)
        {
            FrontLayer.enabled = true;
            FrontLayer.sprite = CustomCosmeticHat.Front;
        }
        if (CustomCosmeticHat.Options.back != HatOptionType.None)
        {
            BackLayer.enabled = true;
            BackLayer.sprite = CustomCosmeticHat.Back;
        }
        if (HideHat())
        {
            FrontLayer.enabled = false;
            BackLayer.enabled = false;
        }
    }

    public void UpdateBounceHatZipline()
    {
        SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
        if ((bool)spriteAnimNodeSync)
        {
            spriteAnimNodeSync.NodeId = 1;
        }
    }

    public bool HideHat()
    {
        return false;
    }

    public void LateUpdate()
    {
        if (!Parent || !HasHat() || Hat.Asset == null)
        {
            return;
        }
        FlipX = Parent.flipX;
        if (FrontLayer.sprite != CustomCosmeticHat.Climb)
        {
            if (CustomCosmeticHat.Options.front != HatOptionType.None && CustomCosmeticHat.Options.front_left != HatOptionType.None)
            {
                FrontLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.FrontLeft : CustomCosmeticHat.Front);
            }
            if (CustomCosmeticHat.Options.back != HatOptionType.None && CustomCosmeticHat.Options.back_left != HatOptionType.None)
            {
                BackLayer.sprite = ((Parent.flipX || shouldFaceLeft) ? CustomCosmeticHat.BackLeft : CustomCosmeticHat.Back);
            }
        }
        else if (FrontLayer.sprite == CustomCosmeticHat.Climb || FrontLayer.sprite == CustomCosmeticHat.ClimbLeft)
        {
            SpriteAnimNodeSync spriteAnimNodeSync = spriteSyncNode ?? GetComponent<SpriteAnimNodeSync>();
            if ((bool)spriteAnimNodeSync)
            {
                spriteAnimNodeSync.NodeId = 0;
            }
        }
    }
}