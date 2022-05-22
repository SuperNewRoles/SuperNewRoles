using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles

{
    class JackalFriends
    {
        public static List<byte> CheckedJackal;
        public static bool CheckJackal(PlayerControl p)
        {
            if (!p.isRole(RoleId.JackalFriends)) return false;
            if (CheckedJackal.Contains(p.PlayerId)) return true;
            var taskdata = TaskCount.TaskDate(p.Data).Item1;
            if (p.isRole(RoleId.JackalFriends))
            {
                if (!RoleClass.JackalFriends.IsJackalCheck) return false;
                if (RoleClass.JackalFriends.JackalCheckTask <= taskdata)
                {
                    CheckedJackal.Add(p.PlayerId);
                    return true;
                }
            }
            return false;
        }
    }
}