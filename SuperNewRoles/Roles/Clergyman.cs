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
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            RoleClass.Clergyman.ButtonTimer = DateTime.Now;
        }
        public static bool isClergyman(PlayerControl Player)
        {
            if (RoleClass.Clergyman.ClergymanPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void LightOutStart()
        {
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DownImpoVision;
            RoleClass.Clergyman.IsLightOff = true;
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCClergymanLightOut, Hazel.SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
        }
        public static void LightOutStartRPC()
        {

            SuperNewRolesPlugin.Logger.LogInfo("-----------");
            SuperNewRolesPlugin.Logger.LogInfo("Light Out Start");
            SuperNewRolesPlugin.Logger.LogInfo("-----------");
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                new CustomMessage(ModTranslation.getString("ClergymanLightOutMessage"), RoleClass.Clergyman.DurationTime);
            }
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DownImpoVision;
            RoleClass.Clergyman.OldButtonTimer = DateTime.Now;
        }
        public static void LightOutEnd()
        {
            if (!RoleClass.Clergyman.IsLightOff) return;

            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DefaultImpoVision;
            RoleClass.Clergyman.IsLightOff = false;

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
        public static void EndMeeting()
        {
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            RoleClass.Clergyman.ButtonTimer = DateTime.Now;
            PlayerControl.GameOptions.ImpostorLightMod = RoleClass.Clergyman.DefaultImpoVision;
            RoleClass.Clergyman.IsLightOff = false;
        }
    }
}
