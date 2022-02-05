using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Moving
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.Moving.CoolTime;
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
        public static bool IsSetPostion()
        {
            if (!(RoleClass.Moving.setpostion == new Vector3(0, 0, 0))) return true;
            return false;
        }
        public static void TP()
        {
            PlayerControl.LocalPlayer.transform.position = RoleClass.Moving.setpostion;
        }
        public static void SetPostion()
        {
            RoleClass.Moving.setpostion = PlayerControl.LocalPlayer.transform.position;
        }
        public static bool IsMoving(PlayerControl Player)
        {
            if (RoleClass.Moving.MovingPlayer.IsCheckListPlayerControl(Player))
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
            HudManagerStartPatch.MovingTpButton.MaxTimer = RoleClass.Moving.CoolTime;
            RoleClass.Moving.ButtonTimer = DateTime.Now;
        }
    }
}
