
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate.Observer;

public class Observer : RoleBase, ICrewmate, ISupportSHR, IMeetingHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Observer),
        (p) => new Observer(p),
        RoleId.Observer,
        "Observer",
        new(127, 127, 127, byte.MaxValue),
        new(RoleId.Observer, TeamTag.Crewmate, RoleTag.Information),
        TeamRoleType.Crewmate,
        TeamType.Crewmate,
        QuoteMod.TheOtherRolesGM
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Observer, 403000, true,
            optionCreator: null);
    public static new IntroInfo Introinfo =
        new(RoleId.Observer, introSound: RoleTypes.Crewmate);
    public Observer(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }

    // ISupportSHR
    public RoleTypes RealRole => RoleTypes.Crewmate;

    // IMeetingHandler
    public void StartMeeting() { }
    public void CloseMeeting() { }
    public bool EnableAnonymousVotes => false;
}