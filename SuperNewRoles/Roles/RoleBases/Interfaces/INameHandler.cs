using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface INameHandler
{
    /// <summary>
    /// 全員の役職を見れるか
    /// </summary>
    public bool AmAllRoleVisible => false;
    /// <summary>
    /// 自分視点で他の人の名前や役職名を表示等する時に
    /// </summary>
    public void OnHandleName()
    {

    }
    /// <summary>
    /// 全員の役職を見れる人が実行するやつ
    /// </summary>
    public void OnHandleDeadPlayer()
    {

    }
}