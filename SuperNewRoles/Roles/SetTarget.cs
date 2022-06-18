using SuperNewRoles.Patches;
using System.Collections.Generic;

namespace SuperNewRoles.Roles
{
    public static class SetTarget
    {
        public static void ImpostorSetTarget()
        {
            List<PlayerControl> untarget = new List<PlayerControl>();
            untarget.AddRange(RoleClass.SideKiller.MadKillerPlayer);
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget(untargetablePlayers: untarget, onlyCrewmates:true));
        }
    }
}