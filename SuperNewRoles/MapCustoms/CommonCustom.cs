using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using System.Linq;

namespace SuperNewRoles.MapCustoms;

public static class CommonCustom
{
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
    class NormalPlayerTaskInitializePatch
    {
        static void Postfix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.FixWiring || !MapEditSettingsOptions.WireTaskIsRandom) return;
            List<Console> orgList = ShipStatus.Instance.AllConsoles.Where((global::Console t) => t.TaskTypes.Contains(__instance.TaskType)).ToList<global::Console>();
            List<Console> list = new(orgList);

            __instance.MaxStep = MapEditSettingsOptions.WireTaskNum;
            __instance.Data = new byte[MapEditSettingsOptions.WireTaskNum];
            for (int i = 0; i < __instance.Data.Length; i++)
            {
                if (list.Count == 0)
                    list = new List<Console>(orgList);
                int index = ModHelpers.GetRandomIndex(list);
                __instance.Data[i] = (byte)list[index].ConsoleId;
                list.RemoveAt(index);
            }
            __instance.StartAt = orgList.FirstOrDefault(console => console.ConsoleId == __instance.Data[0]).Room;
        }
    }

    // MODカラー禁止
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
    public static class PlayerControlCheckColorPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
        {
            if (!GeneralSettingOptions.DisableModColor)
                return true;

            // MODカラーを禁止
            if (bodyColor >= Palette.PlayerColors.Length)
            {
                return false;
            }
            return true;
        }
    }
}