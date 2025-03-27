using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Ability;
using SuperNewRoles.SuperTrophies;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class PenguinAbility : TargetCustomButtonBase, IButtonEffect
{
    private float coolDown;
    // IButtonEffect
    private float effectDuration;
    public float EffectDuration => effectDuration;
    public Action OnEffectEnds => () => { RpcKillPenguinTarget(PlayerControl.LocalPlayer, this); };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public bool effectCancellable => false;
    private bool meetingKill;
    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PenguinButton.png");
    public override string buttonText => ModTranslation.GetString("PenguinButtonText");
    protected override KeyCode? hotkey => KeyCode.F;
    public override float DefaultTimer => coolDown;
    public override Color32 OutlineColor => Penguin.Instance.RoleColor;

    public override bool OnlyCrewmates => true;

    private ExPlayerControl targetPlayer;

    private EventListener fixedUpdateEvent;
    private KillableAbility customKillButtonAbility;
    private bool CanKill;
    public PenguinAbility(float coolDown, float effectDuration, bool meetingKill, bool CanKill)
    {
        this.coolDown = coolDown;
        this.effectDuration = effectDuration;
        this.meetingKill = meetingKill;
        this.CanKill = CanKill;
    }

    public override void OnClick()
    {
        targetPlayer = Target;
        RpcStartPenguin(PlayerControl.LocalPlayer, Target, AbilityId);
        ResetTimer();
    }
    public override void Detach()
    {
        base.Detach();
        if (fixedUpdateEvent != null)
            FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEvent);
    }
    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    private void OnFixedUpdate()
    {
        if (targetPlayer != null && targetPlayer.IsAlive())
            targetPlayer.NetTransform.SnapTo(Player.transform.position);
    }
    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && Target != null && Target.IsAlive();
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        customKillButtonAbility = new KillableAbility(() => CanKill || (targetPlayer != null && targetPlayer.IsAlive()));
        Player.AttachAbility(customKillButtonAbility, new AbilityParentAbility(this));
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (_onMeetingStartEvent != null)
            MeetingStartEvent.Instance.RemoveListener(_onMeetingStartEvent);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (isEffectActive && targetPlayer != null && targetPlayer.IsAlive() && meetingKill && Player.AmOwner)
        {
            RpcKillPenguinTarget(Player, this);
        }
        else if (isEffectActive && Player.AmOwner)
        {
            RpcEndPenguin(Player, this);
        }
    }

    [CustomRPC]
    public static void RpcStartPenguin(ExPlayerControl source, PlayerControl target, ulong abilityId)
    {
        var ability = source.GetAbility<PenguinAbility>(abilityId);
        if (ability != null)
        {
            ability.targetPlayer = target;
        }
    }

    [CustomRPC]
    public static void RpcEndPenguin(ExPlayerControl source, PenguinAbility ability)
    {
        ability.targetPlayer = null;
    }

    [CustomRPC]
    public static void RpcKillPenguinTarget(ExPlayerControl source, PenguinAbility ability)
    {
        if (ability.targetPlayer != null && ability.targetPlayer.IsAlive())
        {
            source.RpcCustomDeath(ability.targetPlayer, CustomDeathType.Kill);
        }
        RpcEndPenguin(source, ability);
    }
}

public class PenguinKillTrophy : SuperTrophyAbility<PenguinKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.PenguinKill;
    public override TrophyRank TrophyRank => TrophyRank.Bronze;

    public override Type[] TargetAbilities => new Type[] { typeof(PenguinAbility) };

    private PenguinAbility _penguinAbility;
    private EventListener<MurderEventData> _onMurderEvent;

    public override void OnRegister()
    {
        // ペンギン能力の取得
        _penguinAbility = ExPlayerControl.LocalPlayer.PlayerAbilities
            .FirstOrDefault(x => x is PenguinAbility) as PenguinAbility;

        // 殺害イベントのリスナーを登録
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        // ローカルプレイヤーによるアクション以外は無視する
        if (data.killer != PlayerControl.LocalPlayer)
        {
            return;
        }

        // ペンギンの能力によるキルかどうか確認
        if (_penguinAbility != null && _penguinAbility.isEffectActive)
        {
            // 1回ペンギンでキルした実績を解除
            Complete();
        }
    }

    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }
    }
}

public class PenguinHundredKillTrophy : SuperTrophyAbility<PenguinHundredKillTrophy>
{
    public override TrophiesEnum TrophyId => TrophiesEnum.PenguinHundredKill;
    public override TrophyRank TrophyRank => TrophyRank.Gold;

    public override Type[] TargetAbilities => new Type[] { typeof(PenguinAbility) };

    private PenguinAbility _penguinAbility;
    private EventListener<MurderEventData> _onMurderEvent;

    private const int RequiredKills = 100;

    public override void OnRegister()
    {
        // ペンギン能力の取得
        _penguinAbility = ExPlayerControl.LocalPlayer.PlayerAbilities
            .FirstOrDefault(x => x is PenguinAbility) as PenguinAbility;

        // 殺害イベントのリスナーを登録
        _onMurderEvent = MurderEvent.Instance.AddListener(HandleMurderEvent);
    }

    private void HandleMurderEvent(MurderEventData data)
    {
        // ローカルプレイヤーによるアクション以外は無視する
        if (data.killer != PlayerControl.LocalPlayer)
        {
            return;
        }

        // ペンギンの能力によるキルかどうか確認
        if (_penguinAbility != null && _penguinAbility.isEffectActive)
        {
            // キルカウントを増やす
            TrophyData++;

            // 100回達成したらトロフィー獲得
            if (TrophyData >= RequiredKills)
            {
                Complete();
            }
        }
    }

    public override void OnDetached()
    {
        if (_onMurderEvent != null)
        {
            MurderEvent.Instance.RemoveListener(_onMurderEvent);
            _onMurderEvent = null;
        }
    }
}
