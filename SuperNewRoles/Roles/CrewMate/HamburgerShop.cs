using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class HamburgerShop : RoleBase<HamburgerShop>
{
    public override RoleId Role { get; } = RoleId.HamburgerShop;
    public override Color32 RoleColor { get; } = new(255, 165, 0, byte.MaxValue); // オレンジ色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CustomTaskTypeAbility(TaskTypes.MakeBurger, HamburgerShopChangeAllTasksToBurger, MapNames.Airship),
        () => new CustomTaskAbility(
            () => (true, HamburgerShopTaskOptionAvailable, HamburgerShopTaskOptionAvailable ? HamburgerShopTaskOption.Total : null),
            HamburgerShopTaskOptionAvailable ? HamburgerShopTaskOption : null
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionBool("HamburgerShopChangeAllTasksToBurger", true, displayMode: DisplayModeId.Default)]
    public static bool HamburgerShopChangeAllTasksToBurger;

    [CustomOptionBool("HamburgerShopTaskOptionAvailable", false)]
    public static bool HamburgerShopTaskOptionAvailable;

    [CustomOptionTask("HamburgerShopTaskOption", 2, 1, 1, parentFieldName: nameof(HamburgerShopTaskOptionAvailable), parentActiveValue: true)]
    public static TaskOptionData HamburgerShopTaskOption;
}