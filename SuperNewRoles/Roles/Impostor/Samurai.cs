using Hazel;
using SuperNewRoles.Buttons;

using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Samurai
    {
        //自爆魔関連
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SamuraiButton.MaxTimer = RoleClass.Samurai.SwordCoolTime;
            HudManagerStartPatch.SamuraiButton.Timer = RoleClass.Samurai.SwordCoolTime;
        }
        public static void SamuraiKill()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (Getsword(PlayerControl.LocalPlayer, p))
                    {
                        RPCProcedure.BySamuraiKillRPC(CachedPlayer.LocalPlayer.PlayerId, p.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BySamuraiKillRPC, SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        Writer.Write(p.PlayerId);
                        RoleClass.Samurai.Sword = true;
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    }
                }
            }
        }
        public static bool Getsword(PlayerControl source, PlayerControl player)
        {
            Vector3 position = source.transform.position;
            Vector3 playerposition = player.transform.position;
            var r = CustomOptions.SamuraiScope.GetFloat();
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