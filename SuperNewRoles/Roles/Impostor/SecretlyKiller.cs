using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles;

public static class SecretlyKiller
{
    public static void EndMeeting()
        => AllResetCooldown();

    //リセクール (by:Buttons.cs)
    public static void MainResetCooldown()
    {
        //ノーマルリセット
        HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.KillCoolTime;
        HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.KillCoolTime;
    }
    public static void SecretlyResetCooldown()
    {
        //シークレットリーリセット
        HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
        HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
    }
    public static void AllResetCooldown()
    {
        //シークレットリーリセット
        SecretlyResetCooldown();
        //ノーマルリセット
        MainResetCooldown();
    }

    //シークレットキル (by:Buttons.cs)
    public static void SecretlyKill()
    {
        RoleClass.SecretlyKiller.target.RpcMurderPlayer(RoleClass.SecretlyKiller.target, true);
        RoleClass.SecretlyKiller.target = null;
    }
}