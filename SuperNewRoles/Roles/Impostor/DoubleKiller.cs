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
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;

    [CustomOptionBool("DoubleKillerKillCountRemaining", true)]
    public static bool DoubleKillerKillCountRemaining;

    [CustomOptionInt("DoubleKillerMaxKillCount", 1, 10, 1, 1)]
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

    public override Color32 OutlineColor => ExPlayerControl.LocalPlayer.roleBase.RoleColor;
    public override Sprite Sprite => HudManager.Instance?.KillButton?.graphic?.sprite;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel);
    protected override KeyType keytype => KeyType.Kill;
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
        Func<ExPlayerControl, bool> customKillHandler = null)
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
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!CanKill()) return;
        PlayerControl target = Target;
        bool customKilled = _customKillHandler?.Invoke(target) ?? false;
        if (!customKilled)
            ExPlayerControl.LocalPlayer.RpcCustomDeath(target, CustomDeathType.Kill);
        ResetTimer();
        KilledCallback?.Invoke(Target);
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
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        // 通常のキルボタン（Murder イベントでクールダウンされる）
        Player.AttachAbility(new CustomKillButtonAbility(
            () => true, () => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown), () => true), new AbilityParentAbility(this));
        // 独立したキルボタン（Murder イベントでクールダウンされない）
        Player.AttachAbility(new IndependentKillButtonAbility(
            () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? Count <= DoubleKillerAbilityData.DoubleKillerCount.Value : true, () => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown), onlyCrewmates: () => true,
            killedCallback: x => this.UseAbilityCount(),
            showTextType: () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? ShowTextType.Show : ShowTextType.Hidden,
            showText: () => DoubleKillerAbilityData.DoubleKillerCount.HasValue ? string.Format(ModTranslation.GetString("RemainingText"), (DoubleKillerAbilityData.DoubleKillerCount - Count).ToString()) : ""
        ), new AbilityParentAbility(this));
    }
}