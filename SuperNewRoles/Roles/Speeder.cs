using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;
using System.Linq;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public class Speeder
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SpeederButton.MaxTimer = RoleClass.Speeder.CoolTime;
            RoleClass.Speeder.ButtonTimer = DateTime.Now;
        }
        public static void DownStart()
        {
            RoleClass.Speeder.IsSpeedDown = true;
      /*    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(true, PlayerControl.LocalPlayer.PlayerId);*/
            Speeder.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            RoleClass.Speeder.IsSpeedDown = false;
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsSpeeder(PlayerControl Player)
        {
            if (RoleClass.Speeder.SpeederPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {

            HudManagerStartPatch.SpeederButton.MaxTimer = RoleClass.Speeder.CoolTime;
            RoleClass.Speeder.ButtonTimer = DateTime.Now;
            ResetSpeed();
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (RoleClass.Speeder.IsSpeedDown)
                {
                    __instance.body.velocity = __instance.body.velocity * 0.0001f;
                }
            }
        }
    }
}
