using BepInEx.IL2CPP.Utils;
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
            AmongUsClient.Instance.StartCoroutine(ResetName());
            IEnumerator ResetName()
            {
                yield return new WaitForSeconds(0.1f);
                FixedUpdate.SetNames();
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
