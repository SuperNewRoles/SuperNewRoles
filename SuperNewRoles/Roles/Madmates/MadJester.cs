using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

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
                MadJesterKnowImpostorTaskCount,
                MadJesterIsSpecialTasks,
                MadJesterSpecialTasks,
                MadJesterWinOnTaskComplete,
                MadJesterWinOnExiled,
                MadJesterWinRequiredTaskCount
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

    [CustomOptionInt("MadJesterKnowImpostorTaskCount", 0, 30, 1, 5, parentFieldName: nameof(MadJesterCanKnowImpostors))]
    public static int MadJesterKnowImpostorTaskCount;

    [CustomOptionBool("MadJesterIsSpecialTasks", false, translationName: "MadmateIsSpecialTasks")]
    public static bool MadJesterIsSpecialTasks;

    [CustomOptionTask("MadJesterSpecialTasks", 1, 1, 1, parentFieldName: nameof(MadJesterIsSpecialTasks), translationName: "MadmateSpecialTasks")]
    public static TaskOptionData MadJesterSpecialTasks;

    [CustomOptionBool("MadJesterCouldUseVent", false, translationName: "CanUseVent")]
    public static bool MadJesterCouldUseVent;

    [CustomOptionBool("MadJesterHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool MadJesterHasImpostorVision;

    [CustomOptionBool("MadJesterWinOnExiled", true)]
    public static bool MadJesterWinOnExiled;

    [CustomOptionInt("MadJesterWinRequiredTaskCount", 0, 30, 1, 5, parentFieldName: nameof(MadJesterWinOnExiled))]
    public static int MadJesterWinRequiredTaskCount;


    [CustomOptionBool("MadJesterWinOnTaskComplete", false, displayMode: DisplayModeId.None)]
    public static bool MadJesterWinOnTaskComplete;
}