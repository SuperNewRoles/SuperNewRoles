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
    public float CoolTime; // Separate cooldown for the shapeshift ability itself

    public override Sprite Sprite => FastDestroyableSingleton<RoleManager>.Instance.GetRole(RoleTypes.Shapeshifter).buttonManager.graphic.sprite;
    public override string buttonText => ModTranslation.GetString("ShapeshiftButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => CoolTime;

    public override ShowTextType showTextType => ShowTextType.Show;
    public override string showText => isEffectActive ? string.Format(ModTranslation.GetString("ShapeshiftDurationTimerText"), (int)EffectTimer) : "";

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
        _shapeTarget = null;
        PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, false); // Revert shape
        ResetTimer(); // Start cooldown
    };

    public float EffectDuration => DurationTime;
    public float EffectTimer { get; set; }
    public bool effectCancellable => true; // Allow cancelling the shapeshift early

    private PlayerControl _shapeTarget;
    public PlayerControl ShapeTarget => _shapeTarget;

    // Constructor to set duration and cooldown
    public ShapeshiftButtonAbility(float durationTime, float coolTime)
    {
        DurationTime = durationTime;
        CoolTime = coolTime;
    }

    public override void OnClick()
    {
        RoleTypes baseRole = ExPlayerControl.LocalPlayer.Data.Role.Role;
        RoleManager.Instance.SetRole(Player, RoleTypes.Shapeshifter);
        ExPlayerControl.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>()?.UseAbility();
        RoleManager.Instance.SetRole(Player, baseRole);
    }

    public override bool CheckIsAvailable()
    {
        if (!ExPlayerControl.LocalPlayer.IsAlive()) return false;
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        // Additional checks might be needed, e.g., if already shapeshifted
        return true;
    }

    public override bool CheckHasButton()
    {
        // Check if the player is alive and potentially if they are the correct role (e.g., Impostor)
        return ExPlayerControl.LocalPlayer.IsAlive();
    }

    private void OnShapeshift(ShapeshiftEventData data)
    {
        // Only react if this player is the one shapeshifting and not reverting
        if (data.shapeshifter != Player || data.shapeshifter == data.target) return;

        _shapeTarget = data.target;
        isEffectActive = true;
        EffectTimer = DurationTime;
        // Kill cooldown might be set by ShapeshifterRole itself, or needs adjustment here
    }

    // Called when the shapeshift effect naturally ends or is cancelled
    private void HandleShapeRevert()
    {
        if (!isEffectActive) return;
        isEffectActive = false;
        _shapeTarget = null;
        // RpcShapeshift is likely handled by OnEffectEnds action
        ResetTimer(); // Ensure cooldown starts
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // Ensure player is reverted at the end of the round
        if (isEffectActive)
        {
            isEffectActive = false;
            _shapeTarget = null;
            EffectTimer = 0;
            PlayerControl.LocalPlayer.RpcShapeshiftModded(ExPlayerControl.LocalPlayer, false);
        }
        ResetTimer(); // Reset cooldown for next round
    }

    private EventListener<ShapeshiftEventData> _shapeshiftEvent;
    private EventListener<WrapUpEventData> _wrapUpEvent;

    public void AttachToLocalPlayer(PlayerControl player)
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

