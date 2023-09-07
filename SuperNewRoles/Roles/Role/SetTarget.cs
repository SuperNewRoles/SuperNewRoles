using System.Collections.Generic;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles;

public static class SetTarget
{
    public static void ImpostorSetTarget()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Kunoichi))
        {
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControl.LocalPlayer);
            return;
        }
        else if (PlayerControl.LocalPlayer.IsRole(RoleId.Penguin) &&
            (!CustomOptionHolder.PenguinCanDefaultKill.GetBool()
            ||
            !ModeHandler.IsMode(ModeId.Default))
            && RoleClass.Penguin.currentTarget is null)
        {
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            return;
        }
        List<PlayerControl> untarget = new();
        untarget.AddRange(RoleClass.SideKiller.MadKillerPlayer);
        untarget.AddRange(RoleClass.Spy.SpyPlayer);
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Vampire)) untarget.AddRange(RoleClass.Dependents.DependentsPlayer);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.SetTarget(untargetablePlayers: untarget, onlyCrewmates: true));
    }
}