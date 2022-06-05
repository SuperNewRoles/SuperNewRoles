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
                if (PlayerControl.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Samurai))
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
                    HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
                }
                if (!RoleClass.Samurai.UseSabo)
                {
                    HudManager.Instance.SabotageButton.gameObject.SetActive(false);
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
            HudManagerStartPatch.SamuraiButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            HudManagerStartPatch.SamuraiButton.Timer = PlayerControl.GameOptions.KillCooldown;
        }
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SamuraiButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            HudManagerStartPatch.SamuraiButton.Timer = PlayerControl.GameOptions.KillCooldown;
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
        public static void SelfBomb() {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p.isAlive() && p.PlayerId!= PlayerControl.LocalPlayer.PlayerId) {
                    if (GetIsBomb(PlayerControl.LocalPlayer, p)) {

                        CustomRPC.RPCProcedure.ByBomKillRPC(PlayerControl.LocalPlayer.PlayerId, p.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ByBomKillRPC, Hazel.SendOption.Reliable, -1);
                        Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
            CustomRPC.RPCProcedure.BomKillRPC(PlayerControl.LocalPlayer.PlayerId);
            MessageWriter Writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.BomKillRPC, Hazel.SendOption.Reliable, -1);
            Writer2.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer2);
        }
        public static bool GetIsBomb(PlayerControl source,PlayerControl player)
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
    }
}
