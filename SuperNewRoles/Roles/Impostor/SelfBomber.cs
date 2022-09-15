using Hazel;
using SuperNewRoles.Buttons;

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
        public static bool IsSelfBomber(PlayerControl Player)
        {
            return Player.IsRole(RoleId.SelfBomber);
        }
        public static void SelfBomb()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (GetIsBomb(PlayerControl.LocalPlayer, p,CustomOptions.SelfBomberScope.GetFloat()))
                    {

                        RPCProcedure.ByBomKillRPC(CachedPlayer.LocalPlayer.PlayerId, p.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ByBomKillRPC, SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
            RPCProcedure.BomKillRPC(CachedPlayer.LocalPlayer.PlayerId);
            MessageWriter Writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BomKillRPC, SendOption.Reliable, -1);
            Writer2.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer2);
        }
        /// <summary>
        /// playerがsourceを中心としscope内にいるか
        /// </summary>
        public static bool GetIsBomb(PlayerControl source, PlayerControl player, float scope)
        {
            var position = source.transform.position;
            var playerposition = player.transform.position;
            if ((position.x + scope >= playerposition.x) && (playerposition.x >= position.x - scope))
            {
                if ((position.y + scope >= playerposition.y) && (playerposition.y >= position.y - scope))
                {
                    if ((position.z + scope >= playerposition.z) && (playerposition.z >= position.z - scope))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}