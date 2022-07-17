using System;
using SuperNewRoles.Buttons;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Moving
    {
        public static void ResetCoolDown()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.EvilMoving))
            {
                HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.EvilMoving.CoolTime;
            }
            else
            {
                HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.Moving.CoolTime;
            }
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
        public static bool IsSetPostion()
        {
            if (!(RoleClass.Moving.setpostion == new Vector3(0, 0, 0))) return true;
            return false;
        }
        public static void TP()
        {
            CachedPlayer.LocalPlayer.transform.position = RoleClass.Moving.setpostion;
        }
        public static void SetPostion()
        {
            RoleClass.Moving.setpostion = CachedPlayer.LocalPlayer.transform.position;
        }
        public static bool IsMoving(PlayerControl Player)
        {
            if (Player.isRole(RoleId.Moving) || Player.isRole(RoleId.EvilMoving))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.MovingSetButton.Timer = 0f;
            if (PlayerControl.LocalPlayer.isRole(RoleId.EvilMoving))
            {
                HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.EvilMoving.CoolTime;
            }
            else
            {
                HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.Moving.CoolTime;
            }
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
    }
}
