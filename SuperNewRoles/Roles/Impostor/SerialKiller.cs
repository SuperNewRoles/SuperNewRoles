using Hazel;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles;

public static class SerialKiller
{
    public static void SHRFixedUpdate(RoleId role)
    {
        if (!RoleClass.IsMeeting)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (PlayerControl p in RoleClass.SerialKiller.SerialKillerPlayer)
                {
                    if (p.IsAlive())
                    {
                        if (RoleClass.SerialKiller.IsSuicideViews.TryGetValue(p.PlayerId, out bool IsView) && IsView)
                        {
                            if (!RoleClass.SerialKiller.SuicideTimers.ContainsKey(p.PlayerId)) RoleClass.SerialKiller.SuicideTimers[p.PlayerId] = RoleClass.SerialKiller.SuicideDefaultTime;
                            RoleClass.SerialKiller.SuicideTimers[p.PlayerId] -= Time.fixedDeltaTime;
                            if (RoleClass.SerialKiller.SuicideTimers[p.PlayerId] <= 0)
                            {
                                p.RpcMurderPlayer(p);
                                p.RpcSetFinalStatus(FinalStatus.SerialKillerSelfDeath);
                            }
                        }
                    }
                }
            }
        }
        if (role == RoleId.SerialKiller)
        {
            if (!RoleClass.IsMeeting && RoleClass.SerialKiller.IsSuicideView)
            {
                RoleClass.SerialKiller.SuicideTime -= Time.fixedDeltaTime;
                RoleClass.SerialKiller.SuicideKillText.text = string.Format(ModTranslation.GetString("SerialKillerSuicideText"), ((int)RoleClass.SerialKiller.SuicideTime) + 1);
            }
            else if (RoleClass.SerialKiller.SuicideKillText.text != "")
            {
                RoleClass.SerialKiller.SuicideKillText.text = "";
            }
        }
    }
    public static void FixedUpdate()
    {
        if (!RoleClass.IsMeeting)
        {
            if (RoleClass.SerialKiller.IsSuicideView)
            {
                RoleClass.SerialKiller.SuicideTime -= Time.fixedDeltaTime;
                if (RoleClass.SerialKiller.SuicideTime <= 0)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.PlayerId, byte.MaxValue);
                    PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.SerialKillerSelfDeath);
                }
            }
        }
        if (!RoleClass.IsMeeting && RoleClass.SerialKiller.IsSuicideView)
        {
            RoleClass.SerialKiller.SuicideKillText.text = string.Format(ModTranslation.GetString("SerialKillerSuicideText"), ((int)RoleClass.SerialKiller.SuicideTime) + 1);
        }
        else
        {
            if (RoleClass.SerialKiller.SuicideKillText.text != "")
            {
                RoleClass.SerialKiller.SuicideKillText.text = "";
            }
        }
    }
    public static void MurderPlayer(PlayerControl __instance, PlayerControl target)
    {
        if (__instance.IsRole(RoleId.SerialKiller))
        {
            if (target.IsRole(RoleId.Fox) && RoleClass.Fox.Killer.ContainsKey(__instance.PlayerId)) return;
            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                RoleClass.SerialKiller.SuicideTime = RoleClass.SerialKiller.SuicideDefaultTime;
                RoleClass.SerialKiller.IsSuicideView = true;
            }
            RoleClass.SerialKiller.IsSuicideViews[__instance.PlayerId] = true;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                RoleClass.SerialKiller.SuicideTimers[__instance.PlayerId] = RoleClass.SerialKiller.SuicideDefaultTime;
            }
            else if (ModeHandler.IsMode(ModeId.Default))
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