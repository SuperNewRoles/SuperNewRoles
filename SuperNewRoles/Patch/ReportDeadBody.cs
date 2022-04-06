using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Patch
{
    class ReportDeadBody
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        class ReportDeadBodyPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (__instance.isRole(RoleId.Amnesiac))
                    {
                        if (!target.Disconnected)
                        {
                            __instance.RPCSetRoleUnchecked(target.Role.Role);
                            if (target.Role.IsSimpleRole)
                            {
                                __instance.setRoleRPC(target.Object.getRole());
                            }
                        }
                    }
                }
                if (!MapOptions.MapOption.UseDeadBodyReport && target != null) return false;
                if (!MapOptions.MapOption.UseMeetingButton && target == null) return false;
                if (ModeHandler.isMode(ModeId.HideAndSeek)) return false;
                if (ModeHandler.isMode(ModeId.BattleRoyal)) return false;
                if (ModeHandler.isMode(ModeId.SuperHostRoles)) return Mode.SuperHostRoles.ReportDeadBody.ReportDeadBodyPatch(__instance,target);
                if (ModeHandler.isMode(ModeId.Zombie)) return false;
                if (ModeHandler.isMode(ModeId.Detective) && target == null && Mode.Detective.main.IsNotDetectiveMeetingButton && __instance.PlayerId != Mode.Detective.main.DetectivePlayer.PlayerId) return false;
                return true;
            }
        }
    }
}
