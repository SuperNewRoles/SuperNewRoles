using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles
{
    public static class SecretlyKiller
    {
        public static void EndMeeting()
            => AllResetCoolDown();

        //リセクール (by:Buttons.cs)
        public static void MainResetCoolDown()
        {
            //ノーマルリセット
            HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.KillCoolTime;
            HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.KillCoolTime;
        }
        public static void SecretlyResetCoolDown()
        {
            //シークレットリーリセット
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
        }
        public static void AllResetCoolDown()
        {
            //シークレットリーリセット
            SecretlyResetCoolDown();
            //ノーマルリセット
            MainResetCoolDown();
        }

        //シークレットキル (by:Buttons.cs)
        public static void SecretlyKill()
        {
            RoleClass.SecretlyKiller.target.RpcMurderPlayer(RoleClass.SecretlyKiller.target);
            RoleClass.SecretlyKiller.target = null;
        }
    }
}