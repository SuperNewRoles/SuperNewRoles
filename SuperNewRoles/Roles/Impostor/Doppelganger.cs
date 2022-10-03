using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor
{
    internal class Doppelganger
    {
        public static void DoppelgangerShape()
        {
            bool isShapeshift = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
            {
                if (p.Key == PlayerControl.LocalPlayer.PlayerId)
                {
                    isShapeshift = true;
                    break;
                }
            }
            if (!isShapeshift)
            {
                float nowKillCool = PlayerControl.LocalPlayer.killTimer;
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                PlayerControl.LocalPlayer.SetKillTimerUnchecked(nowKillCool);
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
            bool shape = false;
            foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
            {
                if (p.Key == PlayerControl.LocalPlayer.PlayerId && p.Value != PlayerControl.LocalPlayer)
                {
                    shape = true;
                    break;
                }
            }
            if (!RoleClass.IsMeeting && shape)
            {
                RoleClass.Doppelganger.Duration -= Time.fixedDeltaTime;
                if (RoleClass.Doppelganger.Duration <= 0) DoppelgangerShape();
                RoleClass.Doppelganger.DoppelgangerDurationText.text = RoleClass.Doppelganger.Duration > RoleClass.Doppelganger.DurationTime
                    ? string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), (int)RoleClass.Doppelganger.DurationTime)
                    : string.Format(ModTranslation.GetString("DoppelgangerDurationTimerText"), ((int)RoleClass.Doppelganger.Duration) + 1);
            }
            else if (RoleClass.Doppelganger.DoppelgangerDurationText.text != "")
            {
                RoleClass.Doppelganger.DoppelgangerDurationText.text = "";
            }
            if (shape && RoleClass.IsMeeting) PlayerControl.LocalPlayer.RpcRevertShapeshift(false);
        }
        public class KillCoolSetting
        {
           
            public static void MurderPrefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (ModeHandler.IsMode(ModeId.Default) && !__instance.AmOwner) return;
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !AmongUsClient.Instance.AmHost) return;
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    bool targetKill = false;
                    foreach (KeyValuePair<byte, PlayerControl> p in RoleClass.Doppelganger.DoppelgangerTargets)
                    {
                        if (p.Key == __instance.PlayerId && p.Value == target)
                        {
                            targetKill = true;
                            break;
                        }
                    }
                    SuperNewRolesPlugin.Logger.LogInfo($"ドッペルゲンガーがキルしたことを感知, ターゲット : {RoleClass.Doppelganger.DoppelgangerTargets[__instance.PlayerId].Data.PlayerName}");
                    SuperNewRolesPlugin.Logger.LogInfo($"{__instance.Data.PlayerName},{__instance.PlayerId} => {target.Data.PlayerName},{target.PlayerId}");
                    SuperNewRolesPlugin.Logger.LogInfo($"ドッペルゲンガーの{__instance.Data.PlayerName}のキルクールを{(targetKill ? RoleClass.Doppelganger.SucTime : RoleClass.Doppelganger.NotSucTime)}秒に変更しました");
                    var optdata = SyncSetting.OptionData.DeepCopy();
                    optdata.killCooldown = SyncSetting.KillCoolSet(targetKill ? RoleClass.Doppelganger.SucTime     //ﾀｰｹﾞｯﾄだったら
                                                                              : RoleClass.Doppelganger.NotSucTime);//ﾀｰｹﾞｯﾄ以外だったら
                    PlayerControl.GameOptions = optdata;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, __instance.GetClientId());
                    writer.WriteBytesAndSize(optdata.ToBytes(5));
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
            public static void ResetKillCool(PlayerControl __instance)
            {
                if (ModeHandler.IsMode(ModeId.Default) && !__instance.AmOwner) return;
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !AmongUsClient.Instance.AmHost) return;
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    var optdata = SyncSetting.OptionData.DeepCopy();
                    optdata.killCooldown = SyncSetting.KillCoolSet(RoleClass.Doppelganger.DefaultKillCool);
                    PlayerControl.GameOptions = optdata;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, __instance.GetClientId());
                    writer.WriteBytesAndSize(optdata.ToBytes(5));
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
        }
    }
}