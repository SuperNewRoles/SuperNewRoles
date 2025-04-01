using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using AmongUs.Data;

namespace SuperNewRoles.Roles.Ability;

public class CustomKillButtonAbility : TargetCustomButtonBase
{
    public Func<bool> CanKill { get; }
    public Func<float?> KillCooldown { get; }
    public Func<bool> OnlyCrewmatesValue { get; }
    public Func<bool> TargetPlayersInVentsValue { get; }
    public Func<ExPlayerControl, bool> IsTargetableValue { get; }
    public Action<ExPlayerControl> KilledCallback { get; }
    public Action<float> OnCooldownStarted;

    public override Color32 OutlineColor => ExPlayerControl.LocalPlayer.roleBase.RoleColor;
    public override Sprite Sprite => HudManager.Instance?.KillButton?.graphic?.sprite;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel);
    protected override KeyType keytype => KeyType.Kill;
    public override float DefaultTimer => KillCooldown?.Invoke() ?? 0;
    public override bool OnlyCrewmates => OnlyCrewmatesValue?.Invoke() ?? false;
    public override bool TargetPlayersInVents => TargetPlayersInVentsValue?.Invoke() ?? false;
    public override Func<ExPlayerControl, bool>? IsTargetable => IsTargetableValue;
    public override ShowTextType showTextType => _showTextType?.Invoke() ?? ShowTextType.Hidden;
    public override string showText => _showText?.Invoke() ?? "";
    private Func<ShowTextType> _showTextType { get; } = () => ShowTextType.Hidden;
    private Func<string> _showText { get; } = () => "";
    public CustomKillButtonAbility(Func<bool> canKill, Func<float?> killCooldown, Func<bool> onlyCrewmates, Func<bool> targetPlayersInVents = null, Func<ExPlayerControl, bool> isTargetable = null, Action<ExPlayerControl> killedCallback = null, Func<ShowTextType> showTextType = null, Func<string> showText = null)
    {
        CanKill = canKill;
        KillCooldown = killCooldown;
        OnlyCrewmatesValue = onlyCrewmates;
        TargetPlayersInVentsValue = targetPlayersInVents;
        IsTargetableValue = isTargetable;
        KilledCallback = killedCallback;
        _showTextType = showTextType;
        _showText = showText;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!CanKill()) return;

        ExPlayerControl.LocalPlayer.RpcCustomDeath(Target, CustomDeathType.Kill);
        ResetTimer();
        KilledCallback?.Invoke(Target);
        OnCooldownStarted?.Invoke(DefaultTimer);
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!CanKill()) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && CanKill();
    }

    [CustomRPC]
    public static void RpcMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return;
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
                Minigame.Instance.Close();
            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
        }
        target.Data.IsDead = true;
        target.Visible = false;
        target.MyPhysics.ResetMoveState();
        target.NetTransform.RpcSnapTo(target.transform.position);
        if (killer.AmOwner)
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
        if (FastDestroyableSingleton<HudManager>.Instance != null)
            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(killer.Data, target.Data);
    }
}