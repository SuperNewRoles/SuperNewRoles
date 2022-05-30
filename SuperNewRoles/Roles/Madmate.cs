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
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            int CheckTask = 0;
            switch (p.getRole())
            {
                case RoleId.MadMate:
                    if (!RoleClass.MadMate.IsImpostorCheck) return false;
                    CheckTask = RoleClass.MadMate.ImpostorCheckTask;
                    break;
                case RoleId.MadMayor:
                    if (!RoleClass.MadMayor.IsImpostorCheck) return false;
                    CheckTask = RoleClass.MadMayor.ImpostorCheckTask;
                    break;
                case RoleId.MadJester:
                    if (!RoleClass.MadJester.IsImpostorCheck) return false;
                    CheckTask = RoleClass.MadJester.ImpostorCheckTask;
                    break;
                case RoleId.MadSeer:
                    if (!RoleClass.MadSeer.IsImpostorCheck) return false;
                    CheckTask = RoleClass.MadSeer.ImpostorCheckTask;
                    break;
                default:
                    return false;
            }
            var taskdata = TaskCount.TaskDate(p.Data).Item1;
            if (CheckTask <= taskdata)
            {
                CheckedImpostor.Add(p.PlayerId);
                return true;
            }
            return false;
        }
    }
}
