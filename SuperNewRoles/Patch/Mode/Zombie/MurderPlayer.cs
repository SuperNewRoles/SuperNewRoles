using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.Zombie
{
    [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.MurderPlayer))]
    class MurderPlayer
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (ModeHandler.isMode(ModeId.Zombie))
            {
                target.RpcProtectPlayer(target, 0);
            }
        }
    }
}
