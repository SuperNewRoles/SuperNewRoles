using System.Collections.Generic;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Buttons;

public class NormalButtonDestroy
{
    private enum NormalButton
    {
        KillButton,
        ReportButton,
        UseButton
    }
    private static readonly Dictionary<RoleId, (NormalButton, bool)> SetActiveDictionary = new() {
            //{ RoleId.FastMaker, (NormalButton.KillButton, !RoleClass.FastMaker.IsCreatedMadmate) }, 試合中に変動する必要がある為辞書に入れず直接パッチ
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
        var hm = FastDestroyableSingleton<HudManager>.Instance;

        // ファストメーカーのキルボタン
        if (PlayerControl.LocalPlayer.IsRole(RoleId.FastMaker) && !RoleClass.FastMaker.IsCreatedMadmate && hm.KillButton.gameObject.active)
            hm.UseButton.gameObject.SetActive(false);// 使用ボタンを無効化

        // ニートの使用ボタン
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Neet) && hm.UseButton.gameObject.active)
            hm.UseButton.gameObject.SetActive(false);// 使用ボタンを無効化

        if (!SetActiveDictionary.ContainsKey(PlayerControl.LocalPlayer.GetRole())) return;
        if (!SetActiveDictionary[PlayerControl.LocalPlayer.GetRole()].Item2) return;
        switch (SetActiveDictionary[PlayerControl.LocalPlayer.GetRole()].Item1)
        {
            case NormalButton.KillButton: // キルボタン
                if (hm.KillButton.gameObject.active)
                    hm.KillButton.gameObject.SetActive(false);
                break;
            case NormalButton.ReportButton: // 通報ボタン
                if (hm.ReportButton.gameObject.active)
                {
                    hm.ReportButton.SetActive(false);//通報
                    hm.ReportButton.gameObject.SetActiveRecursively(false);
                    hm.ReportButton.graphic.enabled = false;
                    hm.ReportButton.enabled = false;
                    hm.ReportButton.graphic.sprite = null;
                    hm.ReportButton.buttonLabelText.enabled = false;
                    hm.ReportButton.buttonLabelText.SetText("");
                }
                break;
            case NormalButton.UseButton: // 使用ボタン
                if (hm.UseButton.gameObject.active)
                    hm.UseButton.gameObject.SetActive(false);
                break;
        }
    }
}