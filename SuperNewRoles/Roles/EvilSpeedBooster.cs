using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class EvilSpeedBooster { 
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
            RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
        }
        public static void BoostStart()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(true, CachedPlayer.LocalPlayer.PlayerId);
            RoleClass.EvilSpeedBooster.IsSpeedBoost = true;
            EvilSpeedBooster.ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, Hazel.SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetSpeedBoost(false, CachedPlayer.LocalPlayer.PlayerId);
            RoleClass.EvilSpeedBooster.IsSpeedBoost = false;
        }

        public static void SpeedBoostCheck()
        {
            if (!RoleClass.EvilSpeedBooster.IsSpeedBoost) return;
            if (HudManagerStartPatch.EvilSpeedBoosterBoostButton.Timer + RoleClass.EvilSpeedBooster.DurationTime <= RoleClass.EvilSpeedBooster.CoolTime) SpeedBoostEnd();
        }
        public static void SpeedBoostEnd()
        {
            ResetSpeed();
            ResetCoolDown();
        }
        public static bool IsEvilSpeedBooster(PlayerControl Player)
        {
            if (RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.IsCheckListPlayerControl(Player))
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

            HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
            RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
            ResetSpeed();

        }
    }
}
