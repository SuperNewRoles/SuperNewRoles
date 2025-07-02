using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using System.Linq;
using SuperNewRoles.CustomCosmetics;

namespace SuperNewRoles.MapCustoms;

public static class CommonCustom
{
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
    class NormalPlayerTaskInitializePatch
    {
        static void Postfix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.FixWiring || !MapSettingOptions.WireTaskIsRandom) return;
            List<Console> orgList = ShipStatus.Instance.AllConsoles.Where((global::Console t) => t.TaskTypes.Contains(__instance.TaskType)).ToList<global::Console>();
            List<Console> list = new(orgList);

            __instance.MaxStep = MapSettingOptions.WireTaskNum;
            __instance.Data = new byte[MapSettingOptions.WireTaskNum];
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
    private static class PlayerControlCheckColorPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor)
        {
            if (GeneralSettingOptions.DisableModColor && CustomColors.DefaultPickAbleColors < PlayerControl.AllPlayerControls.Count) return true;
            uint pickAble = GeneralSettingOptions.DisableModColor ? CustomColors.DefaultPickAbleColors : CustomColors.PickAbleColors;
            // Fix incorrect color assignment
            uint color = bodyColor;
            if (IsTaken(__instance, color) || color >= pickAble)
            {
                int num = 0;
                while (num++ < 50 && (color >= pickAble || IsTaken(__instance, color)))
                {
                    color = (color + 1) % pickAble;
                }
            }
            //Logger.Info(color.ToString() + "をセット:" + isTaken(__instance, color).ToString()+":"+ (color >= Palette.PlayerColors.Length));
            __instance.RpcSetColor((byte)color);
            return false;
        }
        private static bool IsTaken(PlayerControl player, uint color)
        {
            foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers)
            {
                //Logger.Info($"{!p.Disconnected} は {p.PlayerId != player.PlayerId} は {p.DefaultOutfit.ColorId == color}", "isTaken");
                if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                    return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    private static class GameStartManagerUpdatePatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if (!GeneralSettingOptions.DisableModColor) return;
            if (CustomColors.DefaultPickAbleColors < PlayerControl.AllPlayerControls.Count) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player) continue;
                if (player.Data.DefaultOutfit.ColorId < CustomColors.DefaultPickAbleColors) continue;
                player.CheckColor((byte)player.Data.DefaultOutfit.ColorId);
            }
        }
    }
}