using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class MadMayor
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (p.isRole(RoleId.MadMayor)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            var taskdata = TaskCount.TaskDate(p.Data).Item1;

            if (p.isRole(RoleId.MadMayor))
            {
                if (!RoleClass.MadMayor.IsImpostorCheck) return false;
                if (RoleClass.MadMayor.ImpostorCheckTask <= taskdata)
                {
                    CheckedImpostor.Add(p.PlayerId);
                    return true;
                }
            }
            return false;
        }
    }
}