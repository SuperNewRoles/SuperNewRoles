using AmongUs.Data;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Modifiers;
using TMPro;
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
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClient_CoStartGame
{
    public static void Postfix()
    {
        PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Hat2, CustomCosmeticsSaver.CurrentHat2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
        PlayerControlRpcExtensions.RpcCustomSetCosmetics(PlayerControl.LocalPlayer.PlayerId, CostumeTabType.Visor2, CustomCosmeticsSaver.CurrentVisor2Id, (PlayerControl.LocalPlayer.Data?.DefaultOutfit?.ColorId).GetValueOrDefault());
        if (ModHelpers.IsHnS())
        {
            foreach (ExPlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.Data.Role.IsImpostor) continue;
                CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(pc.cosmetics);
                customCosmeticsLayer.visor1.gameObject.SetActive(false);
                customCosmeticsLayer.visor2.gameObject.SetActive(false);
            }
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
        Logger.Info("PoolablePlayer_SetVisor_String.Postfix: " + visorId);
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
[HarmonyPatch(typeof(VitalsPanel), nameof(VitalsPanel.SetPlayer))]
public static class VitalsPanel_SetPlayer
{
    public static void Postfix(VitalsPanel __instance)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.PlayerIcon.cosmetics);
        customCosmeticsLayer.hat1.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat1.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat2.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat2.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.visor1.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.visor2.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromEitherPlayerDataOrCache))]
