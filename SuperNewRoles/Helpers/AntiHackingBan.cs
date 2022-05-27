using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Helpers
{
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
                    __instance.SetVisor(visorId);
                }
                MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 42, SendOption.None);
                obj.Write(visorId);
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
    }
}
