using HarmonyLib;
using Hazel;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Roles;
using SuperNewRoles.Patch;
using SuperNewRoles.Sabotage;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.RepairDamage))]
    class HeliSabotageSystemRepairDamagePatch
    {
        static void Postfix(HeliSabotageSystem __instance, PlayerControl player, byte amount)
        {
            HeliSabotageSystem.Tags tags = (HeliSabotageSystem.Tags)(amount & 240);
            if (tags != HeliSabotageSystem.Tags.ActiveBit)
            {
                if (tags == HeliSabotageSystem.Tags.DamageBit)
                {
                    if (PlayerControl.GameOptions.MapId != 1)
                    {
                        __instance.Countdown = Options.skeldReactorDuration.getFloat();
                    }
                    if (PlayerControl.GameOptions.MapId != 2)
                    {
                        __instance.Countdown = Options.miraReactorDuration.getFloat();
                    }
                    if (PlayerControl.GameOptions.MapId != 3)
                    {
                        __instance.Countdown = Options.polusReactorDuration.getFloat();
                    }
                    if (PlayerControl.GameOptions.MapId != 4)
                    {
                        __instance.Countdown = Options.airshipReactorDuration.getFloat();
                    }
                }
            }
        }
    }
}
