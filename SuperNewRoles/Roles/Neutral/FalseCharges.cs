using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;

namespace SuperNewRoles.Roles.Neutral;

class FalseCharges : RoleBase<FalseCharges>
{
    public override RoleId Role { get; } = RoleId.FalseCharges;
    public override Color32 RoleColor { get; } = Color.green;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new FalseChargesAbility(new FalseChargesData(
            coolTime: FalseChargesCoolTime,
            exileTurn: FalseChargesExileTurn
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionFloat("FalseChargesCoolTime", 2.5f, 120f, 2.5f, 30f, translationName: "CoolTime")]
    public static float FalseChargesCoolTime;

    [CustomOptionInt("FalseChargesExileTurn", 1, 5, 1, 1)]
    public static int FalseChargesExileTurn;
}

public class FalseChargesAbility : TargetCustomButtonBase
{
    private readonly FalseChargesData _data;
    private EventListener<WrapUpEventData> _wrapUpListener;

    private int _turn;
    private PlayerControl _target;

    public FalseChargesAbility(FalseChargesData data)
    {
        _data = data;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        Logger.Info($"OnWrapUp: {data.exiled?.PlayerId ?? 255} {_target?.PlayerId ?? 255}");
        _turn--;
        // 追放されたプレイヤーがいる場合
        if (data.exiled == null || data.exiled.PlayerId != _target?.PlayerId) return;
        if (_turn >= 0)
        {
            EndGamer.RpcEndGameWithWinner(CustomGameOverReason.FalseChargesWin, WinType.SingleNeutral, [Player], FalseCharges.Instance.RoleColor, "FalseCharges", string.Empty);
        }
    }

    public override void DetachToLocalPlayer()
    {
        // イベントリスナーを削除
        _wrapUpListener?.RemoveListener();
    }

    public override Color32 OutlineColor => FalseCharges.Instance.RoleColor;
    public override float DefaultTimer => _data.CoolTime;
    public override string buttonText => ModTranslation.GetString("FalseCharges.ButtonTitle");
    public override bool OnlyCrewmates => false;
    protected override KeyType keytype => KeyType.Kill;
    public override Sprite Sprite => FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite;

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override void OnClick()
    {
        if (Target == null) return;

        // 冤罪師の処理を実行
        SetFalseChargeRPC(this, Target);
    }

    [CustomRPC]
    public static void SetFalseChargeRPC(FalseChargesAbility ability, ExPlayerControl target)
    {
        ability._target = target;
        ability._turn = ability._data.ExileTurn;
        ability.Player.CustomDeath(CustomDeathType.FalseCharges, source: target);
    }
}

public class FalseChargesData
{
    public float CoolTime { get; }
    public int ExileTurn { get; }

    public FalseChargesData(float coolTime, int exileTurn)
    {
        CoolTime = coolTime;
        ExileTurn = exileTurn;
    }
}