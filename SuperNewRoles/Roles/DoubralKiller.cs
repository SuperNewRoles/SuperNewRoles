using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class DoubralKiller
    {
        public static void FixedUpdate()
        {
            bool IsViewButtonText = false;
            if (!RoleClass.IsMeeting)
            {
                if (!RoleClass.IsMeeting)
                {
                    if (ModeHandler.isMode(ModeId.Default))
                    {
                        if (PlayerControl.LocalPlayer.isRole(RoleId.DoubralKiller) && RoleClass.DoubralKiller.IsSuicideViewL && RoleClass.DoubralKiller.IsSuicideViewR)
                        {
                            IsViewButtonText = true;
                            RoleClass.DoubralKiller.SuicideLTime -= Time.fixedDeltaTime;
                            RoleClass.DoubralKiller.SuicideRTime -= Time.fixedDeltaTime;
                            if (RoleClass.DoubralKiller.SuicideLTime <= 0)
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                writer.Write(byte.MaxValue);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.RPCMurderPlayer(PlayerControl.LocalPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                            }
                            if (RoleClass.DoubralKiller.SuicideRTime <= 0)
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                                writer.Write(byte.MaxValue);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.RPCMurderPlayer(PlayerControl.LocalPlayer.PlayerId, PlayerControl.LocalPlayer.PlayerId, byte.MaxValue);
                            }
                        }
                    }
                    else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        if (PlayerControl.LocalPlayer.isRole(RoleId.SerialKiller))
                        {
                            IsViewButtonText = true;
                        }
                        if (AmongUsClient.Instance.AmHost)
                        {
                            foreach (PlayerControl p in RoleClass.SerialKiller.SerialKillerPlayer)
                            {
                                if (p.isAlive())
                                {
                                    if (RoleClass.SerialKiller.IsSuicideViews.TryGetValue(p.PlayerId, out bool IsView) && IsView)
                                    {
                                        if (!RoleClass.SerialKiller.SuicideTimers.ContainsKey(p.PlayerId)) RoleClass.SerialKiller.SuicideTimers[p.PlayerId] = RoleClass.SerialKiller.SuicideDefaultTime;
                                        RoleClass.SerialKiller.SuicideTimers[p.PlayerId] -= Time.fixedDeltaTime;
                                        if (RoleClass.SerialKiller.SuicideTimers[p.PlayerId] <= 0)
                                        {
                                            p.RpcMurderPlayer(p);
                                        }
                                    }
                                }
                            }
                        }
                        if (PlayerControl.LocalPlayer.isRole(RoleId.SerialKiller) && RoleClass.SerialKiller.IsSuicideView)
                        {
                            RoleClass.SerialKiller.SuicideTime -= Time.fixedDeltaTime;
                        }
                    }
                }
            }
            if (IsViewButtonText && RoleClass.DoubralKiller.IsSuicideViewL && RoleClass.DoubralKiller.IsSuicideViewR && PlayerControl.LocalPlayer.isAlive())
            {
                RoleClass.DoubralKiller.SuicideKillLText.text = string.Format(ModTranslation.getString("DoubralKillerSuicideLText"), ((int)RoleClass.DoubralKiller.SuicideLTime) + 1);
                RoleClass.DoubralKiller.SuicideKillRText.text = string.Format(ModTranslation.getString("DoubralKillerSuicideRText"), ((int)RoleClass.DoubralKiller.SuicideRTime) + 1);
            }
            else
            {
                if (RoleClass.DoubralKiller.SuicideKillLText.text != "")
                {
                    RoleClass.DoubralKiller.SuicideKillLText.text = "";
                }
                if (RoleClass.DoubralKiller.SuicideKillRText.text != "")
                {
                    RoleClass.DoubralKiller.SuicideKillRText.text = "";
                }
            }
        }
        public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
        {
            if (__instance.isRole(RoleId.DoubralKiller))
            {
                if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                    RoleClass.DoubralKiller.IsSuicideViewL = true;
                    RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
                    RoleClass.DoubralKiller.IsSuicideViewR = true;
                }
                RoleClass.DoubralKiller.IsSuicideViewsL[__instance.PlayerId] = true;
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    RoleClass.DoubralKiller.SuicideTimersL[__instance.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultLTime;
                }
                RoleClass.DoubralKiller.IsSuicideViewsR[__instance.PlayerId] = true;
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    RoleClass.DoubralKiller.SuicideTimersR[__instance.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultRTime;
                }
                else if(ModeHandler.isMode(ModeId.Default))
                {
                    if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.SetKillTimerUnchecked(RoleClass.DoubralKiller.KillTime);
                        RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                        RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
                    }
                }
            }
        }
        public static void WrapUp()
        {
            if (RoleClass.DoubralKiller.IsMeetingReset)
            {
                RoleClass.DoubralKiller.SuicideLTime = RoleClass.DoubralKiller.SuicideDefaultLTime;
                foreach (PlayerControl p in RoleClass.DoubralKiller.DoubralKillerPlayer)
                {
                    if (RoleClass.DoubralKiller.SuicideTimersL.ContainsKey(p.PlayerId))
                    {
                        RoleClass.DoubralKiller.SuicideTimersL[p.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultLTime;
                    }
                }
                RoleClass.DoubralKiller.SuicideRTime = RoleClass.DoubralKiller.SuicideDefaultRTime;
                foreach (PlayerControl p in RoleClass.DoubralKiller.DoubralKillerPlayer)
                {
                    if (RoleClass.DoubralKiller.SuicideTimersR.ContainsKey(p.PlayerId))
                    {
                        RoleClass.DoubralKiller.SuicideTimersR[p.PlayerId] = RoleClass.DoubralKiller.SuicideDefaultRTime;
                    }
                }
            }
        }
    }
}
