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

        public static void TaskPhaseTimer()
        {
            timer = new Timer(RoleClass.Celebrity.FlashTime);
            timer.Elapsed += TimerEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            Logger.Info("タイマーセット!");
        }

        private static void TimerEvent(System.Object source, ElapsedEventArgs e)
        {
            Seer.ShowFlash(Color.yellow);
            Logger.Info("フラッシュ!!");
        }

        public static void WrapUp()
        {
            TaskPhaseTimer();
            Logger.Info($"{RoleClass.Celebrity.FlashTime}");
        }
    }
}