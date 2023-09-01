using System.Collections.Generic;
using HarmonyLib;
using Il2CppSystem.Text;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedSampleTask
{
    public static Dictionary<uint, CursedSample> Data;
    public const float TimePerStep = 60f;

    [HarmonyPatch(typeof(SampleMinigame))]
    public static class SampleMinigamePatch
    {
        [HarmonyPatch(nameof(SampleMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(SampleMinigame __instance)
        {
            if (!Main.IsCursed) return;
            __instance.TimePerStep = TimePerStep;
        }

        [HarmonyPatch(nameof(SampleMinigame.FixedUpdate)), HarmonyPostfix]
        public static void FixedUpdatePostfix(SampleMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (!Data.ContainsKey(__instance.MyTask.Id)) return;
            if (Data[__instance.MyTask.Id].IsDeterioration)
            {
                if (Data[__instance.MyTask.Id].DeteriorationTimer > 0) __instance.UpperText.text = ModTranslation.GetString("CursedSampleTaskUpperText1");
                else __instance.UpperText.text = ModTranslation.GetString("CursedSampleTaskUpperText2");

                SpriteRenderer sprite = __instance.Tubes[__instance.AnomalyId];
                if (!sprite) return;
                Color color = Color.red;
                float dealing = Data[__instance.MyTask.Id].DeteriorationTimer / CursedSample.DeteriorationTime;
                color.r = Mathf.Clamp01(dealing);
                color.b = Mathf.Clamp01(1 - dealing);
                sprite.color = color;
            }
        }

        [HarmonyPatch(nameof(SampleMinigame.NextStep)), HarmonyPostfix]
        public static void NextStepPostfix(SampleMinigame __instance)
        {
            if (!Main.IsCursed) return;
            Data[__instance.MyTask.Id] = new();
        }

        [HarmonyPatch(nameof(SampleMinigame.SelectTube)), HarmonyPrefix]
        public static void SelectTubePrefix(SampleMinigame __instance, ref int tubeId)
        {
            if (!Main.IsCursed) return;
            if (!Data.ContainsKey(__instance.MyTask.Id)) return;
            if (Data[__instance.MyTask.Id].IsDeterioration && Data[__instance.MyTask.Id].DeteriorationTimer <= 0)
            {
                tubeId = __instance.AnomalyId == 0 ? 1 : 0;
                Data[__instance.MyTask.Id].IsMiss = true;
            }
            Data[__instance.MyTask.Id].IsStart = false;
            Data[__instance.MyTask.Id].IsDeterioration = false;
            Data[__instance.MyTask.Id].DeteriorationTimer = CursedSample.DeteriorationTime;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.FixedUpdate)), HarmonyPostfix]
        public static void FixedUpdatePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (!Data.ContainsKey(__instance.Id)) return;
            if (__instance.TaskType != TaskTypes.InspectSample) return;
            if (Data[__instance.Id].IsStart)
            {
                if (__instance.TaskTimer > 0) return;
                Data[__instance.Id].IsDeterioration = true;
                if (Data[__instance.Id].DeteriorationTimer > 0) Data[__instance.Id].DeteriorationTimer -= Time.fixedDeltaTime;
                else Data[__instance.Id].DeteriorationTimer = 0;
            }
        }

        [HarmonyPatch(nameof(NormalPlayerTask.AppendTaskText)), HarmonyPrefix]
        public static bool AppendTaskTextPrefix(NormalPlayerTask __instance, [HarmonyArgument(0)] StringBuilder sb)
        {
            if (!Main.IsCursed) return true;
            if (!Data.ContainsKey(__instance.Id)) return true;
            if (__instance.TaskType != TaskTypes.InspectSample) return true;

            string room = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt);
            string task = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.TaskType);
            string timer = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv), (int)Data[__instance.Id].DeteriorationTimer);

            if (Data[__instance.Id].IsMiss)
            {
                sb.AppendLine($"{room}: {task}");
                return false;
            }
            if (!Data[__instance.Id].IsDeterioration) return true;

            if (Data[__instance.Id].DeteriorationTimer > 0) sb.AppendLine($"<color=#FFFF00FF>{room}: {task} </color><color=#B00000>({timer})</color>");
            if (Data[__instance.Id].DeteriorationTimer <= 0) sb.AppendLine($"{room}: {task}");
            return false;
        }
    }

    public class CursedSample
    {
        public static readonly float DeteriorationTime = 25f;
        public bool IsStart;
        public bool IsDeterioration;
        public float DeteriorationTimer;
        public bool IsMiss;

        public CursedSample()
        {
            IsStart = true;
            IsDeterioration = false;
            DeteriorationTimer = DeteriorationTime;
            IsMiss = false;
        }
    }
}
