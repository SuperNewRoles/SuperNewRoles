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
    public Action OnEffectEnds => () => { RpcKillPenguinTarget(PlayerControl.LocalPlayer, this, targetPlayer, false); };
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public bool effectCancellable => false;
    private bool meetingKill;
    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    public override Sprite Sprite => _sprite;
    public override string buttonText => ModTranslation.GetString("PenguinButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolDown;
    public override Color32 OutlineColor => Penguin.Instance.RoleColor;

    public override bool OnlyCrewmates => true;

    private ExPlayerControl targetPlayer;

    private EventListener fixedUpdateEvent;
    private EventListener<WrapUpEventData> wrapUpEvent;
    private KillableAbility customKillButtonAbility;
    private bool CanKill;
    private Sprite _sprite;
    public PenguinAbility(float coolDown, float effectDuration, bool meetingKill, bool CanKill)
    {
        this.coolDown = coolDown;
        this.effectDuration = effectDuration;
        this.meetingKill = meetingKill;
        this.CanKill = CanKill;
        _sprite = AssetManager.GetAsset<Sprite>($"PenguinButton_{ModHelpers.GetRandomInt(min: 1, max: 2)}.png");
    }

    public override void OnClick()
    {
        targetPlayer = Target;
        RpcStartPenguin(Target);
        new LateTask(() => ExPlayerControl.LocalPlayer.SetKillTimerUnchecked(0.00001f, 0.00001f), 0f);
        ResetTimer();
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        fixedUpdateEvent?.RemoveListener();
        _onMeetingStartEvent?.RemoveListener();
        wrapUpEvent?.RemoveListener();
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
    public override void AttachToAlls()
    {
        SyncKillCoolTimeAbility.CreateAndAttach(this);
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        customKillButtonAbility = new KillableAbility(() => CanKill || (targetPlayer != null && targetPlayer.IsAlive()));
        Player.AttachAbility(customKillButtonAbility, new AbilityParentAbility(this));
        fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (targetPlayer != null && targetPlayer.IsAlive() && meetingKill)
        {
            if (targetPlayer != null && targetPlayer.IsAlive())
            {
                targetPlayer.CustomDeath(CustomDeathType.Kill, source: Player);
            }
            targetPlayer = null;
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
        targetPlayer = null;
    }

    [CustomRPC]
    public static void RpcKillPenguinTarget(ExPlayerControl source, PenguinAbility ability, ExPlayerControl target, bool afterMeeting)
    {
        if (target != null && target.IsAlive())
        {
            if (afterMeeting)
                target.CustomDeath(CustomDeathType.PenguinAfterMeeting, source: source);
            else
                target.CustomDeath(CustomDeathType.Kill, source: source);
        }
        ability.targetPlayer = null;
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
