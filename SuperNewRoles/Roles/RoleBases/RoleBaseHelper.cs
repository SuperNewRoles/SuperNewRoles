using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.RoleBases;
public static class RoleBaseHelper
{
    public static Dictionary<RoleId, Type> allRoleIds = new()
        {
            // Crew
            { RoleId.SoothSayer, typeof(RoleBase<SoothSayer>) },
            { RoleId.Lighter, typeof(RoleBase<Lighter>) },
            { RoleId.Sheriff, typeof(RoleBase<Sheriff>) },
            { RoleId.RemoteSheriff, typeof(RoleBase<RemoteSheriff>) },

            // Impostor
            { RoleId.EvilScientist, typeof(RoleBase<EvilScientist>) },

            // Neutral
            { RoleId.Jester, typeof(RoleBase<Jester>) },
            { RoleId.Fox, typeof(RoleBase<Fox>) },
            { RoleId.FireFox, typeof(RoleBase<FireFox>) },

            // Other
        };
    public static void SetUpOptions()
    {
        new SoothSayer().SetUpOption();
        new Jester().SetUpOption();
        new EvilScientist().SetUpOption();
        new Lighter().SetUpOption();
        new Sheriff().SetUpOption();
        new RemoteSheriff().SetUpOption();
        new Fox().SetUpOption();
        new FireFox().SetUpOption();
    }
}