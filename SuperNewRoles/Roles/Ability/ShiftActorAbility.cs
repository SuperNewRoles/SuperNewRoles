using AmongUs.GameOptions;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using System;
using SuperNewRoles.Roles.Impostor;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public class ShiftActorAbility : ShapeshiftButtonAbility
{
    public bool CanSeeSharedRoles { get; }
    public const float SHIFTACTOR_MESSAGE_DURATION = 5f;

    public ShiftActorAbility(float coolTime, float durationTime, int maxUseCount, bool canSeeSharedRoles)
        : base(coolTime, durationTime, maxUseCount)
    {
        CanSeeSharedRoles = canSeeSharedRoles;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _shiftActorShapeshiftEvent = ShapeshiftEvent.Instance.AddListener(OnShiftActorShapeshift);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _shiftActorShapeshiftEvent?.RemoveListener();
    }

    private EventListener<ShapeshiftEventData> _shiftActorShapeshiftEvent;

    private void OnShiftActorShapeshift(ShapeshiftEventData data)
    {
        // シェイプシフトした対象の役職を調べる
        if (data.shapeshifter == Player && data.target != data.shapeshifter && Player.AmOwner)
        {
            DiscoverTargetRole(data.target);
        }
    }

    private void DiscoverTargetRole(ExPlayerControl target)
    {
        if (target == null) return;

        var targetRole = target.Role;

        // マーリンは発見できない（仕様）
        // if (targetRole == RoleId.Merlin) return;

        // ローカルプレイヤーにメッセージを表示
        string roleName = ModTranslation.GetString(targetRole.ToString());
        string message = ModTranslation.GetString("ShiftActorRoleMessage", target.Data.PlayerName, roleName);

        // メッセージをログに出力（自分にのみ）
        if (Player.AmOwner)
            new CustomMessage(message, SHIFTACTOR_MESSAGE_DURATION);
    }
}