using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SuperNewRoles.Roles.EvilGambler;

namespace SuperNewRoles.Roles
{
    class IntroHandler
    {
        public static void Handler() {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Pursuer))
            {
                RoleClass.Pursuer.arrow.arrow.SetActive(false);
                RoleClass.Pursuer.arrow.arrow.SetActive(true);
            }
            if (Mode.ModeHandler.isMode(Mode.ModeId.Zombie))
            {
                Mode.Zombie.main.SetTimer();
            }
            if (Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles))
            {
                Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
            }
        }
    }
}
