using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Jackal
    {
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.myRend == null) return;

            target.myRend.material.SetFloat("_Outline", 1f);
            target.myRend.material.SetColor("_OutlineColor", color);
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class JackalFixedPatch
        {

            static void jackalSetTarget() {
                HudManager.Instance.KillButton.Show();
                setPlayerOutline(PlayerControlFixedUpdatePatch.setTarget(), RoleClass.Jackal.color);
            }
            public static void Postfix(PlayerControl __instance) {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost) {
                    var jackalalldead = true;
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer) {
                        if (p.isAlive()) {
                            jackalalldead = false;
                        }
                    }
                    if (jackalalldead) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.SidekickPromotes();
                    }
                }
                if (RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer)) {
                    jackalSetTarget();
                }
            }
        }
    }
}
