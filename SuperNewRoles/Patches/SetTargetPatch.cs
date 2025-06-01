using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using System.Linq;

namespace SuperNewRoles.Patches;

public static class SetTargetPatch
{
    public static void Register()
    {
        FixedUpdateEvent.Instance.AddListener(ImpostorSetTarget);
    }
    private static void ImpostorSetTarget()
    {
        if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled)
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(SetImpostorTarget());
    }
    /// <summary>
    /// MadKillerのターゲット設定を行う
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <returns>ターゲット設定が可能かどうか</returns>
    public static bool ValidMadkiller(ExPlayerControl player)
    {
        if (player.IsImpostor())
            return false;
        if (player.Role == Roles.RoleId.MadKiller)
            return SideKiller.CannotSeeMadKillerBeforePromotion;
        return true;
    }
    public static ExPlayerControl SetImpostorTarget()
    {
        return TargetCustomButtonBase.SetTarget(onlyCrewmates: true, isTargetable: ValidMadkiller);
    }
}