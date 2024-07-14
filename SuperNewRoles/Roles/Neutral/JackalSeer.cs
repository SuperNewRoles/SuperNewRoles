using Hazel;
using SuperNewRoles.Buttons;
using UnityEngine;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles;

class JackalSeer
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.JackalSeer.KillCooldown;
        HudManagerStartPatch.JackalKillButton.Timer = RoleClass.JackalSeer.KillCooldown;
    }
    public static void EndMeetingResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.JackalSeer.KillCooldown;
        HudManagerStartPatch.JackalKillButton.Timer = RoleClass.JackalSeer.KillCooldown;
        HudManagerStartPatch.JackalSeerSidekickButton.MaxTimer = CustomOptionHolder.JackalSeerSKCooldown.GetFloat();
        HudManagerStartPatch.JackalSeerSidekickButton.Timer = CustomOptionHolder.JackalSeerSKCooldown.GetFloat();
    }
    public static void EndMeeting() => EndMeetingResetCooldown();
    public static void SetPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null || target.MyRend() == null) return;

        target.MyRend().material.SetFloat("_Outline", 1f);
        target.MyRend().material.SetColor("_OutlineColor", color);
    }
    public class JackalSeerFixedPatch
    {
        public static void JackalSeerPlayerOutLineTarget()
        {
            SetPlayerOutline(JackalSetTarget(), RoleClass.JackalSeer.color);
        }
        public static void Postfix(PlayerControl __instance, RoleId role)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (RoleClass.JackalSeer.SidekickSeerPlayer.Count > 0)
                {
                    var upflag = true;
                    foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer.AsSpan())
                    {
                        if (p.IsAlive())
                        {
                            upflag = false;
                        }
                    }
                    if (upflag)
                    {
                        byte jackalId = (byte)RoleId.JackalSeer;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                        writer.Write(jackalId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.SidekickPromotes(jackalId);
                    }
                }
            }
            if (role == RoleId.JackalSeer)
            {
                JackalSeerPlayerOutLineTarget();
            }
        }
    }
}