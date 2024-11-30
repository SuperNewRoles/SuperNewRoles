using System;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles;

class TeleportingJackal
{
    public static void ResetCooldowns()
    {
        // テレポート能力のリセットは, インポスター役職 : テレポーター側で行われている。

        HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.TeleportingJackal.KillCooldown;
        HudManagerStartPatch.JackalKillButton.Timer = RoleClass.TeleportingJackal.KillCooldown;
    }
    public static void EndMeeting() => ResetCooldowns();
    public static void SetPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null || target.MyRend() == null) return;

        target.MyRend().material.SetFloat("_Outline", 1f);
        target.MyRend().material.SetColor("_OutlineColor", color);
    }
    public class JackalFixedPatch
    {
        public static void SetOutline()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.TeleportingJackal))
            {
                SetPlayerOutline(Patches.PlayerControlFixedUpdatePatch.JackalSetTarget(), RoleClass.TeleportingJackal.color);
            }
        }
    }
}