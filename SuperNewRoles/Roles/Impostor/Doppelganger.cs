using System.Collections.Generic;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor
{
    public class Doppelganger
    {
        public static void DoppelgangerShape()
        {
            bool isShapeshift = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.Targets)
            {
                if (p.Key == CachedPlayer.LocalPlayer.PlayerControl.PlayerId)
                {
                    isShapeshift = true;
                    break;
                }
            }
            if (!isShapeshift)
            {
                float nowKillCool = CachedPlayer.LocalPlayer.PlayerControl.killTimer;
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer.PlayerControl, RoleTypes.Shapeshifter);
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer.PlayerControl, RoleTypes.Impostor);
                CachedPlayer.LocalPlayer.PlayerControl.SetKillTimerUnchecked(nowKillCool);
            }
            else if (CachedPlayer.LocalPlayer.PlayerControl.inVent)
            {
                CachedPlayer.LocalPlayer.PlayerControl.RpcRevertShapeshift(false);
            }
            else
            {
                CachedPlayer.LocalPlayer.PlayerControl.NetTransform.Halt();
                CachedPlayer.LocalPlayer.PlayerControl.RpcRevertShapeshift(true);
            }
        }
        public static void ResetShapeCool()
        {
            RoleClass.Doppelganger.Duration = RoleClass.Doppelganger.DurationTime + 1.1f;
            HudManagerStartPatch.DoppelgangerButton.MaxTimer = RoleClass.Doppelganger.CoolTime;
            HudManagerStartPatch.DoppelgangerButton.Timer = RoleClass.Doppelganger.CoolTime;
        }
        public static void FixedUpdate()
        {
            bool shape = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.Targets)
            {
                if (p.Key == CachedPlayer.LocalPlayer.PlayerControl.PlayerId && p.Value != CachedPlayer.LocalPlayer.PlayerControl)
                {
                    shape = true;
                    break;
                }
            }
            if (!RoleClass.IsMeeting && shape)
            {
                RoleClass.Doppelganger.Duration -= Time.fixedDeltaTime;
                if (RoleClass.Doppelganger.Duration <= 0) DoppelgangerShape();
                RoleClass.Doppelganger.DoppelgangerDurationText.text = RoleClass.Doppelganger.Duration > RoleClass.Doppelganger.DurationTime
                    ? string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), (int)RoleClass.Doppelganger.DurationTime)
                    : string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), ((int)RoleClass.Doppelganger.Duration) + 1);
            }
            else if (RoleClass.Doppelganger.DoppelgangerDurationText.text != "")
            {
                RoleClass.Doppelganger.DoppelgangerDurationText.text = "";
            }
            if (shape && RoleClass.IsMeeting) CachedPlayer.LocalPlayer.PlayerControl.RpcRevertShapeshift(false);
        }
        public class KillCoolSetting
        {
            public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
            {
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    if (!ModeHandler.IsMode(ModeId.SuperHostRoles) && __instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (RoleClass.Doppelganger.Targets.ContainsKey(__instance.PlayerId))
                        {
                            RoleClass.Doppelganger.CurrentCool = RoleClass.Doppelganger.Targets[__instance.PlayerId].PlayerId == target.PlayerId ?
                                                                     RoleClass.Doppelganger.SucCool :
                                                                     RoleClass.Doppelganger.NotSucCool;
                        }
                        else RoleClass.Doppelganger.CurrentCool = RoleClass.Doppelganger.NotSucCool;
                    }
                }
            }
            public static void SHRMurderPlayer(PlayerControl __instance, PlayerControl target)
            {
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        SyncSetting.DoppelgangerCool(__instance, target);
                    }
                }
            }
        }
    }
}