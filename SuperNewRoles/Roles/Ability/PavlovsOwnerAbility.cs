using System;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record PavlovsOwnerData(
    float sidekickCooldown,
    int maxSidekickCount,
    bool suicideOnImpostorSidekick
);

public class PavlovsOwnerAbility : AbilityBase
{
    private readonly PavlovsOwnerData _data;
    private int _sidekickCount;
    private CustomSidekickButtonAbility _sidekickAbility;
    private KnowOtherAbility _knowOtherAbility;
    private PavlovsDogAbility dogAbility;

    public PavlovsOwnerAbility(PavlovsOwnerData data)
    {
        _data = data;
        _sidekickCount = 0;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _sidekickAbility = CreateSidekickAbility(player);
        _knowOtherAbility = new KnowOtherAbility(
            p => p.IsPavlovsTeam(),
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(_sidekickAbility, parentAbility);
        exPlayer.AttachAbility(_knowOtherAbility, parentAbility);
    }

    private CustomSidekickButtonAbility CreateSidekickAbility(PlayerControl player)
    {
        return new CustomSidekickButtonAbility(new(
            _ => CanCreateSidekick(player),
            () => _data.sidekickCooldown,
            () => RoleId.PavlovsDog,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("PavlovsownerCreatedogButton.png"),
            ModTranslation.GetString("PavlovsDogButtonText"),
            isTargetable: x => !x.IsPavlovsTeam(),
            sidekickCount: () => _data.maxSidekickCount > 0 ? _data.maxSidekickCount - _sidekickCount : 0,
            sidekickSuccess: (player) => !_data.suicideOnImpostorSidekick || !player.IsImpostor(),
            onSidekickCreated: (p) => OnSidekickCreated(player, p),
            showSidekickLimitText: () => _data.maxSidekickCount > 0
        ));
    }

    private bool CanCreateSidekick(ExPlayerControl player)
    {
        return player.IsAlive() &&
            (dogAbility == null || dogAbility.Player == null || dogAbility.Player.IsDead()) &&
            HasRemainingDogCount();
    }

    private void OnSidekickCreated(ExPlayerControl owner, ExPlayerControl sidekick)
    {
        _sidekickCount++;
        if (_data.suicideOnImpostorSidekick && sidekick.IsImpostor())
        {
            owner.RpcCustomDeath(CustomDeathType.Suicide);
        }
        else
        {
            RpcSetDogAbility(this, sidekick.GetAbility<PavlovsDogAbility>(), _sidekickCount);
        }
    }

    public override void AttachToLocalPlayer()
    {
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
    }
    [CustomRPC]
    public static void RpcSetDogAbility(PavlovsOwnerAbility owner, PavlovsDogAbility dog, int sidekickCount)
    {
        owner.dogAbility = dog;
        dog.ownerAbility = owner;
        owner._sidekickCount = sidekickCount;
    }
    public bool HasRemainingDogCount()
    {
        return _data.maxSidekickCount <= 0 || _sidekickCount < _data.maxSidekickCount;
    }
}