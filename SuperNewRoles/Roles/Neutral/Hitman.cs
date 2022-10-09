using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using UnityEngine;


namespace SuperNewRoles.Roles.Neutral
{
    public static class Hitman
    {
        public static void KillSuc()
        {
            RoleClass.Hitman.WinKillCount--;
            if (RoleClass.Hitman.WinKillCount <= 0)
            {
                RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(Writer);

                Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                Writer.Write((byte)CustomGameOverReason.HitmanWin);
                Writer.EndRPC();
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);
                //SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
                var reason = (GameOverReason)CustomGameOverReason.HitmanWin;
                if (AmongUsClient.Instance.AmHost)
                {
                    CheckGameEndPatch.CustomEndGame(reason, false);
                }
                else
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                    writer.Write((byte)reason);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }
        public static void EndMeeting()
        {
            Buttons.HudManagerStartPatch.HitmanKillButton.MaxTimer = CustomOptions.HitmanKillCoolTime.GetFloat();
            Buttons.HudManagerStartPatch.HitmanKillButton.Timer = Buttons.HudManagerStartPatch.HitmanKillButton.MaxTimer;
        }
        public static void FixedUpdate()
        {
            if (RoleClass.IsMeeting) return;
            RoleClass.Hitman.UpdateTime -= Time.fixedDeltaTime;
            if (RoleClass.Hitman.UpdateTime <= 0)
            {
                SetTarget();
                LimitDown();
                RoleClass.Hitman.UpdateTime = CustomOptions.HitmanChangeTargetTime.GetFloat();
            }
            if (PlayerControl.LocalPlayer.IsDead())
            {
                if (RoleClass.Hitman.cooldownText != null)
                {
                    Object.Destroy(RoleClass.Hitman.cooldownText.gameObject);
                    RoleClass.Hitman.cooldownText = null;
                }
            }
            else
            {
                if (RoleClass.Hitman.cooldownText != null)
                {
                    RoleClass.Hitman.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(RoleClass.Hitman.UpdateTime, 0, CustomOptions.HitmanChangeTargetTime.GetFloat())).ToString();
                }
                if (RoleClass.Hitman.Target != null)
                {
                    foreach (var icondata in MapOptions.MapOption.playerIcons)
                    {
                        if (icondata.Key == RoleClass.Hitman.Target.PlayerId)
                        {
                            icondata.Value.gameObject.SetActive(true);
                        }
                        else
                        {
                            icondata.Value.gameObject.SetActive(false);
                        }
                    }
                }
            }
            if (RoleClass.Hitman.TargetArrow != null)
            {
                RoleClass.Hitman.TargetArrow.arrow.SetActive(true);
                RoleClass.Hitman.TargetArrow.Update(RoleClass.Hitman.ArrowPosition);
                RoleClass.Hitman.ArrowUpdateTime -= Time.fixedDeltaTime;
                if (RoleClass.Hitman.ArrowUpdateTime <= 0)
                {
                    RoleClass.Hitman.ArrowUpdateTime = RoleClass.Hitman.ArrowUpdateTimeDefault;
                    RoleClass.Hitman.ArrowPosition = RoleClass.Hitman.Target.transform.position;
                }
            }
        }
        public static void Death()
        {
            if (RoleClass.Hitman.TargetArrow != null)
            {
                RoleClass.Hitman.TargetArrow.arrow.SetActive(false);
            }
        }
        public static void WrapUp()
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.Hitman)) return;
            SetTarget();
            RoleClass.Hitman.UpdateTime = CustomOptions.HitmanChangeTargetTime.GetFloat();
        }
        public static void SetTarget()
        {
            List<PlayerControl> targets = PlayerControl.AllPlayerControls.ToArray().ToList();
            targets.RemoveAll(player =>
            {
                return player.IsDead() || player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            });
            if (targets.Count > 0)
            {
                RoleClass.Hitman.Target = ModHelpers.GetRandom(targets);
            }
        }
        public static void LimitDown()
        {
            if (RoleClass.Hitman.OutMissionLimit == -1) return;
            RoleClass.Hitman.OutMissionLimit--;
            if (RoleClass.Hitman.OutMissionLimit <= 0)
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            }
        }
        public static void DestroyIntroHandle(IntroCutscene __instance)
        {
            if (RoleClass.Hitman.ArrowUpdateTimeDefault != -1)
            {
                RoleClass.Hitman.TargetArrow = new Arrow(RoleClass.Hitman.color);
                if (RoleClass.Hitman.Target != null)
                {
                    RoleClass.Hitman.ArrowPosition = RoleClass.Hitman.Target.transform.position;
                    RoleClass.Hitman.TargetArrow.Update(RoleClass.Hitman.Target.transform.position);
                }
            }
        }
    }
}