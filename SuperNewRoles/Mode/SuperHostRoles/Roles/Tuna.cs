using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Tuna
    {
        public static void Postfix()
        {
            foreach (PlayerControl TunaPlayer in RoleClass.Tuna.TunaPlayer)
            {
                if (!TunaPlayer.Data.IsDead && TunaPlayer.CanMove && !RoleClass.IsMeeting && Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.Tuna.Position[TunaPlayer.PlayerId] == TunaPlayer.transform.position)
                    {
                        if (Timer[TunaPlayer.PlayerId] <= 0.1f)
                        {
                            CachedPlayer.LocalPlayer.PlayerControl.RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl);
                        }
                        Timer[TunaPlayer.PlayerId] -= Time.deltaTime;
                    }
                    else
                    {
                        Timer[TunaPlayer.PlayerId] = RoleClass.Tuna.StoppingTime;
                        RoleClass.Tuna.Position[TunaPlayer.PlayerId] = CachedPlayer.LocalPlayer.PlayerControl.transform.position;
                    }
                }
            }
        }
        public static Dictionary<byte, float> Timer;
    }
}