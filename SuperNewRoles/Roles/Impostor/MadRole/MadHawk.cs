using System;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class MadHawk
    {
        public static void TimerEnd() { }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                if (RoleClass.MadHawk.Timer >= 0.1 && !RoleClass.IsMeeting)
                {
                    Camera.main.orthographicSize = RoleClass.MadHawk.CameraDefault * 3f;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.MadHawk.Default * 3f;

                }
                else
                {
                    Camera.main.orthographicSize = RoleClass.MadHawk.CameraDefault;
                    FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = RoleClass.MadHawk.Default;
                }
                if (RoleClass.MadHawk.timer1 >= 0.1 && !RoleClass.IsMeeting)
                {
                    var TimeSpanDate = new TimeSpan(0, 0, 0, 10);
                    RoleClass.MadHawk.timer1 = (float)(RoleClass.MadHawk.Timer2 + TimeSpanDate - DateTime.Now).TotalSeconds;
                    CachedPlayer.LocalPlayer.transform.localPosition = RoleClass.MadHawk.Postion;
                    SuperNewRolesPlugin.Logger.LogInfo(RoleClass.MadHawk.timer1);
                }
            }
        }
    }
}