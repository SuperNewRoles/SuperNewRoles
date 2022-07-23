using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.MapOptions;
using UnityEngine;
using HarmonyLib;

namespace SuperNewRoles.Roles
{
    public class SuicidalIdeation
    {
        public static void Postfix()
        {
            var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.SuicidalIdeation.TimeLeft);
            var DateTimeNow1 = DateTime.Now;
            float DateTimeNow2 = 0;
            bool DateTimeNow3 = float.TryParse(DateTimeNow1.ToString("ss"),out DateTimeNow2);
            if (DateTimeNow3) RoleClass.SuicidalIdeation.DateTimeCount += 1;
            DateTimeNow2 += 60 * RoleClass.SuicidalIdeation.DateTimeCount;
            if (RoleClass.IsMeeting || !RoleClass.SuicidalIdeation.IsMeetingEnd) RoleClass.SuicidalIdeation.IsMeetingTime += DateTimeNow2;
            HudManagerStartPatch.SuicidalIdeationButton.Timer = RoleClass.SuicidalIdeation.IsMeetingTime + (float)(RoleClass.SuicidalIdeation.ButtonTimer + TimeSpanDate - DateTimeNow1).TotalSeconds;
            if (HudManagerStartPatch.SuicidalIdeationButton.Timer <= 0f && RoleClass.SuicidalIdeation.IsMeetingEnd) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            SuperNewRolesPlugin.Logger.LogInfo("内部データDateTimeNow1" + DateTimeNow1);
            SuperNewRolesPlugin.Logger.LogInfo("内部データDateTimeNow2" + DateTimeNow2);
            SuperNewRolesPlugin.Logger.LogInfo("内部データDateTimeNow3" + DateTimeNow3);
            SuperNewRolesPlugin.Logger.LogInfo("内部データDateTimeCount" + RoleClass.SuicidalIdeation.DateTimeCount);
            SuperNewRolesPlugin.Logger.LogInfo("内部データButtonTimer" + HudManagerStartPatch.SuicidalIdeationButton.Timer);
            SuperNewRolesPlugin.Logger.LogInfo("内部データIsMeetingTime" + RoleClass.SuicidalIdeation.IsMeetingTime);
            SuperNewRolesPlugin.Logger.LogInfo("内部データ経過時間" + (float)(RoleClass.SuicidalIdeation.ButtonTimer + TimeSpanDate - DateTimeNow1).TotalSeconds);
        }
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
        static void Prefix()
        {
            RoleClass.SuicidalIdeation.IsMeetingEnd = true;
        }
    }
}