using HarmonyLib;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using Hazel;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    
    public class Samurai
    {
    
        public class MurderPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Samurai))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Samurai.KillCoolTime);
                }
            }
        }
        public static void SetSamuraiButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Samurai))
            {
                if (!RoleClass.Samurai.UseVent)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Samurai.UseSabo)
                {
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);
                }
            }
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetSamuraiButton();
            }
        }
         //自爆魔関連
        public static void EndMeeting()
        {
            HudManagerStartPatch.SamuraiButton.MaxTimer = RoleClass.Samurai.SwordCoolTime;
            HudManagerStartPatch.SamuraiButton.Timer = RoleClass.Samurai.SwordCoolTime;
        }
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SamuraiButton.MaxTimer = RoleClass.Samurai.SwordCoolTime;
            HudManagerStartPatch.SamuraiButton.Timer = RoleClass.Samurai.SwordCoolTime;
        }
        public static bool isSamurai(PlayerControl Player)
        {
            if (RoleClass.Samurai.SamuraiPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void SamuraiKill() {
            foreach (PlayerControl p in CachedPlayer.AllPlayers) {
                if (p.isAlive() && p.PlayerId!= CachedPlayer.LocalPlayer.PlayerId) {
                    if (Getsword(PlayerControl.LocalPlayer, p)) {

                        CustomRPC.RPCProcedure.BySamuraiKillRPC(CachedPlayer.LocalPlayer.PlayerId, p.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.BySamuraiKillRPC, Hazel.SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        RoleClass.Samurai.Sword = true;
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        
                    }
                }
            }
        }
        public static bool Getsword(PlayerControl source,PlayerControl player)
        {
            Vector3 position = source.transform.position;
                Vector3 playerposition = player.transform.position;
            var r = CustomOption.CustomOptions.SamuraiScope.getFloat();
                if ((position.x + r >= playerposition.x) && (playerposition.x >= position.x - r))
                {
                    if ((position.y + r >= playerposition.y) && (playerposition.y >= position.y - r))
                    {
                        if ((position.z + r >= playerposition.z) && (playerposition.z >= position.z - r))
                        {
                        return true;
                        }
                    }
                }
            return false;
        }
        public static void IsSword()
        {
            RoleClass.Samurai.Sword = true;
        }
    }
}
