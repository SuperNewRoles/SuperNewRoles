using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Hawk
    {
        private static float count;
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SuperNewRolesPlugin.Logger.LogInfo(count);
                SuperNewRolesPlugin.Logger.LogInfo(RoleClass.Hawk.Timer);
                if (RoleClass.Hawk.Timer >= 0.1 && !RoleClass.IsMeeting)
                {
                    Camera.main.orthographicSize = RoleClass.Hawk.CameraDefault * 3f;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default * 3f;
                    if (count == 0)
                    {
                        count = 1;
                        RoleClass.Hawk.Timer = 0;
                        return;
                    }
                }
                else
                {
                    Camera.main.orthographicSize = RoleClass.Hawk.CameraDefault;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default;
                }
            }
        }
    }
}