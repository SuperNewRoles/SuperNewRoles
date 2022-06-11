using HarmonyLib;
using Hazel;
using System;
using SuperNewRoles.Patches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Helpers;

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
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCClergymanLightOut, Hazel.SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
        }
        public static bool IsLightOutVision() {
            if (RoleClass.Clergyman.OldButtonTime <= 0) return false;
            if (CountChanger.GetRoleType(PlayerControl.LocalPlayer) == TeamRoleType.Impostor) return true;
            if (CountChanger.IsChangeMadmate(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadMayor(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadJester(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadStuntMan(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadHawk(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadSeer(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeMadMaker(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeJackal(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeSidekick(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeJackalFriends(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeSeerFriends(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeJackalSeer(PlayerControl.LocalPlayer)) return true;
            if (CountChanger.IsChangeSidekickSeer(PlayerControl.LocalPlayer)) return true;
            return false;
        }
        public static bool IsLightOutVisionNoTime()
        {
            if (CountChanger.GetRoleType(PlayerControl.LocalPlayer) == TeamRoleType.Impostor) return true;
            return false;
        }
        public static void LightOutStartRPC()
        {
            if (IsLightOutVisionNoTime())
            {
                new CustomMessage(ModTranslation.getString("ClergymanLightOutMessage"), RoleClass.Clergyman.DurationTime);
            }
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.ClergymanLightOut))
            {
                RoleClass.Clergyman.OldButtonTimer = DateTime.Now;
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            RoleClass.Clergyman.ButtonTimer = DateTime.Now;
            RoleClass.Clergyman.IsLightOff = false;
        }
    }
}
