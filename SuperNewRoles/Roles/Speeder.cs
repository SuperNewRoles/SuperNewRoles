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
            HudManagerStartPatch.SpeederButton.Timer = HudManagerStartPatch.SpeederButton.MaxTimer;
            HudManagerStartPatch.SpeederButton.actionButton.cooldownTimerText.color = Color.white;
        }
        public static void DownStart()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedDown(true);
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
            writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedDown(false);
        }
        public static void SpeedDownEnd()
        {
            ResetSpeed();
            Speeder.ResetCoolDown();
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
            Speeder.ResetCoolDown();
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
                    __instance.body.velocity /= 10f;
                }
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        public static void Postfix()
        {
            if (HudManagerStartPatch.SpeederButton.Timer <= 0.1 && RoleClass.Speeder.IsSpeedDown)
            {
                Speeder.SpeedDownEnd();
            }
        }
    }
}
