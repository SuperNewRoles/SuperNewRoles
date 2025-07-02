using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.Ability;

public class JSidekickAbility : AbilityBase
{
    public bool CanUseVent { get; }

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public ImpostorVisionAbility ImpostorVisionAbility { get; private set; }
    public bool canInfinite { get; set; }

    public JSidekickAbility(bool canUseVent)
    {
        CanUseVent = canUseVent;
    }

    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(
            () => CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );
        ImpostorVisionAbility = new ImpostorVisionAbility(
            () => Jackal.JackalImpostorVision
        );

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
        Player.AttachAbility(ImpostorVisionAbility, parentAbility);
    }

    [CustomRPC]
    public void RpcSetCanInfinite(bool canInfinite)
    {
        this.canInfinite = canInfinite;
        var pOnParentDeathAbility = Player.GetAbility<PromoteOnParentDeathAbility>();
        if (pOnParentDeathAbility is not PromoteOnParentDeathAbility promoteOnParentDeathAbility)
            return;
        if (canInfinite)
            return;
        promoteOnParentDeathAbility.OnPromoted += (player) =>
        {
            var jackal = player.GetAbility<JackalAbility>();
            if (jackal != null)
                jackal.JackData.CanCreateSidekick = false;
        };
    }
}