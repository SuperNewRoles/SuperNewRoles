using System.Linq;
using AmongUs.GameOptions;
using Rewired;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

public class JSidekickAbility : AbilityBase
{
    public bool CanUseVent { get; }

    public CustomVentAbility VentAbility { get; private set; }
    public KnowOtherAbility KnowJackalAbility { get; private set; }
    public bool canInfinite { get; set; }

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
    [CustomRPC]
    public static void RpcSetCanInfinite(bool canInfinite, ulong abilityId, ExPlayerControl player)
    {
        var jsidekick = player.GetAbility<JSidekickAbility>(abilityId);
        if (jsidekick != null)
        {
            jsidekick.canInfinite = canInfinite;
            var pOnParentDeathAbility = player.PlayerAbilities.FirstOrDefault(x => x is PromoteOnParentDeathAbility);
            if (pOnParentDeathAbility is PromoteOnParentDeathAbility promoteOnParentDeathAbility)
            {
                promoteOnParentDeathAbility.OnPromoted += () =>
                {
                    if (!canInfinite)
                    {
                        var jackal = player.PlayerAbilities.FirstOrDefault(x => x is JackalAbility);
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