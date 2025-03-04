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
    private MadKillerData _data;
    private bool _isAwakened;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowImpostorAbility _knowImpostorAbility;
    private EventListener<DieEventData> _dieEventListener;
    private EventListener<DisconnectEventData> _disconnectEventListener;
    public SideKillerAbility ownerAbility;
    private bool _cannotBeSeenBeforePromotion;

    public MadKillerAbility(MadKillerData data)
    {
        _data = data;
        _isAwakened = false;
        _cannotBeSeenBeforePromotion = false;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _ventAbility = new CustomVentAbility(() => _data.couldUseVent || _isAwakened);
        _visionAbility = new ImpostorVisionAbility(() => _data.hasImpostorVision);
        _knowImpostorAbility = new KnowImpostorAbility(() => !_cannotBeSeenBeforePromotion);

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

    // 新しい設定を適用するメソッド
    public void UpdateSettings(bool canUseVent, bool hasImpostorVision, bool cannotBeSeenBeforePromotion)
    {
        _data = _data with { couldUseVent = canUseVent, hasImpostorVision = hasImpostorVision };
        _cannotBeSeenBeforePromotion = cannotBeSeenBeforePromotion;
    }

    // 他のプレイヤーから見えるかどうかを判定するメソッド
    public bool IsVisibleTo(PlayerControl viewer)
    {
        if (!_cannotBeSeenBeforePromotion)
            return true;

        if (_isAwakened)
            return true;

        // サイドキラー自身からは見える
        if (ownerAbility?.Player != null && viewer.PlayerId == ownerAbility.Player.PlayerId)
            return true;

        // 自分自身からは見える
        if (viewer.PlayerId == Player.PlayerId)
            return true;

        return false;
    }
}