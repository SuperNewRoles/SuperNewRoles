
using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
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
                foreach (PlayerControl p1 in PlayerControl.AllPlayerControls)
                {
                    if (p1.Data.PlayerId != exiled.PlayerId && p1.isAlive())
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
            SuperNewRolesPlugin.Logger.LogInfo("ローカル死亡か:"+ PlayerControl.LocalPlayer.Data.IsDead);
            random.RpcMurderPlayer(random);
            SuperNewRolesPlugin.Logger.LogInfo("ローカル死亡か:" + PlayerControl.LocalPlayer.Data.IsDead);
            random.Exiled();
            FinalStatusPatch.FinalStatusData.FinalStatuses[random.PlayerId] = FinalStatus.NekomataExiled;
            SuperNewRolesPlugin.Logger.LogInfo("ローカル死亡か:" + PlayerControl.LocalPlayer.Data.IsDead);
            SuperNewRolesPlugin.Logger.LogInfo("猫又の道連れ:"+random.getDefaultName());
            new LateTask(() => {
                SuperNewRolesPlugin.Logger.LogInfo("ローカル死亡かLateTask:" + PlayerControl.LocalPlayer.Data.IsDead);
            }, 1f, "SetHostDead");
            if ((random.isRole(CustomRPC.RoleId.NiceNekomata) || random.isRole(CustomRPC.RoleId.EvilNekomata)) && RoleClass.NiceNekomata.IsChain)
            {
                p.RemoveAt(rdm);
                NekomataProc(p);
            }
            Jester.WrapUp(random.Data);
        }
    }
}
