using AmongUs.Data;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using UnityEngine;
using static CosmeticsLayer;

namespace SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

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
        customCosmeticsLayer?.visor1?.SetLocalPlayer(true);
        customCosmeticsLayer?.visor2?.SetLocalPlayer(true);
        new LateTask(() =>
        {
            PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
            PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Visor2, CustomCosmeticsSaver.CurrentVisor2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
        }, 0.1f, "SetHat2");
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
public static class AmongUsClient_OnPlayerJoined
{
    private static DelayTask delayTask;
    public static void Postfix(InnerNet.ClientData data)
    {
        if (PlayerControl.LocalPlayer != null)
        {
            DelayTask.UpdateOrAdd(() =>
            {
                PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
                PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Visor2, CustomCosmeticsSaver.CurrentVisor2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
            }, 0.15f, ref delayTask, "SetHat2");
        }
    }
}
[HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetFlipX))]
public static class VisorLayer_SetFlipX
{
    public static void Postfix(VisorLayer __instance, bool flipX)
    {
        var (layer1, layer2) = CustomCosmeticsLayers.GetVisorLayers(__instance);
        if (layer1 != null)
        {
            layer1.SetFlipX(flipX);
        }
        if (layer2 != null)
        {
            layer2.SetFlipX(flipX);
        }
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetDeadFlipX))]
public static class CosmeticsLayer_SetDeadFlipX
{
    public static void Postfix(CosmeticsLayer __instance, bool flipped)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.visor1?.SetFlipX(flipped);
        customCosmeticsLayer?.visor2?.SetFlipX(flipped);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetFlipXWithoutPet))]
public static class CosmeticsLayer_SetFlipXWithoutPet
{
    public static void Postfix(CosmeticsLayer __instance, bool flipped)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.visor1?.SetFlipX(flipped);
        customCosmeticsLayer?.visor2?.SetFlipX(flipped);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ClientInitialize))]
public static class PlayerControl_ClientInitialize
{
    public static void Postfix(PlayerControl __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        /*PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Hat1, DataManager.Player.Customization.Hat, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Visor1, DataManager.Player.Customization.Visor, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        PlayerControl.LocalPlayer.RpcCustomSetCosmetics(CostumeTabType.Visor2, CustomCosmeticsSaver.CurrentVisor2Id, PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
        */

        customCosmeticsLayer?.hat1?.SetLocalPlayer(false);
        customCosmeticsLayer?.hat2?.SetLocalPlayer(false);
        customCosmeticsLayer?.visor1?.SetLocalPlayer(false);
        customCosmeticsLayer?.visor2?.SetLocalPlayer(false);
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
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetVisor), [typeof(string), typeof(int)])]
public static class PoolablePlayer_SetVisor_String
{
    public static void Postfix(CosmeticsLayer __instance, string visorId, int color)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.visor1?.SetVisor(visorId, color);
        __instance.OnCosmeticSet?.Invoke(visorId, color, CosmeticKind.VISOR);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetVisor), [typeof(VisorData), typeof(int)])]
public static class PoolablePlayer_SetVisor_VisorData
{
    public static void Postfix(CosmeticsLayer __instance, VisorData visorData, int color)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.visor1?.SetVisor(new CosmeticDataWrapperVisor(visorData), color);
        __instance.OnCosmeticSet?.Invoke(visorData.ProdId, color, CosmeticKind.VISOR);
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromDataManager))]
public static class PoolablePlayer_UpdateFromDataManager
{
    public static void Postfix(PoolablePlayer __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(CustomCosmeticsSaver.CurrentHat2Id, DataManager.Player.Customization.Color);
        customCosmeticsLayer?.visor2?.SetVisor(CustomCosmeticsSaver.CurrentVisor2Id, DataManager.Player.Customization.Color);
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
        customCosmeticsLayer?.visor2?.SetVisor(pcLayer.visor2.Visor?.ProdId ?? "", pData.DefaultOutfit.ColorId);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.AnimateClimb))]
public static class CosmeticsLayer_AnimateClimb
{
    public static void Postfix(CosmeticsLayer __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat1?.SetClimbAnim();
        customCosmeticsLayer?.hat2?.SetClimbAnim();
        customCosmeticsLayer?.visor1?.SetClimbAnim(__instance.bodyType);
        customCosmeticsLayer?.visor2?.SetClimbAnim(__instance.bodyType);
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.Visible), MethodType.Setter)]
public static class CosmeticsLayer_Visible
{
    public static void Postfix(CosmeticsLayer __instance, bool value)
    {
        __instance.UpdateVisibility();
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.UpdateVisibility))]
public static class CosmeticsLayer_UpdateVisibility
{
    public static void Postfix(CosmeticsLayer __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer.hat1.Visible = __instance.visible;
        customCosmeticsLayer.hat2.Visible = __instance.visible;
        customCosmeticsLayer.visor1.Visible = __instance.visible;
        customCosmeticsLayer.visor2.Visible = __instance.visible;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetBodyCosmeticsVisible))]
public static class CosmeticsLayer_SetBodyCosmeticsVisible
{
    public static void Postfix(CosmeticsLayer __instance, bool b)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer.hat1.Visible = b;
        customCosmeticsLayer.hat2.Visible = b;
        customCosmeticsLayer.visor1.Visible = b;
        customCosmeticsLayer.visor2.Visible = b;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetHatVisorVisible))]
public static class CosmeticsLayer_SetHatVisorVisible
{
    public static void Postfix(CosmeticsLayer __instance, bool isVisible)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer.hat1.Visible = isVisible;
        customCosmeticsLayer.hat2.Visible = isVisible;
        customCosmeticsLayer.visor1.Visible = isVisible;
        customCosmeticsLayer.visor2.Visible = isVisible;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.ToggleHat))]
public static class CosmeticsLayer_ToggleHat
{
    public static void Postfix(CosmeticsLayer __instance, bool b)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer.hat1.Visible = b;
        customCosmeticsLayer.hat2.Visible = b;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetHatAndVisorIdle))]
public static class CosmeticsLayer_SetHatAndVisorIdle
{
    public static void Postfix(CosmeticsLayer __instance, int colorId)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat1?.SetIdleAnim(colorId);
        customCosmeticsLayer?.hat2?.SetIdleAnim(colorId);
        customCosmeticsLayer?.visor1?.SetIdleAnim(colorId);
        customCosmeticsLayer?.visor2?.SetIdleAnim(colorId);
    }
}
[HarmonyPatch(typeof(VisorLayer), nameof(VisorLayer.SetVisor), [typeof(string), typeof(int)])]
public static class VisorLayer_SetVisor
{
    public static void Postfix(VisorLayer __instance, string visorId, int colorId)
    {
        if (StoreMenu.InstanceExists) return;
        __instance.Image.enabled = false;
    }
}
[HarmonyPatch(typeof(HatParent), nameof(HatParent.LateUpdate))]
public static class HatParent_LateUpdate
{
    public static void Postfix(HatParent __instance)
    {
        if (StoreMenu.InstanceExists) return;
        if (__instance.FrontLayer != null)
            __instance.FrontLayer.enabled = false;
        if (__instance.BackLayer != null)
            __instance.BackLayer.enabled = false;
    }
}