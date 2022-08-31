using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;

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
                if (target != null && RoleClass.BlockPlayers.Contains(target.PlayerId)) return false;
                if (ModeHandler.IsMode(ModeId.Default))
                {
                    if (__instance.IsRole(RoleId.Amnesiac))
                    {
                        if (!target.Disconnected)
                        {
                            __instance.RPCSetRoleUnchecked(target.Role.Role);
                            if (target.Role.IsSimpleRole)
                            {
                                __instance.SetRoleRPC(target.Object.GetRole());
                            }
                        }
                    }
                }
                return (RoleClass.Assassin.TriggerPlayer != null)
                || (!MapOptions.MapOption.UseDeadBodyReport && target != null)
                || (!MapOptions.MapOption.UseMeetingButton && target == null)
                || ModeHandler.IsMode(ModeId.HideAndSeek)
                || ModeHandler.IsMode(ModeId.BattleRoyal)
                || ModeHandler.IsMode(ModeId.CopsRobbers)
                    ? false
                    : ModeHandler.IsMode(ModeId.SuperHostRoles)
                    ? Mode.SuperHostRoles.ReportDeadBody.ReportDeadBodyPatch(__instance, target)
                    : !ModeHandler.IsMode(ModeId.Zombie)
&& (!ModeHandler.IsMode(ModeId.Detective) || target != null || !Mode.Detective.Main.IsNotDetectiveMeetingButton || __instance.PlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId);
            }
        }
    }
}