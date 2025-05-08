using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using System.Collections.Generic;

namespace SuperNewRoles.Roles.Ability;

public class CustomSaboAbility : CustomButtonBase
{
    public Func<bool> CanSabotage { get; }
    public Action SabotageCallback { get; }

    public override Sprite Sprite => HudManager.Instance?.SabotageButton?.graphic?.sprite;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SabotageLabel);
    protected override KeyType keytype => KeyType.None;
    public override float DefaultTimer => 0f;

    public CustomSaboAbility(Func<bool> canSabotage, Action sabotageCallback = null)
    {
        CanSabotage = canSabotage;
        SabotageCallback = sabotageCallback;
    }
    public bool CheckCanSabotage()
    {
        return CanSabotage();
    }
    public override void OnClick()
    {
        if (!CanSabotage()) return;

        // サボタージュマップを開く
        if (!PlayerControl.LocalPlayer.inVent && GameManager.Instance.SabotagesEnabled())
        {
            DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
            {
                Mode = MapOptions.Modes.Sabotage
            });
        }

        // コールバックがあれば実行
        SabotageCallback?.Invoke();
    }

    public override bool CheckIsAvailable()
    {
        // プレイヤーが移動不可または死亡している場合は使用不可
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead())
            return false;

        // サボタージュが使用可能かどうか
        if (!CanSabotage())
            return false;

        // 会議中やサボタージュ中は使用不可
        if (MeetingHud.Instance || PlayerControl.LocalPlayer.inVent)
            return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return CanSabotage();
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
    }
}
