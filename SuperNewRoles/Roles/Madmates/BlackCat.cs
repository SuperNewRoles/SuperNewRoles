using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using System.Diagnostics.Tracing;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using System.Linq;

namespace SuperNewRoles.Roles.Madmates;

class BlackCat : RoleBase<BlackCat>
{
    public override RoleId Role { get; } = RoleId.BlackCat;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MadmateAbility(new(BlackCatHasImpostorVision, BlackCatCouldUseVent, BlackCatCanKnowImpostors, BlackCatNeededTaskCount, BlackCatIsSpecialTasks ? BlackCatSpecialTasks : null)),
        () => new RevengeExileAbility(BlackCatRevengeNotImpostorExile)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGMH;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Phantom;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Madmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("BlackCatRevengeNotImpostorExile", true)]
    public static bool BlackCatRevengeNotImpostorExile;

    [CustomOptionBool("BlackCatCouldUseVent", true, translationName: "CanUseVent")]
    public static bool BlackCatCouldUseVent;

    [CustomOptionBool("BlackCatHasImpostorVision", true, translationName: "HasImpostorVision")]
    public static bool BlackCatHasImpostorVision;

    [CustomOptionBool("BlackCatCanKnowImpostors", true, translationName: "MadmateCanKnowImpostors")]
    public static bool BlackCatCanKnowImpostors;

    [CustomOptionInt("BlackCatNeededTaskCount", 0, 10, 1, 1, parentFieldName: nameof(BlackCatCanKnowImpostors), translationName: "MadmateNeededTaskCount")]
    public static int BlackCatNeededTaskCount;

    [CustomOptionBool("BlackCatIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool BlackCatIsSpecialTasks;
    [CustomOptionTask("BlackCatSpecialTasks", 1, 1, 1, translationName: "MadmateSpecialTasks", parentFieldName: nameof(BlackCatIsSpecialTasks))]
    public static TaskOptionData BlackCatSpecialTasks;
}
public class RevengeExileAbility : AbilityBase
{
    private bool IsNotImpostor { get; }
    private EventListener<ExileEventData> exileEvent;
    public RevengeExileAbility(bool isNotImpostor)
    {
        IsNotImpostor = isNotImpostor;
    }

    public override void AttachToLocalPlayer()
    {
        exileEvent = ExileEvent.Instance.AddListener(data =>
        {
            if (data.exiled?.PlayerId == PlayerControl.LocalPlayer.Data.PlayerId) RandomExile();
        });
    }
    public void RandomExile()
    {
        // 生きているプレイヤーをリストアップ
        var alivePlayers = ExPlayerControl.ExPlayerControls
            .Where(p => p != null && p.IsAlive() && (!IsNotImpostor || !p.IsImpostor()))
            .ToList();

        // リストが空でない場合、ランダムなプレイヤーを選択
        if (alivePlayers.Count > 0)
        {
            ExPlayerControl randomPlayer = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            randomPlayer.RpcCustomDeath(CustomDeathType.Exile);
        }
    }
}
