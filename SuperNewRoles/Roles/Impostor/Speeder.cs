using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using UnityEngine;

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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
            writer.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetSpeedDown(true);
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
            writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetSpeedDown(false);
        }
        public static void SpeedDownEnd()
        {
            ResetSpeed();
            ResetCoolDown();
        }
        public static bool IsSpeeder(PlayerControl Player)
        {
            return Player.IsRole(RoleId.Speeder);
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
            ResetSpeed();
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsSpeedPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (ModeHandler.IsMode(ModeId.Default))
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