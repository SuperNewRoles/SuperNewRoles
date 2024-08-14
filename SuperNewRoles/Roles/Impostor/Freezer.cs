using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Freezer
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.FreezerButton.MaxTimer = RoleClass.Freezer.CoolTime;
        HudManagerStartPatch.FreezerButton.Timer = HudManagerStartPatch.FreezerButton.MaxTimer;
        HudManagerStartPatch.FreezerButton.actionButton.cooldownTimerText.color = Color.white;
        HudManagerStartPatch.FreezerButton.effectCancellable = false;
        HudManagerStartPatch.FreezerButton.EffectDuration = RoleClass.Freezer.DurationTime;
        HudManagerStartPatch.FreezerButton.HasEffect = true;
        HudManagerStartPatch.FreezerButton.isEffectActive = false;
    }
    public static void DownStart()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedFreeze, SendOption.Reliable, -1);
        writer.Write(true);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedFreeze(true);
    }
    public static void ResetSpeed()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedFreeze, SendOption.Reliable, -1);
        writer.Write(false);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedFreeze(false);
    }
    public static void SpeedDownEnd()
    {
        ResetSpeed();
        ResetCooldown();
    }
    public static bool IsFreezer(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Freezer);
    }
    public static void EndMeeting()
    {
        ResetCooldown();
        ResetSpeed();
    }
}