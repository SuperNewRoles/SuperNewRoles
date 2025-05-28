using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Modifiers;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CreateLoversAbility : TargetCustomButtonBase
{
    public float CoolTime { get; }
    public string _buttonText { get; }
    public Sprite _sprite { get; }
    public bool _isLoversMe { get; }
    public Action<List<ExPlayerControl>> _callback { get; }
    public override float DefaultTimer => CoolTime;

    public override string buttonText => _buttonText;

    public override Sprite Sprite => _sprite;

    protected override KeyType keytype => KeyType.Ability1;

    public ExPlayerControl CurrentTarget { get; private set; }

    public override Color32 OutlineColor => Lovers.Instance.RoleColor;

    public bool _enabledTimeLimit { get; }
    public float _timeLimit { get; }

    private bool _created = false;
    private EventListener _fixedUpdateListener;
    public override bool OnlyCrewmates => false;
    private float _currentTimer = 0f;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => !player.IsLovers() && CurrentTarget != player;
    public override ShowTextType showTextType => _enabledTimeLimit ? ShowTextType.Show : ShowTextType.Hidden;
    public override string showText => _enabledTimeLimit ? ModTranslation.GetString("DurationTimerText", (int)(_timeLimit - _currentTimer) + 1) : string.Empty;

    public LoversCouple CreatedCouple { get; private set; }

    public CreateLoversAbility(float coolTime, string buttonText, Sprite sprite, bool IsLoversMe, Action<List<ExPlayerControl>> callback = null, bool enabledTimeLimit = false, float timeLimit = 0f)
    {
        CoolTime = coolTime;
        _buttonText = buttonText;
        _sprite = sprite;
        _isLoversMe = IsLoversMe;
        _callback = callback;
        _enabledTimeLimit = enabledTimeLimit;
        _timeLimit = timeLimit;
    }

    public override bool CheckIsAvailable()
    {
        return Player.IsAlive() && Target != null;
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && !_created;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        if (_isLoversMe)
            CurrentTarget = Player;
        else
            CurrentTarget = null;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (ModHelpers.Not(_enabledTimeLimit && !_created)) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null || ExileController.Instance != null) return;
        _currentTimer += Time.deltaTime;
        if (_currentTimer >= _timeLimit)
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        }
    }

    public override void OnClick()
    {
        if (Target.HasAbility<LoversBreakerAbility>())
            return;
        if (CurrentTarget == null || CurrentTarget.IsDead() || CurrentTarget.IsLovers())
        {
            CurrentTarget = Target;
            NameText.UpdateNameInfo(CurrentTarget);
        }
        else
        {
            RpcCustomCreateLovers(CurrentTarget, Target);
            _callback?.Invoke([CurrentTarget, Target]);
        }
    }
    [CustomRPC]
    public void RpcCustomCreateLovers(ExPlayerControl playerA, ExPlayerControl playerB)
    {
        CreatedCouple = AssignRoles.CustomSetLovers(playerA, playerB, AssignRoles.LoversIndex, true);
        _created = true;
        NameText.UpdateNameInfo(playerA);
        NameText.UpdateNameInfo(playerB);
    }
}