using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Bait
    {
        public static void MurderPostfix(PlayerControl __instance, PlayerControl target)
        {
            if (target.isRole(CustomRPC.RoleId.Bait) && (!__instance.isRole(CustomRPC.RoleId.Minimalist) || RoleClass.Minimalist.UseReport))
            {
                new LateTask(() =>
                {
                    if (!(__instance.isRole(CustomRPC.RoleId.Minimalist) && !RoleClass.Minimalist.UseReport))
                    {
                        RoleClass.Bait.ReportedPlayer.Add(target.PlayerId);
                        __instance.CmdReportDeadBody(target.Data);
                    }
                }, RoleClass.Bait.ReportTime, "ReportBaitBody");
            }
        }
    }
}
