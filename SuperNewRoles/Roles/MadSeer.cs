using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class MadSeer
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (p.isRole(RoleId.MadSeer)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            var taskdata = TaskCount.TaskDate(p.Data).Item1;

            if (p.isRole(RoleId.MadSeer))
            {
                if (!RoleClass.MadSeer.IsImpostorCheck) return false;
                if (RoleClass.MadSeer.ImpostorCheckTask <= taskdata)
                {
                    CheckedImpostor.Add(p.PlayerId);
                    return true;
                }
            }
            return false;
        }
    }
}
