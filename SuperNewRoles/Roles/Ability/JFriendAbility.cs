using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.Ability;

public class JFriendAbility : AbilityBase
{
    public bool CanUseVent { get; }
    public bool IsImpostorVision { get; }

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }

    public JFriendAbility(bool canUseVent, bool isImpostorVision)
    {
        CanUseVent = canUseVent;
        IsImpostorVision = isImpostorVision;
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
        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => IsImpostorVision
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;

        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(VentAbility, parentAbility);
        exPlayer.AttachAbility(KnowJackalAbility, parentAbility);
        exPlayer.AttachAbility(ImpostorVisionAbility, parentAbility);
        base.Attach(player, abilityId, parent);
    }

    public override void AttachToLocalPlayer()
    {
    }
}