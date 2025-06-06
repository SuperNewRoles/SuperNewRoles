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
    public bool IsLimitUses { get; }
    public bool CanShapeshiftAfterUsesExhausted { get; }
    public int ShiftActorCount { get; private set; } // ShiftActor側の独自カウント
    public const float SHIFTACTOR_MESSAGE_DURATION = 5f;

    public ShiftActorAbility(float coolTime, float durationTime, int maxUseCount, bool canSeeSharedRoles, bool isLimitUses, bool canShapeshiftAfterUsesExhausted)
        : base(coolTime, durationTime, (isLimitUses && !canShapeshiftAfterUsesExhausted) ? maxUseCount : -1)
    {
        CanSeeSharedRoles = canSeeSharedRoles;
        IsLimitUses = isLimitUses;
        CanShapeshiftAfterUsesExhausted = canShapeshiftAfterUsesExhausted;

        // ShiftActor側で制限を管理する場合は、独自のカウントを設定
        if (IsLimitUses)
        {
            Count = maxUseCount;
            ShiftActorCount = maxUseCount; // ShiftActor側の独自カウント
        }
        else
        {
            ShiftActorCount = int.MaxValue;
        }
    }

    public override bool CheckHasButton()
    {
        if (!ExPlayerControl.LocalPlayer.IsAlive()) return false;

        // ShiftActorIsLimitUsesがfalseなら制限なし
        if (!IsLimitUses)
            return true;

        // ShiftActorCanShapeshiftAfterUsesExhaustedがtrueなら制限なし（シフトアクター側で制限管理）
        if (CanShapeshiftAfterUsesExhausted)
            return true;

        // 両方の条件を満たさない場合は、使用回数で制限
        return Count > 0;
    }

    public override void OnClick()
    {
        base.OnClick();
        // ShiftActor側の使用回数を減らす（役職発見機能の制限用）
        if (IsLimitUses && ShiftActorCount > 0)
        {
            ShiftActorCount--;
        }
    }

    public override ShowTextType showTextType => (IsLimitUses && !CanShapeshiftAfterUsesExhausted) ? ShowTextType.ShowWithCount : ShowTextType.Hidden;

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
            // 使用回数制限が有効で、ShiftActor側の使用回数を使い切った場合は役職発見しない
            if (IsLimitUses && ShiftActorCount <= 0)
                return;

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