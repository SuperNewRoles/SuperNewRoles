using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Roles.CrewMate;

namespace SuperNewRoles.Roles.RoleBases;
public static class RoleBaseHelper
{
    public static Dictionary<RoleId, Type> allRoleTypes = new()
        {
            // Crew
            { RoleId.SoothSayer, typeof(RoleBase<SoothSayer>) },

            // Impostor

            // Neutral

            // Other
        };
    public static void SetUpOptions()
    {
        new SoothSayer().SetUpOption();
    }
}