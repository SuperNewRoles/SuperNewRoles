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
            timer.Elapsed += (source, e) =>
            {
                Seer.ShowFlash(Color.yellow);
                Logger.Info($"{RoleClass.Celebrity.FlashTime / 1000}s 経過した為発光しました。", "CelebrityFlash");
            };
            timer.AutoReset = EnabledSetting();
            timer.Enabled = EnabledSetting();
            if (!EnabledSetting()) return;
            Logger.Info($"{RoleClass.Celebrity.FlashTime}[ミリ秒]にタイマーセット ", "CelebrityFlash");
        }

        public static void WrapUp()
        {
            if (IsFlash()) CelebrityTimerSet();
        }

        public static void TimerStop()
        {
            if (timer != null) timer.Stop();
            Logger.Info("発光用タイマーを止めました。", "CelebrityFlash");
        }

        public static bool EnabledSetting()
        {
            if (!IsFlash()) return false; //この条件をWarapUpでも判断している為、必要なさそうな場合最後に消してください。
            if (!RoleClass.Celebrity.ChangeRoleView)
            {
                if (RoleClass.Celebrity.CelebrityPlayer.Count <= 0) return false;
            }
            foreach (PlayerControl p in RoleClass.Celebrity.ViewPlayers)
            {
                if (p.IsDead()) return false;
            }
            return true;
        }

        public static bool IsFlash()
        {
            if (RoleClass.Celebrity.ViewPlayers.Count <= 0) return false;
            if (!CustomOptionHolder.CelebrityIsTaskPhaseFlash.GetBool()) return false;
            return true;
        }

        /*
            スターが死んでもフラッシュする設定があってもいい?

            死亡した時にタイマーストップ
                SKの昇格方式でタイマーストップ
        */
    }
}