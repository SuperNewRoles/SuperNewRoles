using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Ability;

public class AreaKillButtonAbility : CustomButtonBase
{
    public Func<bool> CanKill { get; }
    public Func<float?> KillCooldown { get; }
    public Func<float> KillRadius { get; }
    public Func<int>? MaxKillCount { get; } // 一度に殺せる最大人数（0以下なら無制限）
    public Func<bool> OnlyCrewmatesValue { get; }
    public Func<bool> TargetPlayersInVentsValue { get; }
    public Func<ExPlayerControl, bool> IsTargetableValue { get; }
    public Func<bool> IgnoreWallsValue { get; }
    public Action<List<ExPlayerControl>> KilledCallback { get; }
    public Sprite CustomSprite { get; }
    public string CustomButtonText { get; }
    public Color? CustomColor { get; }
    public bool IsUsed { get; private set; }

    public override Color? color => CustomColor ?? Palette.ImpostorRed;
    public override Vector3 LocalScale => Vector3.one;
    public override Sprite Sprite => CustomSprite ?? HudManager.Instance?.KillButton?.graphic?.sprite;
    public override string buttonText => CustomButtonText ?? FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel);
    public override Vector3 PositionOffset => new Vector3(0, 1f, 0);
    protected override KeyCode? hotkey => KeyCode.F;
    protected override int joystickkey => 0;
    public override float DefaultTimer => KillCooldown?.Invoke() ?? 0;

    public AreaKillButtonAbility(
        Func<bool> canKill,
        Func<float> killRadius,
        Func<float?> killCooldown = null,
        Func<int>? maxKillCount = null,
        Func<bool> onlyCrewmates = null,
        Func<bool> targetPlayersInVents = null,
        Func<ExPlayerControl, bool> isTargetable = null,
        Func<bool> ignoreWalls = null,
        Action<List<ExPlayerControl>> killedCallback = null,
        Sprite customSprite = null,
        string customButtonText = null,
        Color? customColor = null)
    {
        CanKill = canKill;
        KillRadius = killRadius;
        KillCooldown = killCooldown;
        MaxKillCount = maxKillCount;
        OnlyCrewmatesValue = onlyCrewmates;
        TargetPlayersInVentsValue = targetPlayersInVents;
        IsTargetableValue = isTargetable;
        IgnoreWallsValue = ignoreWalls;
        KilledCallback = killedCallback;
        CustomSprite = customSprite;
        CustomButtonText = customButtonText;
        CustomColor = customColor;

        IsUsed = false;
    }

    public override void OnClick()
    {
        if (!CanKill()) return;

        var killedPlayers = new List<ExPlayerControl>();
        var localPlayer = ExPlayerControl.LocalPlayer;
        var radius = KillRadius();
        var maxKills = MaxKillCount?.Invoke() ?? 0;
        var onlyCrewmates = OnlyCrewmatesValue?.Invoke() ?? true;
        var targetPlayersInVents = TargetPlayersInVentsValue?.Invoke() ?? false;
        var ignoreWalls = IgnoreWallsValue?.Invoke() ?? false;

        // プレイヤーの位置を取得
        Vector2 myPosition = localPlayer.Player.GetTruePosition();

        // 範囲内のプレイヤーを取得
        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            if (player.PlayerId == localPlayer.PlayerId) continue; // 自分自身はスキップ
            if (player.IsDead()) continue; // 死亡済みのプレイヤーはスキップ
            if (onlyCrewmates && player.IsImpostor()) continue; // クルーのみモードでインポスターはスキップ
            if (!targetPlayersInVents && player.Player.inVent) continue; // ベント内のプレイヤーをターゲットにしない設定の場合はスキップ
            if (IsTargetableValue != null && !IsTargetableValue(player)) continue; // カスタムターゲット条件をチェック

            // 距離を計算
            Vector2 targetPosition = player.Player.GetTruePosition();
            float distance = Vector2.Distance(myPosition, targetPosition);

            // 範囲内かつ壁などの障害物がない場合（ignoreWallsオプションを追加）
            if (distance <= radius && (ignoreWalls || !PhysicsHelpers.AnyNonTriggersBetween(
                myPosition,
                (targetPosition - myPosition).normalized,
                distance,
                Constants.ShipAndObjectsMask)))
            {
                // キルを実行
                localPlayer.RpcCustomDeath(player, CustomDeathType.Kill);
                killedPlayers.Add(player);

                // 最大キル数に達したらループを抜ける
                if (maxKills > 0 && killedPlayers.Count >= maxKills) break;
            }
        }
        IsUsed = true;
        KilledCallback?.Invoke(killedPlayers);
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        if (!CanKill()) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return CanKill() && !IsUsed;
    }
}