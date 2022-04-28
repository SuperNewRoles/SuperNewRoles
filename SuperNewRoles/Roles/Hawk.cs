using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Hawk
    {
        public static void TimerEnd()
        {
            /**
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk))
            {
                MapBehaviour.Instance.Close();
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                HudManager.Instance.ReportButton.gameObject.SetActive(true);
                HudManager.Instance.SabotageButton.gameObject.SetActive(true);
            }
            **/
        }
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
                    HudManager.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default * 3f;
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
                    HudManager.Instance.UICamera.orthographicSize = RoleClass.Hawk.Default;
                }
            }
        }
    }
}
