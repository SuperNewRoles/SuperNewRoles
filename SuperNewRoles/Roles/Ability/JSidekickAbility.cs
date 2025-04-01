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
    public static void RpcSetCanInfinite(bool canInfinite, JSidekickAbility jsidekick)
    {
        if (jsidekick != null)
        {
            jsidekick.canInfinite = canInfinite;
            var pOnParentDeathAbility = jsidekick.Player.PlayerAbilities.FirstOrDefault(x => x is PromoteOnParentDeathAbility);
            if (pOnParentDeathAbility is PromoteOnParentDeathAbility promoteOnParentDeathAbility)
            {
                promoteOnParentDeathAbility.OnPromoted += () =>
                {
                    if (!canInfinite)
                    {
                        var jackal = jsidekick.Player.PlayerAbilities.FirstOrDefault(x => x is JackalAbility);
                        if (jackal is JackalAbility jackalAbility)
                        {
                            jackalAbility.JackData.CanCreateSidekick = false;
                        }
                    }
                };
            }
        }
    }
    public override void AttachToLocalPlayer()
    {
    }
}