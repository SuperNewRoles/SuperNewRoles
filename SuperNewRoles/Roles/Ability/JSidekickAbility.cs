using AmongUs.GameOptions;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class JSidekickAbility : AbilityBase
{
    public bool CanUseVent { get; }

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }

    public JSidekickAbility(bool canUseVent)
    {
        CanUseVent = canUseVent;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        VentAbility = new CustomVentAbility(
            () => CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );
        ExPlayerControl exPlayer = (ExPlayerControl)player;

        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(VentAbility, parentAbility);
        base.Attach(player, abilityId, parent);
    }

    public override void AttachToLocalPlayer()
    {
    }
}