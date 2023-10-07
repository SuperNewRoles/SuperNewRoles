using System.Timers;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;
class Celebrity
{
    internal class AbilityOverflowingBrilliance
    {
        private static Timer timer;

        /// <summary>
        /// 試合中に変動しない「タスクフェイズ中に画面を光らせるか」の条件を取得する。
        /// 条件:SHRでない 且つ スターがアサインされている 且つ 設定が有効である
        /// </summary>
        /// <returns>trueが返る時スターの能力[溢れ出る輝き]が有効になっている</returns>
        private static bool IsFirstDecisionAboutFlash()
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return false;
            if (RoleClass.Celebrity.ViewPlayers.Count <= 0) return false;
            if (!CustomOptionHolder.CelebrityIsTaskPhaseFlash.GetBool()) return false;
            return true;
        }

        /// <summary>
        /// 試合中に変動する「タスクフェイズ中に画面を光らせるか」の条件を取得する。
        /// </summary>
        /// <returns>trueでタイマーを有効に,falseでタイマーを無効にしている。</returns>
        private static bool EnabledSetting()
        {
            // スターが存在し、生きている時
            foreach (PlayerControl p in RoleClass.Celebrity.CelebrityPlayer)
            {
                if (p.IsAlive()) return true;
            }

            // スターがSKされてもスターの能力を失わない設定の時
            if (RoleClass.Celebrity.ChangeRoleView)
            {// SKスターが生存している時
                foreach (PlayerControl p in RoleClass.Celebrity.CelebrityPlayer)
                    if (p.IsAlive()) return true;
            }

            // スターが死んでいても発光する場合
            if (!CustomOptionHolder.CelebrityIsFlashWhileAlivingOnly.GetBool())
            {
                // スターがSKされてもスターの能力を失わない設定の時
                if (RoleClass.Celebrity.ChangeRoleView) return true;
                // 失う場合
                else
                {
                    // 「スター」が死んでいる時 (「スターが生きている」からの漏れを拾う)
                    foreach (PlayerControl p in RoleClass.Celebrity.CelebrityPlayer)
                        if (p.IsDead()) return true;
                }
            }

            Logger.Info("スターの輝きが現れる条件を満たしませんでした。「あなたたちは 真の闇におちるのです」", "CelebrityFlash");
            return false;
        }
        public static void WrapUp()
        {
            if (IsFirstDecisionAboutFlash()) CelebrityTimerSet();
        }

        /// <summary>
        /// タイマーをセットする
        /// </summary>
        private static void CelebrityTimerSet()
        {
            timer = new Timer(RoleClass.Celebrity.FlashTime);
            timer.Elapsed += (source, e) =>
            {
                Seer.ShowFlash(Color.yellow);
                Logger.Info($"{RoleClass.Celebrity.FlashTime / 1000}s 経過した為発光しました。「走れ、光よ！」", "CelebrityFlash");
            };
            bool enabled = EnabledSetting();
            timer.AutoReset = enabled;
            timer.Enabled = enabled;
            if (!enabled) return;
            Logger.Info($"{RoleClass.Celebrity.FlashTime}[ミリ秒]にタイマーセット ", "CelebrityFlash");
        }

        /// <summary>
        /// タイマーを止める
        /// </summary>
        public static void TimerStop(bool isEndGame = false)
        {
            if (timer == null) return;

            timer.Stop();
            if (isEndGame) timer.Dispose();
            Logger.Info("発光用タイマーを止めました。", "CelebrityFlash");
        }
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public class CelebrityTimerStop
{
    /// <summary>
    /// リザルト画面でタイマーをストップする
    /// </summary>
    public static void Postfix()
    {
        const bool isEndGame = true;
        Celebrity.AbilityOverflowingBrilliance.TimerStop(isEndGame);
        Neutral.TheThreeLittlePigs.TheFirstLittlePig.TimerStop(isEndGame);
        Impostor.MadRole.MadRaccoon.Button.ResetShapeDuration(false, isEndGame);
        Neutral.Crook.Ability.TimerStop(isEndGame);
    }
}