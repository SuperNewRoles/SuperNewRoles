using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Ability;

public record MadKillerData(
    bool hasImpostorVision,
    bool couldUseVent,
    float killCooldown
);

public class MadKillerAbility : AbilityBase
{
    private MadKillerData _data;
    public bool IsAwakened => _isAwakened;
    private bool _isAwakened;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowImpostorAbility _knowImpostorAbility;
    private EventListener<DieEventData> _dieEventListener;
    private EventListener<DisconnectEventData> _disconnectEventListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEventListener;
    public SideKillerAbility ownerAbility;
    public CustomKillButtonAbility _killButtonAbility;
    private bool _cannotBeSeenBeforePromotion;
    private float _currentKillCooldown;

    public MadKillerAbility(MadKillerData data)
    {
        _data = data;
        _isAwakened = false;
        _cannotBeSeenBeforePromotion = false;
        _currentKillCooldown = data.killCooldown;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        _nameTextUpdateEventListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == Player && ExPlayerControl.LocalPlayer.IsImpostor())
        {
            if (!_cannotBeSeenBeforePromotion || !_isAwakened) return;
            data.Player.cosmetics.nameText.color = Palette.ImpostorRed;
            if (data.Player.VoteArea != null)
                data.Player.VoteArea.NameText.color = Palette.ImpostorRed;
        }
    }
    public override void AttachToLocalPlayer()
    {
        _dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
        _disconnectEventListener = DisconnectEvent.Instance.AddListener(OnPlayerDisconnect);
        _ventAbility = new CustomVentAbility(() => _data.couldUseVent);
        _visionAbility = new ImpostorVisionAbility(() => _data.hasImpostorVision);
        _knowImpostorAbility = new KnowImpostorAbility(() => !_cannotBeSeenBeforePromotion);
        _killButtonAbility = new CustomKillButtonAbility(() => _isAwakened, () => _currentKillCooldown, () => true, isTargetable: (player) => SetTargetPatch.ValidMadkiller(player));

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(_ventAbility, parentAbility);
        Player.AttachAbility(_visionAbility, parentAbility);
        Player.AttachAbility(_knowImpostorAbility, parentAbility);
        Player.AttachAbility(_killButtonAbility, parentAbility);
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
    public void UpdateSettings(bool canUseVent, bool hasImpostorVision, bool cannotBeSeenBeforePromotion, float killCooldown)
    {
        _data = _data with { couldUseVent = canUseVent, hasImpostorVision = hasImpostorVision };
        _cannotBeSeenBeforePromotion = cannotBeSeenBeforePromotion;
        _currentKillCooldown = killCooldown;
    }
}