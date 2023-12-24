using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.Role;
public static class RoleInfoManager
{
    public static readonly Dictionary<RoleId, RoleInfo> RoleInfos = new();
    public static RoleInfo GetRoleInfo(RoleId role)
    {
        return RoleInfos.TryGetValue(role, out RoleInfo result) ? result : null;
    }
}