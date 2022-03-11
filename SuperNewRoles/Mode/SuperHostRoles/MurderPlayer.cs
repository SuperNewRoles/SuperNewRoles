using BepInEx.IL2CPP.Utils;
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
            Roles.Bait.MurderPostfix(__instance,target);
        }
    }
}
