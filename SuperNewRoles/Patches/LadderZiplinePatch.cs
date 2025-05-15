using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;
using InnerNet; // ShipStatus を使うため
using static SuperNewRoles.CustomOptions.Categories.MapEditSettingsOptions;
using static SuperNewRoles.CustomOptions.Categories.MapSettingOptions; // ZiplineCoolChangeOptionなど汎用設定のため

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(Ladder))]
public static class LadderPatch
{
    [HarmonyPatch(nameof(Ladder.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
    public static bool LadderMaxCoolDownGetterPrefix(ref float __result)
    {
        if (MapSettingOptions.LadderCoolChangeOption)
        {
            __result = MapSettingOptions.LadderCoolTimeOption;
            if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __result = MapSettingOptions.LadderImpostorCoolTimeOption;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(Ladder.Use)), HarmonyPostfix]
    public static void LadderUsePostfix(Ladder __instance)
    {
        if (!MapSettingOptions.LadderCoolChangeOption) return;
        __instance.CoolDown = MapSettingOptions.LadderCoolTimeOption;
        if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.CoolDown = MapSettingOptions.LadderImpostorCoolTimeOption;
    }

    [HarmonyPatch(nameof(Ladder.SetDestinationCooldown)), HarmonyPostfix]
    public static void LadderSetDestinationCooldownPostfix(Ladder __instance)
    {
        if (!MapSettingOptions.LadderCoolChangeOption) return;
        __instance.Destination.CoolDown = MapSettingOptions.LadderCoolTimeOption;
        if (MapSettingOptions.LadderImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
            __instance.Destination.CoolDown = MapSettingOptions.LadderImpostorCoolTimeOption;
    }
}

[HarmonyPatch(typeof(ZiplineConsole))]
public static class ZiplineConsolePatch
{
    private static bool IsFungleMap()
    {
        return ShipStatus.Instance != null && ShipStatus.Instance.Type == ShipStatus.MapType.Fungle;
    }

    [HarmonyPatch(nameof(ZiplineConsole.CanUse))]
    [HarmonyPrefix]
    public static bool CanUsePrefix(ZiplineConsole __instance, ref float __result, PlayerControl pc, out bool canUse, out bool couldUse)
    {
        canUse = true;
        couldUse = true;
        if (ModHelpers.Not(IsFungleMap() && TheFungleSetting && TheFungleZiplineOption)) return true;
        if (!TheFungleCanUseZiplineOption)
        {
            canUse = false;
            couldUse = false;
            __result = float.MaxValue;
            return false; // Useメソッドの実行を止める
        }

        // Ziplineの昇り降り方向のチェック
        if (TheFungleZiplineUpOrDown != FungleZiplineDirectionOptions.TheFungleZiplineAlways)
        {
            // Ziplineには直接的に「上がコンソールAで下がコンソールB」のような情報がない場合がある。
            bool isMovingUp = __instance.transform.position.y > __instance.transform.position.y;
            bool isMovingDown = __instance.destination.transform.position.y < __instance.transform.position.y;

            if (isMovingUp && TheFungleZiplineUpOrDown == FungleZiplineDirectionOptions.TheFungleZiplineOnlyDown)
            {
                // 上昇しようとしているが、下降のみ許可されている場合
                canUse = false;
                couldUse = false;
                __result = float.MaxValue;
                return false;
            }
            else if (isMovingDown && TheFungleZiplineUpOrDown == FungleZiplineDirectionOptions.TheFungleZiplineOnlyUp)
            {
                // 下降しようとしているが、上昇のみ許可されている場合
                canUse = false;
                couldUse = false;
                __result = float.MaxValue;
                return false;
            }
        }
        return true; // 通常の処理を続行
    }

    [HarmonyPatch(nameof(ZiplineConsole.MaxCoolDown), MethodType.Getter), HarmonyPrefix]
    public static bool ZiplineConsoleMaxCoolDownGetterPrefix(ref float __result)
    {
        if (ZiplineCoolChangeOption)
        {
            __result = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __result = ZiplineImpostorCoolTimeOption;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(ZiplineConsole.Use)), HarmonyPostfix]
    public static void ZiplineConsoleUsePostfix(ZiplineConsole __instance)
    {
        if (ZiplineCoolChangeOption)
        {
            __instance.CoolDown = ZiplineCoolTimeOption; // MaxCoolDownの値が使われるはずなので、これは不要になる可能性
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __instance.CoolDown = ZiplineImpostorCoolTimeOption; // 同上
        }
    }

    [HarmonyPatch(nameof(ZiplineConsole.SetDestinationCooldown)), HarmonyPostfix]
    public static void ZiplineConsoleSetDestinationCooldownPostfix(ZiplineConsole __instance)
    {
        if (ZiplineCoolChangeOption)
        {
            // Useと同様の理由で、MaxCoolDown側で設定されていれば不要な可能性あり
            __instance.destination.CoolDown = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                __instance.destination.CoolDown = ZiplineImpostorCoolTimeOption;
        }
    }
}