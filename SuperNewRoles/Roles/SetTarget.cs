using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Text;

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
