using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class MadHawk
    {
        public static void TimerEnd()
        {
            /**
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk))
            {
                MapBehaviour.Instance.Close();
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
                FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(true);
            }
            **/
        }
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
                    var TimeSpanDate = new TimeSpan(0, 0, 0, (int)10);
                    RoleClass.MadHawk.timer1 = (float)((Roles.RoleClass.MadHawk.Timer2 + TimeSpanDate) - DateTime.Now).TotalSeconds;
                    CachedPlayer.LocalPlayer.transform.localPosition = RoleClass.MadHawk.Postion;
                    SuperNewRolesPlugin.Logger.LogInfo(RoleClass.MadHawk.timer1);
                }
            }
        }
    }
}