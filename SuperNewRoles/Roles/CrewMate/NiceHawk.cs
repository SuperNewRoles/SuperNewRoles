using System;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class NiceHawk
    {
        public class FixedUpdate
        {
            public static void Postfix()
            {
                if (RoleClass.NiceHawk.Timer >= 0.1 && !RoleClass.IsMeeting)
                {
                    Camera.main.orthographicSize = RoleClass.NiceHawk.CameraDefault * 3f;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default * 3f;
                }
                else
                {
                    Camera.main.orthographicSize = RoleClass.NiceHawk.CameraDefault;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default;
                }
                if (RoleClass.NiceHawk.timer1 >= 0.1 && !RoleClass.IsMeeting)
                {
                    var TimeSpanDate = new TimeSpan(0, 0, 0, 10);
                    RoleClass.NiceHawk.timer1 = (float)(RoleClass.NiceHawk.Timer2 + TimeSpanDate - DateTime.Now).TotalSeconds;
                    CachedPlayer.LocalPlayer.transform.localPosition = RoleClass.NiceHawk.Postion;
                    SuperNewRolesPlugin.Logger.LogInfo(RoleClass.NiceHawk.timer1);
                }
            }
        }
    }
}