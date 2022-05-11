using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class Madmate
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!p.isRole(RoleId.MadMayor) && !p.isRole(RoleId.MadMate) && !p.isRole(RoleId.MadJester) && !p.isRole(RoleId.MadSeer)) return false;
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
            else if (p.isRole(RoleId.MadMate))
            {
                if (!RoleClass.MadMate.IsImpostorCheck) return false;
                if (RoleClass.MadMate.ImpostorCheckTask <= taskdata)
                {
                    CheckedImpostor.Add(p.PlayerId);
                    return true;
                }
            }
            else if (p.isRole(RoleId.MadJester))
            {
                if (!RoleClass.MadJester.IsImpostorCheck) return false;
                if (RoleClass.MadJester.ImpostorCheckTask <= taskdata)
                {
                    CheckedImpostor.Add(p.PlayerId);
                    return true;
                }
            }
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
