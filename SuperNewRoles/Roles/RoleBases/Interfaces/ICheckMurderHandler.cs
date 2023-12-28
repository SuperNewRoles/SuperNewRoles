using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface ICheckMurderHandler
{
    /// <summary>
    /// 自分が関係なくても叩かれる
    /// </summary>
    public bool OnCheckMurderPlayer(PlayerControl source, PlayerControl target)
    {
        return true;
    }
    /// <summary>
    /// 自分がキラーの時に叩かれる
    /// </summary>
    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        return true;
    }
    /// <summary>
    /// 自分がターゲットの時に叩かれる
    /// </summary>
    public bool OnCheckMurderPlayerAmTarget(PlayerControl source)
    {
        return true;
    }
}