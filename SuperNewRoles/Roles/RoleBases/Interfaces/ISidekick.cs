using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

// ジャッカルのサイドキック専用ではなく、全ての役職変更系に使えるインターフェースです。
public interface ISidekick
{
    public RoleId TargetRole { get; }
}