using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

// ジャッカルのサイドキック専用ではなく、全ての役職変更系に使えるインターフェースです。
public interface ISidekick
{
    /// <summary>
    /// 昇格先の役職
    /// </summary>
    public RoleId TargetRole { get; }

    public void SetParent(PlayerControl player);
}