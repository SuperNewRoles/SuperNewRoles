using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    public static class SetTarget
    {
        public static void ImpostorSetTarget()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.Kunoichi))
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControl.LocalPlayer);
                return;
            }
            List<PlayerControl> untarget = new();
            untarget.AddRange(RoleClass.SideKiller.MadKillerPlayer);
            untarget.AddRange(RoleClass.Spy.SpyPlayer);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget(untargetablePlayers: untarget, onlyCrewmates: true));
        }
    }
}
