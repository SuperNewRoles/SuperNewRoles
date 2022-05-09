using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class Traitor
    {
        public static List<byte> CheckedFox;
        public static bool CheckFox(PlayerControl p)
        {
            if (!RoleClass.Fox.IsFoxCheck) return false;
            if (!p.isRole(RoleId.Traitor)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("インポスターチェックタスク量:"+RoleClass.MadJester.ImpostorCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("終了タスク量:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.Traitor.FoxCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.Traitor.FoxCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true;
            }
            // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");
            return false;
        }
    }
}
