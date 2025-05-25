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
    float killCooldown,
    bool cannotBeSeenBeforePromotion,
    bool cannotSeeImpostorBeforePromotion
);

public class MadKillerAbility : AbilityBase
{
    private MadKillerData _data;
    public bool IsAwakened => _isAwakened;
    private bool _isAwakened;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _visionAbility;
    private KnowImpostorAbility _knowImpostorAbility;
    private EventListener _fixedUpdateEventListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEventListener;
    public SideKillerAbility ownerAbility;
    public CustomKillButtonAbility _killButtonAbility;
    private bool _cannotBeSeenBeforePromotion;
    private float _currentKillCooldown;

    public MadKillerAbility(MadKillerData data)
    {
        _data = data;
        _isAwakened = false;
        _cannotBeSeenBeforePromotion = data.cannotBeSeenBeforePromotion;
        _currentKillCooldown = data.killCooldown;
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == Player && ExPlayerControl.LocalPlayer.IsImpostor())
        {
            if (_cannotBeSeenBeforePromotion && !_isAwakened) return;
            NameText.SetNameTextColor(data.Player, Palette.ImpostorRed);
        }
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _nameTextUpdateEventListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _ventAbility = new CustomVentAbility(() => _data.couldUseVent);
        _visionAbility = new ImpostorVisionAbility(() => _data.hasImpostorVision);
        _knowImpostorAbility = new KnowImpostorAbility(() => !_cannotBeSeenBeforePromotion || !_data.cannotSeeImpostorBeforePromotion);
        _killButtonAbility = new CustomKillButtonAbility(() => _isAwakened, () => _currentKillCooldown, () => true, isTargetable: (player) => SetTargetPatch.ValidMadkiller(player));

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(_ventAbility, parentAbility);
        Player.AttachAbility(_visionAbility, parentAbility);
        Player.AttachAbility(_knowImpostorAbility, parentAbility);
        Player.AttachAbility(_killButtonAbility, parentAbility);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateEventListener?.RemoveListener();
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEventListener?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (_isAwakened || Player.IsDead()) return;
        if (ownerAbility?.Player == null || ownerAbility.Player.IsDead())
        {
            Awaken();
        }
    }


    private void Awaken()
    {
        _isAwakened = true;
        if (Player.IsDead()) return;
        RpcMadkillerAwaken(Player);
    }

    [CustomRPC]
    public static void RpcMadkillerAwaken(ExPlayerControl player)
    {
        if (player.IsDead()) return;
        RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
        NameText.UpdateAllNameInfo();
    }

    // 新しい設定を適用するメソッド
    public void UpdateSettings(bool canUseVent, bool hasImpostorVision, bool cannotBeSeenBeforePromotion, float killCooldown)
    {
        _data = _data with { couldUseVent = canUseVent, hasImpostorVision = hasImpostorVision };
        _cannotBeSeenBeforePromotion = cannotBeSeenBeforePromotion;
        _currentKillCooldown = killCooldown;
    }
}