using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedFrisbeeTask
{
    public static Dictionary<uint, int> ThrowCount = new();

    // 失敗時のFrisbee落下位置リスト (TestFrisbeeMinigameのローカル座標)
    // originalFrisbeePositionからの相対位置として定義
    private static readonly List<Vector3> FailPositions = new()
    {
        new Vector3(0f, -0.8f, 0f),   // 少し下
        new Vector3(0.5f, -0.5f, 0f), // 右下
        new Vector3(-0.5f, -0.5f, 0f), // 左下
        new Vector3(0.3f, -0.3f, 0f),  // 右下寄り
        new Vector3(-0.3f, -0.3f, 0f), // 左下寄り
        new Vector3(0f, -1.2f, 0f),   // もっと下
    };

    // 各タスクの失敗位置を保存するための辞書
    private static Dictionary<uint, Vector3> FailPositionsCache = new();

    [HarmonyPatch(typeof(TestFrisbeeMinigame))]
    public static class TestFrisbeeMinigamePatch
    {
        [HarmonyPatch(nameof(TestFrisbeeMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(TestFrisbeeMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (!ThrowCount.ContainsKey(__instance.MyTask.Id)) ThrowCount.Add(__instance.MyTask.Id, 0);

            // デバッグ: Frisbeeの位置情報をログ出力
            SuperNewRoles.Logger.Info($"Frisbee Task Begin - Original: {__instance.originalFrisbeePosition}, Final: {__instance.finalFrisbeePosition.localPosition}");
        }

        [HarmonyPatch(nameof(TestFrisbeeMinigame.ThrowFrisbee)), HarmonyPostfix]
        public static void ThrowFrisbeePostfix(TestFrisbeeMinigame __instance)
        {
            if (!Main.IsCursed) return;
            ThrowCount[__instance.MyTask.Id]++;

            // 新しい投げごとに失敗位置をリセット
            if (FailPositionsCache.ContainsKey(__instance.MyTask.Id))
            {
                FailPositionsCache.Remove(__instance.MyTask.Id);
            }
        }

        [HarmonyPatch(nameof(TestFrisbeeMinigame.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(TestFrisbeeMinigame __instance)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;

            if (__instance.thrown && !__instance.isCompleting)
            {
                __instance.time += Time.deltaTime;
                float t = __instance.time / __instance.totalSeconds;

                int currentThrow = ThrowCount[__instance.MyTask.Id];

                if (currentThrow >= 4)
                {
                    // 4回目で成功 - 元の成功アニメーションを実行
                    __instance.throwableFrisbee.transform.localPosition = Vector3.Lerp(__instance.throwStartPosition, __instance.finalFrisbeePosition.localPosition, t);
                    __instance.throwableFrisbee.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, t);

                    if (Mathf.Approximately(0f, __instance.throwableFrisbee.transform.localScale.x))
                    {
                        __instance.thrown = false;
                        SuperNewRoles.Logger.Info($"Frisbee Success - Throw: {currentThrow}");
                        __instance.StartCoroutine(__instance.CoFinalAnimation());
                    }
                }
                else
                {
                    // 3回までは失敗 - 途中で失敗するアニメーション
                    // 投げ始めてから少しの間だけ飛んで、途中で失敗位置に落ちる
                    float failTimeRatio = 0.3f; // アニメーションの30%の位置で失敗

                    // 失敗位置をキャッシュから取得、なければ新規作成
                    if (!FailPositionsCache.ContainsKey(__instance.MyTask.Id))
                    {
                        FailPositionsCache[__instance.MyTask.Id] = FailPositions[ModHelpers.GetRandomIndex(FailPositions)];
                    }
                    Vector3 failPosition = FailPositionsCache[__instance.MyTask.Id];

                    if (t < failTimeRatio)
                    {
                        // まだ飛んでいる途中
                        Vector3 currentTarget = Vector3.Lerp(__instance.throwStartPosition, __instance.finalFrisbeePosition.localPosition, t / failTimeRatio);
                        __instance.throwableFrisbee.transform.localPosition = currentTarget;
                        __instance.throwableFrisbee.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        // 失敗位置に落ち始める
                        float fallT = (t - failTimeRatio) / (1f - failTimeRatio);

                        // 現在の位置から失敗位置へ落下
                        Vector3 currentPos = __instance.throwableFrisbee.transform.localPosition;
                        __instance.throwableFrisbee.transform.localPosition = Vector3.Lerp(currentPos, failPosition, fallT * 2f); // 早く落ちる
                        __instance.throwableFrisbee.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.8f, fallT); // 少し縮小

                        if (fallT >= 1f || t >= 1f)
                        {
                            __instance.thrown = false;

                            // 最終的に失敗位置に設定
                            __instance.throwableFrisbee.transform.localPosition = failPosition;
                            __instance.throwableFrisbee.transform.localScale = Vector3.one;

                            // デバッグ: 失敗位置をログ出力
                            SuperNewRoles.Logger.Info($"Frisbee Fail - Throw: {currentThrow}, Position: {failPosition}");

                            // 投げ状態をリセットして再度投げられるようにする
                            __instance.time = 0f;
                        }
                    }
                }

                return false;
            }

            return true;
        }
    }

}
