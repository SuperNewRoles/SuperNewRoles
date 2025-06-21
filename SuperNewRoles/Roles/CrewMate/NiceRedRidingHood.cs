using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles.Crewmate;

internal class NiceRedRidingHood : RoleBase<NiceRedRidingHood>
{
    public override RoleId Role { get; } = RoleId.NiceRedRidingHood;
    public override Color32 RoleColor { get; } = new(250, 128, 114, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new NiceRedRidingHoodReviveAbility(NiceRedRidingHoodCount, NiceRedRidinIsKillerDeathRevive), () => new NiceRedRidingHoodGhostAbility()
        ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;
    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    // カスタムオプション
    [CustomOptionInt("NiceRedRidingHoodCount", 1, 15, 1, 1)]
    public static int NiceRedRidingHoodCount;

    [CustomOptionBool("NiceRedRidinIsKillerDeathRevive", true)]
    public static bool NiceRedRidinIsKillerDeathRevive;

}