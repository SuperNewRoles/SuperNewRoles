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
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);//通報
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.sprite = null;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.SetText("");

                        }
                    }
                }
                else if (role == RoleId.Fox)
                {
                    if (!RoleClass.Fox.UseReport)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);//通報
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.sprite = null;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.enabled = false;
                            FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.SetText("");
                        }

                    }
                }
                else if (role == RoleId.SecretlyKiller)
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                    //FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                }
                else if (role == RoleId.DoubleKiller)
                {
                    //ボタン削除
                    if (!RoleClass.DoubleKiller.CanUseSabo)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);
                        }
                    }
                    if (!RoleClass.DoubleKiller.CanUseVent)
                    {
                        if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.active)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                        }
                    }
                    //純正キルボタン削除
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                }
                else if (role == RoleId.Smasher)
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                }
                else if (role == RoleId.Neet)//ニートのボタン削除
                {
                    if (FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.active)//使うボタンが有効の時
                    {
                        HudManager.Instance.UseButton.gameObject.SetActive(false);//使うボタンを無効化
                    }
                    if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);//通報
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.sprite = null;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.enabled = false;
                        FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.SetText("");
                    }
                }
                else if (role == RoleId.FastMaker)
                {
                    //純正キルボタン削除
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                }
            }
        }
    }
}
