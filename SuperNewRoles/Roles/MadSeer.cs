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
            if (!RoleClass.MadSeer.IsImpostorCheck) return false;
            if (!p.isRole(RoleId.MadSeer)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("インポスターチェックタスク量:"+RoleClass.MadSeer.ImpostorCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("終了タスク量:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.MadSeer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadMayor.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true;
            }
            // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");
            return false;
        }
    }
}