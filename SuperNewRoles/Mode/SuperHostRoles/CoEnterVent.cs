using BepInEx.IL2CPP.Utils;
using Hazel;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class CoEnterVent
    {
        public static void Prefix(PlayerPhysics __instance, int id)
        {
            if (!RoleClass.Minimalist.UseVent && __instance.myPlayer.isRole(CustomRPC.RoleId.Minimalist))
            {
                MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1);
                val.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(val);
                AmongUsClient.Instance.StartCoroutine(Vent());
                IEnumerator Vent()
                {
                    yield return new WaitForSeconds(0.5f);
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter val2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1, clientId);
                    val2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(val2);
                }
            } else if (!RoleClass.Egoist.UseVent && __instance.myPlayer.isRole(CustomRPC.RoleId.Egoist)) {
                MessageWriter val = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1);
                val.WritePacked(127);
                AmongUsClient.Instance.FinishRpcImmediately(val);
                AmongUsClient.Instance.StartCoroutine(Vent());
                IEnumerator Vent()
                {
                    yield return new WaitForSeconds(0.5f);
                    int clientId = __instance.myPlayer.getClientId();
                    MessageWriter val2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, (SendOption)1, clientId);
                    val2.Write(id);
                    AmongUsClient.Instance.FinishRpcImmediately(val2);
                }
            }
        }
    }
}
