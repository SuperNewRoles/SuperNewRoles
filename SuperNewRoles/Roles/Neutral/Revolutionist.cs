using Hazel;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Revolutionist
    {
        public static void FixedUpdate()
        {
            if (RoleClass.Revolutionist.CurrentTarget != null)
            {
                SetPlayerOutline(RoleClass.Revolutionist.CurrentTarget, RoleClass.Revolutionist.color);
                if (RoleClass.Revolutionist.CurrentTarget.IsDead() || Vector2.Distance(RoleClass.Revolutionist.CurrentTarget.GetTruePosition(), CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) > 1f)
                {
                    RoleClass.Revolutionist.CurrentTarget = null;
                    Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new(1f, 1f, 1f, 1f);
                    Buttons.HudManagerStartPatch.RevolutionistButton.Timer = 0;
                    Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.CoolTime;
                }
                else
                {
                    if (Buttons.HudManagerStartPatch.RevolutionistButton.Timer <= 0)
                    {
                        RoleClass.Revolutionist.RevolutionedPlayerId.Add(RoleClass.Revolutionist.CurrentTarget.PlayerId);
                        RoleClass.Revolutionist.CurrentTarget = null;
                        Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new(1f, 1f, 1f, 1f);
                        Buttons.HudManagerStartPatch.RevolutionistButton.Timer = RoleClass.Revolutionist.CoolTime;
                        Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.CoolTime;
                        bool IsFlag = true;
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player.IsAlive() && CachedPlayer.LocalPlayer.PlayerId != player.PlayerId && !RoleClass.Revolutionist.RevolutionedPlayerId.Contains(player.PlayerId))
                            {
                                IsFlag = false;
                            }
                        }
                        if (IsFlag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StartRevolutionMeeting, SendOption.Reliable, -1);
                            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.StartRevolutionMeeting(CachedPlayer.LocalPlayer.PlayerId);
                            RoleClass.Revolutionist.IsEndMeeting = true;
                        }
                    }
                }
            }
            else
            {
                PlayerControl target = Buttons.HudManagerStartPatch.SetTarget(untarget: RoleClass.Revolutionist.RevolutionedPlayer);
                SetPlayerOutline(target, RoleClass.Revolutionist.color);
                if (Buttons.HudManagerStartPatch.RevolutionistButton.Timer <= 0)
                {
                    if (target != null &&
                        Vector2.Distance(target.GetTruePosition(), CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) <= 1f
                        )
                    {
                        RoleClass.Revolutionist.CurrentTarget = target;
                        Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new Color(0f, 0.8f, 0f, 1f);
                        Buttons.HudManagerStartPatch.RevolutionistButton.Timer = RoleClass.Revolutionist.TouchTime;
                        Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.TouchTime;
                    }
                }
            }
        }
        public static void WrapUp()
        {
            if (RoleClass.Revolutionist.MeetingTrigger != null)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.Revolutionist.WinPlayer != null)
                    {
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(RoleClass.Revolutionist.WinPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RPCProcedure.ShareWinner(RoleClass.Revolutionist.WinPlayer.PlayerId);
                        ShipStatus.Instance.enabled = false;
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.RevolutionistWin, false);
                    }
                }
                RoleClass.Revolutionist.MeetingTrigger = null;
            }
        }
    }
}