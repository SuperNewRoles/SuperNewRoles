using Hazel;
using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class SelfBomber
    {
        public static void EndMeeting()
        {
            HudManagerStartPatch.SelfBomberButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            HudManagerStartPatch.SelfBomberButton.Timer = PlayerControl.GameOptions.KillCooldown;
        }
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SelfBomberButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            HudManagerStartPatch.SelfBomberButton.Timer = PlayerControl.GameOptions.KillCooldown;
        }
        public static bool isSelfBomber(PlayerControl Player)
        {
            if (RoleClass.SelfBomber.SelfBomberPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void SelfBomb() {
            foreach (PlayerControl p in CachedPlayer.AllPlayers) {
                if (p.isAlive() && p.PlayerId!= CachedPlayer.LocalPlayer.PlayerId) {
                    if (GetIsBomb(PlayerControl.LocalPlayer, p)) {

                        CustomRPC.RPCProcedure.ByBomKillRPC(CachedPlayer.LocalPlayer.PlayerId, p.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ByBomKillRPC, Hazel.SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
            CustomRPC.RPCProcedure.BomKillRPC(CachedPlayer.LocalPlayer.PlayerId);
            MessageWriter Writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.BomKillRPC, Hazel.SendOption.Reliable, -1);
            Writer2.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer2);
        }
        public static bool GetIsBomb(PlayerControl source,PlayerControl player)
        {
            Vector3 position = source.transform.position;
                Vector3 playerposition = player.transform.position;
            var r = CustomOption.CustomOptions.SelfBomberScope.getFloat();
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
