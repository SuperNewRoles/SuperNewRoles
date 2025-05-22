using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class LadderDeathAbility : AbilityBase
{
    public int DeathChance { get; private set; }
    private Dictionary<byte, Vector3> _targetLadderData = new();
    private EventListener _fixedUpdateListener;
    private EventListener<ClimbLadderEventData> _climbLadderListener;

    public LadderDeathAbility(int deathChance)
    {
        DeathChance = deathChance;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _climbLadderListener = ClimbLadderEvent.Instance.AddListener(OnClimbLadder);
    }

    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        _climbLadderListener?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (!_targetLadderData.TryGetValue(ExPlayerControl.LocalPlayer.PlayerId, out Vector3 pos)) return;
        if (Vector2.Distance(pos, ExPlayerControl.LocalPlayer.transform.position) >= 0.5f) return;
        if (!ExPlayerControl.LocalPlayer.Player.CanMove) return;

        ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.LadderDeath);
        _targetLadderData.Remove(ExPlayerControl.LocalPlayer.PlayerId);
    }

    public void SetLadderDeathPosition(Vector3 position)
    {
        _targetLadderData[ExPlayerControl.LocalPlayer.PlayerId] = position;
    }

    public void ClearLadderDeathPosition()
    {
        _targetLadderData.Remove(ExPlayerControl.LocalPlayer.PlayerId);
    }

    private void OnClimbLadder(ClimbLadderEventData data)
    {
        var sourcepos = data.source.transform.position;
        var targetpos = data.source.Destination.transform.position;

        // 降りている場合
        if (sourcepos.y > targetpos.y)
        {
            // LadderDeathAbilityを持つ役職の場合
            var ladderDeathAbility = data.player.GetAbility<LadderDeathAbility>();
            if (ladderDeathAbility != null && ModHelpers.IsSuccessChance(ladderDeathAbility.DeathChance))
            {
                ladderDeathAbility.SetLadderDeathPosition(targetpos);
            }
        }
    }
}