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
    public Action OnEffectEnds => () =>
    {
        if (Player == null || Player.IsDead() || targetPlayer == null || targetPlayer.IsDead())
        {
            RpcEndPenguin();
            return;
        }
        RpcKillPenguinTarget(Player, this, targetPlayer, false);
    };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public bool effectCancellable => false;
    private bool meetingKill;
    private EventListener<CalledMeetingEventData> _preCalledMeeting;
    private EventListener<TryKillEventData> tryKillEvent;
    public override Sprite Sprite => _sprite;
    public override string buttonText => ModTranslation.GetString("PenguinButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolDown;
    public override Color32 OutlineColor => Penguin.Instance.RoleColor;

    public override bool OnlyCrewmates => true;

    private ExPlayerControl targetPlayer;

    private EventListener fixedUpdateEvent;
    private EventListener<WrapUpEventData> wrapUpEvent;
    private EventListener<DieEventData> dieEvent;
    private KillableAbility customKillButtonAbility;
    private bool CanDefaultKill;
    private Sprite _sprite;
    public PenguinAbility(float coolDown, float effectDuration, bool meetingKill, bool CanDefaultKill)
    {
        this.coolDown = coolDown;
        this.effectDuration = effectDuration;
        this.meetingKill = meetingKill;
        this.CanDefaultKill = CanDefaultKill;
        _sprite = AssetManager.GetAsset<Sprite>($"PenguinButton_{ModHelpers.GetRandomInt(min: 1, max: 2)}.png");
    }

    public override void OnClick()
    {
        if (isEffectActive) return;
        if (Target == null || Target.IsDead()) return;
        targetPlayer = Target;
        RpcStartPenguin(targetPlayer);
        new LateTask(() => ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(0.00001f, 0.00001f), 0f);
        ResetTimer();
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        fixedUpdateEvent?.RemoveListener();
        _preCalledMeeting?.RemoveListener();
        wrapUpEvent?.RemoveListener();
        dieEvent?.RemoveListener();
        tryKillEvent?.RemoveListener();
    }
    private void OnFixedUpdate()
    {
        if (targetPlayer == null) return;
        // 死亡しても掴んでいる問題の対策
        if (targetPlayer.IsDead() || Player.IsDead())
        {
            targetPlayer = null;
            return;
        }
        // ここに来た時点で誰か掴んでる
        targetPlayer.NetTransform.SnapTo(Player.transform.position);
    }
    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && Target != null && Target.IsAlive();
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SyncKillCoolTimeAbility.CreateAndAttach(this);
        _preCalledMeeting = PreCalledMeetingEvent.Instance.AddListener(OnPreCalledMeeting);
        customKillButtonAbility = new KillableAbility(() => CanDefaultKill || (targetPlayer != null && targetPlayer.IsAlive()));
        Player.AttachAbility(customKillButtonAbility, new AbilityParentAbility(this));
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
        dieEvent = DieEvent.Instance.AddListener(OnDie);
        tryKillEvent = TryKillEvent.Instance.AddListener(OnTryKill);
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (data.Killer != Player || data.RefTarget != targetPlayer) return;
        EndPenguin();
    }

    private void OnDie(DieEventData data)
    {
        if (targetPlayer == null) return;
        ExPlayerControl deadPlayer = data.player;
        if (deadPlayer != Player && deadPlayer != targetPlayer) return;
        RpcEndPenguin();
    }

    private void OnPreCalledMeeting(CalledMeetingEventData data)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        if (targetPlayer != null && Player.IsAlive() && targetPlayer.IsAlive() && meetingKill)
        {
            RpcKillPenguinTargetBeforeMeeting(Player, this, targetPlayer);
        }
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        if (targetPlayer == null) return;
        if (data.exiled?.PlayerId == Player.PlayerId || Player.IsDead())
            targetPlayer = null;
        else if (Player.AmOwner)
        {
            RpcKillPenguinTarget(Player, this, targetPlayer, true);
        }
    }
    [CustomRPC]
    public void RpcStartPenguin(PlayerControl target)
    {
        targetPlayer = target;
    }

    [CustomRPC]
    public void RpcEndPenguin()
    {
        EndPenguin();
    }

    private void EndPenguin()
    {
        targetPlayer = null;
        isEffectActive = false;
        EffectTimer = EffectDuration;
    }

    [CustomRPC]
    public static void RpcKillPenguinTargetBeforeMeeting(ExPlayerControl source, PenguinAbility ability, ExPlayerControl target)
    {
        if (source != null && source.IsAlive() && target != null && target.IsAlive())
        {
            // 会議開始時の死体集計に間に合わせるため、通常キルアニメーションを経由せず死体を生成する
            target.CustomDeath(CustomDeathType.KillWithoutKillAnimation, source: source);
        }
        ability?.EndPenguin();
    }

    [CustomRPC]
    public static void RpcKillPenguinTarget(ExPlayerControl source, PenguinAbility ability, ExPlayerControl target, bool afterMeeting)
    {
        if (source != null && source.IsAlive() && target != null && target.IsAlive())
        {
            if (afterMeeting)
                target.CustomDeath(CustomDeathType.PenguinAfterMeeting, source: source);
            else
                target.CustomDeath(CustomDeathType.Kill, source: source);
        }
        ability?.EndPenguin();
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
