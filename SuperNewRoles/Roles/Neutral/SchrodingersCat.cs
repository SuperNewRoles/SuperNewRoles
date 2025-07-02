using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class SchrodingersCat : RoleBase<SchrodingersCat>
{
    public override RoleId Role { get; } = RoleId.SchrodingersCat;
    public override Color32 RoleColor { get; } = new(126, 126, 126, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = new()
    {
        () => new SchrodingersCatAbility(new SchrodingersCatData(
            HasKillAbility: SchrodingersCatHasKillAbility,
            KillCooldown: SchrodingersCatKillCooldown,
            KillVictimSuicide: SchrodingersCatKillVictimSuicide,
            SuicideTime: SchrodingersCatSuicideTime,
            BeCrewOnExile: SchrodingersCatBeCrewOnExile,
            CrewOnKillByNonSpecific: SchrodingersCatCrewOnKillByNonSpecific
        )), () => new CustomTaskAbility(() => (false, false, 0))
    };

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGMH;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = Array.Empty<RoleId>();

    [CustomOptionBool("SchrodingersCatHasKillAbility", true)]
    public static bool SchrodingersCatHasKillAbility;

    [CustomOptionFloat("SchrodingersCatKillCooldown", 2.5f, 60f, 2.5f, 45f, parentFieldName: nameof(SchrodingersCatHasKillAbility))]
    public static float SchrodingersCatKillCooldown;

    [CustomOptionBool("SchrodingersCatKillVictimSuicide", false, parentFieldName: nameof(SchrodingersCatHasKillAbility))]
    public static bool SchrodingersCatKillVictimSuicide;

    [CustomOptionFloat("SchrodingersCatSuicideTime", 0f, 60f, 2.5f, 15f, parentFieldName: nameof(SchrodingersCatKillVictimSuicide))]
    public static float SchrodingersCatSuicideTime;

    [CustomOptionBool("SchrodingersCatBeCrewOnExile", false)]
    public static bool SchrodingersCatBeCrewOnExile;

    [CustomOptionBool("SchrodingersCatCrewOnKillByNonSpecific", false)]
    public static bool SchrodingersCatCrewOnKillByNonSpecific;
}