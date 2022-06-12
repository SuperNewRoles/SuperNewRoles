
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Nekomata
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (exiled.Object.isRole(CustomRPC.RoleId.NiceNekomata) || exiled.Object.isRole(CustomRPC.RoleId.EvilNekomata))
            {
                NekomataEnd(exiled);
            }
        }
        public static void NekomataEnd(GameData.PlayerInfo exiled)
        {
                List<PlayerControl> p = new List<PlayerControl>();
                foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
                {
                    if (p1.Data.PlayerId != exiled.PlayerId && p1.isAlive() && p1.IsPlayer())
                    {
                        p.Add(p1);
                    }
                }
                NekomataProc(p);
        }
        public static void NekomataProc(List<PlayerControl> p)
        {
            var rdm = ModHelpers.GetRandomIndex(p);
            var random = p[rdm];
            random.RpcCheckExile();
            if ((random.isRole(CustomRPC.RoleId.NiceNekomata) || random.isRole(CustomRPC.RoleId.EvilNekomata)) && RoleClass.NiceNekomata.IsChain)
            {
                p.RemoveAt(rdm);
                NekomataProc(p);
            }
            Jester.WrapUp(random.Data);
        }
    }
}
