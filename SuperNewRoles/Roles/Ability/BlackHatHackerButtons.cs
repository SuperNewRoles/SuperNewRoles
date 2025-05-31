using System;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Patches;
using TMPro;

namespace SuperNewRoles.Roles.Ability;

public class BlackHatHackerHackButton : CustomButtonBase, IAbilityCount
{
    private BlackHatHackerAbility _ability;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("CrackerButton.png");
    public override string buttonText => ModTranslation.GetString("BlackHatHackerHackButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _ability.Data.HackCooldown;
    public override ShowTextType showTextType => ShowTextType.ShowWithCount;
    public override string showText => string.Format(ModTranslation.GetString("BlackHatHackerHackNumText"), Count);

    public BlackHatHackerHackButton(BlackHatHackerAbility ability)
    {
        _ability = ability;
        Count = _ability.Data.HackCount;
    }

    public override bool CheckIsAvailable()
    {
        if (Count <= 0) return false;

        PlayerControl target = GetTarget();
        if (!target) return false;

        // 既に感染済みかチェック
        if (_ability.InfectionTimer.ContainsKey(target.PlayerId) &&
            _ability.InfectionTimer[target.PlayerId] >= _ability.Data.HackInfectiousTime)
        {
            return false;
        }

        return PlayerControl.LocalPlayer.CanMove;
    }

    public override bool CheckHasButton() => base.CheckHasButton() && HasCount;

    public override void OnClick()
    {
        if (Count <= 0) return;

        PlayerControl target = GetTarget();
        if (!target) return;

        _ability.InfectionTimer[target.PlayerId] = _ability.Data.HackInfectiousTime;
        this.UseAbilityCount();
        ResetTimer();
    }

    private PlayerControl GetTarget()
    {
        return TargetCustomButtonBase.SetTarget(
            onlyCrewmates: false,
            targetPlayersInVents: false,
            untargetablePlayers: null,
            targetingPlayer: PlayerControl.LocalPlayer,
            isTargetable: null,
            isDeadPlayerOnly: null,
            ignoreWalls: false
        );
    }
}

public class BlackHatHackerVitalsButton : CustomButtonBase
{
    private BlackHatHackerAbility _ability;

    public override Sprite Sprite => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.VitalsLabel);
    protected override KeyType keytype => KeyType.Vent;
    public override float DefaultTimer => 0f;
    public override bool IsFirstCooldownTenSeconds => false;

    public BlackHatHackerVitalsButton(BlackHatHackerAbility ability)
    {
        _ability = ability;
    }

    public override bool CheckHasButton() => base.CheckHasButton() && _ability.Data.CanInfectedVitals;

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        // バイタル表示状態を設定
        BlackHatHackerVitalsState.IsUsingVitals = true;
        DevicesPatch.DontCountBecausePortableVitals = true;

        var originalRole = PlayerControl.LocalPlayer.Data.Role.Role;
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, AmongUs.GameOptions.RoleTypes.Scientist);
        PlayerControl.LocalPlayer.Data.Role.TryCast<ScientistRole>().UseAbility();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, originalRole);
    }

    public override void OnMeetingEnds()
    {
        base.OnMeetingEnds();
        BlackHatHackerVitalsState.IsUsingVitals = false;
        DevicesPatch.DontCountBecausePortableVitals = false;
    }
}

// バイタル状態管理用クラス
public static class BlackHatHackerVitalsState
{
    public static bool IsUsingVitals { get; set; } = false;
}
