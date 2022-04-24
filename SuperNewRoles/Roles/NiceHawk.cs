using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class NiceHawk
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
        public class FixedUpdate
        {
            public static void Postfix()
            {
                if (RoleClass.NiceHawk.Timer >= 0.1 && !RoleClass.IsMeeting)
                {
                    Camera.main.orthographicSize = RoleClass.NiceHawk.CameraDefault * 3f;
                    HudManager.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default * 3f;
                    
                }
                else
                {
                    Camera.main.orthographicSize = RoleClass.NiceHawk.CameraDefault;
                    HudManager.Instance.UICamera.orthographicSize = RoleClass.NiceHawk.Default;
                }
                if (RoleClass.NiceHawk.timer1 >= 0.1 && !RoleClass.IsMeeting)
                {
                    var TimeSpanDate = new TimeSpan(0, 0, 0, (int)10);
                    RoleClass.NiceHawk.timer1 = (float)((Roles.RoleClass.NiceHawk.Timer2 + TimeSpanDate) - DateTime.Now).TotalSeconds;
                    PlayerControl.LocalPlayer.transform.localPosition = RoleClass.NiceHawk.Postion;
                    SuperNewRolesPlugin.Logger.LogInfo(RoleClass.NiceHawk.timer1);
                }
            }               
        }
    }
}