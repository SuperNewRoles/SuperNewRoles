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
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadMate)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadMayor)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadJester)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadJester)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadHawk)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadSeer)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.MadMaker)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.Jackal)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.Sidekick)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.JackalFriends)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.SeerFriends)
                || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.JackalSeer)
                ? true
                : CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.SidekickSeer) || CountChanger.IsChange(PlayerControl.LocalPlayer,RoleId.BlackCat);
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