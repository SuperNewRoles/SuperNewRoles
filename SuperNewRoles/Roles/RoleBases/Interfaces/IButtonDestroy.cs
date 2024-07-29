using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;

public interface IButtonDestroy
{
    /// <summary>
    /// Killボタンを消すかどうか<br />
    /// trueならば消さない
    /// </summary>
    public bool CanKillButton => true;
    
    /// <summary>
    /// Reportボタンを消すかどうか<br />
    /// trueならば消さない
    /// </summary>
    public bool CanReportButton => true;

    /// <summary>
    /// Useボタンを消すかどうか<br />
    /// trueならば消さない
    /// </summary>
    public bool CanUseButton => true;
}
