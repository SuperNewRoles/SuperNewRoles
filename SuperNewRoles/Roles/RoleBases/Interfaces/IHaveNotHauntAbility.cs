namespace SuperNewRoles.Roles.RoleBases.Interfaces;

/// <summary>
/// 憑依能力を制御するインターフェイス
/// </summary>
public interface IHaveNotHauntAbility
{
    /// <summary>
    /// 憑依能力を使用可能か
    /// </summary>
    /// <value>true : 使用可能 / false : 使用不可</value>
    public bool CanUseHauntAbility => false;
}