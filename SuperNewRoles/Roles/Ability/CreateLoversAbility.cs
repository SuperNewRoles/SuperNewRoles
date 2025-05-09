using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Modifiers;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class CreateLoversAbility : TargetCustomButtonBase
{
    public float CoolTime { get; }
    public string _buttonText { get; }
    public Sprite _sprite { get; }
    public bool _isLoversMe { get; }
    public override float DefaultTimer => CoolTime;

    public override string buttonText => _buttonText;

    public override Sprite Sprite => _sprite;

    protected override KeyType keytype => KeyType.Ability1;

    private ExPlayerControl CurrentTarget { get; set; }

    public override Color32 OutlineColor => Lovers.Instance.RoleColor;

    public override bool OnlyCrewmates => false;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => !player.IsLovers();
    public CreateLoversAbility(float coolTime, string buttonText, Sprite sprite, bool IsLoversMe)
    {
        CoolTime = coolTime;
        _buttonText = buttonText;
        _sprite = sprite;
        _isLoversMe = IsLoversMe;
    }

    public override bool CheckIsAvailable()
    {
        return Player.IsAlive() && Target != null;
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && !Player.IsLovers();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        if (_isLoversMe)
            CurrentTarget = Player;
        else
            CurrentTarget = null;
    }

    public override void OnClick()
    {
        if (CurrentTarget == null || CurrentTarget.IsDead() || CurrentTarget.IsLovers())
            CurrentTarget = Target;
        else
            AssignRoles.RpcCustomSetLovers(CurrentTarget, Target, AssignRoles.LoversIndex);
    }
}