using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomRPC;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public static class SecretlyKiller
    {
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SecretlyKillerButton.MaxTimer = RoleClass.SecretlyKiller.CoolTime;
            HudManagerStartPatch.SecretlyKillerButton.Timer = RoleClass.SecretlyKiller.CoolTime;
            RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
        }
    }
}
