using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Ability;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Roles.Ability;

public class SluggerAbility : CustomButtonBase, IButtonEffect
{
    private readonly bool isMultiKill;
    private readonly bool isSyncKillCoolTime;
    private readonly bool canKillWhileCharging;
    private KillableAbility _killableAbility;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public float EffectDuration { get; }
    public Action OnEffectEnds => OnChargeComplete;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SluggerButton.png");
    public override string buttonText => ModTranslation.GetString("SluggerButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer { get; } // クールタイムはオプションで調整可
    private CustomPlayerAnimationSimple _chargeAnimation;
    private AudioSource _chargeAudio;
    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<MurderEventData> _murderEvent;
    private EventListener<DieEventData> _dieEvent;

    public SluggerAbility(float coolTime, float chargeTime, bool isMultiKill, bool isSyncKillCoolTime, bool canKillWhileCharging)
    {
        DefaultTimer = coolTime;
        EffectDuration = chargeTime;
        this.isMultiKill = isMultiKill;
        this.isSyncKillCoolTime = isSyncKillCoolTime;
        this.canKillWhileCharging = canKillWhileCharging;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        if (canKillWhileCharging && isSyncKillCoolTime)
            _murderEvent = MurderEvent.Instance.AddListener(OnMurder);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _murderEvent?.RemoveListener();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _dieEvent = DieEvent.Instance.AddListener(OnDie);
        if (!canKillWhileCharging)
        {
            _killableAbility = new KillableAbility(() => !isEffectActive);
            Player.AttachAbility(_killableAbility, new AbilityParentAbility(this));
        }
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _onMeetingStartEvent?.RemoveListener();
        _dieEvent?.RemoveListener();
        CleanupChargeEffects(cancelEffect: true);
    }
    public override void OnClick()
    {
        // チャージ開始アニメーション
        var localPlayer = ExPlayerControl.LocalPlayer;
        PlayChargeAnimation(localPlayer.Player);
        RpcSluggerChargeStart();
    }

    private void OnMeetingStart(MeetingStartEventData _)
    {
        CleanupChargeEffects(cancelEffect: true);
    }

    private void OnDie(DieEventData data)
    {
        if (data.player?.PlayerId != Player?.PlayerId) return;
        CleanupChargeEffects(cancelEffect: true);
    }

    private void OnMurder(MurderEventData data)
    {
        if (!isEffectActive) return;
        if (data.killer != Player || !Player.AmOwner) return;
        CancelCharge();
    }

    private void CancelCharge()
    {
        RpcSluggerChargeStop();
        CleanupChargeEffects(cancelEffect: true);
        if (actionButton != null)
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
    }

    private void CleanupChargeEffects(bool cancelEffect)
    {
        if (cancelEffect)
        {
            isEffectActive = false;
            EffectTimer = 0f;
        }

        if (_chargeAnimation != null)
        {
            GameObject.Destroy(_chargeAnimation.gameObject);
            _chargeAnimation = null;
        }

        if (_chargeAudio != null)
        {
            _chargeAudio.Stop();
            _chargeAudio = null;
        }
    }

    private void PlayChargeAnimation(PlayerControl player)
    {
        // チャージアニメーション（4フレーム、ループ）
        var sprites = CustomPlayerAnimationSimple.GetSprites("tame_{0}.png", 1, 4, zeroPadding: 3);
        var option = new CustomPlayerAnimationSimpleOption(
            Sprites: sprites,
            PlayerFlipX: true,
            IsLoop: true,
            frameRate: 12,
            Adaptive: false,
            DestroyOnMeeting: true,
            localPosition: new(-0.5f, -0.2f, -1),
            localScale: Vector3.one * 0.8f
        );
        _chargeAnimation = CustomPlayerAnimationSimple.Spawn(player, option);
        if (Vector2.Distance(player.transform.position, ExPlayerControl.LocalPlayer.Player.transform.position) <= 5)
            _chargeAudio = SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("Slugger_Charge.mp3"), true, audioMixer: SoundManager.Instance.SfxChannel);
    }

