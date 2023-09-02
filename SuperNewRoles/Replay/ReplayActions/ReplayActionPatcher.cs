using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace SuperNewRoles.Replay.ReplayActions
{
    public static class ReplayActionPatcher
    {
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class ChatControllerAddChatPatch
        {
            public static void Postfix(PlayerControl sourcePlayer, string chatText)
            {
                ReplayActionAddChat.Create(sourcePlayer.PlayerId, chatText);
            }
        }
        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        public static class GameDataHandleDisconnectPatch
        {
            public static void Postfix(PlayerControl player, DisconnectReasons reason)
            {
                ReplayActionDisconnect.Create(player.PlayerId, reason);
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics))]
        public static class PlayerPhysicsPatch
        {
            [HarmonyPatch(nameof(PlayerPhysics.CoEnterVent))]
            [HarmonyPostfix]
            public static void CoEnterVent(PlayerPhysics __instance, int id)
            {
                ReplayActionVent.Create(__instance.myPlayer.PlayerId, id, true);
            }
            [HarmonyPatch(nameof(PlayerPhysics.CoExitVent))]
            [HarmonyPostfix]
            public static void CoExitVent(PlayerPhysics __instance, int id)
            {
                ReplayActionVent.Create(__instance.myPlayer.PlayerId, id, false);
            }
            [HarmonyPatch(nameof(PlayerPhysics.ClimbLadder))]
            [HarmonyPostfix]
            public static void ClimbLadder(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
            {
                ReplayActionClimbLadder.Create(__instance.myPlayer.PlayerId, source.Id, climbLadderSid);
            }
        }
        [HarmonyPatch(typeof(PlayerControl))]
        public static class PlayerControlPatch
        {
            [HarmonyPatch(nameof(PlayerControl.SetHat))]
            [HarmonyPostfix]
            public static void SetHatPatch(PlayerControl __instance, string hatId, int colorId)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Hat, hatId, colorId);
            }
            [HarmonyPatch(nameof(PlayerControl.SetColor))]
            [HarmonyPostfix]
            public static void SetColorPatch(PlayerControl __instance, int bodyColor)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Color, "", bodyColor);
            }
            [HarmonyPatch(nameof(PlayerControl.SetPet), new Type[] { typeof(string), typeof(int) })]
            [HarmonyPostfix]
            public static void SetPetPatch(PlayerControl __instance, string petId, int colorId)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Pet, petId, colorId);
            }
            [HarmonyPatch(nameof(PlayerControl.SetPet), new Type[] { typeof(string) })]
            [HarmonyPostfix]
            public static void SetPetSimplePatch(PlayerControl __instance, string petId)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Pet, petId, __instance.Data.DefaultOutfit.ColorId);
            }
            [HarmonyPatch(nameof(PlayerControl.SetVisor))]
            [HarmonyPostfix]
            public static void SetVisorPatch(PlayerControl __instance, string visorId, int colorId)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Visor, visorId, colorId);
            }
            [HarmonyPatch(nameof(PlayerControl.SetNamePlate))]
            [HarmonyPostfix]
            public static void SetNamePlatePatch(PlayerControl __instance, string namePlateId)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.NamePlate, namePlateId);
            }
            [HarmonyPatch(nameof(PlayerControl.SetSkin))]
            [HarmonyPostfix]
            public static void SetSkinPatch(PlayerControl __instance, string skinId, int color)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Skin, skinId, color);
            }
            [HarmonyPatch(nameof(PlayerControl.SetName))]
            [HarmonyPostfix]
            public static void SetNamePatch(PlayerControl __instance, string name, bool dontCensor)
            {
                ReplayActionSetCosmetics.Create(__instance.PlayerId, ReplayCosmeticsType.Name, name, dontCensor: dontCensor);
            }
            [HarmonyPatch(nameof(PlayerControl.CompleteTask))]
            [HarmonyPostfix]
            public static void SetNamePatch(PlayerControl __instance, uint idx)
            {
                ReplayActionCompleteTask.Create(__instance.PlayerId, idx);
            }
        }
    }
}