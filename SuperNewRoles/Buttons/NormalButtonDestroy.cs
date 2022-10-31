using System.Collections.Generic;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Buttons
{
    public class NormalButtonDestroy
    {
        private enum NormalButton
        {
            KillButton,
            ReportButton,
            UseButton
        }
        private static readonly Dictionary<RoleId, (NormalButton, bool)> SetActiveDictionary = new() {
            { RoleId.FastMaker, (NormalButton.KillButton, !RoleClass.FastMaker.IsCreatedMadmate) },
            { RoleId.SecretlyKiller, (NormalButton.KillButton, true) },
            { RoleId.DoubleKiller, (NormalButton.KillButton, true) },
            { RoleId.Smasher, (NormalButton.KillButton, true) },
            { RoleId.Conjurer, (NormalButton.KillButton, true) },
            { RoleId.Tasker, (NormalButton.KillButton, !CustomOptionHolder.TaskerCanKill.GetBool()) },

            { RoleId.Minimalist, (NormalButton.ReportButton, !RoleClass.Minimalist.UseReport) },
            { RoleId.Fox, (NormalButton.ReportButton, !RoleClass.Fox.UseReport) },
            { RoleId.Neet, (NormalButton.ReportButton, true) },

            //{ RoleId.Neet, (NormalButton.UseButton, true) }, Key重複のため辞書に入れず直接パッチ
        };
        public static void SetActiveState()
        {
            // ニートの使用ボタン
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Neet) && FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.active)
                FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(false);// 使用ボタンを無効化

            if (!SetActiveDictionary.ContainsKey(PlayerControl.LocalPlayer.GetRole())) return;
            if (!SetActiveDictionary[PlayerControl.LocalPlayer.GetRole()].Item2) return;
            switch (SetActiveDictionary[PlayerControl.LocalPlayer.GetRole()].Item1)
            {
                case NormalButton.KillButton: // キルボタン
                    if (FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.active)
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                    break;
                case NormalButton.ReportButton: // 通報ボタン
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
                    break;
                case NormalButton.UseButton: // 使用ボタン
                    if (FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.active)
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(false);
                    break;
            }
        }
    }
}