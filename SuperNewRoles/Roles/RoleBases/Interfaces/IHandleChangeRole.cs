namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// 役職変更時に処理する必要がある場合に使うインターフェース
/// </summary>
public interface IHandleChangeRole
{
    /// <summary>
    /// 役職が変更される時, 変更前の役職で行う処理
    /// </summary>
    public void OnChangeRole();
}