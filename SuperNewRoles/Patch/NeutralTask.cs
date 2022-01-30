using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class NeutralTask
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            if (PlayerControl.LocalPlayer == __instance)
            {
                refreshRoleDescription(__instance);
            }
        }
        
        public static void refreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            if (!(player.getRole() == CustomRPC.RoleId.EvilSpeedBooster)) return;
            var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
            task.transform.SetParent(player.transform, false);
            task.Text = ModHelpers.cs(Color.red,"イビルスピードブースター : スピードを早くしよう");
        }
    }
}
