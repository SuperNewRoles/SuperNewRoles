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
            foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                if (p.isAlive()) {
                    if (GetIsBomb(PlayerControl.LocalPlayer, p)) {

                        CustomRPC.RPCProcedure.RPCMurderPlayer(PlayerControl.LocalPlayer.PlayerId, p.PlayerId,byte.MaxValue);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, Hazel.SendOption.Reliable, -1);
                        Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        Writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
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
