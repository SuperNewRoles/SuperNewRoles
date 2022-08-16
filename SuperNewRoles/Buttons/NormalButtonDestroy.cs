using SuperNewRoles.CustomRPC;
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
                RoleId.SecretlyKiller or RoleId.DoubleKiller or RoleId.Smasher => true,
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
        public static bool IsDestroySabo(this PlayerControl player)
        {
            return player.GetRole() switch
            {
                RoleId.Minimalist => !RoleClass.Minimalist.UseSabo,
                RoleId.DoubleKiller => !RoleClass.DoubleKiller.CanUseSabo,
                //サボタージュボタン無効か
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
        public static bool IsDestroyVent(this PlayerControl player)
        {
            return player.GetRole() switch
            {
                RoleId.Minimalist => !RoleClass.Minimalist.UseVent,
                RoleId.DoubleKiller => !RoleClass.DoubleKiller.CanUseVent,
                //ベントボタン無効化
                _ => false,
            };
        }
    }
    public class NormalButtonDestroy
    {
        public static void Postfix(PlayerControl player)
        {
            if (player.IsDestroyKill())
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }
            if (player.IsDestroyReport())
            {
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
            }
            if (player.IsDestroySabo())
            {
                if (FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.active)
                {
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(false);
                }
            }
            if (player.IsDestroyUse())
            {
                if (FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.active)//使うボタンが有効の時
                {
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(false);//使うボタンを無効化
                }
            }
            if (player.IsDestroyVent())
            {
                if (FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.active)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                }
            }
        }
    }
}