using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Mode;
using UnityEngine;
using System.Collections.Generic;

namespace SuperNewRoles.Roles.Impostor
{
    internal class Doppelganger
    {
        public static void DoppelgangerShape()
        {
            bool IsShapeshift = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
            {
                if (p.Key == PlayerControl.LocalPlayer.PlayerId)
                {
                    IsShapeshift = true;
                    break;
                }
            }
            if (!IsShapeshift)
            {
                float NowKillCool = PlayerControl.LocalPlayer.killTimer;
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(NowKillCool);
            }
            else if (PlayerControl.LocalPlayer.inVent)
            {
                PlayerControl.LocalPlayer.RpcRevertShapeshift(false);
            }
            else
            {
                PlayerControl.LocalPlayer.NetTransform.Halt();
                PlayerControl.LocalPlayer.RpcRevertShapeshift(true);
            }
        }
        public static void ResetShapeCool()
        {
            RoleClass.Doppelganger.Duration = RoleClass.Doppelganger.DurationTime + 1.1f;
            HudManagerStartPatch.DoppelgangerButton.MaxTimer = RoleClass.Doppelganger.CoolTime;
            HudManagerStartPatch.DoppelgangerButton.Timer = RoleClass.Doppelganger.CoolTime;
        }
        public static void FixedUpdate()
        {
            bool Shape = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
            {
                if (p.Key == PlayerControl.LocalPlayer.PlayerId && p.Value != PlayerControl.LocalPlayer)
                {
                    Shape = true;
                    break;
                }
            }
            if (!RoleClass.IsMeeting && Shape)
            {
                RoleClass.Doppelganger.Duration -= Time.fixedDeltaTime;
                if (RoleClass.Doppelganger.Duration <= 0) DoppelgangerShape();
                RoleClass.Doppelganger.DoppelgangerDurationText.text = RoleClass.Doppelganger.Duration > RoleClass.Doppelganger.DurationTime
                    ? string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), (int)RoleClass.Doppelganger.DurationTime)
                    : string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), ((int)RoleClass.Doppelganger.Duration) + 1);
            }
            else
            {
                if (RoleClass.Doppelganger.DoppelgangerDurationText.text != "")
                {
                    RoleClass.Doppelganger.DoppelgangerDurationText.text = "";
                }
            }
            if (Shape && RoleClass.IsMeeting) PlayerControl.LocalPlayer.RpcRevertShapeshift(false);
        }
        public static class KillCoolSetting
        {
            public static void DoppelgangerMurderPrefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    bool TargetKill = false;
                    foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
                    {
                        if (p.Key == __instance.PlayerId && p.Value == target)
                        {
                            TargetKill = true;
                            break;
                        }
                    }

                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        if (!AmongUsClient.Instance.AmHost) return;
                        var role = __instance.GetRole();
                        var optdata = SyncSetting.OptionData.DeepCopy();
                        optdata.KillCooldown = SyncSetting.KillCoolSet(TargetKill ? RoleClass.Doppelganger.SucTime     //ﾀｰｹﾞｯﾄだったら
                                                                                  : RoleClass.Doppelganger.NotSucTime);//ﾀｰｹﾞｯﾄ以外だったら
                        if (__instance.AmOwner) PlayerControl.GameOptions = optdata;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, __instance.GetClientId());
                        writer.WriteBytesAndSize(optdata.ToBytes(5));
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                    else if (ModeHandler.IsMode(ModeId.Default) && __instance == PlayerControl.LocalPlayer)
                    {
                        var role = PlayerControl.LocalPlayer.GetRole();
                        var optdata = SyncSetting.OptionData.DeepCopy();
                        optdata.KillCooldown = SyncSetting.KillCoolSet(TargetKill ? RoleClass.Doppelganger.SucTime     //ﾀｰｹﾞｯﾄだったら
                                                                                  : RoleClass.Doppelganger.NotSucTime);//ﾀｰｹﾞｯﾄ以外だったら
                        if (PlayerControl.LocalPlayer.AmOwner) PlayerControl.GameOptions = optdata;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, PlayerControl.LocalPlayer.GetClientId());
                        writer.WriteBytesAndSize(optdata.ToBytes(5));
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }

                    SuperNewRolesPlugin.Logger.LogInfo("ドッペルゲンガーがキルしたことを感知");
                    SuperNewRolesPlugin.Logger.LogInfo($"{__instance.Data.PlayerName},{__instance.PlayerId} => {target.Data.PlayerName},{target.PlayerId}");
                    SuperNewRolesPlugin.Logger.LogInfo($"ドッペルゲンガーの結果がtrueかどうか : {TargetKill.ToString()}");
                    SuperNewRolesPlugin.Logger.LogInfo($"ドッペルゲンガーの{__instance.Data.PlayerName}のキルクールを{(TargetKill ? RoleClass.Doppelganger.SucTime : RoleClass.Doppelganger.NotSucTime)}秒に変更しました, プレイヤーのキルクール : {__instance.killTimer}");
                }
            }
            public static void DoppelgangerResetKillCool()
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                var optdata = SyncSetting.OptionData.DeepCopy();
                optdata.KillCooldown = SyncSetting.KillCoolSet(RoleClass.Doppelganger.DefaultKillCool);
                if (PlayerControl.LocalPlayer.AmOwner) PlayerControl.GameOptions = optdata;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, PlayerControl.LocalPlayer.GetClientId());
                writer.WriteBytesAndSize(optdata.ToBytes(5));
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
    }
}
