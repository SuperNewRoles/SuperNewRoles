using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedRecordsTask
{
    [HarmonyPatch(typeof(RecordsMinigame))]
    public static class RecordsMinigamePatch
    {
        public static List<(Vector3 pos, Vector3 angles)> Data = new()
        {
            (new(-1.0799f, 1.4986f, -50f), new(0f, 0f, 0f)),
            (new(1.0273f, 0.3575f, -50f), new(0f, 0f, 339.4907f)),
            (new(-3.1164f, -1.0466f, -50f), new(0f, 0f, 29.1632f)),
            (new(-0.8909f, -1.1273f, -50f), new(0f, 0f, 318.0361f)),
            (new(0.9164f, -1.3884f, -50f), new(0f, 0f, 12.6179f)),
            (new(2.6764f, 0.5993f, -50f), new(0f, 0f, 36.9813f)),
            (new(1.0509f, 1.5593f, -50f), new(0f, 0f, 349.6722f)),
            (new(3.2436f, -1.0557f, -50f), new(0f, 0f, 316.0715f)),
        };

        [HarmonyPatch(nameof(RecordsMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(RecordsMinigame __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.ConsoleId == 0)
            {
                GameObject copy = Object.Instantiate(__instance.Folders[0].gameObject, __instance.transform);
                foreach (var folder in __instance.Folders) Object.Destroy(folder.gameObject);
                __instance.transform.Find("BackgroundClose").gameObject.SetActive(false);
                __instance.Folders = new SpriteRenderer[__instance.MyNormTask.Data.Length];
                for (int i = 0;  i < __instance.MyNormTask.Data.Length; i++)
                {
                    GameObject folder = Object.Instantiate(copy, __instance.FoldersContent.transform);
                    folder.name = $"records_folder {i}";
                    folder.transform.localPosition = Data[i].pos;
                    folder.transform.eulerAngles = Data[i].angles;
                    SpriteRenderer sprite = folder.GetComponent<SpriteRenderer>();
                    PassiveButton button = folder.GetComponent<PassiveButton>();
                    button.OnClick = new();
                    int id = i;
                    button.OnClick.AddListener((Action)(() =>
                    {
                        Color32 color = sprite.color;
                        color.a = (byte)(byte.MaxValue - id);
                        sprite.color = color;
                        Logger.Info($"id : {id}", "CursedRecordsTask");
                        __instance.GrabFolder(sprite);
                    }));
                    __instance.Folders[i] = sprite;
                    folder.SetActive(__instance.MyNormTask.Data[i] == 0);
                }
                Object.Destroy(copy);
            }
        }

        [HarmonyPatch(nameof(RecordsMinigame.GrabFolder)), HarmonyPrefix]
        public static bool GrabFolderPrefix(RecordsMinigame __instance, SpriteRenderer folder)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None) return false;
            folder.gameObject.SetActive(false);
            int id = byte.MaxValue - ((Color32)folder.color).a;
            __instance.MyNormTask.Data[id] = IntRange.NextByte(1, 9);
            __instance.MyNormTask.UpdateArrowAndLocation();
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.grabDocument, false, 1f, null);
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
            if (__instance.TaskType != TaskTypes.SortRecords) return;
            __instance.Data = new byte[8];
            __instance.MaxStep = 8;
        }
    }
}
