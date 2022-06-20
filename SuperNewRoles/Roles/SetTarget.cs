using System.Collections.Generic;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    public static class SetTarget
    {
        public static void ImpostorSetTarget()
        {
            List<PlayerControl> untarget = new();
            untarget.AddRange(RoleClass.SideKiller.MadKillerPlayer);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget(untargetablePlayers: untarget, onlyCrewmates: true));
        }
    }
}
