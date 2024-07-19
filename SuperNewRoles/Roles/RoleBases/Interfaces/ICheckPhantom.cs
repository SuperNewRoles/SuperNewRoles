using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

public interface ICheckPhantom
{
    /// <summary>
    /// 消える時のCheck処理
    /// </summary>
    /// <returns>処理を実行するか</returns>
    public bool CheckVanish()
    {
        return true;
    }
    /// <summary>
    /// 出現時のCheck処理
    /// </summary>
    /// <param name="shouldAnimate">アニメーションをするか</param>
    /// <returns>処理を実行するか</returns>
    public bool CheckAppear(bool shouldAnimate)
    {
        return true;
    }
}
