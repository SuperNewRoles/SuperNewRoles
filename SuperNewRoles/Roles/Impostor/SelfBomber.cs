using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles
{
    class SelfBomber
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.SelfBomberButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
            HudManagerStartPatch.SelfBomberButton.Timer = PlayerControl.GameOptions.KillCooldown;
        }
        public static void SelfBomb()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsAlive() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (GetIsBomb(PlayerControl.LocalPlayer, p, CustomOptions.SelfBomberScope.GetFloat()))
                    {
                        CachedPlayer.LocalPlayer.PlayerControl.UncheckedMurderPlayer(p, showAnimation: false);
                        p.RpcSetFinalStatus(FinalStatus.BySelfBomberBomb);
                    }
                }
            }
            CachedPlayer.LocalPlayer.PlayerControl.UncheckedMurderPlayer(CachedPlayer.LocalPlayer, showAnimation: false);
            CachedPlayer.LocalPlayer.PlayerControl.RpcSetFinalStatus(FinalStatus.SelfBomberBomb);
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