using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 死亡時の処理を書く際に使うインターフェース
/// </summary>
public interface IDeathHandler
{
    /// <summary>
    /// 誰かが死んだ時の処理
    /// </summary>
    /// <param name="deathInfo">死亡情報</param>
    public void OnDeath(DeathInfo deathInfo)
    {
    }
    /// <summary>
    /// MurderPlayer時の処理(自分が関係してなくても発生)
    /// </summary>
    /// <param name="info">死亡情報</param>
    public void OnMurderPlayer(DeathInfo info)
    {

    }
    /// <summary>
    /// 追放時の処理(自分が関係してなくても発生)
    /// </summary>
    /// <param name="info">死亡情報</param>
    public void OnExiled(DeathInfo info)
    {

    }
}