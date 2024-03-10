namespace SuperNewRoles.Roles.RoleBases.Interfaces;
/// <summary>
/// Introで特定の条件下でテキストを変える際に使うインターフェース
/// </summary>
public interface IGroupIntro
{
    public bool IsGroupIntro(out string IntroText);
}