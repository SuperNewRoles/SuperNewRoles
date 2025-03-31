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

    public override void AttachToAlls()
    {
        VentAbility = new CustomVentAbility(
            () => CanUseVent
        );
        KnowJackalAbility = new KnowOtherAbility(
            (player) => player.IsJackalTeam(),
            () => true
        );

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(VentAbility, parentAbility);
        Player.AttachAbility(KnowJackalAbility, parentAbility);
    }
    [CustomRPC]
    public static void RpcSetCanInfinite(bool canInfinite, JSidekickAbility jsidekick)
    {
        Logger.Info("RpcSetCanInfinite1: " + canInfinite);
        if (jsidekick != null)
        {
            Logger.Info("RpcSetCanInfinite2: " + canInfinite);
            jsidekick.canInfinite = canInfinite;
            var pOnParentDeathAbility = jsidekick.Player.PlayerAbilities.FirstOrDefault(x => x is PromoteOnParentDeathAbility);
            Logger.Info("RpcSetCanInfinite3: " + pOnParentDeathAbility);
            if (pOnParentDeathAbility is PromoteOnParentDeathAbility promoteOnParentDeathAbility)
            {
                Logger.Info("RpcSetCanInfinite4: " + promoteOnParentDeathAbility);
                promoteOnParentDeathAbility.OnPromoted += () =>
                {
                    Logger.Info("RpcSetCanInfinite5: " + canInfinite);
                    if (!canInfinite)
                    {
                        Logger.Info("RpcSetCanInfinite6: " + canInfinite);
                        var jackal = jsidekick.Player.PlayerAbilities.FirstOrDefault(x => x is JackalAbility);
                        if (jackal is JackalAbility jackalAbility)
                        {
                            Logger.Info("RpcSetCanInfinite7: " + jackalAbility);
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