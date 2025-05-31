using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

internal class Pursuer : RoleBase<Pursuer>
{
    public override RoleId Role { get; } = RoleId.Pursuer;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ArrowToNearPlayerAbility(PursuerUpdateInterval, true)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 0;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("PursuerUpdateInterval", 0f, 10f, 0.2f, 0f)]
    public static float PursuerUpdateInterval;
}

internal class ArrowToNearPlayerAbility : AbilityBase
{
    private Arrow _arrow;
    private EventListener _fixedUpdateListener;
    private float _timer = 0f;
    private float _updateInterval = 0f;
    private bool _targetOnlyCrews = false;
    private Vector3 _lastTargetPosition = Vector3.zero;
    public ArrowToNearPlayerAbility(float updateInterval, bool targetOnlyCrews)
    {
        _updateInterval = updateInterval;
        _targetOnlyCrews = targetOnlyCrews;
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _arrow = new(Pursuer.Instance.RoleColor);
        UpdateArrows();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        DestroyArrowIfExist();
    }

    private void OnFixedUpdate()
    {
        if (Player.IsDead())
        {
            DestroyArrowIfExist();
            return;
        }

        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0f)
        {
            UpdateArrows();
            _timer = _updateInterval;
        }
        _arrow.Update(_lastTargetPosition);
    }

    private void UpdateArrows()
    {
        // インポスター陣営以外のプレイヤーを取得
        var nonImpostorPlayers = ExPlayerControl.ExPlayerControls
            .Where(p => p.IsAlive() && !(_targetOnlyCrews && p.IsImpostor()) && p != Player);

        float min_target_distance = float.MaxValue;
        PlayerControl target = null;
        Vector3 mypos = Player.transform.position;

        foreach (var p in nonImpostorPlayers)
        {
            if (p.IsAlive() && !p.IsImpostor())
            {
                float target_distance = Vector3.Distance(mypos, p.transform.position);

                if (target_distance < min_target_distance)
                {
                    min_target_distance = target_distance;
                    target = p;
                }
            }
        }
        if (target != null)
            _lastTargetPosition = target.transform.position;
    }

    private void DestroyArrowIfExist()
    {
        if (_arrow == null) return;
        GameObject.Destroy(_arrow.arrow);
        _arrow = null;
    }
}