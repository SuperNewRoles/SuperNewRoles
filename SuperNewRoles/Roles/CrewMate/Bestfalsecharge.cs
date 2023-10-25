using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles;

public class Bestfalsecharge : RoleBase, IWrapUpHandler, ICrewmate
{
    public static new RoleInfo Roleinfo = new(
        typeof(Bestfalsecharge),
        (p) => new Bestfalsecharge(p),
        RoleId.Bestfalsecharge,
        "Bestfalsecharge",
        RoleClass.CrewmateWhite,
        new(RoleId.Bestfalsecharge, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo = new(RoleId.Bestfalsecharge, 403200, true);
    public static new IntroInfo Introinfo = new(RoleId.Bestfalsecharge);
    public bool IsOnMeeting = false;
    public Bestfalsecharge(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        IsOnMeeting = false;
    }
    public void OnWrapUp()
    {
        if (!AmongUsClient.Instance.AmHost ||
            IsOnMeeting)
            return;
        if (ModeHandler.IsMode(ModeId.Default))
            Player.RpcExiledUnchecked();
        else
            Player.RpcInnerExiled();
        Player.RpcSetFinalStatus(FinalStatus.BestFalseChargesFalseCharge);

        IsOnMeeting = true;

        //===========以下さつまいも===========//
        RoleClass.SatsumaAndImo.TeamNumber = RoleClass.SatsumaAndImo.TeamNumber == 1 ? 2 : 1;
    }
}