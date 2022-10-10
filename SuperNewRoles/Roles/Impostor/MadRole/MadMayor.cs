using System.Collections.Generic;

using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    class MadMayor
    {
        public static List<byte> CheckedImpostor;
        public static bool CheckImpostor(PlayerControl p)
        {
            if (!RoleClass.MadMayor.IsImpostorCheck) return false;
            if (!p.IsRole(RoleId.MadMayor)) return false;
            if (CheckedImpostor.Contains(p.PlayerId)) return true;
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.MadMayor.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadMayor.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true;
            }
            return false;
        }
    }
}