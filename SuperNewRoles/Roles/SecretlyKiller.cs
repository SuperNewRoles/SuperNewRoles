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
            AllResetCoolDown();
        }
        //リセクール (by:Buttons.cs)
        public static void MainResetCoolDown()
        {
            //ノーマルリセット
            HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            //シークレットリーリセット
            if (SuperNewRoles.CustomOption.CustomOptions.SecretlyKillerKillCoolTimeChange.getBool()){
                HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
                HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            }
            //RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
        }
        public static void SecretlyResetCoolDown()
        {
            //シークレットリーリセット
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            //ノーマルリセット
            if (SuperNewRoles.CustomOption.CustomOptions.SecretlyKillerKillCoolTimeChange.getBool()){
                HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
                HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            }
            //RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
        }
        public static void AllResetCoolDown()
        {
            //シークレットリーリセット
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            //ノーマルリセット
            HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.SecretlyKillerKillCoolTime;
            //RoleClass.SecretlyKiller.ButtonTimer = DateTime.Now;
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
