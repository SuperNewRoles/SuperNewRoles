using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode.SuperHostRoles;
using HarmonyLib;

//クルー1
//マッド2
//ジャカフレ3
namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class SatsumaAndImo
    {
        public static void Prefix(MeetingHud __instance)
        {
            if (RoleClass.SatsumaAndImo.TeamNumber == 1)//クルーなら
            {
                SuperNewRolesPlugin.Logger.LogDebug("まｄｄ");
                RoleClass.SatsumaAndImo.TeamNumber = 2;//マッドに
                RoleClass.SatsumaAndImo.color = RoleClass.ImpostorRed;
            }
            else if (RoleClass.SatsumaAndImo.TeamNumber == 2)//マッドなら
            {
                SuperNewRolesPlugin.Logger.LogDebug("かｓｆ");
                RoleClass.SatsumaAndImo.TeamNumber = 2;//ジャカフレに
                RoleClass.SatsumaAndImo.color = RoleClass.JackalFriends.color;
            }
            //SatsumaRoleSelect.SetRole();//役職割り当て
        }
    }
}
