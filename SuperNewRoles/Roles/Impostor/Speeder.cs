using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles;

public class Speeder
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.SpeederButton.MaxTimer = RoleClass.Speeder.CoolTime;
        HudManagerStartPatch.SpeederButton.Timer = HudManagerStartPatch.SpeederButton.MaxTimer;
        HudManagerStartPatch.SpeederButton.effectCancellable = false;
        HudManagerStartPatch.SpeederButton.EffectDuration = RoleClass.Speeder.DurationTime;
        HudManagerStartPatch.SpeederButton.HasEffect = true;
    }
    public static void DownStart()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
        writer.Write(true);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedDown(true);
    }
    public static void ResetSpeed()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSpeedDown, SendOption.Reliable, -1);
        writer.Write(false);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetSpeedDown(false);
    }
    public static void SpeedDownEnd()
    {
        ResetSpeed();
        ResetCooldown();
    }
    public static bool IsSpeeder(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Speeder);
    }
    public static void WrapUp()
    {
        RoleClass.Speeder.IsSpeedDown = false;
    }
    public static void EndMeeting()
    {
        ResetCooldown();
        ResetSpeed();
    }
}