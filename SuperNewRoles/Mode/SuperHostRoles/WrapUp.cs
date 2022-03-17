using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class WrapUpClass
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            AmongUsClient.Instance.StartCoroutine(nameof(ResetName));
            IEnumerator ResetName()
            {
                yield return new WaitForSeconds(1);
                FixedUpdate.SetNames();
            }
            Roles.BestFalseCharge.WrapUp();
            if (exiled == null) return;
            Roles.Jester.WrapUp(exiled);
            Roles.Nekomata.WrapUp(exiled);
        }
    }
}
