using HarmonyLib;
using UnityEngine;
using System.Linq;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedFindCritterEggTask
{
    [HarmonyPatch(typeof(FindCritterEggMinigame))]
    public static class FindCritterEggMinigamePatch
    {
        private static int LeafDuplicateCount = 100;
        [HarmonyPatch(nameof(FindCritterEggMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(FindCritterEggMinigame __instance)
        {
            if (!Main.IsCursed) return;

            // leavesの中から座標で左上と右下を特定
            var positions = __instance.leaves.Select(leaf => leaf.transform.localPosition).ToArray();
            float minX = positions.Min(p => p.x) + 2f;
            float maxX = positions.Max(p => p.x) - 2f;
            float minY = positions.Min(p => p.y) + 2f;
            float maxY = positions.Max(p => p.y) - 2f;

            // 定数個（例: 3個）まで複製して追加
            int duplicateCount = LeafDuplicateCount;
            var originalLeaves = __instance.leaves.ToList();

            for (int i = 0; i < duplicateCount; i++)
            {
                // ランダムに元のleaf（複製されていないもの）のみを選択して複製
                var randomLeaf = __instance.leaves[ModHelpers.GetRandomInt(__instance.leaves.Length - 1)];
                var newLeaf = Object.Instantiate(randomLeaf, __instance.transform);

                // 左上と右下の範囲内でランダムな位置に配置
                float randomX = ModHelpers.GetRandomFloat(maxX, minX);
                float randomY = ModHelpers.GetRandomFloat(maxY, minY);
                newLeaf.transform.localPosition = new Vector3(randomX, randomY, randomLeaf.transform.localPosition.z - 1);

                // 新しいleafをリストに追加
                originalLeaves.Add(newLeaf);
            }

            // 更新されたリストを配列に戻す
            __instance.leaves = originalLeaves.ToArray();
        }
    }
}