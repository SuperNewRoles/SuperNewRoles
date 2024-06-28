using System;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using InnerNet;

namespace SuperNewRoles.Helpers;

class AntiHackingBan
{
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