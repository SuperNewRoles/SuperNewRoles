using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Tuna
{
    public static void HudUpdate()
    {
        if (RoleClass.IsMeeting) return;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (!RoleClass.IsFirstMeetingEnd || PlayerControl.LocalPlayer.IsDead() || !PlayerControl.LocalPlayer.IsRole(RoleId.Tuna))
                return;
            if (RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerId] == (Vector2)CachedPlayer.LocalPlayer.transform.position)
            {
                if (RoleClass.Tuna.Timer <= 0.1f)
                {
                    CachedPlayer.LocalPlayer.PlayerControl.RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl, true);
                    PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.TunaSelfDeath);
                }
                RoleClass.Tuna.Timer -= Time.deltaTime;
            }
            else
            {
                RoleClass.Tuna.Timer = RoleClass.Tuna.StoppingTime;
                RoleClass.Tuna.Position[CachedPlayer.LocalPlayer.PlayerId] = CachedPlayer.LocalPlayer.transform.position;
            }
        }
        else
        {
            foreach (PlayerControl p in RoleClass.Tuna.TunaPlayer)
            {
                if (!RoleClass.IsFirstMeetingEnd || p.IsDead() || MapCustoms.AirShipRandomSpawn.IsLoading)
                    continue;
                if (RoleClass.Tuna.Position[p.PlayerId] == (Vector2)p.transform.position)
                {
                    RoleClass.Tuna.Timers[p.PlayerId] -= Time.deltaTime;
                    if (RoleClass.Tuna.Timers[p.PlayerId] <= 0)
                    {
                        p.RpcMurderPlayer(p, true);
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