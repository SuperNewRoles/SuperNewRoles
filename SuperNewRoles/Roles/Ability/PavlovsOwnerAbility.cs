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
    bool canUseVent,
    bool isImpostorVision,
    float sidekickCooldown,
    int maxSidekickCount,
    bool suicideOnImpostorSidekick
);

public class PavlovsOwnerAbility : AbilityBase
{
    private readonly PavlovsOwnerData _data;
    private int _sidekickCount;
    private CustomVentAbility _ventAbility;
    private CustomSidekickButtonAbility _sidekickAbility;
    private ImpostorVisionAbility _visionAbility;
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

        _ventAbility = new CustomVentAbility(() => _data.canUseVent);
        _visionAbility = new ImpostorVisionAbility(() => _data.isImpostorVision);
        _sidekickAbility = new CustomSidekickButtonAbility(
            _ => _data.maxSidekickCount <= 0 || _sidekickCount < _data.maxSidekickCount,
            () => _data.sidekickCooldown,
            () => RoleId.PavlovsDog,
            () => RoleTypes.Crewmate,
            AssetManager.GetAsset<Sprite>("PavlovsownerCreatedogButton.png"),
            ModTranslation.GetString("PavlovsDogButtonText"),
            null,
            sidekickSuccess: (player) => (dogAbility == null || dogAbility.Player == null || ((ExPlayerControl)dogAbility.Player).IsDead()) && (!_data.suicideOnImpostorSidekick || !player.IsImpostor()),
            onSidekickCreated: (player) =>
            {
                _sidekickCount++;
                if (_data.suicideOnImpostorSidekick && player.IsImpostor())
                {
                    ExPlayerControl exPlayer = (ExPlayerControl)Player;
                    exPlayer.RpcCustomDeath(CustomDeathType.Suicide);
                }
                else
                {
                    dogAbility = player.PlayerAbilities.FirstOrDefault(x => x is PavlovsDogAbility) as PavlovsDogAbility;
                }
            }
        );
        _knowOtherAbility = new KnowOtherAbility(
            p => p.IsPavlovsTeam(),
            () => true
        );

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(_ventAbility, parentAbility);
        exPlayer.AttachAbility(_visionAbility, parentAbility);
        exPlayer.AttachAbility(_sidekickAbility, parentAbility);
        exPlayer.AttachAbility(_knowOtherAbility, parentAbility);
    }

    public override void AttachToLocalPlayer()
    {
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
    }
    [CustomRPC]
    public static void RpcSetDogAbility(PavlovsOwnerAbility owner, PavlovsDogAbility dog)
    {
        owner.dogAbility = dog;
        dog.ownerAbility = owner;
    }
}