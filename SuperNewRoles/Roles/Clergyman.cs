using HarmonyLib;
using Hazel;
using System;
using SuperNewRoles.Patches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Roles
{
    class Clergyman
    {
        
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.ClergymanLightOutButton.Timer = RoleClass.Clergyman.CoolTime;
        }
        public static bool isClergyman(PlayerControl Player)
        {
            if (RoleClass.Clergyman.ClergymanPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            } else
            {
                return false;
            }
        }
        public static void LightOutStart()
        {
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DownImpoVision;
            RoleClass.Clergyman.IsLightOff = true;
            Buttons.HudManagerStartPatch.ClergymanLightOutButton.effectCancellable = true;
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCClergymanLightOut, Hazel.SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
            Clergyman.ResetCoolDown();
        }
        public static void LightOutStartRPC()
        {

            SuperNewRolesPlugin.Logger.LogInfo("-----------");
            SuperNewRolesPlugin.Logger.LogInfo("Light Out Start");
            SuperNewRolesPlugin.Logger.LogInfo("-----------");
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DownImpoVision;
        }
        public static void LightOutEnd()
        {
            if (!RoleClass.Clergyman.IsLightOff) return;
            
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DefaultImpoVision;
            RoleClass.Clergyman.IsLightOff = false;
            Buttons.HudManagerStartPatch.ClergymanLightOutButton.effectCancellable = true;

            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCClergymanLightOut, Hazel.SendOption.Reliable, -1);
            RPCWriter.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
        }
        public static void LightOutEndRPC()
        {
            if (!RoleClass.Clergyman.IsLightOff)
            {
                SuperNewRolesPlugin.Logger.LogInfo("-----------");
                SuperNewRolesPlugin.Logger.LogInfo("Light Out End");
                SuperNewRolesPlugin.Logger.LogInfo("-----------");
                PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DefaultImpoVision;
            }
        }
        public static void LightOutCheck()
        {
            if (!RoleClass.Clergyman.IsLightOff) return;
            if (HudManagerStartPatch.ClergymanLightOutButton.Timer + RoleClass.Clergyman.DurationTime <= RoleClass.Clergyman.CoolTime) LightOutEnd();
        }
        public static void EndMeeting()
        {
            Clergyman.ResetCoolDown();
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DefaultImpoVision;
            RoleClass.Clergyman.IsLightOff = false;
        }
    }
}
