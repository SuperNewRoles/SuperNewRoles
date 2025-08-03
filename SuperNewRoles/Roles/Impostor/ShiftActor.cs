using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

class ShiftActor : RoleBase<ShiftActor>
{
    public override RoleId Role { get; } = RoleId.ShiftActor;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ShiftActorAbility(
            coolTime: ShiftActorCooldown,
            durationTime: ShiftActorDuration,
            maxUseCount: ShiftActorMaxUseCount,
            canSeeSharedRoles: ShiftActorCanSeeSharedRoles,
            isLimitUses: ShiftActorIsLimitUses,
            canShapeshiftAfterUsesExhausted: ShiftActorCanShapeshiftAfterUsesExhausted
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("ShiftActorCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float ShiftActorCooldown = 30f;

    [CustomOptionFloat("ShiftActorDuration", 2.5f, 60f, 2.5f, 10f)]
    public static float ShiftActorDuration = 10f;

    [CustomOptionBool("ShiftActorIsLimitUses", true)]
    public static bool ShiftActorIsLimitUses = false;

    [CustomOptionInt("ShiftActorMaxUseCount", 1, 10, 1, 3, parentFieldName: nameof(ShiftActorIsLimitUses))]
    public static int ShiftActorMaxUseCount = 3;

    [CustomOptionBool("ShiftActorCanShapeshiftAfterUsesExhausted", true, parentFieldName: nameof(ShiftActorIsLimitUses))]
    public static bool ShiftActorCanShapeshiftAfterUsesExhausted = true;

    [CustomOptionBool("ShiftActorCanSeeSharedRoles", false)]
    public static bool ShiftActorCanSeeSharedRoles = false;
}