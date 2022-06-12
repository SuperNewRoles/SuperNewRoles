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
    class SpeedBooster { 
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
        }
        public static void BoostStart()
        {
            RoleClass.SpeedBooster.IsSpeedBoost = true;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(true,CachedPlayer.LocalPlayer.PlayerId);
            SpeedBooster.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(false, CachedPlayer.LocalPlayer.PlayerId);
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
        }
        public static bool IsSpeedBooster(PlayerControl Player)
        {
            if (RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(Player))
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

            HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
            RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
            ResetSpeed();
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsSpeedPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (__instance.AmOwner && __instance.myPlayer.isRole(CustomRPC.RoleId.SpeedBooster) && RoleClass.SpeedBooster.IsBoostPlayers.ContainsKey(__instance.myPlayer.PlayerId) && __instance.myPlayer.CanMove && GameData.Instance && RoleClass.SpeedBooster.IsBoostPlayers[__instance.myPlayer.PlayerId])
                    {
                        __instance.body.velocity = __instance.body.velocity * RoleClass.SpeedBooster.Speed;
                    }
                    else if (__instance.AmOwner && RoleClass.EvilSpeedBooster.IsBoostPlayers.ContainsKey(__instance.myPlayer.PlayerId) && __instance.myPlayer.CanMove && GameData.Instance && RoleClass.EvilSpeedBooster.IsBoostPlayers[__instance.myPlayer.PlayerId])
                    {
                        __instance.body.velocity = __instance.body.velocity * RoleClass.EvilSpeedBooster.Speed;
                    }
                }
            }
        }
    }
}