    private float PlayAttackAnimation(PlayerControl player)
    {
        // 攻撃アニメーション（8フレーム、1回再生）
        var sprites = CustomPlayerAnimationSimple.GetSprites("harisen_{0}.png", 1, 10, zeroPadding: 3);
        var option = new CustomPlayerAnimationSimpleOption(
            Sprites: sprites,
            PlayerFlipX: true,
            IsLoop: false,
            frameRate: 25,
            Adaptive: false,
            DestroyOnMeeting: true,
            localScale: Vector3.one * 0.6f,
            UpdatePlayerFlipX: false
        );
        CustomPlayerAnimationSimple.Spawn(player, option);
        if (Vector2.Distance(player.transform.position, ExPlayerControl.LocalPlayer.Player.transform.position) <= 5)
            SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("Slugger_Hit.mp3"), false, audioMixer: SoundManager.Instance.SfxChannel);

        return option.frameRate > 0 ? (sprites.Length / (float)option.frameRate) : 0f;
    }

    private void OnChargeComplete()
    {
        var localPlayer = ExPlayerControl.LocalPlayer;
        RpcSluggerChargeStop();
        CleanupChargeEffects(cancelEffect: false);

        // 攻撃アニメーション（ローカル即時 + 他クライアントへ同期）
        var attackDuration = PlayAttackAnimation(localPlayer.Player);
        RpcSluggerAttackAnimation();
        // 範囲内のターゲットを取得
        List<ExPlayerControl> targets = new();
        float killRadius = 1.5f;
        if (isMultiKill)
        {
            foreach (var player in ExPlayerControl.ExPlayerControls)
            {
                if (player.PlayerId == localPlayer.PlayerId) continue;
                if (player.IsDead()) continue;
                if (Vector2.Distance(localPlayer.Player.transform.position, player.Player.transform.position) > killRadius) continue;
                targets.Add(player);
            }
        }
        else
        {
            var target = TargetCustomButtonBase.SetTarget(onlyCrewmates: false, targetPlayersInVents: false);
            if (target != null)
            {
                ExPlayerControl exTarget = target;
                if (exTarget != null && !exTarget.IsDead())
                    targets.Add(exTarget);
            }
        }
        // キル処理
        RpcSluggerKill(targets);

        // キルクール同期は「ハリセンを振り終わった瞬間」に合わせる
        if (isSyncKillCoolTime && localPlayer != null && localPlayer.AmOwner)
        {
            new LateTask(
                () => ExPlayerControl.LocalPlayer?.ResetKillCooldown(),
                attackDuration,
                "SluggerSyncKillCooldownAfterSwing");
        }

        ResetTimer();
    }

    [CustomRPC]
    public void RpcSluggerAttackAnimation()
    {
        // オーナー(本人)は既にローカルで再生しているため二重再生を避ける
        if (Player.AmOwner) return;
        PlayAttackAnimation(Player.Player);
    }

    [CustomRPC]
    public void RpcSluggerChargeStart()
    {
        // オーナー(本人)は既にローカルで再生しているため二重再生を避ける
        if (Player.AmOwner) return;
        PlayChargeAnimation(Player.Player);
    }

    [CustomRPC]
    public void RpcSluggerChargeStop()
    {
        CleanupChargeEffects(cancelEffect: false);
    }

    [CustomRPC]
    public void RpcSluggerKill(List<ExPlayerControl> targets)
    {
        foreach (var target in targets)
        {
            target.CustomDeath(CustomDeathType.SluggerSlug, source: Player);
            // CustomDeathで死亡していない場合があるので、実際に死亡していない場合は除外
            if (target.IsDead())
                SluggerDeadbody.Spawn(Player, target.Player);
        }
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !isEffectActive;
    }
}
