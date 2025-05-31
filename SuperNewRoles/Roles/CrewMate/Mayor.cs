using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Crewmate;

class Mayor : RoleBase<Mayor>
{
    public override RoleId Role { get; } = RoleId.Mayor;
    public override Color32 RoleColor { get; } = new(0, 128, 128, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new AdditionalVoteAbility(() => MayorVoteAdditionalVote)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionInt("MayorVoteAdditionalVote", 1, 10, 1, 2)]
    public static int MayorVoteAdditionalVote;
}