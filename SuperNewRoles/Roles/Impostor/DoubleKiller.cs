using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Ability;

namespace SuperNewRoles.Roles.Impostor;

class DoubleKiller : RoleBase<DoubleKiller>
{
    public override RoleId Role { get; } = RoleId.DoubleKiller;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DoubleKillerAbility(new(DoubleKillerKillCountRemaining ? DoubleKillerMaxKillCount : null)),
                () => new CustomSaboAbility(
            canSabotage: () => DoubleKillerCanSabotage
        ),
        () => new CustomVentAbility(
            canUseVent: () => DoubleKillerCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("DoubleKillerMainKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float DoubleKillerMainKillCooldown;

    [CustomOptionFloat("DoubleKillerSubKillCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float DoubleKillerSubKillCooldown;

    [CustomOptionBool("DoubleKillerKillCountRemaining", true)]
    public static bool DoubleKillerKillCountRemaining;

    [CustomOptionInt("DoubleKillerMaxKillCount", 1, 10, 1, 1,parentFieldName: nameof(DoubleKillerKillCountRemaining))]
    public static int DoubleKillerMaxKillCount;
    [CustomOptionBool("DoubleKillerCanUseVent", true, translationName: "CanUseVent")]
    public static bool DoubleKillerCanUseVent;

    [CustomOptionBool("DoubleKillerCanSabotage", true, translationName: "CanSabotage")]
    public static bool DoubleKillerCanSabotage;
}

public record DoubleKillerAbilityData(int? DoubleKillerCount);

// 独立したキルクールを持つキルボタンクラス
public class IndependentKillButtonAbility : TargetCustomButtonBase
{
    public Func<bool> CanKill { get; }
    public Func<float?> KillCooldown { get; }
    public Func<bool> OnlyCrewmatesValue { get; }
    public Func<bool> TargetPlayersInVentsValue { get; }
    public Func<ExPlayerControl, bool> IsTargetableValue { get; }
    public Action<ExPlayerControl> KilledCallback { get; }
    public Action<float> OnCooldownStarted;
    public Func<ShowTextType> ShowTextTypeValue { get; }
    public Func<string> ShowTextValue { get; }
    private Func<ExPlayerControl, bool> _customKillHandler { get; }
    private readonly KeyType _keyType;
    private EventListener<MurderEventData> _murderEventListener;
    private byte _pendingTargetId = byte.MaxValue;

    public override Color32 OutlineColor => ExPlayerControl.LocalPlayer.roleBase.RoleColor;
    public override Sprite Sprite => HudManager.Instance?.KillButton?.graphic?.sprite;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel);
    protected override KeyType keytype => _keyType;
    public override float DefaultTimer => KillCooldown?.Invoke() ?? 0;
    public override bool OnlyCrewmates => OnlyCrewmatesValue?.Invoke() ?? false;
    public override bool TargetPlayersInVents => TargetPlayersInVentsValue?.Invoke() ?? false;
    public override Func<ExPlayerControl, bool>? IsTargetable => IsTargetableValue;
    public override ShowTextType showTextType => ShowTextTypeValue?.Invoke() ?? ShowTextType.Hidden;
    public override string showText => ShowTextValue?.Invoke() ?? "";

    public IndependentKillButtonAbility(
        Func<bool> canKill,
        Func<float?> killCooldown,
        Func<bool> onlyCrewmates,
        Func<bool> targetPlayersInVents = null,
        Func<ExPlayerControl, bool> isTargetable = null,
        Action<ExPlayerControl> killedCallback = null,
        Func<ShowTextType> showTextType = null,
        Func<string> showText = null,
        Func<ExPlayerControl, bool> customKillHandler = null,
        KeyType keyType = KeyType.Kill)
    {
        CanKill = canKill;
        KillCooldown = killCooldown;
        OnlyCrewmatesValue = onlyCrewmates;
        TargetPlayersInVentsValue = targetPlayersInVents;
        IsTargetableValue = isTargetable;
        KilledCallback = killedCallback;
        ShowTextTypeValue = showTextType;
        ShowTextValue = showText;
        _customKillHandler = customKillHandler;
        _keyType = keyType;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _murderEventListener = MurderEvent.Instance.AddListener(OnMurder);
    }

    public override void DetachToAlls()
    {
        _murderEventListener?.RemoveListener();
        _murderEventListener = null;
        _pendingTargetId = byte.MaxValue;
        base.DetachToAlls();
    }

    private void OnMurder(MurderEventData data)
    {
        if (Player == null || !Player.AmOwner || _pendingTargetId == byte.MaxValue)
            return;
        if (data.killer?.PlayerId != Player.PlayerId || data.target?.PlayerId != _pendingTargetId)
            return;

        _pendingTargetId = byte.MaxValue;
        if (data.resultFlags.HasFlag(MurderResultFlags.Succeeded))
            KilledCallback?.Invoke(data.target);
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!CanKill()) return;
        PlayerControl target = Target;
        bool customKilled = _customKillHandler?.Invoke(target) ?? false;
        if (!customKilled)
        {
            byte targetId = ((ExPlayerControl)target).PlayerId;
            _pendingTargetId = targetId;
            ExPlayerControl.LocalPlayer.RpcCustomDeath(target, CustomDeathType.Kill);
            if (_pendingTargetId == targetId)
                _pendingTargetId = byte.MaxValue;
        }
        ResetTimer();
        OnCooldownStarted?.Invoke(DefaultTimer);
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!CanKill()) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && CanKill();
    }
}

public class DoubleKillerAbility : AbilityBase, IAbilityCount
{
    public DoubleKillerAbilityData DoubleKillerAbilityData { get; set; }

