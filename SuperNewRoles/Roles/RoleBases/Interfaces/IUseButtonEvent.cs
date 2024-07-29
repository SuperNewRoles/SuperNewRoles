namespace SuperNewRoles.Roles.RoleBases.Interfaces;

/// <summary>
/// UseButtonのパッチを当てる奴<br />
/// falseにする事で本来のコードを無効化する
/// </summary>
public interface IUseButtonEvent
{
    public bool UseButtonDoClick(UseButton button) => true;

    public bool UseButtonSetTarget(UseButton button, IUsable target) => true;
}
