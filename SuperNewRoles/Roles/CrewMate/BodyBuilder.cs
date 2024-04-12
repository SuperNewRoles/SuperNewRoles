
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate.BodyBuilder;

public class BodyBuilder : RoleBase, ICrewmate, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(BodyBuilder),
        (p) => new BodyBuilder(p),
        RoleId.BodyBuilder,
        "BodyBuilder",
        new(214, 143, 94, byte.MaxValue),
        new(RoleId.BodyBuilder, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.BodyBuilder, 452900, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.BodyBuilder, introSound: RoleTypes.Crewmate);
    private static void CreateOption()
    {

    }
    public BodyBuilder(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }

}