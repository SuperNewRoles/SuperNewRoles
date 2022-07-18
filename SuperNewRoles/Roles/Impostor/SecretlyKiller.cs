using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class SecretlyKiller
    {
        public static void EndMeeting()
        {
            AllResetCoolDown();
        }
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
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.MaxTimer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer = RoleClass.SecretlyKiller.SecretlyKillCoolTime;
            //ノーマルリセット
            HudManagerStartPatch.SecretlyKillerMainButton.MaxTimer = RoleClass.SecretlyKiller.KillCoolTime;
            HudManagerStartPatch.SecretlyKillerMainButton.Timer = RoleClass.SecretlyKiller.KillCoolTime;
        }

        //シークレットキル (by:Buttons.cs)
        public static void SecretlyKill()
        {
            RoleClass.SecretlyKiller.target.RpcMurderPlayer(RoleClass.SecretlyKiller.target);
            RoleClass.SecretlyKiller.target = null;
        }
        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
    }
}