public static class PoolablePlayer_UpdateFromEitherPlayerDataOrCache
{
    public static void Postfix(PoolablePlayer __instance, NetworkedPlayerInfo pData, PlayerMaterial.MaskType maskType)
    {
        if (pData.Object == null) return;
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        CustomCosmeticsLayer pcLayer = CustomCosmeticsLayers.ExistsOrInitialize(pData.Object.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(pcLayer.hat2.DefaultHat?.ProdId ?? "", pData.DefaultOutfit.ColorId);
        customCosmeticsLayer?.visor2?.SetVisor(pcLayer.visor2.DefaultVisor?.ProdId ?? "", pData.DefaultOutfit.ColorId);
        customCosmeticsLayer?.hat2?.SetMaskType(maskType);
        customCosmeticsLayer?.visor2?.SetMaskType(maskType);
    }
}
[HarmonyPatch(typeof(PoolablePlayer), nameof(PoolablePlayer.UpdateFromPlayerData))]
public static class PoolablePlayer_UpdateFromPlayerData
{
    public static void Postfix(PoolablePlayer __instance, NetworkedPlayerInfo pData, PlayerMaterial.MaskType maskType)
    {
        if (pData.Object == null) return;
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        CustomCosmeticsLayer pcLayer = CustomCosmeticsLayers.ExistsOrInitialize(pData.Object.cosmetics);
        customCosmeticsLayer?.hat2?.SetHat(pcLayer.hat2.DefaultHat?.ProdId ?? "", pData.DefaultOutfit.ColorId);
        customCosmeticsLayer?.visor2?.SetVisor(pcLayer.visor2.DefaultVisor?.ProdId ?? "", pData.DefaultOutfit.ColorId);
        customCosmeticsLayer?.hat2?.SetMaskType(maskType);
        customCosmeticsLayer?.visor2?.SetMaskType(maskType);
    }
}
[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetCosmetics))]
public static class ChatBubble_SetCosmetics
{
    public static void Postfix(ChatBubble __instance)
    {
        Logger.Info("ChatBubble_SetCosmetics");
        Logger.Info($"ChatBubble_SetCosmetics: {__instance.Player.name}");
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.Player.cosmetics);
        customCosmeticsLayer.hat1.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat1.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat2.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.hat2.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.visor1.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        customCosmeticsLayer.visor2.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }
}
[HarmonyPatch(typeof(CosmeticsLayer), nameof(CosmeticsLayer.SetMaskType))]
public static class CosmeticsLayer_SetMaskType
{
    public static void Postfix(CosmeticsLayer __instance, PlayerMaterial.MaskType type)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance);
        customCosmeticsLayer?.hat2?.SetMaskType(type);
        customCosmeticsLayer?.visor2?.SetMaskType(type);
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
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
public static class MeetingHud_PopulateButtons
{
    public static void Postfix(MeetingHud __instance)
    {
        Logger.Info("MeetingHud_PopulateButtons.Postfix");
        new LateTask(() =>
        {
            foreach (var playerState in __instance.playerStates)
            {
                ExPlayerControl player = ExPlayerControl.ById(playerState.TargetPlayerId);
                CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
                CustomCosmeticsLayer pcLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
                Logger.Info($"{player.Data.PlayerId} : {pcLayer.hat2.DefaultHat?.ProdId} {pcLayer.visor2.DefaultVisor?.ProdId}");
                customCosmeticsLayer?.hat2?.SetHat(pcLayer.hat2.DefaultHat?.ProdId ?? "", player.Data.DefaultOutfit.ColorId);
                customCosmeticsLayer?.visor2?.SetVisor(pcLayer.visor2.DefaultVisor?.ProdId ?? "", player.Data.DefaultOutfit.ColorId);
                // fix mask
                customCosmeticsLayer.visor1.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                customCosmeticsLayer.visor2.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }, 3f, "MeetingHud_PopulateButtons");
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    public static void Postfix(MeetingHud __instance)
    {
        foreach (var playerState in __instance.playerStates)
        {
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(playerState.PlayerIcon.cosmetics);
            customCosmeticsLayer.visor1.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.visor2.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClient_CoStartGame_Patch
{
    public static void Postfix()
    {
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);

            foreach (var textMeshPro in player.cosmetics.nameTextContainer.GetComponentsInChildren<TextMeshPro>())
            {
                textMeshPro.sortingOrder = 500;
            }
        }
    }
}
[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
public static class ExileController_Begin
{
    public static void Postfix(ExileController __instance)
    {
        if (__instance.initData.networkedPlayer == null) return;
        if (__instance.initData.networkedPlayer.Object == null) return;
        __instance.Player.UpdateFromEitherPlayerDataOrCache(__instance.initData.networkedPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false, (Il2CppSystem.Action)(() =>
        {
            SkinViewData skinViewData = null;
            skinViewData = (!(GameManager.Instance != null)) ? __instance.Player.GetSkinView() : ShipStatus.Instance.CosmeticsCache.GetSkin(__instance.initData.outfit.SkinId);
            if (GameManager.Instance != null && !DestroyableSingleton<HatManager>.Instance.CheckLongModeValidCosmetic(__instance.initData.outfit.SkinId, __instance.Player.GetIgnoreLongMode()))
            {
                skinViewData = ShipStatus.Instance.CosmeticsCache.GetSkin("skin_None");
            }
            if (__instance.useIdleAnim)
            {
                __instance.Player.FixSkinSprite(skinViewData.IdleFrame);
            }
            else
            {
                __instance.Player.FixSkinSprite(skinViewData.EjectFrame);
            }
        }));
        if (!__instance.useIdleAnim)
        {
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.Player.cosmetics);
            customCosmeticsLayer.hat1.transform.localPosition = __instance.exileHatPosition;
            customCosmeticsLayer.hat2.transform.localPosition = __instance.exileHatPosition;
            customCosmeticsLayer.visor1.transform.localPosition = __instance.exileVisorPosition;
            customCosmeticsLayer.visor2.transform.localPosition = __instance.exileVisorPosition;
            switch ((MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId)
            {
                case MapNames.Airship:
                    customCosmeticsLayer.ModdedCosmetics.transform.localPosition = new(0.6f, 0.4f, -0.0001f);
                    customCosmeticsLayer.ModdedCosmetics.transform.Rotate(new(0, 0, 50));
                    break;
                case MapNames.Polus:
                    customCosmeticsLayer.ModdedCosmetics.transform.localPosition = new(0.4f, 0.2f, -0.0001f);
                    customCosmeticsLayer.ModdedCosmetics.transform.Rotate(new(0, 0, 10));
                    break;
            }
        }
    }
}
[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class MeetingIntroAnimation_Init
{
    public static void Postfix(MeetingIntroAnimation __instance)
    {
        new LateTask(() =>
        {
            PlayerVoteArea area = __instance.GetComponentInChildren<PlayerVoteArea>();
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(area.PlayerIcon.cosmetics);
            customCosmeticsLayer.visor1.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.visor2.Image.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.hat1.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.hat1.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.hat2.FrontLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            customCosmeticsLayer.hat2.BackLayer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }, 0.1f, "MeetingIntroAnimation_Init");
    }
}
[HarmonyCoroutinePatch(typeof(MushroomMixupPlayerAnimation), nameof(MushroomMixupPlayerAnimation.CoPlay))]
public static class MushroomMixupPlayerAnimation_CoPlay
{
    public static void Postfix(object __instance)
    {
        MushroomMixupPlayerAnimation instance = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<MushroomMixupPlayerAnimation>(__instance);
        if (instance == null) return;
        instance.sprite.sortingOrder = 1000;
        if (((ExPlayerControl)instance.player).TryGetAbility<JumboAbility>(out var jumboAbility))
            instance.transform.localScale = Vector3.one * ((jumboAbility._currentSize + 1f) / 2.7f);
    }
}
[HarmonyPatch(typeof(RoleEffectAnimation), nameof(RoleEffectAnimation.Play))]
public static class RoleEffectAnimation_Play
{
    public static void Postfix(RoleEffectAnimation __instance)
    {
        __instance.Renderer.sortingOrder = 1000;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControl_Shapeshift
{
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(__instance.cosmetics);
        CustomCosmeticsLayer pcLayer = CustomCosmeticsLayers.ExistsOrInitialize(targetPlayer.cosmetics);
        if (__instance == targetPlayer)
        {
            customCosmeticsLayer?.hat1?.FinishShapeshift(__instance.Data.DefaultOutfit.ColorId);
            customCosmeticsLayer?.hat2?.FinishShapeshift(__instance.Data.DefaultOutfit.ColorId);
            customCosmeticsLayer?.visor1?.FinishShapeshift(__instance.Data.DefaultOutfit.ColorId);
            customCosmeticsLayer?.visor2?.FinishShapeshift(__instance.Data.DefaultOutfit.ColorId);
            return;
        }
        customCosmeticsLayer?.hat1?.SetShapeshiftHat(pcLayer.hat1.DefaultHat?.ProdId ?? "", targetPlayer.Data.DefaultOutfit.ColorId);
        customCosmeticsLayer?.visor1?.SetShapeshiftVisor(pcLayer.visor1.DefaultVisor?.ProdId ?? "", targetPlayer.Data.DefaultOutfit.ColorId);
        customCosmeticsLayer?.hat2?.SetShapeshiftHat(pcLayer.hat2.DefaultHat?.ProdId ?? "", targetPlayer.Data.DefaultOutfit.ColorId);
        customCosmeticsLayer?.visor2?.SetShapeshiftVisor(pcLayer.visor2.DefaultVisor?.ProdId ?? "", targetPlayer.Data.DefaultOutfit.ColorId);
    }
}
[HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.MushroomMixUp))]
public static class MushroomMixupSabotageSystem_MushroomMixUp
{
    public static void Postfix(MushroomMixupSabotageSystem __instance)
    {
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
            customCosmeticsLayer.hat2.gameObject.SetActive(false);
            customCosmeticsLayer.visor2.gameObject.SetActive(false);
        }
    }
}
[HarmonyPatch(typeof(MushroomMixupSabotageSystem), nameof(MushroomMixupSabotageSystem.Deteriorate))]
public static class MushroomMixupSabotageSystem_Deteriorate
{
    public static void Prefix(MushroomMixupSabotageSystem __instance)
    {
        if (!(__instance.currentSecondsUntilHeal <= 0f))
        {
            return;
        }
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
            customCosmeticsLayer.hat2.gameObject.SetActive(true);
            customCosmeticsLayer.visor2.gameObject.SetActive(true);
        }
    }
}