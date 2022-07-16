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
            if (RoleClass.SatsumaAndImo.TeamNumber == 1) { RoleClass.SatsumaAndImo.color = Palette.White; }
            else if (RoleClass.SatsumaAndImo.TeamNumber == 2) { RoleClass.SatsumaAndImo.color = RoleClass.ImpostorRed; }
            else if (RoleClass.SatsumaAndImo.TeamNumber == 3) { RoleClass.SatsumaAndImo.color = RoleClass.JackalFriends.color; }
            if (RoleClass.SatsumaAndImo.TeamNumber == 1)//クルーなら
            {
                SuperNewRolesPlugin.Logger.LogDebug("まｄｄ");
                RoleClass.SatsumaAndImo.TeamNumber = 2;//マッドに
                RoleClass.SatsumaAndImo.color = RoleClass.ImpostorRed;
            }
            else if (RoleClass.SatsumaAndImo.TeamNumber == 2)//マッドなら
            {
                SuperNewRolesPlugin.Logger.LogDebug("かｓｆ");
                RoleClass.SatsumaAndImo.TeamNumber = 3;//ジャカフレに
                RoleClass.SatsumaAndImo.color = RoleClass.JackalFriends.color;
            }
            else//ジャカフレなら
            {
                SuperNewRolesPlugin.Logger.LogDebug("kuruu");
                RoleClass.SatsumaAndImo.TeamNumber = 1;//クルーに
                RoleClass.SatsumaAndImo.color = Palette.White;
            }
            //SatsumaRoleSelect.SetRole();//役職割り当て
        }
    }/*
    public static class SatsumaRoleSelect
    {
        public static void SetRole()
        {
            foreach (PlayerControl p in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
            {
                if (RoleClass.SatsumaAndImo.TeamNumber == 1)//クルーなら
                {
                    SuperNewRolesPlugin.Logger.LogDebug("くるう 1");
                    p.RpcSetRoleDesync(RoleTypes.Crewmate);//
                    SuperNewRolesPlugin.Logger.LogDebug("くるう 2");
                    p.setRoleRPC(RoleId.DefaultRole);//クルーに
                    SuperNewRolesPlugin.Logger.LogDebug("くるう 3");
                }
                else if (RoleClass.SatsumaAndImo.TeamNumber == 2)
                {
                    SuperNewRolesPlugin.Logger.LogDebug("まっど1");
                    p.setRoleRPC(RoleId.MadMate);//madに
                    SuperNewRolesPlugin.Logger.LogDebug("まっど2");
                }
                else//ジャカフレなら
                {
                    SuperNewRolesPlugin.Logger.LogDebug("かすふれ1");
                    p.setRoleRPC(RoleId.JackalFriends);//madに
                    SuperNewRolesPlugin.Logger.LogDebug("かすふれ2");
                }
            }
        }
    }*/
}
