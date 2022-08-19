using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    internal class Doppelganger
    {
        public static void DoppelgangerShape()
        {
            float NowKillCool = PlayerControl.LocalPlayer.killTimer;
            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
            CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
            PlayerControl.LocalPlayer.SetKillTimerUnchecked(NowKillCool);
        }
        public static void DoppelgangerResetCoolDown()
        {
            RoleClass.Doppelganger.ShapeButton = DateTime.Now;
            RoleClass.Doppelganger.Duration = RoleClass.Doppelganger.DurationTime + 1.1f;
            HudManagerStartPatch.DoppelgangerButton.MaxTimer = RoleClass.Doppelganger.CoolTime + 1;
            HudManagerStartPatch.DoppelgangerButton.Timer = RoleClass.Doppelganger.CoolTime + 1;
        }
        public static void DoppelgangerShapeDuration()
        {
            if (!RoleClass.IsMeeting)
            {
                if (RoleClass.Doppelganger.Target != PlayerControl.LocalPlayer)
                {
                    RoleClass.Doppelganger.Duration -= Time.fixedDeltaTime;
                    if (RoleClass.Doppelganger.Duration <= 0)
                    {
                        DoppelgangerShape();
                        if (RoleClass.Doppelganger.Duration <= 0) RoleClass.Doppelganger.Duration = -1;
                    }
                }
            }
            if(!RoleClass.IsMeeting && RoleClass.Doppelganger.Target != PlayerControl.LocalPlayer)
            {
                if(RoleClass.Doppelganger.Duration > RoleClass.Doppelganger.DurationTime)
                {
                    //RoleClass.Doppelganger.DoppelgangerDurationText.text = string.Format(ModTranslation.getString("DoppelgangerDurationTimeText"), (int)RoleClass.Doppelganger.DurationTime);
                    RoleClass.Doppelganger.DoppelgangerDurationText.text = string.Format(ModTranslation.getString("解除まで{0}秒"), (int)RoleClass.Doppelganger.DurationTime);
                }
                else
                {
                    //RoleClass.Doppelganger.DoppelgangerDurationText.text = string.Format(ModTranslation.getString("DoppelgangerDurationTimeText"), ((int)RoleClass.Doppelganger.Duration) + 1);
                    RoleClass.Doppelganger.DoppelgangerDurationText.text = string.Format(ModTranslation.getString("解除まで{0}秒"), ((int)RoleClass.Doppelganger.Duration) + 1);
                }
            }
            else
            {
                if(RoleClass.Doppelganger.DoppelgangerDurationText.text != "")
                {
                    RoleClass.Doppelganger.DoppelgangerDurationText.text = "";
                }
            }
        }

        public static void DoppelgangerSHR()
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {

            }
        }
    }
}
