using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    class WaveCannonJackal
    {
        public static void ResetCooldowns()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = CustomOptionHolder.WaveCannonJackalKillCooldown.GetFloat();
            HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
        }
    }
}