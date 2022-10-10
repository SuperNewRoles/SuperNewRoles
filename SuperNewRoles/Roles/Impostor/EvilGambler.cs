using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Roles
{
    class EvilGambler
    {
        public static void MurderPlayerPrefix(PlayerControl __instance, PlayerControl target)
        {
            if (__instance.IsRole(RoleId.EvilGambler))
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    SyncSetting.GamblersetCool(__instance);
                    return;
                }
            }
        }
        public static void MurderPlayerPostfix(PlayerControl __instance)
        {
            if (!__instance.IsRole(RoleId.EvilGambler)) return;
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles) && __instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            { // 成功 : 失敗
                RoleClass.EvilGambler.currentCool = RoleClass.EvilGambler.GetSuc() ? RoleClass.EvilGambler.SucCool : RoleClass.EvilGambler.NotSucCool;
            }
        }
    }
}