using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameTimeEnd
    {
        public static void Prefix()
        {
            SpeedBooster.ResetSpeed();
            EvilSpeedBooster.ResetSpeed();
            Lighter.LightOutEnd();
            Camera.main.orthographicSize = RoleClass.Hawk.Default;
            FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default;
            Camera.main.orthographicSize = RoleClass.NiceHawk.Default;
            FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default;
        }
    }
}