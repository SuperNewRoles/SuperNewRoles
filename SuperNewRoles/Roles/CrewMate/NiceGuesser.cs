
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;
public class NiceGuesser : GuesserBase, ICrewmate
{
    public static new RoleInfo Roleinfo = new(
        typeof(NiceGuesser),
        (p) => new NiceGuesser(p),
        RoleId.NiceGuesser,
        "NiceGuesser",
        Color.yellow,
        new(RoleId.NiceGuesser, TeamTag.Crewmate,
            RoleTag.SpecialKiller, RoleTag.PowerPlayResistance),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.NiceGuesser, 400400, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.NiceGuesser, introSound: RoleTypes.Impostor);

    public static CustomOption ShotOneMeetingCount;
    public static CustomOption ShotMaxCount;
    public static CustomOption CanShotCrewOption;
    private static void CreateOption()
    {
        ShotMaxCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, Optioninfo.RoleOption);
        ShotOneMeetingCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "EvilGuesserOneMeetingShortSetting", true, Optioninfo.RoleOption);
        CanShotCrewOption = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "EvilGuesserCanCrewShotSetting", true, Optioninfo.RoleOption);
    }
    public NiceGuesser(PlayerControl p) : base(ShotMaxCount.GetInt(), ShotOneMeetingCount.GetBool(), CanShotCrewOption.GetBool(), p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}