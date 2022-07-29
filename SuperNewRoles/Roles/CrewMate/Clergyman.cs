using System;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Clergyman
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            RoleClass.Clergyman.ButtonTimer = DateTime.Now;
        }
        public static bool IsClergyman(PlayerControl Player)
        {
            return Player.IsRole(RoleId.Clergyman);
        }
        public static void LightOutStart()
        {
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCClergymanLightOut, SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
        }
        public static bool IsLightOutVision()
        {
            return RoleClass.Clergyman.OldButtonTime <= 0
                ? false
                : (CountChanger.GetRoleType(PlayerControl.LocalPlayer) == TeamRoleType.Impostor)
                || CountChanger.IsChangeMadmate(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadMayor(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadJester(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadStuntMan(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadHawk(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadSeer(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeMadMaker(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeJackal(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeSidekick(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeJackalFriends(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeSeerFriends(PlayerControl.LocalPlayer)
                || CountChanger.IsChangeJackalSeer(PlayerControl.LocalPlayer)
                ? true
                : CountChanger.IsChangeSidekickSeer(PlayerControl.LocalPlayer) || CountChanger.IsChangeBlackCat(PlayerControl.LocalPlayer);
        }
        public static bool IsLightOutVisionNoTime()
        {
            return CountChanger.GetRoleType(PlayerControl.LocalPlayer) == TeamRoleType.Impostor;
        }
        public static void LightOutStartRPC()
        {
            if (IsLightOutVisionNoTime())
            {
                new CustomMessage(ModTranslation.GetString("ClergymanLightOutMessage"), RoleClass.Clergyman.DurationTime);
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