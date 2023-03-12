using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles.RoleBases;
public static class RoleBaseHelper
{
    public static Dictionary<RoleId, Type> allRoleIds = new()
        {
            // Crew
            { RoleId.SoothSayer, typeof(RoleBase<SoothSayer>) },

            // Impostor

            // Neutral
            { RoleId.FireFox, typeof(RoleBase<FireFox>) },

            // Other
        };
    public static void SetUpOptions()
    {
        new SoothSayer().SetUpOption();
        new Jester().SetUpOption();
        new FireFox().SetUpOption();
    }
}