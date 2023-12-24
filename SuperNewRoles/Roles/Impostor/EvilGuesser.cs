using AmongUs.GameOptions;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Impostor;

public class EvilGuesser : GuesserBase, IImpostor
{
    public static new RoleInfo Roleinfo = new(
        typeof(EvilGuesser),
        (p) => new EvilGuesser(p),
        RoleId.EvilGuesser,
        "EvilGuesser",
        RoleClass.ImpostorRed,
        new(RoleId.EvilGuesser, TeamTag.Impostor,
            RoleTag.Killer, RoleTag.SpecialKiller),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.EvilGuesser, 200400, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.EvilGuesser, introSound: RoleTypes.Impostor);

    public static CustomOption ShotOneMeetingCount;
    public static CustomOption ShotMaxCount;
    public static CustomOption CanShotCrewOption;
    private static void CreateOption()
    {
        ShotMaxCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, Optioninfo.RoleOption);
        ShotOneMeetingCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilGuesserOneMeetingShortSetting", true, Optioninfo.RoleOption);
        CanShotCrewOption = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilGuesserCanCrewShotSetting", true, Optioninfo.RoleOption);
    }
    public EvilGuesser(PlayerControl p) : base(ShotMaxCount.GetInt(), ShotOneMeetingCount.GetBool(), CanShotCrewOption.GetBool(), p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}