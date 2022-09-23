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
            HudManagerStartPatch.DoubleKillerMainKillButton.MaxTimer = RoleClass.DoubleKiller.MainCoolTime;
            HudManagerStartPatch.DoubleKillerMainKillButton.Timer = RoleClass.DoubleKiller.MainCoolTime;
        }
        public static void ResetSubCoolDown()
        {
            HudManagerStartPatch.DoubleKillerSubKillButton.MaxTimer = RoleClass.DoubleKiller.SubCoolTime;
            HudManagerStartPatch.DoubleKillerSubKillButton.Timer = RoleClass.DoubleKiller.SubCoolTime;
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