using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles;

public class Samurai
{
    public static void ResetCooldown()
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
                if (SelfBomber.GetIsBomb(PlayerControl.LocalPlayer, p, CustomOptionHolder.SamuraiScope.GetFloat()))
                {
                    PlayerControl.LocalPlayer.UncheckedMurderPlayer(p, showAnimation: false);
                    p.RpcSetFinalStatus(FinalStatus.SamuraiKill);
                }
            }
        }
    }
}