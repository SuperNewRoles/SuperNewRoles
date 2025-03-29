using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class ArsonistData
{
    public float DouseCooldown { get; }
    public float DouseDuration { get; }
    public bool CanUseVent { get; }
    public bool IsImpostorVision { get; }

    public ArsonistData(
        float douseCooldown,
        float douseDuration,
        bool canUseVent,
        bool isImpostorVision)
    {
        DouseCooldown = douseCooldown;
        DouseDuration = douseDuration;
        CanUseVent = canUseVent;
        IsImpostorVision = isImpostorVision;
    }
}

public class ArsonistAbility : AbilityBase
{
    private readonly ArsonistData _data;
    private List<byte> _dousedPlayers = new();
    private DouseButtonAbility _douseAbility;
    private IgniteButtonAbility _igniteAbility;
    private CustomVentAbility _ventAbility;
    private ImpostorVisionAbility _impostorVisionAbility;
    public ArsonistAbility(ArsonistData data)
    {
        _data = data;
    }

    public override void AttachToLocalPlayer()
    {
        _douseAbility = new DouseButtonAbility(
            douseCooldown: _data.DouseCooldown,
            douseDuration: _data.DouseDuration,
            onDoused: OnPlayerDoused,
            isDousable: IsDousable
        );
        _igniteAbility = new IgniteButtonAbility(
            canIgnite: CanIgnite
        );
        _ventAbility = new CustomVentAbility(
            canUseVent: () => _data.CanUseVent
        );
        _impostorVisionAbility = new ImpostorVisionAbility(
            hasImpostorVision: () => _data.IsImpostorVision
        );

        Player.AddAbility(_douseAbility, new AbilityParentAbility(this));
        Player.AddAbility(_igniteAbility, new AbilityParentAbility(this));
        Player.AddAbility(_ventAbility, new AbilityParentAbility(this));
        Player.AddAbility(_impostorVisionAbility, new AbilityParentAbility(this));
    }

    private bool IsDousable(ExPlayerControl target)
    {
        return !_dousedPlayers.Contains(target.PlayerId);
    }

    private void OnPlayerDoused(ExPlayerControl target)
    {
        if (!_dousedPlayers.Contains(target.PlayerId))
        {
            _dousedPlayers.Add(target.PlayerId);
            RpcDousePlayer(Player, target);
        }
    }

    private bool CanIgnite()
    {
        // 自分以外の生存プレイヤー全員に油がついていたら点火可能
        var allPlayers = PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => !p.Data.IsDead && p.PlayerId != Player.PlayerId);
        return !allPlayers.Any(p => !_dousedPlayers.Contains(p.PlayerId));
    }

    [CustomRPC]
    public static void RpcDousePlayer(ExPlayerControl source, ExPlayerControl target)
    {
        var sourceComponent = source.GetAbility<ArsonistAbility>();
        if (sourceComponent != null && !sourceComponent._dousedPlayers.Contains(target.PlayerId))
        {
            sourceComponent._dousedPlayers.Add(target.PlayerId);
        }
    }

    [CustomRPC]
    public static void RpcIgniteAll(ExPlayerControl source)
    {
        var sourceComponent = source.GetAbility<ArsonistAbility>();
        if (sourceComponent != null)
        {
            foreach (byte playerId in sourceComponent._dousedPlayers)
            {
                ExPlayerControl target = ExPlayerControl.ById(playerId);
                if (target != null && target.IsAlive())
                {
                    target.CustomDeath(CustomDeathType.Ignite, source);
                }
            }
            CustomRpcExts.RpcEndGameForHost((GameOverReason)CustomGameOverReason.ArsonistWin);
        }
    }
}

public class DouseButtonAbility : TargetCustomButtonBase, IButtonEffect
{
    private readonly float _douseCooldown;
    private readonly float _douseDuration;
    private readonly Action<ExPlayerControl> _onDoused;
    private readonly Func<ExPlayerControl, bool> _isDousable;
    private ExPlayerControl _currentTarget;

    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () => OnDouseEnd();
    public float EffectDuration => _douseDuration;
    public bool effectCancellable => true;
    public float EffectTimer { get; set; }

    public override Color32 OutlineColor => new Color32(255, 102, 0, 255); // オレンジ
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ArsonistDouseButton.png") ?? HudManager.Instance.KillButton.graphic.sprite;
    public override string buttonText => ModTranslation.GetString("ArsonistDouseButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _douseCooldown;
    public override bool OnlyCrewmates => false;
    public override bool TargetPlayersInVents => false;
    public override Func<ExPlayerControl, bool>? IsTargetable => _isDousable;

    public DouseButtonAbility(float douseCooldown, float douseDuration, Action<ExPlayerControl> onDoused, Func<ExPlayerControl, bool> isDousable)
    {
        _douseCooldown = douseCooldown;
        _douseDuration = douseDuration;
        _onDoused = onDoused;
        _isDousable = isDousable;
    }

    public override bool CheckIsAvailable()
    {
        return !Player.IsDead() && Target != null;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        _currentTarget = Target;
    }

    private void OnDouseEnd()
    {
        if (_currentTarget != null && !_currentTarget.IsDead())
        {
            _onDoused?.Invoke(_currentTarget);
        }
        _currentTarget = null;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (isEffectActive && _currentTarget != null)
        {
            // 油をかける処理中に目標が移動して範囲外になった場合やプレイヤーが死亡した場合はキャンセル
            if (_currentTarget.IsDead() || Target != _currentTarget)
            {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                return;
            }
        }
    }
}

public class IgniteButtonAbility : CustomButtonBase
{
    private readonly Func<bool> _canIgnite;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ArsonistIgniteButton.png") ?? HudManager.Instance.KillButton.graphic.sprite;
    public override string buttonText => ModTranslation.GetString("ArsonistIgniteButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => 0f;

    public IgniteButtonAbility(Func<bool> canIgnite)
    {
        _canIgnite = canIgnite;
    }

    public override bool CheckIsAvailable()
    {
        return !Player.IsDead() && _canIgnite();
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;

        // 点火処理
        ArsonistAbility.RpcIgniteAll(Player);
    }
}