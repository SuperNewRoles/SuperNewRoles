using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class EvilGambler
    {
        public static class EvilGamblerMurder
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (__instance.isRole(CustomRPC.RoleId.EvilGambler))
                    {
                        SyncSetting.GamblersetCool(__instance);
                    }
                    return;
                }
                else if (__instance == PlayerControl.LocalPlayer && RoleClass.EvilGambler.EvilGamblerPlayer.IsCheckListPlayerControl(__instance)) {
                    if (RoleClass.EvilGambler.GetSuc()) {
                        //成功
                        PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                    } else {
                        //失敗
                        PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                    };
                }
            }
        }
    }
}
