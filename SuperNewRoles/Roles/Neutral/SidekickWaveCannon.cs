using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Neutral;

public class SidekickWaveCannon : RoleBase
{
    public static new RoleInfo Roleinfo = new(
        typeof(SidekickWaveCannon),
        (p) => new SidekickWaveCannon(p),
        RoleId.SidekickWaveCannon,
        "SidekickWaveCannon",
        RoleClass.JackalBlue,
        new(RoleId.SidekickWaveCannon, TeamTag.Jackal),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );
    public static new IntroInfo Introinfo =
        new(RoleId.SidekickWaveCannon, introSound: RoleTypes.Shapeshifter);
    public SidekickWaveCannon(PlayerControl p) : base(p, Roleinfo, null, Introinfo)
    {
    }
}