using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Tuna
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (RoleClass.IsMeeting) return;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.IsRole(RoleId.Tuna) && RoleClass.Tuna.IsMeetingEnd)
                {
                    if (RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerId] == CachedPlayer.LocalPlayer.transform.position)
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
                        RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerId] = CachedPlayer.LocalPlayer.transform.position;
                    }
                }
            }
            else
            {
                foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
                {
                    if (p.IsAlive() && RoleClass.Tuna.IsMeetingEnd)
                    {
                        if (RoleClass.Tuna.Position[p.PlayerId] == p.transform.position)
                        {
                            RoleClass.Tuna.Timers[p.PlayerId] -= Time.deltaTime;
                            if (RoleClass.Tuna.Timers[p.PlayerId] <= 0)
                            {
                                p.RpcMurderPlayer(p);
                            }
                        }
                        else
                        {
                            RoleClass.Tuna.Timers[p.PlayerId] = RoleClass.Tuna.StoppingTime;
                        }
                        RoleClass.Tuna.Position[p.PlayerId] = p.transform.position;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
        static void Prefix()
        {
            RoleClass.Tuna.IsMeetingEnd = true;
        }
    }
}