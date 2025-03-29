using System;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

/// <summary>
/// アドミンマップに表示されなくなる能力
/// </summary>
public class HideInAdminAbility : AbilityBase
{
    private Func<bool> hideInAdmin;

    /// <summary>
    /// 現在アドミンに表示されるかどうか
    /// </summary>
    public bool IsHideInAdmin => hideInAdmin?.Invoke() ?? true;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="hideInAdmin">アドミンに表示されるかどうかを決定する関数</param>
    public HideInAdminAbility(Func<bool> hideInAdmin = null)
    {
        this.hideInAdmin = hideInAdmin;
    }

    public override void AttachToLocalPlayer()
    {
    }
}