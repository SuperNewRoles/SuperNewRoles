using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles
{
    public class Bestfalsecharge
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        public class MeetingEnd
        {
            static void Prefix(MeetingHud __instance)
            {
                if (AmongUsClient.Instance.AmHost && !RoleClass.Bestfalsecharge.IsOnMeeting)
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
}