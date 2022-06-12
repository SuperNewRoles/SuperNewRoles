using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class SerialKiller
    {
        public static void FixedUpdate()
        {
            bool IsViewButtonText = false;
            if (!RoleClass.IsMeeting)
            {
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.SerialKiller) && RoleClass.SerialKiller.IsSuicideView)
                    {
                        IsViewButtonText = true;
                        RoleClass.SerialKiller.SuicideTime -= Time.fixedDeltaTime;
                        if (RoleClass.SerialKiller.SuicideTime <= 0)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.PlayerId, byte.MaxValue);
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
                if (IsViewButtonText && RoleClass.SerialKiller.IsSuicideView && PlayerControl.LocalPlayer.isAlive())
                {
                    RoleClass.SerialKiller.SuicideKillText.text = string.Format(ModTranslation.getString("SerialKillerSuicideText"), ((int)RoleClass.SerialKiller.SuicideTime) + 1);
                }
                else
                {
                    if (RoleClass.SerialKiller.SuicideKillText.text != "")
                    {
                        RoleClass.SerialKiller.SuicideKillText.text = "";
                    }
                }
            }
        }
        public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
        {
            if (__instance.isRole(RoleId.SerialKiller))
            {
                if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    RoleClass.SerialKiller.SuicideTime = RoleClass.SerialKiller.SuicideDefaultTime;
                    RoleClass.SerialKiller.IsSuicideView = true;
                }
                RoleClass.SerialKiller.IsSuicideViews[__instance.PlayerId] = true;
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    RoleClass.SerialKiller.SuicideTimers[__instance.PlayerId] = RoleClass.SerialKiller.SuicideDefaultTime;
                }
                else if (ModeHandler.isMode(ModeId.Default))
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        __instance.SetKillTimerUnchecked(RoleClass.SerialKiller.KillTime);
                        RoleClass.SerialKiller.SuicideTime = RoleClass.SerialKiller.SuicideDefaultTime;
                    }
                }
            }
        }
        public static void WrapUp()
        {
            if (RoleClass.SerialKiller.IsMeetingReset)
            {
                RoleClass.SerialKiller.SuicideTime = RoleClass.SerialKiller.SuicideDefaultTime;
                foreach (PlayerControl p in RoleClass.SerialKiller.SerialKillerPlayer)
                {
                    if (RoleClass.SerialKiller.SuicideTimers.ContainsKey(p.PlayerId))
                    {
                        RoleClass.SerialKiller.SuicideTimers[p.PlayerId] = RoleClass.SerialKiller.SuicideDefaultTime;
                    }
                }
            }
        }
    }
}
