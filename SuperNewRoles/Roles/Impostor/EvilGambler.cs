using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Roles
{
    class EvilGambler
    {
        public static class EvilGamblerMurder
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (__instance.IsRole(RoleId.EvilGambler))
                {
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        SyncSetting.GamblersetCool(__instance);
                        return;
                    }
                    else if (__instance == PlayerControl.LocalPlayer)
                    {
                        if (RoleClass.EvilGambler.GetSuc())//成功
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                        else//失敗
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                    }
                }
            }
        }
    }
}