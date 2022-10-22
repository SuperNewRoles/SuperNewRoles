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
            MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.RPCClergymanLightOut, SendOption.Reliable, -1);
            RPCWriter.Write(true);
            AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
            RPCProcedure.RPCClergymanLightOut(true);
        }
        public static bool IsLightOutVision()
        {
            return !RoleClass.Clergyman.IsLightOff
                ? false
                : (CountChanger.GetRoleType(CachedPlayer.LocalPlayer.PlayerControl) == TeamRoleType.Impostor)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadMate)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadMayor)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadJester)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadJester)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadHawk)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadSeer)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.MadMaker)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.Jackal)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.Sidekick)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.JackalFriends)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.SeerFriends)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.JackalSeer)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.JackalSeer)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.SidekickSeer)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.BlackCat)
                || CountChanger.IsChange(CachedPlayer.LocalPlayer.PlayerControl, RoleId.Hitman);
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