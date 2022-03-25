
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MurderPlayer
    {
        public static void Postfix(PlayerControl __instance,PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AmongUsClient.Instance.StartCoroutine(nameof(ResetName));
            IEnumerator ResetName()
            {
                yield return new WaitForSeconds(0.1f);
                FixedUpdate.SetDefaultNames();
            }
            if (target.isRole(RoleId.Sheriff) || target.isRole(RoleId.truelover))
            {
                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
            }
            if (RoleClass.Lovers.SameDie && target.IsLovers())
            {
                PlayerControl Side = target.GetOneSideLovers();
                if (Side.isAlive())
                {
                    Side.RpcMurderPlayer(Side);
                }
            }
            Roles.Bait.MurderPostfix(__instance,target);
        }
    }
}
