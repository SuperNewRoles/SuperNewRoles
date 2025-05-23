using AmongUs.GameOptions;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using System;
using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.Roles.Ability;

public class ShapeshiftButtonAbility : CustomButtonBase, IButtonEffect
{
    public float DurationTime;
    public float CoolTime;

    public override Sprite Sprite => SpriteName != null ? AssetManager.GetAsset<Sprite>(SpriteName) : FastDestroyableSingleton<RoleManager>.Instance.GetRole(RoleTypes.Shapeshifter).Ability.Image;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ShapeshiftAbility);
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => CoolTime;

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
        _shapeTarget = null;
        PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true); // Revert shape
        ResetTimer(); // Start cooldown
    };

    public float EffectDuration => DurationTime;
    public float EffectTimer { get; set; }
    public bool effectCancellable => true; // Allow cancelling the shapeshift early

    private PlayerControl _shapeTarget;
    public PlayerControl ShapeTarget => _shapeTarget;
    public string SpriteName { get; }

    public ShapeshiftButtonAbility(float coolTime, float durationTime, string spriteName = null)
    {
        DurationTime = durationTime;
        CoolTime = coolTime;
        SpriteName = spriteName;
    }

    public override void OnClick()
    {
        RoleTypes baseRole = ExPlayerControl.LocalPlayer.Data.Role.Role;
        float killTimer = PlayerControl.LocalPlayer.killTimer;
        RoleManager.Instance.SetRole(Player, RoleTypes.Shapeshifter);
        ExPlayerControl.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>()?.UseAbility();
        RoleManager.Instance.SetRole(Player, baseRole);
        PlayerControl.LocalPlayer.killTimer = killTimer;
        new LateTask(() =>
        {
            isEffectActive = false;
            Timer = 0.0001f;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }, 2f / 60f, "ShapeshiftButtonAbility");
    }

    public override bool CheckIsAvailable()
    {
        if (!ExPlayerControl.LocalPlayer.IsAlive()) return false;
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive();
    }

    private void OnShapeshift(ShapeshiftEventData data)
    {
        // Only react if this player is the one shapeshifting and not reverting
        if (data.shapeshifter != Player || data.shapeshifter == data.target) return;

        _shapeTarget = data.target;
        if (!Player.AmOwner) return;
        ResetTimer();
        isEffectActive = true;
        EffectTimer = DurationTime;
        actionButton.cooldownTimerText.color = IButtonEffect.color;
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // Ensure player is reverted at the end of the round
        if (isEffectActive)
        {
            isEffectActive = false;
            _shapeTarget = null;
            EffectTimer = 0;
        }
        PlayerControl.LocalPlayer.RpcShapeshiftModded(ExPlayerControl.LocalPlayer, false);
        ResetTimer(); // Reset cooldown for next round
    }

    private EventListener<ShapeshiftEventData> _shapeshiftEvent;
    private EventListener<WrapUpEventData> _wrapUpEvent;

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _shapeshiftEvent = ShapeshiftEvent.Instance.AddListener(OnShapeshift);
        _wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _shapeshiftEvent?.RemoveListener();
        _wrapUpEvent?.RemoveListener();

        // Ensure player reverts shape if the ability is detached while active
        if (isEffectActive)
        {
            isEffectActive = false;
            _shapeTarget = null;
            PlayerControl.LocalPlayer.RpcShapeshiftModded(ExPlayerControl.LocalPlayer, false);
        }
    }
}

