using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record MatryoshkaData(bool WearReport, int WearLimit, float WearTime, float AdditionalKillCoolTime, float CoolTime);
public class MatryoshkaAbility : CustomButtonBase, IButtonEffect, IAbilityCount
{
    private DeadBody currentWearingBody;
    private PlayerControl targetPlayer;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(currentWearingBody != null ? "MatryoshkaTakeOffButton.png" : "MatryoshkaPutOnButton.png");
    public override string buttonText => currentWearingBody != null ? ModTranslation.GetString("MatryoshkaTakeOffButtonName") : ModTranslation.GetString("MatryoshkaPutOnButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => Data.CoolTime;

    public MatryoshkaData Data { get; }

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () => RpcSetMatryoshkaDeadBody(this, null, false);
    public float EffectDuration => Data.WearTime;
    public virtual bool effectCancellable => true;

    public float EffectTimer { get; set; }

    private int Counter = 0;

    private ChangeKillTimerAbility changeKillTimerAbility;


    public MatryoshkaAbility(MatryoshkaData data) : base()
    {
        Data = data;
        Count = Data.WearLimit;
    }

    public override bool CheckIsAvailable()
    {
        targetPlayer = GetClosestDeadBody();
        return targetPlayer != null && PlayerControl.LocalPlayer.CanMove;
    }
    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && HasCount;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        changeKillTimerAbility = new ChangeKillTimerAbility(() => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.KillCooldown) + Data.AdditionalKillCoolTime * Counter);

        ExPlayerControl.LocalPlayer.AttachAbility(changeKillTimerAbility, new AbilityParentAbility(this));
    }

    public override void OnClick()
    {
        if (targetPlayer == null) return;

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        bool isWearing = currentWearingBody != null;

        if (isWearing)
        {
            // 着用している死体を脱ぐ
            RpcSetMatryoshkaDeadBody(this, null, false);
            Counter++;
            this.UseAbilityCount();
        }
        else
        {
            // 新しい死体を着る
            DeadBody targetBody = GetBodyByPlayerId(targetPlayer.PlayerId);
            if (targetBody != null)
            {
                RpcSetMatryoshkaDeadBody(this, targetPlayer, true);
            }
        }
    }

    private PlayerControl GetClosestDeadBody()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        float closestDistance = float.MaxValue;
        PlayerControl result = null;

        // 既に着用中の場合は、その死体を返す
        if (currentWearingBody != null)
        {
            return ExPlayerControl.ById(currentWearingBody.ParentId);
        }

        // 死体を探す
        DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        foreach (DeadBody body in deadBodies)
        {
            // 既に誰かが着用している死体はスキップ
            if (ExPlayerControl.ExPlayerControls.Any(x => x.Role == RoleId.Matryoshka && x.GetAbility<MatryoshkaAbility>()?.currentWearingBody == body)) continue;

            float distance = Vector2.Distance(localPlayer.transform.position, body.transform.position);
            if (distance <= 2f && distance < closestDistance)
            {
                closestDistance = distance;
                result = ExPlayerControl.ById(body.ParentId);
            }
        }

        return result;
    }

    private DeadBody GetBodyByPlayerId(byte playerId)
    {
        DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        foreach (DeadBody body in deadBodies)
        {
            if (body.ParentId == playerId)
            {
                return body;
            }
        }
        return null;
    }

    [CustomRPC]
    public static void RpcSetMatryoshkaDeadBody(MatryoshkaAbility source, ExPlayerControl target, bool isWearing)
    {
        if (source == null) return;

        if (!isWearing)
        {
            DeadBody deadBody = source.currentWearingBody;
            // 着用解除
            if (deadBody != null)
            {
                // 元の見た目に戻す
                source.Player.Player.setOutfit(source.Player.Data.DefaultOutfit);

                // 報告可能に戻す
                deadBody.Reported = false;

                // レンダラーを表示する
                foreach (SpriteRenderer renderer in deadBody.bodyRenderers)
                {
                    renderer.enabled = true;
                }

                deadBody.myCollider.enabled = true;
            }
            source.currentWearingBody = null;
        }
        else
        {
            // 新しい死体を着用
            if (target == null) return;

            DeadBody targetBody = null;
            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            foreach (DeadBody body in deadBodies)
            {
                if (body.ParentId == target.PlayerId)
                {
                    targetBody = body;
                    break;
                }
            }

            if (targetBody == null) return;

            // 死体の見た目にする
            source.Player.Player.setOutfit(target.Data.DefaultOutfit);

            // 報告不可能にする
            targetBody.Reported = !source.Data.WearReport;

            // レンダラーを非表示にする
            foreach (SpriteRenderer renderer in targetBody.bodyRenderers)
            {
                renderer.enabled = false;
            }

            targetBody.myCollider.enabled = source.Data.WearReport;

            source.currentWearingBody = targetBody;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (currentWearingBody != null)
            // 死体を自分の位置に移動
            currentWearingBody.transform.position = PlayerControl.LocalPlayer.transform.position;
    }

    public override void OnMeetingEnds()
    {
        base.OnMeetingEnds();

        // 会議終了時に自動的に死体を脱ぐ
        if (currentWearingBody != null)
        {
            RpcSetMatryoshkaDeadBody(this, null, false);
        }
    }
}