
using System.Collections.Generic;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 追加勝利の場合に勝利できるかなどを返すインターフェース
/// </summary>
public interface IAdditionalWinner
{
    /// <summary>
    /// 追加勝利に関する情報を返す。
    /// </summary>
    /// <returns></returns>
    public AdditionalWinData CanWin();
    public struct AdditionalWinData
    {
        public bool CanWin { get; }
        public WinCondition winCondition { get; }
        public AdditionalWinData(IAdditionalWinner IA, bool CanWin, WinCondition cond)
        {
            this.CanWin = CanWin;
            this.winCondition = cond;
        }
    }
}