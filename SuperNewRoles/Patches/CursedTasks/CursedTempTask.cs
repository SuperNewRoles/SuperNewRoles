using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

// Cursed モード時の TempMinigame 挙動を変更するパッチ
// 実装概要:
// - 温度を 0.1°C 単位で扱う（内部は tenths = 値*10 の整数）
// - 表示は小数 1 桁（例: 21.5）で行う
// - 値が一致してから 0.5 秒間同じ値が続いたら正解とする
// 注: Cursed でない場合は元の挙動をそのまま使用する
public class CursedTempTask
{
    // タスクごとに「値が一致してからの継続時間」を保持するマップ
    // key: Task.Id (uint), value: 継続時間 (秒)
    public static Dictionary<uint, float> EqualTimers = new();

    // タスクごとに「ボタンが押され続けている時間」を保持するマップ
    // key: Task.Id (uint), value: 継続時間 (秒)
    public static Dictionary<uint, float> PressTimers = new();

    [HarmonyPatch(typeof(TempMinigame))]
    public static class TempMinigamePatch
    {
        [HarmonyPatch(nameof(TempMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(TempMinigame __instance)
        {
            if (!Main.IsCursed) return;

            // Begin 時に Log（表示）と Reading（正解）を tenths 単位で設定する
            // 例: LogRange.Next() が 21（= 21°C）の場合、内部値は 21 * 10 = 210（= 21.0°C）となる
            int logTenths = __instance.LogRange.Next() * 10;
            int min = __instance.ReadingRange.min;
            int max = __instance.ReadingRange.max;
            // 0.1°C 単位の範囲でランダムな正解値を生成
            int readingTenths = UnityEngine.Random.Range(min * 10, max * 10);

            // 内部値に代入し、デバウンス用変数をリセット
            __instance.logValue = logTenths;
            __instance.readingValue = readingTenths;
            __instance.deltaSinceLastChangeNumber = 0f;

            // 表示は小数 1 桁で出す
            ((TMPro.TMP_Text)__instance.ReadingText).text = (readingTenths / 10f).ToString("0.0");
            ((TMPro.TMP_Text)__instance.LogText).text = (logTenths / 10f).ToString("0.0");

            // EqualTimers を初期化
            if (!EqualTimers.ContainsKey(__instance.MyTask.Id)) EqualTimers.Add(__instance.MyTask.Id, 0f);
            EqualTimers[__instance.MyTask.Id] = 0f;

            // PressTimers を初期化
            if (!PressTimers.ContainsKey(__instance.MyTask.Id)) PressTimers.Add(__instance.MyTask.Id, 0f);
            PressTimers[__instance.MyTask.Id] = 0f;
        }

        [HarmonyPatch(nameof(TempMinigame.ChangeNumber)), HarmonyPrefix]
        // ChangeNumber の Prefix: Cursed の場合は 0.1°C 単位での増減と表示更新を行う
        public static bool ChangeNumberPrefix(TempMinigame __instance, int dir)
        {
            if (!Main.IsCursed) return true;

            int logVal = (int)__instance.logValue;
            int readingVal = (int)__instance.readingValue;
            float delta = (float)__instance.deltaSinceLastChangeNumber;

            // dir==0 は表示更新のみ
            if (dir == 0)
            {
                ((TMPro.TMP_Text)__instance.LogText).text = (logVal / 10f).ToString("0.0");
                __instance.deltaSinceLastChangeNumber = 0f;
                return false; // オリジナルをスキップ
            }

            // 常に ±1（=0.1°C）を適用（正解値に達しても変更可能）
            if (dir != 0 && Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(__instance.ButtonSound, loop: false);
            }

            logVal += dir;
            __instance.logValue = logVal;
            ((TMPro.TMP_Text)__instance.LogText).text = (logVal / 10f).ToString("0.0");

            // 値が一致した瞬間に EqualTimers を 0 にして計測開始
            if (logVal == readingVal)
            {
                if (!EqualTimers.ContainsKey(__instance.MyTask.Id)) EqualTimers.Add(__instance.MyTask.Id, 0f);
                EqualTimers[__instance.MyTask.Id] = 0f;
            }

            __instance.deltaSinceLastChangeNumber = 0f;

            return false; // オリジナル ChangeNumber を呼ばない
        }

        private static void DoChangeNumber(TempMinigame __instance, int dir)
        {
            // Update 内から呼ばれるための安全なヘルパー
            int logVal = (int)__instance.logValue;
            int readingVal = (int)__instance.readingValue;

            if (dir == 0)
            {
                ((TMPro.TMP_Text)__instance.LogText).text = (logVal / 10f).ToString("0.0");
                __instance.deltaSinceLastChangeNumber = 0f;
                return;
            }

            if (dir != 0 && Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(__instance.ButtonSound, loop: false);
            }
            logVal += dir; // tenths 単位で ±1
            __instance.logValue = logVal;
            ((TMPro.TMP_Text)__instance.LogText).text = (logVal / 10f).ToString("0.0");
            if (logVal == readingVal)
            {
                if (!EqualTimers.ContainsKey(__instance.MyTask.Id)) EqualTimers.Add(__instance.MyTask.Id, 0f);
                EqualTimers[__instance.MyTask.Id] = 0f;
            }
            __instance.deltaSinceLastChangeNumber = 0f;
        }

        [HarmonyPatch(nameof(TempMinigame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(TempMinigame __instance)
        {
            if (!Main.IsCursed) return true;

            float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(14);
            float delta = (float)__instance.deltaSinceLastChangeNumber + Time.deltaTime;
            __instance.deltaSinceLastChangeNumber = delta;

            int dir = 0;
            if ((double)axisRaw > 0.9)
            {
                dir = 1;
            }
            else if ((double)axisRaw > 0.7)
            {
                dir = 1;
            }
            else if ((double)axisRaw > 0.5)
            {
                dir = 1;
            }
            else if ((double)axisRaw > 0.4)
            {
                dir = 1;
            }
            else if (axisRaw < -0.9f)
            {
                dir = -1;
            }
            else if (axisRaw < -0.7f)
            {
                dir = -1;
            }
            else if (axisRaw < -0.5f)
            {
                dir = -1;
            }
            else if (axisRaw < -0.4f)
            {
                dir = -1;
            }

            // ボタンが押されている場合、PressTimers を更新
            if (__instance.deltaSinceLastChangeNumber < 0.2f)
            {
                Logger.Info("Yes Dir");
                if (!PressTimers.ContainsKey(__instance.MyTask.Id)) PressTimers.Add(__instance.MyTask.Id, 0f);
                PressTimers[__instance.MyTask.Id] += Time.deltaTime;
            }
            else
            {
                Logger.Info("No Dir");
                // 押されていない場合、リセット
                if (PressTimers.ContainsKey(__instance.MyTask.Id)) PressTimers[__instance.MyTask.Id] = 0f;
            }

            if (dir != 0 && delta > 0.1f) // 固定間隔 0.1秒で変化
            {
                // 押し続け時間に基づいて変化量を計算（0.25秒ごとに1.6倍）
                float pressTime = PressTimers.ContainsKey(__instance.MyTask.Id) ? PressTimers[__instance.MyTask.Id] : 0f;
                float multiplier = Mathf.Pow(1.6f, Mathf.Floor(pressTime / 0.25f));
                int effectiveDir = (int)(dir * multiplier);

                // call our helper to change the number without invoking original ChangeNumber
                DoChangeNumber(__instance, effectiveDir);
            }

            int logVal = (int)__instance.logValue;
            int readingVal = (int)__instance.readingValue;

            if (logVal == readingVal)
            {
                if (!EqualTimers.ContainsKey(__instance.MyTask.Id)) EqualTimers.Add(__instance.MyTask.Id, 0f);
                EqualTimers[__instance.MyTask.Id] += Time.deltaTime;
                if (EqualTimers[__instance.MyTask.Id] >= 0.25f) // 0.25秒安定で完了
                {
                    // complete task
                    __instance.MyNormTask.NextStep();
                    __instance.StartCoroutine(__instance.CoStartClose());
                    EqualTimers[__instance.MyTask.Id] = float.MinValue;
                }
            }
            else
            {
                if (EqualTimers.ContainsKey(__instance.MyTask.Id)) EqualTimers[__instance.MyTask.Id] = 0f;
            }

            return false; // skip original Update
        }
    }
}
