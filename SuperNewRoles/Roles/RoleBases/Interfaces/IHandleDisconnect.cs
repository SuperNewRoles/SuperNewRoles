using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 切断時に処理する必要がある場合に使うインターフェース
/// </summary>
public interface IHandleDisconnect
{
    public void OnDisconnect();
}