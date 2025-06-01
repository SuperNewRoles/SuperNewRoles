using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SuperNewRoles.Patches;

// インポスター数の人数別の制限を削除
[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
class GameOptionsFixPatch
{
    public static bool Prefix(ref int __result)
    {
        __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        return false;
    }
}
[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class GameSettingMenuPatch
{
    // 各オプションの有効範囲を辞書で定義（キー：オプションタイトル、値：有効範囲の最小値と最大値）
    public static readonly Dictionary<StringNames, (float min, float max)> validRanges = new()
    {
        [StringNames.GameNumImpostors] = (0f, 15f),
        [StringNames.GameKillCooldown] = (2.5f, 60f),
        [StringNames.GamePlayerSpeed] = (-5f, 5f),
        [StringNames.GameCommonTasks] = (0f, 12f),
        [StringNames.GameLongTasks] = (0f, 69f),
        [StringNames.GameShortTasks] = (0f, 45f),
    };
    public static void Postfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if (tabNum != 1)
            return;

        if (__instance.GameSettingsTab == null || __instance.GameSettingsTab.Children == null)
            throw new Exception("GameSettingsTab is null");
        foreach (var child in __instance.GameSettingsTab.Children)
        {
            if (validRanges.TryGetValue(child.Title, out var range))
            {
                var numberOption = child.TryCast<NumberOption>();
                if (numberOption == null)
                {
                    Logger.Error("NumberOption is null");
                    continue;
                }
                numberOption.ValidRange = new(range.min, range.max);
            }
        }
    }
}