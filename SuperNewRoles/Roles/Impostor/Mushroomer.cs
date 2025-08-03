using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles.Impostor;

class Mushroomer : RoleBase<Mushroomer>
{
    public override RoleId Role { get; } = RoleId.Mushroomer;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MushroomerAbility(
            MushroomPlantCoolTime,
            MushroomPlantCount,
            MushroomExplosionCoolTime,
            MushroomExplosionCount,
            MushroomExplosionRange,
            MushroomExplosionDurationTime,
            MushroomerHasGasMask,
            MushroomerActiveUsedMushroom
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Killer, RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionBool("MushroomerHasGasMask", true)]
    public static bool MushroomerHasGasMask;

    [CustomOptionFloat("MushroomerMushroomPlantCoolTime", 2.5f, 60f, 2.5f, 20f)]
    public static float MushroomPlantCoolTime;

    [CustomOptionInt("MushroomerMushroomPlantCount", 1, 10, 1, 3)]
    public static int MushroomPlantCount;

    [CustomOptionFloat("MushroomerMushroomExplosionCoolTime", 2.5f, 60f, 2.5f, 30f)]
    public static float MushroomExplosionCoolTime;

    [CustomOptionInt("MushroomerMushroomExplosionCount", 1, 10, 1, 2)]
    public static int MushroomExplosionCount;

    [CustomOptionFloat("MushroomerMushroomExplosionRange", 0.25f, 3f, 0.25f, 0.5f)]
    public static float MushroomExplosionRange;

    [CustomOptionFloat("MushroomerMushroomExplosionDurationTime", 0.5f, 10f, 0.5f, 2.5f, translationName: "DurationTime")]
    public static float MushroomExplosionDurationTime;

    [CustomOptionBool("MushroomerActiveUsedMushroom", true)]
    public static bool MushroomerActiveUsedMushroom;
}