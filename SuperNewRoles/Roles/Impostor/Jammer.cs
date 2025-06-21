using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Impostor;

// 提案者：gamerkun さん
class Jammer : RoleBase<Jammer>
{
    public override RoleId Role => RoleId.Jammer;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new JammerAbility(
            JammerCoolTime,
            JammerDurationTime,
            JammerAbilityCount,
            JammerCanUseAbilitiesAgainstImposter
        )
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("JammerCoolTime", 2.5f, 60f, 2.5f, 25f, translationName: "CoolTime")]
    public static float JammerCoolTime;
    [CustomOptionFloat("JammerDurationTime", 2.5f, 120f, 2.5f, 10f, translationName: "DurationTime")]
    public static float JammerDurationTime;
    [CustomOptionInt("JammerAbilityCount", 1, 15, 1, 3, translationName: "AbilityCount")]
    public static int JammerAbilityCount;
    [CustomOptionBool("JammerCanUseAbilitiesAgainstImposter", false, translationName: "JammerCanUseAbilitiesAgainstImposter")]
    public static bool JammerCanUseAbilitiesAgainstImposter;
}

public class JammerAbility : TargetCustomButtonBase, IButtonEffect
{
    private float _coolTime;
    private float _durationTime;
    private int _abilityCount;
    private bool _canUseAgainstImpostors;
    private int _usedCount;
    private ExPlayerControl _invisibleTarget;
    private EventListener<MeetingStartEventData> _onMeetingStart;
    private EventListener _onFixedUpdate;

    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public float EffectDuration => _durationTime;
    public Action OnEffectEnds => () =>
    {
        if (_invisibleTarget != null)
        {
            RpcSetInvisible(_invisibleTarget, false);
            _invisibleTarget = null;
        }
    };
    public bool effectCancellable => true;

    public override Color32 OutlineColor => Color.red;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("JammerButton.png");
    public override string buttonText => ModTranslation.GetString("JammerButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _coolTime;
    public override bool OnlyCrewmates => !_canUseAgainstImpostors;
    public override bool TargetPlayersInVents => false;

    public JammerAbility(float coolTime, float durationTime, int abilityCount, bool canUseAgainstImpostors)
    {
        _coolTime = coolTime;
        _durationTime = durationTime;
        _abilityCount = abilityCount;
        _canUseAgainstImpostors = canUseAgainstImpostors;
        _usedCount = 0;
    }

    public override bool CheckIsAvailable()
    {
        if (!Player.IsAlive()) return false;
        if (!Player.Player.CanMove) return false;
        if (_usedCount >= _abilityCount) return false;
        if (!TargetIsExist) return false;
        return true;
    }

    public override void OnClick()
    {
        if (Target == null) return;

        _invisibleTarget = Target;
        RpcSetInvisible(Target, true);
        _usedCount++;
        ResetTimer();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _onFixedUpdate = FixedUpdateEvent.Instance.AddListener(() => OnFixedUpdate());
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _onFixedUpdate?.RemoveListener();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onMeetingStart = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        // クリーンアップ：透明効果を確実に解除
        if (_invisibleTarget != null)
        {
            RpcSetInvisible(_invisibleTarget, false);
            _invisibleTarget = null;
        }
        _onMeetingStart?.RemoveListener();
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (_invisibleTarget != null)
        {
            RpcSetInvisible(_invisibleTarget, false);
            _invisibleTarget = null;
        }
    }

    private void OnFixedUpdate()
    {
        if (_invisibleTarget != null && !_invisibleTarget.IsDead())
        {
            SetInvisible(_invisibleTarget, true);
        }
    }

    [CustomRPC]
    public void RpcSetInvisible(ExPlayerControl target, bool isInvisible)
    {
        SetInvisible(target, isInvisible);
    }

    private void SetInvisible(ExPlayerControl target, bool isInvisible)
    {
        if (isInvisible)
        {
            ModHelpers.SetOpacity(target.Player, CanSeeTranslucentState(target) ? 0.4f : 0f);
        }
        else
        {
            ModHelpers.SetOpacity(target.Player, 1f);
        }
    }

    private bool CanSeeTranslucentState(ExPlayerControl invisibleTarget)
    {
        if (invisibleTarget == ExPlayerControl.LocalPlayer)
            return true;
        if (ExPlayerControl.LocalPlayer.IsImpostor())
            return true;
        return false;
    }
}