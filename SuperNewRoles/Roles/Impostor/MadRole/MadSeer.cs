using System.Collections.Generic;

using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    class MadSeer
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!RoleClass.MadSeer.IsImpostorCheck) return false;
            if (!p.IsRole(RoleId.MadSeer)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            SuperNewRolesPlugin.Logger.LogInfo("[MadSeer]Is Validity?:" + (RoleClass.MadSeer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadSeer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[MadSeer]Returned valid.");
                return true;
            }
            return false;
        }
    }
}