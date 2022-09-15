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
                    if (SelfBomber.GetIsBomb(PlayerControl.LocalPlayer, p, CustomOptions.SamuraiScope.GetFloat()))
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
    }
}