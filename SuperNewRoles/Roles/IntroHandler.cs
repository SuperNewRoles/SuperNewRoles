using SuperNewRoles.Mode.SuperHostRoles;
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
        public static void Handler()
        {
            float time = 2f;
            if (PlayerControl.GameOptions.KillCooldown >= 10)
            {
                time = 7f;
            }
            new LateTask(() => {
                RoleClass.IsStart = true;
            }, time, "IsStartOn");
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
            if (Mode.ModeHandler.isMode(Mode.ModeId.Werewolf))
            {
                Mode.Werewolf.main.IntroHandler();
            }
        }
    }
}
