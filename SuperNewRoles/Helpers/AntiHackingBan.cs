using System;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace SuperNewRoles.Helpers;

class AntiHackingBan
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetSkin))]
    class Setcolorskin
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string skinId)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                int valueOrDefault = (__instance.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
                __instance.SetSkin(skinId, valueOrDefault);
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 40, SendOption.None);
            obj.Write(skinId);
            obj.EndMessage();
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetHat))]
    class Sethat
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string hatId)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                int valueOrDefault = (__instance.Data?.DefaultOutfit?.ColorId).GetValueOrDefault();
                __instance.SetHat(hatId, valueOrDefault);
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 39, SendOption.None);
            obj.Write(hatId);
            obj.EndMessage();
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetVisor))]
    class SetVisor
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string visorId)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                __instance.SetVisor(visorId, __instance.Data.DefaultOutfit.ColorId);
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 42, SendOption.None);
            obj.Write(visorId);
            obj.Write(__instance.Data.DefaultOutfit.ColorId);
            obj.EndMessage();
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetPet))]
    class SetPet
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] ref string petId)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                __instance.SetPet(petId);
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 41, SendOption.None);
            obj.Write(petId);
            obj.EndMessage();
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcShapeshift))]
    class RpcShapeShiftPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] bool shouldAnimate)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                __instance.Shapeshift(target, shouldAnimate);
            }
            MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 46, SendOption.None);
            val.WriteNetObject(target);
            val.Write(shouldAnimate);
            AmongUsClient.Instance.FinishRpcImmediately(val);
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    class RpcMurderPlayer
    {
        public static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                writer.Write(__instance.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.RPCMurderPlayer(__instance.PlayerId, target.PlayerId, byte.MaxValue);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, ref bool __result, [HarmonyArgument(0)] string chatText)
        {
            //chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            }
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                FastDestroyableSingleton<UnityTelemetry>.Instance.SendWho();
            }
            MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 13, SendOption.None);
            obj.Write(chatText);
            obj.EndMessage();
            __result = true;
            return false;
        }
    }
}