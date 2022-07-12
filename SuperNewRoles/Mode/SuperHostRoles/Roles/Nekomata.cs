using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Nekomata
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (exiled.Object.isRole(RoleId.NiceNekomata) || exiled.Object.isRole(RoleId.EvilNekomata) || exiled.Object.isRole(RoleId.BlackCat))
            {
                NekomataEnd(exiled);
            }
        }
        public static void NekomataEnd(GameData.PlayerInfo exiled)
        {
            List<PlayerControl> p = new();
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
            if ((random.isRole(RoleId.NiceNekomata) || random.isRole(RoleId.EvilNekomata) || random.isRole(RoleId.BlackCat)) && RoleClass.NiceNekomata.IsChain)
            {
                p.RemoveAt(rdm);
                NekomataProc(p);
            }
            Jester.WrapUp(random.Data);
        }
    }
}
