using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 会議に関する処理を行う際に使うインターフェース
/// </summary>
public interface IMeetingHandler
{
    public void StartMeeting();
    public void CloseMeeting();
}