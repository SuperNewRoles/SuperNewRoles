using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.MapOptions;
using UnityEngine;
using HarmonyLib;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    public class SuicidalIdeation
    {
        public static void Postfix()
        {
            if (HudManagerStartPatch.SuicidalIdeationButton.Timer <= 0f) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
        }
    }
}