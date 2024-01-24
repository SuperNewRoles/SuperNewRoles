using System.Collections.Generic;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles;

class JackalFriends
{
    public static HashSet<byte> CheckedJackal;

    /// <summary>
    /// JFSKされたジャッカルフレンズを保存する
    /// </summary>
    /// <value>
    /// true : 元がタスクをできる役職 / false : 元がタスクができない役職
    /// </value>
    public static PlayerData<bool> ChangeJackalFriendsPlayer;

    public static void ClearAndReload()
    {
        CheckedJackal = new();
        ChangeJackalFriendsPlayer = new(defaultvalue: true);
    }

    public static bool CheckJackal(PlayerControl p)
    {
        if (CheckedJackal.Contains(p.PlayerId)) return true;
        RoleId role = p.GetRole();
        int CheckTask = 0;
        switch (role)
        {
            case RoleId.JackalFriends:
                if (!RoleClass.JackalFriends.IsJackalCheck) return false;
                bool haveTask = !(Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) && !ChangeJackalFriendsPlayer[p.PlayerId]);
                CheckTask = haveTask ? RoleClass.JackalFriends.JackalCheckTask : 0;
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