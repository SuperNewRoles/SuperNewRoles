using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Buttons;

using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class DoubleKiller
    {
        public static void ResetMainCoolDown()
        {
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = CustomOptions.MainKillCoolTime.GetFloat();
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer;
        }
        public static void ResetSubCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = CustomOptions.SubKillCoolTime.GetFloat();
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer;
        }
        public static void EndMeeting()
        {
            ResetSubCoolDown();
            ResetMainCoolDown();
        }
        public class DoubleKillerFixedPatch
        {
            public static void SetOutline()
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.DoubleKiller))
                {
                    Patches.PlayerControlFixedUpdatePatch.SetPlayerOutline(Patches.PlayerControlFixedUpdatePatch.SetTarget(), RoleClass.DoubleKiller.color);
                }
            }
        }
    }
}