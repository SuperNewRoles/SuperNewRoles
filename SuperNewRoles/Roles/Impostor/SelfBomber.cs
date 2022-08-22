using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
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
                    if (GetIsBomb(PlayerControl.LocalPlayer, p))
                    {

                        RPCProcedure.ByBomKillRPC(CachedPlayer.LocalPlayer.PlayerId, p.PlayerId);

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ByBomKillRPC, SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
            RPCProcedure.BomKillRPC(CachedPlayer.LocalPlayer.PlayerId);
            MessageWriter Writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.BomKillRPC, SendOption.Reliable, -1);
            Writer2.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer2);
        }
        public static bool GetIsBomb(PlayerControl source, PlayerControl player)
        {
            Vector3 position = source.transform.position;
            Vector3 playerposition = player.transform.position;
            var r = CustomOption.CustomOptions.SelfBomberScope.GetFloat();
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