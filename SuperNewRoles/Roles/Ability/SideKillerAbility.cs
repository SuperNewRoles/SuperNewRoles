using System;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record SideKillerData(
    float killCooldown,
    float madKillerKillCooldown
);

public class SideKillerAbility : AbilityBase
{
    private readonly SideKillerData _data;
    private CustomKillButtonAbility _killAbility;
    private CustomSidekickButtonAbility _sidekickAbility;
    private KnowOtherAbility _knowOtherAbility;
    public MadKillerAbility madKillerAbility;

    public SideKillerAbility(SideKillerData data)
    {
        _data = data;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _killAbility = new CustomKillButtonAbility(
            () => true,
            () => _data.killCooldown,
            () => false
        );

        _sidekickAbility = new CustomSidekickButtonAbility(
            (bool sidekickCreated) => !sidekickCreated,
            () => _data.killCooldown,
            () => RoleId.MadKiller,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("JackalSidekickButton.png"),
            ModTranslation.GetString("SideKillerSidekickButtonName"),
            (player) => player.IsCrewmate(),
            sidekickedPromoteData: new(RoleId.MadKiller, RoleTypes.Impostor),
            onSidekickCreated: (player) =>
            {
                madKillerAbility = player.PlayerAbilities.FirstOrDefault(x => x is MadKillerAbility) as MadKillerAbility;
                if (madKillerAbility != null)
                {
                    RpcSetMadKillerAbility(this, madKillerAbility);
                }
            }
        );

        _knowOtherAbility = new KnowOtherAbility(
            (player) => player.Role == RoleId.MadKiller,
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(_killAbility, parentAbility);
        exPlayer.AttachAbility(_sidekickAbility, parentAbility);
        exPlayer.AttachAbility(_knowOtherAbility, parentAbility);
    }

    public override void AttachToLocalPlayer()
    {
    }

    [CustomRPC]
    public static void RpcSetMadKillerAbility(SideKillerAbility owner, MadKillerAbility madKiller)
    {
        owner.madKillerAbility = madKiller;
        madKiller.ownerAbility = owner;
    }
}