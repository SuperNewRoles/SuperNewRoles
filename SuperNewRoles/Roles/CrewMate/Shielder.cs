using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Shielder
{
    public static void WrapUp()
    {
        foreach (var data in RoleClass.Shielder.IsShield)
        {
            RoleClass.Shielder.IsShield[data.Key] = false;
        }

    }
    public static void HudUpdate()
    {
        if (HudManagerStartPatch.ShielderButton.Timer <= 0.1f && RoleClass.Shielder.IsShield[CachedPlayer.LocalPlayer.PlayerId] && PlayerControl.LocalPlayer.IsRole(RoleId.Shielder))
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetShielder, SendOption.Reliable, -1);
            Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            Writer.Write(false);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
            RPCProcedure.SetShielder(CachedPlayer.LocalPlayer.PlayerId, false);
            HudManagerStartPatch.ShielderButton.actionButton.cooldownTimerText.color = Color.white;
            HudManagerStartPatch.ShielderButton.MaxTimer = RoleClass.Shielder.CoolTime;
            HudManagerStartPatch.ShielderButton.Timer = HudManagerStartPatch.ShielderButton.MaxTimer;
        }
    }
}