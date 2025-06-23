using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using AmongUs.GameOptions;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class CustomModifierGrantedButtonOptions
{
    /// <summary>モディファイアを付与できるか</summary>
    public Func<bool, bool> CanGrantModifier { get; }
    /// <summary>付与能力のクールダウン</summary>
    public Func<float?> Cooldown { get; }
    /// <summary>付与するモディファイアのId</summary>
    public Func<ModifierRoleId> ModifierRole { get; }
    /// <summary>ボタン画像</summary>
    public Sprite ButtonSprite { get; }
    /// <summary>能力名</summary>
    public string ButtonText { get; }
    /// <summary>能力の最大使用回数</summary>
    public Func<int> MaxCount { get; }
    /// <summary>対象に取れるクルーであるかの判定</summary>
    public Func<ExPlayerControl, bool>? IsTargetable { get; }
    /// <summary>付与が成功するかの判定</summary>
    public Func<ExPlayerControl, bool>? IsGrantingSuccessful { get; }
    /// <summary>付与後の処理(付与失敗時も実行)</summary>
    public Action<ExPlayerControl> OnGranted { get; }
    /// <summary>能力残り回数を表示するかの判定</summary>
    public Func<bool> ShowButtonLimitText { get; }
    /// <summary>2つ目以上のボタンアビリティか</summary>
    public bool IsSubButton { get; }

    /// <summary>モディファイア付与能力のオプション設定</summary>
    /// <param name="canGrantModifier">モディファイアを付与できるか</param>
    /// <param name="cooldown">付与能力のクールダウン</param>
    /// <param name="modifierRole">付与するモディファイアのId</param>
    /// <param name="buttonSprite">ボタン画像</param>
    /// <param name="buttonText">能力名</param>
    /// <param name="maxCount">能力の最大使用回数</param>
    /// <param name="isTargetable">対象に取れるクルーであるかの判定</param>
    /// <param name="isGrantingSuccessful">付与が成功するかの判定</param>
    /// <param name="onGranted">付与後の処理(付与失敗時も実行)</param>
    /// <param name="showButtonLimitText">能力残り回数を表示するかの判定(不可軽減の為, 常時非表示の場合はnullを渡す)</param>
    /// <param name="isSubButton">2つ目以上のボタンアビリティか</param>
    public CustomModifierGrantedButtonOptions(
        Func<bool, bool> canGrantModifier,
        Func<float?> cooldown,
        Func<ModifierRoleId> modifierRole,
        Sprite buttonSprite,
        string buttonText,
        Func<int>? maxCount = null,
        Func<ExPlayerControl, bool>? isTargetable = null,
        Func<ExPlayerControl, bool>? isGrantingSuccessful = null,
        Action<ExPlayerControl> onGranted = null,
        Func<bool> showButtonLimitText = null,
        bool isSubButton = false
    )
    {
        CanGrantModifier = canGrantModifier;
        Cooldown = cooldown;
        ModifierRole = modifierRole;
        ButtonSprite = buttonSprite;
        ButtonText = buttonText;
        MaxCount = maxCount ?? delegate () { return 1; };
        IsTargetable = isTargetable;
        IsGrantingSuccessful = isGrantingSuccessful;
        OnGranted = onGranted;
        ShowButtonLimitText = showButtonLimitText;
        IsSubButton = isSubButton;
    }
}

public class CustomModifierGrantedButtonAbility : TargetCustomButtonBase
{
    CustomModifierGrantedButtonOptions _options;

    private bool _granted = false;

    public override Color32 OutlineColor => new(0, 255, 255, 255);
    public override Sprite Sprite => _options.ButtonSprite;
    public override string buttonText => _options.ButtonText;
    protected override KeyType keytype => _options.IsSubButton ? KeyType.None : KeyType.Ability1; // FIXME : #1169(Dev_Fix) で仕様を変更している為、マージ後 下のコードに変更
    // protected override KeyType keytype => _options.IsSubButton ? KeyType.Ability3 : KeyType.Ability1;
    public override bool OnlyCrewmates => false;
    public override Func<ExPlayerControl, bool>? IsTargetable => _options.IsTargetable;
    public override float DefaultTimer => _options.Cooldown?.Invoke() ?? 0;
    public override ShowTextType showTextType => _options.ShowButtonLimitText == null ? ShowTextType.Hidden : !_options.ShowButtonLimitText.Invoke() ? ShowTextType.Hidden : ShowTextType.ShowWithCount;
    public override string showText => ModTranslation.GetString("RemainingText");

    public CustomModifierGrantedButtonAbility(CustomModifierGrantedButtonOptions options)
    {
        _options = options;
        Count = _options.MaxCount();
        _granted = false;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!_options.CanGrantModifier(_granted)) return;

        ExPlayerControl target = Target;

        if (_options.IsGrantingSuccessful == null || _options.IsGrantingSuccessful(target))
        {
            target.RpcCustomSetModifierRoleInGame(_options.ModifierRole());
        }

        _options.OnGranted?.Invoke(target);
        _granted = true;
        ResetTimer();

        Count--;
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!_options.CanGrantModifier(_granted)) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && _options.CanGrantModifier(_granted);
    }
}