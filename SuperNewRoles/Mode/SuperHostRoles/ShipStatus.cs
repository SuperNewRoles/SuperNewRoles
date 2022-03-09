using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class ShipStatusPatch
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
        class RepairSystemPatch
        {
            public static bool Prefix(ShipStatus __instance,
                [HarmonyArgument(0)] SystemTypes systemType,
                [HarmonyArgument(1)] PlayerControl player,
                [HarmonyArgument(2)] byte amount)
            {
                
                if (!AmongUsClient.Instance.AmHost) return true;
                

                if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4 && player.isRole(CustomRPC.RoleId.MadMate))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
