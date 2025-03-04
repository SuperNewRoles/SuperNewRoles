using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.Patches;

public static class SetTargetPatch
{
    public static void Register()
    {
        FixedUpdateEvent.Instance.AddListener(ImpostorSetTarget);
    }
    private static void ImpostorSetTarget()
    {
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(TargetCustomButtonBase.SetTarget(onlyCrewmates: true, isTargetable: (player) => ValidMadkiller(player)));
    }
    private static bool ValidMadkiller(ExPlayerControl player)
    {
        return SideKiller.CannotSeeMadKillerBeforePromotion ? player.Role != Roles.RoleId.MadKiller : true;
    }
}