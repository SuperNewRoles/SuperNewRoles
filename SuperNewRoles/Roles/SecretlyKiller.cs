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
            //シークレットリーリセット
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            //ノーマルリセット
            if (SuperNewRoles.CustomOption.CustomOptions.SecretlyKillerKillCoolTimeChange.getBool()){
                HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
                HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            }
            RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
        }

        //シークレットキル (by:Buttons.cs)
        public static void SecretlyKill()
        {
            RoleClass.SecretlyKiller.target.RpcMurderPlayer(RoleClass.SecretlyKiller.target);
            RoleClass.SecretlyKiller.target = null;
            //MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
            //killWriter.Write();
            //AmongUsClient.Instance.FinishRpcImmediately(killWriter);
        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
    }
}
