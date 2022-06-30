using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    public class Minimalist
    {
        public class MurderPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
        public class FixedUpdate
        {
            public static void Postfix(RoleId role)
            {
                if (role == RoleId.Minimalist)
                {
                    if (!RoleClass.Minimalist.UseVent)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                        }
                    }
                    if (!RoleClass.Minimalist.UseSabo)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);
                        }
                    }
                    if (!RoleClass.Minimalist.UseReport)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                        }
                    }
                }
                if (role == RoleId.Fox)
                {
                    if (!RoleClass.Fox.UseReport)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                        }
                    }
                }
                if (role == RoleId.Conjurer)
                {
                    if (FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.active)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
