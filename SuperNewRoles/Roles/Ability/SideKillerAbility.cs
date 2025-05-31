using System;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record SideKillerData(
    float killCooldown,
    float madKillerKillCooldown,
    bool madKillerCanUseVent,
    bool madKillerHasImpostorVision,
    bool cannotSeeMadKillerBeforePromotion
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

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        _killAbility = new CustomKillButtonAbility(
            () => true,
            () => _data.killCooldown,
            () => true,
            isTargetable: (player) => SetTargetPatch.ValidMadkiller(player)
        );

        _sidekickAbility = new CustomSidekickButtonAbility(new(
            (bool sidekickCreated) => !sidekickCreated,
            () => _data.killCooldown,
            () => RoleId.MadKiller,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("SideKillerSidekickButton.png"),
            ModTranslation.GetString("SideKillerSidekickButtonName"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsImpostor(),
            sidekickedPromoteData: new(RoleId.MadKiller, RoleTypes.Impostor),
            onSidekickCreated: (player) =>
            {
                madKillerAbility = player.PlayerAbilities.FirstOrDefault(x => x is MadKillerAbility) as MadKillerAbility;
                if (madKillerAbility != null)
                {
                    RpcSetMadKillerAbility(this, madKillerAbility);
                }
            }
        )
        );

        _knowOtherAbility = new KnowOtherAbility(
            (player) => player.Role == RoleId.MadKiller && !_data.cannotSeeMadKillerBeforePromotion,
            () => true
        );

        Player.AttachAbility(_killAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_sidekickAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_knowOtherAbility, new AbilityParentAbility(this));

        if (Player.AmOwner)
        {
            // クールタイムの同期を実装
            if (_killAbility != null)
            {
                _killAbility.OnCooldownStarted += SyncCooldowns;
            }
            if (_sidekickAbility != null)
            {
                _sidekickAbility.OnCooldownStarted += SyncCooldowns;
            }
        }
    }

    public override void AttachToLocalPlayer()
    {
    }

    private void SyncCooldowns(float cooldown)
    {
        // サイドキラーのキルクールタイムとサイドキックボタンのクールタイムを同期
        if (_killAbility != null && _sidekickAbility != null)
        {
            _killAbility.ResetTimer();
            _sidekickAbility.ResetTimer();
        }
    }

    [CustomRPC]
    public static void RpcSetMadKillerAbility(SideKillerAbility owner, MadKillerAbility madKiller)
    {
        owner.madKillerAbility = madKiller;
        madKiller.ownerAbility = owner;
    }
}