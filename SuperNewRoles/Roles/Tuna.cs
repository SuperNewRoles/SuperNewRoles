using System;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Tuna
    {
        public static void Postfix()
        {
            if (RoleClass.IsMeeting) return;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (!CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead && CachedPlayer.LocalPlayer.PlayerControl.isRole(CustomRPC.RoleId.Tuna) 　 && Mode.ModeHandler.isMode(Mode.ModeId.Default)&& RoleClass.Tuna.IsMeetingEnd)
                {
                    if (RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerControl.PlayerId] == CachedPlayer.LocalPlayer.PlayerControl.transform.position)
                    {
                        if (RoleClass.Tuna.Timer <= 0.1f)
                        {
                            CachedPlayer.LocalPlayer.PlayerControl.RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl);
                        }
                        RoleClass.Tuna.Timer -= Time.deltaTime;
                    }
                    else
                    {
                        RoleClass.Tuna.Timer = RoleClass.Tuna.StoppingTime;
                        RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerControl.PlayerId] = CachedPlayer.LocalPlayer.PlayerControl.transform.position;
                    }
                }
            }
            else
            {
                foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
                {
                    if (p.isAlive() && RoleClass.Tuna.IsMeetingEnd)
                    {
                        if (RoleClass.Tuna.Position[p.PlayerId] == p.transform.position)
                        {
                            RoleClass.Tuna.Timers[p.PlayerId] -= Time.deltaTime;
                            if (RoleClass.Tuna.Timers[p.PlayerId] <= 0)
                            {
                                p.RpcMurderPlayer(p);
                            }
                        }
                        RoleClass.Tuna.Position[p.PlayerId] = p.transform.position;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    public class MeetingEnd
    {
        static void Prefix(MeetingHud __instance)
        {
            SuperNewRolesPlugin.Logger.LogInfo("----会議終了----");
            if (AmongUsClient.Instance.AmHost && !RoleClass.Tuna.IsMeetingEnd)
            {
                RoleClass.Tuna.IsMeetingEnd = true;
            }
        }
    }
}
