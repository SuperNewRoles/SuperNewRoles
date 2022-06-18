using System;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class Tuna
    {
        public static void Postfix()
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead && CachedPlayer.LocalPlayer.PlayerControl.isRole(CustomRPC.RoleId.Tuna) && PlayerControl.LocalPlayer.CanMove && !RoleClass.IsMeeting && Mode.ModeHandler.isMode(Mode.ModeId.Default))
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
    }
}