    public DoubleKillerAbility(DoubleKillerAbilityData doubleKillerAbilityData)
    {
        DoubleKillerAbilityData = doubleKillerAbilityData;
        // 初期カウントを設定
        if (DoubleKillerAbilityData.DoubleKillerCount.HasValue)
        {
            Count = DoubleKillerAbilityData.DoubleKillerCount.Value;
        }
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        // バニラキルボタンを無効化するためのCustomKillButtonAbilityを追加
        Player.AttachAbility(new CustomKillButtonAbility(
            () => false, // 常に無効化
            () => 0f,
            onlyCrewmates: () => true
        ), new AbilityParentAbility(this));

        // メインキルボタン（独立したクールダウン）
        Player.AttachAbility(new IndependentKillButtonAbility(
            () => true,
            () => DoubleKiller.DoubleKillerMainKillCooldown,
            onlyCrewmates: () => true,
            showTextType: () => ShowTextType.Hidden, // テキストを非表示に変更
            showText: () => ""
        ), new AbilityParentAbility(this));

        // サブキルボタン（独立したクールダウン）
        Player.AttachAbility(new IndependentKillButtonAbility(
            () => {
                // 安全性を確保するためのnull参照チェック
                if (DoubleKillerAbilityData?.DoubleKillerCount == null) return true;
                // 実際のCountプロパティを使用してカウントを確認
                return HasCount;
            },
            () => DoubleKiller.DoubleKillerSubKillCooldown,
            keyType: KeyType.Ability1,
            onlyCrewmates: () => true,
            killedCallback: x => {
                // 安全性を確保するためのnull参照チェック
                if (DoubleKillerAbilityData?.DoubleKillerCount != null && HasCount)
                {
                    try
                    {
                        this.UseAbilityCount();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"DoubleKiller UseAbilityCount error: {ex.Message}", "DoubleKiller");
                    }
                }
            },
            showTextType: () => DoubleKillerAbilityData?.DoubleKillerCount != null ? ShowTextType.Show : ShowTextType.Hidden,
            showText: () => {
                // 安全性を確保するためのnull参照チェック
                if (DoubleKillerAbilityData?.DoubleKillerCount == null) return "";
                return string.Format(ModTranslation.GetString("RemainingText"), Count.ToString());
            }
        ), new AbilityParentAbility(this));
    }
}
