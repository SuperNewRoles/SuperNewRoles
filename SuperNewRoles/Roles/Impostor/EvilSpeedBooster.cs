using System;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class EvilSpeedBooster
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
            RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
        }
        public static void BoostStart()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetSpeedBoost(true, CachedPlayer.LocalPlayer.PlayerId);
            RoleClass.EvilSpeedBooster.IsSpeedBoost = true;
            ResetCoolDown();
        }
        public static void ResetSpeed()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetSpeedBoost, SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetSpeedBoost(false, CachedPlayer.LocalPlayer.PlayerId);
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
            return Player.IsRole(RoleId.EvilSpeedBooster);
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.EvilSpeedBoosterBoostButton.MaxTimer = RoleClass.EvilSpeedBooster.CoolTime;
            RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
            ResetSpeed();
        }
    }
}