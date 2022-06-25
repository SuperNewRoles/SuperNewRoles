using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomRPC;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    public static class SecretlyKiller
    {
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        //リセクール (by:Buttons.cs)
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SecretlyKillerButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
        }

        //シークレットキル (by:Buttons.cs)
        public static void SecretlyKill()
        {
            RoleClass.SecretlyKiller.target.RpcMurderPlayer(RoleClass.SecretlyKiller.target);
            RoleClass.SecretlyKiller.target = null;
        }
        //ボタンセット (by:.cs)
        public static void SecretlyKillButtonSet()
        {

        }
    }
}
