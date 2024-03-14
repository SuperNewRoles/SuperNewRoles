using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 会議に関する処理を行う際に使うインターフェース
/// </summary>
public interface IMeetingHandler
{
    public void StartMeeting();
    public void CloseMeeting();

    /// <summary> 匿名投票か </summary>
    /// <returns> true : 匿名投票 / false : 公開投票</returns>
    public bool EnableAnonymousVotes => GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.AnonymousVotes);
}