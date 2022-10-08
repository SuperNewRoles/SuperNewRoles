using System;
using Hazel;
using SuperNewRoles.Buttons;


namespace SuperNewRoles.Roles
{
    class Clergyman
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            HudManagerStartPatch.ClergymanLightOutButton.Timer = HudManagerStartPatch.ClergymanLightOutButton.MaxTimer;
            HudManagerStartPatch.ClergymanLightOutButton.effectCancellable = true;
            HudManagerStartPatch.ClergymanLightOutButton.EffectDuration = RoleClass.Clergyman.DurationTime;
            HudManagerStartPatch.ClergymanLightOutButton.HasEffect = true;
        }
        public static bool IsClergyman(PlayerControl Player)
        {
            return Player.IsRole(RoleId.Clergyman);
        }
        public static void LightOutStart()
        {
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCClergymanLightOut, SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
            RPCProcedure.RPCClergymanLightOut(true);
        }
        public static bool IsLightOutVision()
        {
            return !RoleClass.Clergyman.IsLightOff
                ? false
                : (CountChanger.GetRoleType(PlayerControl.LocalPlayer) == TeamRoleType.Impostor)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadMate)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadMayor)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadJester)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadJester)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadHawk)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadSeer)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.MadMaker)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.Jackal)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.Sidekick)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.JackalFriends)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.SeerFriends)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.JackalSeer)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.JackalSeer)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.SidekickSeer)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.BlackCat)
                || CountChanger.IsChange(PlayerControl.LocalPlayer, RoleId.Hitman);
        }
        public static void LightOutStartRPC()
        {
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.ClergymanLightOut))
            {
                RoleClass.Clergyman.IsLightOff = true;
            }
            if (IsLightOutVision())
            {
                RoleClass.Clergyman.currentMessage = new(ModTranslation.GetString("ClergymanLightOutMessage"), RoleClass.Clergyman.DurationTime);
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
            HudManagerStartPatch.ClergymanLightOutButton.Timer = HudManagerStartPatch.ClergymanLightOutButton.MaxTimer;
            HudManagerStartPatch.ClergymanLightOutButton.effectCancellable = true;
            Logger.Info(RoleClass.Clergyman.DurationTime.ToString(), "ClergymanDuration");
            HudManagerStartPatch.ClergymanLightOutButton.EffectDuration = RoleClass.Clergyman.DurationTime;
            HudManagerStartPatch.ClergymanLightOutButton.HasEffect = true;
            RoleClass.Clergyman.IsLightOff = false;
        }
    }
}