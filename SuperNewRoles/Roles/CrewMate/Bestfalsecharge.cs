using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public class Bestfalsecharge
    {
        public static void WrapUp()
        {
            if (ModeHandler.IsMode(ModeId.Default) && AmongUsClient.Instance.AmHost && !RoleClass.Bestfalsecharge.IsOnMeeting)
            {
                foreach (PlayerControl p in RoleClass.Bestfalsecharge.BestfalsechargePlayer)
                {
                    p.RpcExiledUnchecked();
                    p.RpcSetFinalStatus(FinalStatus.BestFalseChargesFalseCharge);
                }
                RoleClass.Bestfalsecharge.IsOnMeeting = true;
            }

            //===========以下さつまいも===========//
            RoleClass.SatsumaAndImo.TeamNumber = RoleClass.SatsumaAndImo.TeamNumber == 1 ? 2 : 1;
        }
    }
}