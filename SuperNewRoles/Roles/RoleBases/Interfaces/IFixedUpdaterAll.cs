namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 全員視点のFixedUpdateを使用する際に使うインターフェース
/// </summary>
public interface IFixedUpdaterAll
{
    /// <summary>
    /// DefaultモードでのFixedUpdate
    /// </summary>
    public void FixedUpdateAllDefault();
    /// <summary>
    /// SHRモードでのFixedUpdate
    /// </summary>
    public virtual void FixedUpdateAllSHR() { }
}