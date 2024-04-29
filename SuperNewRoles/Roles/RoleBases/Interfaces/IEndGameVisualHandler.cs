
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

/// <summary>
/// EndGame時に実行されます。勝利判定ではなく、終了画面をいじりたいときにどうぞ
/// </summary>
public interface IEndGameVisualHandler
{
    /// <summary>
    /// この役職をもったプレイヤーがいた場合に実行される
    /// </summary>
    /// <param name="poolablePlayer">実行された人のPoolablePlayer</param>
    public void OnEndGame(PoolablePlayer poolablePlayer)
    {

    }
}