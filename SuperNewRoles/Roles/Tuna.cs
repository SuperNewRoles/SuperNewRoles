using System;
using UnityEngine;
using HarmonyLib;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
    public class Tuna
    {
        public static void Postfix()
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl.Data.IsDead && CachedPlayer.LocalPlayer.PlayerControl.isRole(CustomRPC.RoleId.Tuna) && PlayerControl.LocalPlayer.CanMove && !RoleClass.IsMeeting)
            {
                if (RoleClass.Tuna.Timer <= 0.1f)
                {
                    if (RoleClass.Tuna.Position == CachedPlayer.LocalPlayer.PlayerControl.transform.position)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("[Tuna]Murder");
                        CachedPlayer.LocalPlayer.PlayerControl.RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl);
                        return;
                    }
                    else
                    {
                        RoleClass.Tuna.Position = CachedPlayer.LocalPlayer.PlayerControl.transform.position;
                        RoleClass.Tuna.Timer = RoleClass.Tuna.StoppingTime;
                    }
                }
                RoleClass.Tuna.Timer -= Time.deltaTime;
            }
        }
    }
}