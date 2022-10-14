using System;
using SuperNewRoles.Buttons;
using UnityEngine;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles
{
    class TeleportingJackal
    {
        public static void ResetCoolDowns()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.TeleportingJackal.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.TeleportingJackal.KillCoolDown;
        }
        public static void EndMeeting()
        {
            ResetCoolDowns();
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.TeleportingJackal.CoolTime;
            RoleClass.TeleportingJackal.ButtonTimer = DateTime.Now;
        }
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
}