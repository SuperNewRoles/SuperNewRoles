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
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!RoleClass.MadMate.IsImpostorCheck) return false;
            if (!p.isRole(RoleId.MadMate)) return false;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("インポスターチェックタスク量:"+RoleClass.MadMate.ImpostorCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("終了タスク量:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.MadMate.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadMate.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1) {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true; 
            }
           // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");
            return false;
        }
    }
}
