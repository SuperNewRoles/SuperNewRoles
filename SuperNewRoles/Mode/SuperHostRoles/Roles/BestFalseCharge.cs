using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class BestFalseCharge
    {
        public static void WrapUp()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (AmongUsClient.Instance.AmHost && !RoleClass.Bestfalsecharge.IsOnMeeting && RoleClass.IsMeeting)
            {
                foreach (PlayerControl p in RoleClass.Bestfalsecharge.BestfalsechargePlayer)
                {
                    p.RpcInnerExiled();
                }
                RoleClass.Bestfalsecharge.IsOnMeeting = true;
            }
        }
    }
}