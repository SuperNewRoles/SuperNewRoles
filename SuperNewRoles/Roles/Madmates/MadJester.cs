using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Madmates;
class MadJester : RoleBase<MadJester>
{
    public override RoleId Role => RoleId.MadJester;
    public override Color32 RoleColor => Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities { get; } = new()
        {
            () => new MadJesterAbility(new MadJesterData(
                MadJesterCouldUseVent,
                MadJesterHasImpostorVision,
                MadJesterCanKnowImpostors,
                MadJesterNeededTaskCount,
                MadJesterIsSpecialTasks,
                MadJesterSpecialTasks,
                MadJesterWinOnTaskComplete
            ))
        };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Madmate;
    public override RoleTag[] RoleTags => [];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionBool("MadJesterCanKnowImpostors", false)]
    public static bool MadJesterCanKnowImpostors;

    [CustomOptionInt("MadJesterNeededTaskCount", 0, 30, 1, 6, parentFieldName: nameof(MadJesterWinOnTaskComplete))]
    public static int MadJesterNeededTaskCount;

    [CustomOptionBool("MadJesterIsSpecialTasks", false)]
    public static bool MadJesterIsSpecialTasks;

    [CustomOptionTask("MadJesterSpecialTasks", 1, 1, 1, parentFieldName: nameof(MadJesterIsSpecialTasks))]
    public static TaskOptionData MadJesterSpecialTasks;

    [CustomOptionBool("MadJesterCouldUseVent", false, translationName: "CanUseVent")]
    public static bool MadJesterCouldUseVent;

    [CustomOptionBool("MadJesterHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool MadJesterHasImpostorVision;

    [CustomOptionBool("MadJesterWinOnTaskComplete", false)]
    public static bool MadJesterWinOnTaskComplete;
}