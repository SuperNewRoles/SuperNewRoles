using System.Collections.Generic;

using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    public static class SetTarget
    {
        public static void ImpostorSetTarget()
        {
            if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.Kunoichi))
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(CachedPlayer.LocalPlayer.PlayerControl);
                return;
            }
            List<PlayerControl> untarget = new();
            untarget.AddRange(RoleClass.SideKiller.MadKillerPlayer);
            untarget.AddRange(RoleClass.Spy.SpyPlayer);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.SetTarget(untargetablePlayers: untarget, onlyCrewmates: true));
        }
    }
}