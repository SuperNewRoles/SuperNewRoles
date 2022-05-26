using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class SeerFriends
    {
        public static List<byte> CheckedJackal;
        public static bool CheckJackal(PlayerControl p)
        {
            if (!RoleClass.SeerFriends.IsJackalCheck) return false;
            if (!p.isRole(RoleId.SeerFriends)) return false;
            if (CheckedJackal.Contains(p.PlayerId)) return true;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("ジャッカルチェックタスク量:"+RoleClass.SeerFriends.JackalCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("終了タスク量:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.SeerFriends.JackalCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.SeerFriends.JackalCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true;
            }
            // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");
            return false;
        }
    }
}
