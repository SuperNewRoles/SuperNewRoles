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
    public class Freezer
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.FreezerButton.MaxTimer = RoleClass.Freezer.CoolTime;
            HudManagerStartPatch.FreezerButton.Timer = HudManagerStartPatch.FreezerButton.MaxTimer;
            HudManagerStartPatch.FreezerButton.actionButton.cooldownTimerText.color = Color.white;
        }
        public static void DownStart()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedFreeze, SendOption.Reliable, -1);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedFreeze(true);
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedFreeze, SendOption.Reliable, -1);
            writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedFreeze(false);
        }
        public static void SpeedDownEnd()
        {
            ResetSpeed();
            Freezer.ResetCoolDown();
        }
        public static bool IsFreezer(PlayerControl Player)
        {
            if (RoleClass.Freezer.FreezerPlayer.IsCheckListPlayerControl(Player))
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
            Freezer.ResetCoolDown();
            ResetSpeed();
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch2
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (RoleClass.Freezer.IsSpeedDown)
                {
                    __instance.body.velocity = new Vector2(0f, 0f);
                }
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch2
    {
        public static void Postfix()
        {
            if (HudManagerStartPatch.FreezerButton.Timer <= 0.1f && RoleClass.Freezer.IsSpeedDown)
            {
                Freezer.SpeedDownEnd();
            }
        }
    }
}
