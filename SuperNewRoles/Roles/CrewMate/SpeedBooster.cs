using System;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles;

class SpeedBooster
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
        RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
    }
    public static void BoostStart()
    {
        RoleClass.SpeedBooster.IsSpeedBoost = true;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedBoost, SendOption.Reliable, -1);
        writer.Write(true);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedBoost(true, CachedPlayer.LocalPlayer.PlayerId);
        ResetCooldown();
    }
    public static void ResetSpeed()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedBoost, SendOption.Reliable, -1);
        writer.Write(false);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedBoost(false, CachedPlayer.LocalPlayer.PlayerId);
    }
    public static void SpeedBoostEnd() { ResetSpeed(); }
    public static bool IsSpeedBooster(PlayerControl Player) { return Player.IsRole(RoleId.SpeedBooster); }
    public static void EndMeeting()
    {
        HudManagerStartPatch.SpeedBoosterBoostButton.MaxTimer = RoleClass.SpeedBooster.CoolTime;
        RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
        ResetSpeed();
    }
    public static void PlayerPhysicsSpeedPatchPostfix(PlayerPhysics __instance)
    {
        if (RoleClass.SpeedBooster.IsBoostPlayers.TryGetValue(__instance.myPlayer.PlayerId, out bool result) && result)
            __instance.body.velocity = __instance.body.velocity * RoleClass.SpeedBooster.Speed;
        else if (RoleClass.EvilSpeedBooster.IsBoostPlayers.TryGetValue(__instance.myPlayer.PlayerId, out bool result2) && result2)
            __instance.body.velocity = __instance.body.velocity * RoleClass.EvilSpeedBooster.Speed;
    }   
}