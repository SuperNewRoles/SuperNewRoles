using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Roles.Ability;

public class SluggerAbility : CustomButtonBase, IButtonEffect
{
    private readonly bool isMultiKill;
    private readonly bool isSyncKillCoolTime;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public float EffectDuration { get; }
    public Action OnEffectEnds => OnChargeComplete;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SluggerButton.png");
    public override string buttonText => ModTranslation.GetString("SluggerButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer { get; } // クールタイムはオプションで調整可
    private CustomPlayerAnimationSimple _chargeAnimation;
    public AudioSource _chargeAudio;

    public SluggerAbility(float coolTime, float chargeTime, bool isMultiKill, bool isSyncKillCoolTime)
    {
        DefaultTimer = coolTime;
        EffectDuration = chargeTime;
        this.isMultiKill = isMultiKill;
        this.isSyncKillCoolTime = isSyncKillCoolTime;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        if (isSyncKillCoolTime)
            SyncKillCoolTimeAbility.CreateAndAttach(this);
    }

    public override void OnClick()
    {
        // チャージ開始アニメーション
        var localPlayer = ExPlayerControl.LocalPlayer;
        PlayChargeAnimation(localPlayer.Player);
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
            _chargeAudio = SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("Slugger_Charge.mp3"), true);
    }

    private void PlayAttackAnimation(PlayerControl player)
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
            SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("Slugger_Hit.mp3"), false);
    }

    private void OnChargeComplete()
    {
        var localPlayer = ExPlayerControl.LocalPlayer;
        // チャージアニメーションを破棄
        GameObject.Destroy(_chargeAnimation.gameObject);
        _chargeAnimation = null;
        // チャージ音は距離5以内でないと代入されないのでnullチェックしておく
        if (_chargeAudio != null)
            _chargeAudio.Stop();
        _chargeAudio = null;
        // 攻撃アニメーション
        PlayAttackAnimation(localPlayer.Player);
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
        ResetTimer();
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