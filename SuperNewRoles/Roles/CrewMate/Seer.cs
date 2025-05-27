using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.CrewMate;

/// <summary>
/// シーア役職のメインクラス
/// </summary>
class Seer : RoleBase<Seer>
{
    public override RoleId Role { get; } = RoleId.Seer;
    public override Color32 RoleColor { get; } = new(42, 187, 245, byte.MaxValue); // 青色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new SeerAbility(new SeerData() { Mode = Mode, LimitSoulDuration = LimitSoulDuration, SoulDuration = SoulDuration, IsCustomSoulColor = false })];
    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    [CustomOptionSelect("Seer.Mode", typeof(SeerMode), "Seer.Mode.")]
    public static SeerMode Mode = SeerMode.Both;

    [CustomOptionBool("Seer.LimitSoulDuration", false)]
    public static bool LimitSoulDuration = false;

    [CustomOptionFloat("Seer.SoulDuration", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(LimitSoulDuration))]
    public static float SoulDuration = 30f;
}
public enum SeerMode
{
    Both,
    FlashOnly,
    SoulOnly
}