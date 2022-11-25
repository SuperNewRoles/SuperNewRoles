using System;
using System.Collections.Generic;
using System.Timers;
using HarmonyLib;
using UnityEngine;


namespace SuperNewRoles.Roles.Crewmate
{
    class Celebrity
    {
        private static Timer timer;

        public static void CelebrityTimerSet()
        {
            timer = new Timer(RoleClass.Celebrity.FlashTime);
            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            Logger.Info($"{RoleClass.Celebrity.FlashTime}[ミリ秒]にタイマーセット ", "CelebrityFlash");
        }

        private static void TimerEvent(System.Object source, ElapsedEventArgs e)
        {
            Seer.ShowFlash(Color.yellow);
            Logger.Info($"{RoleClass.Celebrity.FlashTime / 1000}s 経過した為発光しました。", "CelebrityFlash");
        }

        public static void WrapUp()
        {
            CelebrityTimerSet();
        }
    }
}