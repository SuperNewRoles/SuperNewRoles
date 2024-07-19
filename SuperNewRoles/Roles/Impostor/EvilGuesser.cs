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
        new(RoleId.EvilGuesser, 200400, true,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.EvilGuesser, introSound: RoleTypes.Impostor);

    public override RoleTypes RealRole => RoleTypes.Impostor;

    public static CustomOption ShotOneMeetingCount;
    public static CustomOption ShotMaxCount;
    public static CustomOption CannotShotCrewOption;
    public static CustomOption CannotShotCelebrityOption;
    public static CustomOption BecomeShotCelebrityOption;
    public static CustomOption BecomeShotCelebrityTurn;
    private static void CreateOption()
    {
        ShotMaxCount = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, Optioninfo.RoleOption);
        ShotOneMeetingCount = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserOneMeetingShortSetting", true, Optioninfo.RoleOption);
        CannotShotCrewOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserCannotCrewShotSetting", false, Optioninfo.RoleOption);
        CannotShotCelebrityOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserCannotCelebrityShotSetting", false, Optioninfo.RoleOption);
        BecomeShotCelebrityOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserBecomeShotCelebritySetting", true, CannotShotCelebrityOption);
        BecomeShotCelebrityTurn = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Impostor, "EvilGuesserBecomeShotCelebrityTurnSetting", 3f, 1f, 15f, 1f, BecomeShotCelebrityOption);
    }
    public EvilGuesser(PlayerControl p) : base(ShotMaxCount.GetInt(), ShotOneMeetingCount.GetBool(), CannotShotCrewOption.GetBool(), CannotShotCelebrityOption.GetBool(), BecomeShotCelebrityOption.GetBool(), BecomeShotCelebrityTurn.GetInt(), p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}