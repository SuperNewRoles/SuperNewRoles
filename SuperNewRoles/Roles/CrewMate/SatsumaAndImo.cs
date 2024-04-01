using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate;

public class SatsumaAndImo : RoleBase, INeutral, IWrapUpHandler, ISupportSHR
{
    public enum SatsumaTeam
    {
        Crewmate,
        Madmate
    }
    public static new RoleInfo Roleinfo = new(
        typeof(SatsumaAndImo),
        (p) => new SatsumaAndImo(p),
        RoleId.SatsumaAndImo,
        "SatsumaAndImo",
        new(153, 0, 68, byte.MaxValue),
        new(RoleId.SatsumaAndImo, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Neutral
        );
    public RoleTypes RealRole => RoleTypes.Crewmate;
    public static new OptionInfo Optioninfo =
        new(RoleId.SatsumaAndImo, 401900, true);
    public static new IntroInfo Introinfo =
        new(RoleId.SatsumaAndImo, introSound: RoleTypes.Crewmate);
    public SatsumaTeam TeamState;
    public SatsumaAndImo(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        TeamState = SatsumaTeam.Crewmate;
    }
    public void OnWrapUp()
    {
        TeamState = TeamState == SatsumaTeam.Crewmate ? SatsumaTeam.Madmate : SatsumaTeam.Crewmate;
    }
    public string GetSuffixText()
    {
        return TeamState switch
        {
            _ => " (?)"
        };
    }
    private static string TEAMNAME_Hatena = " (?)";
    private static string TEAMNAME_Crewmate = " (C)";
    private static string TEAMNAME_Madmate = " (M)";
}