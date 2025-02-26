using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Samurai : RoleBase<Samurai>
{
    public override RoleId Role { get; } = RoleId.Samurai;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new AreaKillButtonAbility(
            canKill: () => true,
            killRadius: () => SamuraiKillRadius,
            killCooldown: () => SamuraiKillCooldown,
            onlyCrewmates: () => SamuraiOnlyKillCrewmates,
            targetPlayersInVents: () => true,
            ignoreWalls: () => SamuraiIgnoreWalls,
            customSprite: AssetManager.GetAsset<Sprite>("SamuraiButton.png"),
            customButtonText: ModTranslation.GetString("SamuraiKillButtonText"),
            customDeathType: CustomDeathType.Samurai
        ),
        () => new CustomSaboAbility(
            canSabotage: () => SamuraiCanSabotage
        ),
        () => new CustomVentAbility(
            canUseVent: () => SamuraiCanUseVent
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("SamuraiKillCooldown", 10f, 60f, 2.5f, 30f)]
    public static float SamuraiKillCooldown;

    [CustomOptionFloat("SamuraiKillRadius", 1f, 5f, 0.5f, 2f)]
    public static float SamuraiKillRadius;

    [CustomOptionBool("SamuraiOnlyKillCrewmates", false)]
    public static bool SamuraiOnlyKillCrewmates;

    [CustomOptionBool("SamuraiIgnoreWalls", false)]
    public static bool SamuraiIgnoreWalls;

    [CustomOptionBool("SamuraiCanUseVent", true)]
    public static bool SamuraiCanUseVent;

    [CustomOptionBool("SamuraiCanSabotage", true)]
    public static bool SamuraiCanSabotage;
}