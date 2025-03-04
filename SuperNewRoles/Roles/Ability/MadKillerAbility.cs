using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public record MadKillerData(
    bool hasImpostorVision,
    bool couldUseVent,
    float killCooldown
);

public class MadKillerAbility : AbilityBase
{
    private readonly MadKillerData _data;
    private bool _isAwakened;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowImpostorAbility _knowImpostorAbility;
    private EventListener<DieEventData> _dieEventListener;
    private EventListener<DisconnectEventData> _disconnectEventListener;
    public SideKillerAbility ownerAbility;

    public MadKillerAbility(MadKillerData data)
    {
        _data = data;
        _isAwakened = false;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _ventAbility = new CustomVentAbility(() => _data.couldUseVent || _isAwakened);
        _visionAbility = new ImpostorVisionAbility(() => _data.hasImpostorVision);
        _knowImpostorAbility = new KnowImpostorAbility(() => true);

        ExPlayerControl exPlayer = (ExPlayerControl)player;
        AbilityParentAbility parentAbility = new(this);
        exPlayer.AttachAbility(_ventAbility, parentAbility);
        exPlayer.AttachAbility(_visionAbility, parentAbility);
        exPlayer.AttachAbility(_knowImpostorAbility, parentAbility);
    }

    public override void AttachToLocalPlayer()
    {
        _dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
        _disconnectEventListener = DisconnectEvent.Instance.AddListener(OnPlayerDisconnect);
    }

    private void OnPlayerDead(DieEventData data)
    {
        if (ownerAbility?.Player != null && data.player.PlayerId == ownerAbility.Player.PlayerId)
        {
            Awaken();
        }
    }

    private void OnPlayerDisconnect(DisconnectEventData data)
    {
        if (ownerAbility?.Player != null && data.disconnectedPlayer.PlayerId == ownerAbility.Player.PlayerId)
        {
            Awaken();
        }
    }

    private void Awaken()
    {
        _isAwakened = true;
        RpcMadkillerAwaken(Player);
    }

    [CustomRPC]
    public static void RpcMadkillerAwaken(ExPlayerControl player)
    {
        RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
        NameText.UpdateAllNameInfo();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        DieEvent.Instance.RemoveListener(_dieEventListener);
        DisconnectEvent.Instance.RemoveListener(_disconnectEventListener);
    }
}