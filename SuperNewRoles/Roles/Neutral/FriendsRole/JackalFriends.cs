using System.Collections.Generic;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles;

class JackalFriends
{
    public static HashSet<byte> CheckedJackal;
    public static bool CheckJackal(PlayerControl p)
    {
        if (CheckedJackal.Contains(p.PlayerId)) return true;
        RoleId role = p.GetRole();
        int CheckTask = 0;
        switch (role)
        {
            case RoleId.JackalFriends:
                if (!RoleClass.JackalFriends.IsJackalCheck) return false;
                CheckTask = RoleClass.JackalFriends.JackalCheckTask;
                break;
            case RoleId.SeerFriends:
                if (!RoleClass.SeerFriends.IsJackalCheck) return false;
                CheckTask = RoleClass.SeerFriends.JackalCheckTask;
                break;
            case RoleId.MayorFriends:
                if (!RoleClass.MayorFriends.IsJackalCheck) return false;
                CheckTask = RoleClass.MayorFriends.JackalCheckTask;
                break;
            default:
                return false;
        }
        var taskdata = TaskCount.TaskDate(p.Data).Item1;
        if (CheckTask <= taskdata)
        {
            CheckedJackal.Add(p.PlayerId);
            return true;
        }
        return false;
    }
}