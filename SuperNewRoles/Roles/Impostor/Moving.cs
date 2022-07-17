using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Moving
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.MovingTpButton.MaxTimer = PlayerControl.LocalPlayer.isRole(RoleId.EvilMoving) ? RoleClass.EvilMoving.CoolTime : RoleClass.Moving.CoolTime;
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
        public static bool IsSetPostion()
        {
            return !(RoleClass.Moving.setpostion == new Vector3(0, 0, 0));
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
            return Player.isRole(RoleId.Moving) || Player.isRole(RoleId.EvilMoving);
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.MovingSetButton.Timer = 0f;
            HudManagerStartPatch.MovingTpButton.MaxTimer = PlayerControl.LocalPlayer.isRole(RoleId.EvilMoving) ? RoleClass.EvilMoving.CoolTime : RoleClass.Moving.CoolTime;
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
    }
}