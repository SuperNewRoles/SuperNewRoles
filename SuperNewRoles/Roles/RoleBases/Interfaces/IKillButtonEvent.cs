namespace SuperNewRoles.Roles.RoleBases.Interfaces;

/// <summary>
/// KillButtonのパッチを当てる奴<br />
/// falseにする事で本来のコードを無効化する
/// </summary>
public interface IKillButtonEvent
{
    public bool KillButtonDoClick(KillButton button) => true;

    public bool KillButtonCheckClick(KillButton button, PlayerControl target) => true;

    public bool KillButtonSetTarget(KillButton button, PlayerControl target) => true;
}
