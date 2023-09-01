using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedStartFansTask
{
    public static Dictionary<uint, byte[]> Data;
    public static int[] CodeData;

    [HarmonyPatch(typeof(StartFansMinigame))]
    public static class StartFansMinigamePatch
    {
        [HarmonyPatch(nameof(StartFansMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(StartFansMinigame __instance)
        {
            if (!Main.IsCursed) return;
            List<Sprite> icon = __instance.IconSprites.ToList();
            icon.Add(ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Cursed.Mushroom.png", 100f));
            __instance.IconSprites = icon.ToArray();

            CodeData = new int[__instance.CodeIcons.Length];
            for (int i = 0; i < __instance.CodeIcons.Length; i++)
            {
                SpriteRenderer target = __instance.CodeIcons[i];
                __instance.CodeIcons[i].transform.parent.gameObject.SetActive(false);
                if (__instance.ConsoleId == 0) target.sprite = __instance.IconSprites[Data[__instance.MyTask.Id][i]];
                else
                {
                    CodeData[i] = 0;
                    __instance.CodeIcons[i].GetComponent<PassiveButton>().OnClick = new();
                    __instance.CodeIcons[i].GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => __instance.RotateImage(target)));
                }
            }
        }

        [HarmonyPatch(nameof(StartFansMinigame.RotateImage)), HarmonyPrefix]
        public static bool RotateImagePrefix(StartFansMinigame __instance, SpriteRenderer target)
        {
            if (!Main.IsCursed) return true;
            if (__instance.ConsoleId == 0) return false;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            int index = __instance.CodeIcons.ToList().FindIndex(x => x.name == target.name);
            int num = CodeData[index];
            num = (num + 1) % __instance.IconSprites.Length;
            target.sprite = __instance.IconSprites[num];
            CodeData[index] = num;
            for (int i = 0; i < __instance.CodeIcons.Length; i++)
            {
                if (Data[__instance.MyTask.Id][i] != CodeData[i])
                {
                    if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.cycleSound, false, 1f, null);
                    return false;
                }
            }
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.completeSound, false, 1f, null);
            __instance.MyNormTask.NextStep();
            __instance.StartCoroutine(__instance.CoStartClose(0.75f));
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.StartFans) return;
            __instance.MaxStep = 2;
            Data[__instance.Id] = new byte[__instance.Data.Length];
            for (int i = 0; i < __instance.Data.Length; i++) Data[__instance.Id][i] = IntRange.NextByte(0, 5);
            if (Data[__instance.Id].All(x => x == 0)) Data[__instance.Id][IntRange.Next(0, Data[__instance.Id].Length)] = IntRange.NextByte(1, 5);
        }
    }
}
