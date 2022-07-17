using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameTimeEnd
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            Patch();
        }
        public static void Patch()
        {
            Roles.SpeedBooster.ResetSpeed();
            Roles.EvilSpeedBooster.ResetSpeed();
            Roles.Lighter.LightOutEnd();
            Camera.main.orthographicSize = RoleClass.Hawk.Default;
            FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default;
            Camera.main.orthographicSize = RoleClass.NiceHawk.Default;
            FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default;
        }
    }
}