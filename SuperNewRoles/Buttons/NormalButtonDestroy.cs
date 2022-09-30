
using SuperNewRoles.Roles;

namespace SuperNewRoles.Buttons
{
    public static class DestroyPlayerList
    {
        public static bool IsDestroyKill(this PlayerControl player)
        {
            return player.GetRole() switch
            {
                RoleId.FastMaker => !RoleClass.FastMaker.IsCreatedMadMate,
                RoleId.SecretlyKiller or RoleId.DoubleKiller or RoleId.Smasher or RoleId.Conjurer => true,
                RoleId.Tasker => !CustomOptions.TaskerCanKill.GetBool(),
                //キルボタン無効か
                _ => false
            };
        }
        public static bool IsDestroyReport(this PlayerControl player)
        {
            return player.GetRole() switch
            {
                RoleId.Minimalist => !RoleClass.Minimalist.UseReport,
                RoleId.Fox => !RoleClass.Fox.UseReport,
                RoleId.Neet => true,
                //通報ボタン無効か
                _ => false
            };
        }
        public static bool IsDestroyUse(this PlayerControl player)
        {//使用ボタン消す役職少ないと思うのでswitch文にしときます
         //役職増えたり複雑な条件増えてきたらreturn player.GetRole() switchにします
            var IsDestroyUse = false;
            switch (player.GetRole())
            {
                case RoleId.Neet:
                    //使用ボタン無効か
                    IsDestroyUse = true;
                    break;
            }
            return IsDestroyUse;
        }
    }
    public class NormalButtonDestroy
    {
        public static void Postfix()
        {
            PlayerControl player = CachedPlayer.LocalPlayer;
            if (player.IsDestroyKill())
                if (FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.active)
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            if (player.IsDestroyReport())
                if (FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.active)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);//通報
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.enabled = false;
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.enabled = false;
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.graphic.sprite = null;
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.enabled = false;
                    FastDestroyableSingleton<HudManager>.Instance.ReportButton.buttonLabelText.SetText("");
                }
            if (player.IsDestroyUse())
                if (FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.active)//使うボタンが有効の時
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(false);//使うボタンを無効化
        }
    }
}