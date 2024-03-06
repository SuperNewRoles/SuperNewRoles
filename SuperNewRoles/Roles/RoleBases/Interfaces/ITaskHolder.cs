using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// タスクを持つことができるインターフェイス
/// </summary>
public interface ITaskHolder
{
    /// <summary>
    /// タスクをクルーメイト勝利にカウントするか
    /// </summary>
    public bool CountTask =>
        this is ICrewmate && (!Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) || CountTaskWhenSHR);

    /// <summary>
    /// SHRでタスクをクルーメイト勝利にカウントするか
    /// </summary>
    private bool CountTaskWhenSHR =>
        this is not ISupportSHR supportSHR ||
        supportSHR.DesyncRole is not (RoleTypes.Impostor or RoleTypes.Shapeshifter);

    /// <summary>
    /// 独自のタスク数を持っているか
    /// </summary>
    /// <param name="TaskData">独自のタスク数(ない場合はnull)</param>
    /// <returns>独自のタスク数を持っているか</returns>
    public bool HaveMyNumTask(out (int numCommon, int numShort, int numLong)? TaskData)
    {
        TaskData = null;
        return false;
    }
    /// <summary>
    /// 独自でタスクを選出するか
    /// 選出する場合は選出処理も含む
    /// </summary>
    /// <param name="tasks">独自で選出したタスク(ない場合はnull)</param>
    /// <param name="TaskData">割り当てられているタスク数</param>
    /// <returns>独自でタスクを選出するか</returns>
    public bool AssignTask(out List<byte> tasks, (int numCommon, int numShort, int numLong) TaskData)
    {
        tasks = null;
        return false;
    }
}