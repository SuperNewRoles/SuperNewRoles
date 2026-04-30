using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

internal class ElectionCommissioner : RoleBase<ElectionCommissioner>
{
    public override RoleId Role => RoleId.ElectionCommissioner;
    public override Color32 RoleColor => new(127, 127, 127, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } =
        [
                () => new KnowVoteAbility(() => !ElectionCommissionerSeeVote)
        ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    // 保存されるオプション
    public static bool ElectionCommissionerSeeVote = true;


}


