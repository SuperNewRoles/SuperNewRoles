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
            CelebrityTimerSet();
        }

        public static bool EnabledSetting()
        {
            if (RoleClass.Celebrity.ViewPlayers.Count <= 0) return false;
            if (!RoleClass.Celebrity.ChangeRoleView)
            {
                if (RoleClass.Celebrity.CelebrityPlayer.Count <= 0) return false;
            }
            return true;
        }

        /*
            スターが死んでもフラッシュする設定があってもいい?

            死亡した時にタイマーストップ
                SKの昇格方式でタイマーストップ

            ゲーム終了時にタイマーストップ必須
            会議開始した時にタイマーストップ必須
            (現状会議をタイマー開始フラグにしている為ここでタイマーをストップしても、タイマー開始フラグは再度発生する)
        */
    }
}