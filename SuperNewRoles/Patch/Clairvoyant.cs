using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    public class Clairvoyant
    {
        public static void TimerEnd()
        {
            /**
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Hawk))
            {
                MapBehaviour.Instance.Close();
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(true);
            }
            **/
        }
        private static float count;


        //千里眼機能の中身
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SuperNewRolesPlugin.Logger.LogInfo(count);
                SuperNewRolesPlugin.Logger.LogInfo(MapOptions.MapOption.Timer);
                if (MapOptions.MapOption.Timer >= 0.1 && !RoleClass.IsMeeting)
                {
                    Camera.main.orthographicSize = MapOptions.MapOption.CameraDefault * 3f;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = MapOptions.MapOption.Default * 3f;
                    if (count == 0)
                    {
                        count = 1;
                        MapOptions.MapOption.Timer = 0;
                        return;
                    }
                }
                else
                {
                    Camera.main.orthographicSize = MapOptions.MapOption.CameraDefault;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = MapOptions.MapOption.Default;
                }
            }
        }
    }
}