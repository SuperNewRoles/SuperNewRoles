namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 自分視点のみのFixedUpdateを使用する際に使うインターフェース
/// </summary>
public interface IFixedUpdaterMe
{
    /// <summary>
    /// Defaultモードで、自分が生きているときのFixedUpdate
    /// </summary>
    public void FixedUpdateMeDefaultAlive();
    /// <summary>
    /// Defaultモードで、自分が死んでいるときのFixedUpdate
    /// </summary>
    public virtual void FixedUpdateMeDefaultDead() { }
    /// <summary>
    /// Defaultモードで、生存状況に関わらず呼ばれるFixedUpdate
    /// </summary>
    public virtual void FixedUpdateMeDefault() { }
    /// <summary>
    /// SHRモードで、生存状況に関わらず呼ばれるFixedUpdate
    /// </summary>
    public virtual void FixedUpdateMeSHR() { }
    /// <summary>
    /// SHRモードで、自分が生きているときのFixedUpdate
    /// </summary>
    public virtual void FixedUpdateMeSHRAlive() { }
    /// <summary>
    /// SHRモードで、自分が死んでいるときのFixedUpdate
    /// </summary>
    public virtual void FixedUpdateMeSHRDead() { }
}