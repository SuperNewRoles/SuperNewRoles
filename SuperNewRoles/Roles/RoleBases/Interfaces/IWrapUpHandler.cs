using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// WrapUp時に処理を行う際に使うインターフェース
/// </summary>
public interface IWrapUpHandler
{
    /// <summary>
    /// WrapUp時に実行
    /// </summary>
    /// <param name="exiled">追放者(nullの可能性あり)</param>
    public void OnWrapUp(PlayerControl exiled);
}