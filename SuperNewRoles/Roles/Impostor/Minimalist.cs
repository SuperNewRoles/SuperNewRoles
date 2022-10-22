

namespace SuperNewRoles.Roles
{
    public class Minimalist
    {
        public class MurderPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId && CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Minimalist))
                {
                    CachedPlayer.LocalPlayer.PlayerControl.SetKillTimerUnchecked(RoleClass.Minimalist.KillCoolTime);
                }
            }
        }
    }
}