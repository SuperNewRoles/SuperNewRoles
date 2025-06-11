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
    private readonly float chargeTime;
    private readonly bool isMultiKill;
    private readonly bool isSyncKillCoolTime;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public float EffectDuration => chargeTime;
    public Action OnEffectEnds => OnChargeComplete;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SluggerButton.png");
    public override string buttonText => ModTranslation.GetString("SluggerButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => 20f; // クールタイムはオプションで調整可

    public SluggerAbility(float chargeTime, bool isMultiKill, bool isSyncKillCoolTime)
    {
        this.chargeTime = chargeTime;
        this.isMultiKill = isMultiKill;
        this.isSyncKillCoolTime = isSyncKillCoolTime;
    }

    public override void OnClick()
    {
        // チャージ開始アニメーション
        var localPlayer = ExPlayerControl.LocalPlayer;
        PlayChargeAnimation(localPlayer.Player);
        isEffectActive = true;
        EffectTimer = chargeTime;
    }

    private void PlayChargeAnimation(PlayerControl player)
    {
        // チャージアニメーション（4フレーム、ループ）
        var sprites = CustomPlayerAnimationSimple.GetSprites("tame_{0}", 1, 4, zeroPadding: 3);
        var option = new CustomPlayerAnimationSimpleOption(
            Sprites: sprites,
            PlayerFlipX: true,
            IsLoop: true,
            frameRate: 12,
            Adaptive: true,
            DestroyOnMeeting: true
        );
        CustomPlayerAnimationSimple.Spawn(player, option);
    }

    private void PlayAttackAnimation(PlayerControl player)
    {
        // 攻撃アニメーション（8フレーム、1回再生）
        var sprites = CustomPlayerAnimationSimple.GetSprites("harisen_{0}.png", 1, 10, zeroPadding: 3);
        var option = new CustomPlayerAnimationSimpleOption(
            Sprites: sprites,
            PlayerFlipX: true,
            IsLoop: false,
            frameRate: 40,
            Adaptive: true,
            DestroyOnMeeting: true
        );
        CustomPlayerAnimationSimple.Spawn(player, option);
    }

    private void OnChargeComplete()
    {
        if (!isEffectActive) return;
        isEffectActive = false;
        var localPlayer = ExPlayerControl.LocalPlayer;
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
        if (isSyncKillCoolTime)
        {
            localPlayer.ResetKillCooldown();
        }
    }

    [CustomRPC]
    public void RpcSluggerKill(List<ExPlayerControl> targets)
    {
        var localPlayer = ExPlayerControl.LocalPlayer;
        foreach (var target in targets)
        {
            target.CustomDeath(CustomDeathType.Kill, source: localPlayer);
            // Slugger専用死体を生成
            SuperNewRoles.CustomObject.SluggerDeadbody.Spawn(localPlayer.Player, target.Player);
        }
        // TODO: 標準死体を消す処理が必要なら追加
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !isEffectActive;
    }
}