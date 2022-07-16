using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode.SuperHostRoles;
using HarmonyLib;

namespace SuperNewRoles.Roles
{
    public class SatsumaAndImo
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
        static void Prefix(MeetingHud __instance)
        {
            if (RoleClass.SatsumaAndImo.TeamNumber == 0)//impなら
            {
                RoleClass.SatsumaAndImo.TeamNumber = 1;//クルーに
            }
            else//クルーなら
            {
                RoleClass.SatsumaAndImo.TeamNumber = 0;//impに
            }
        }
        static void SetRole()
        {
            foreach (PlayerControl p in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
            if (RoleClass.SatsumaAndImo.TeamNumber == 0)//impなら
            {
                p.setRoleRPC(RoleId.MadMate);//madに
            }
            else//クルーなら
            {
                p.RpcSetRoleDesync(RoleTypes.Crewmate);
                p.setRoleRPC(RoleId.DefaultRole);//クルーに
            }
        }
    }

}
