using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// Introのタイミングで処理を行うインターフェイス
/// </summary>
public interface IIntroHandler
{
    /// <summary>
    /// Introが始まった時に呼ばれる(全視点で)
    /// </summary>
    public void OnIntroStart()
    {

    }
    /// <summary>
    /// Introが始まった時に呼ばれる(自分視点で)
    /// </summary>
    public void OnIntroStartMe()
    {

    }
    /// <summary>
    /// IntroCutsceneが破棄された時に呼ばれる(全視点で)
    /// </summary>
    public void OnIntroDestory()
    {

    }
    /// <summary>
    /// IntroCutsceneが破棄された時に呼ばれる(自分視点で)
    /// </summary>
    public void OnIntroDestoryMe()
    {

    }
}