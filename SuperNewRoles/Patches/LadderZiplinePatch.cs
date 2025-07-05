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
        
        // ジップラインのクールダウンを設定値に合わせて更新
        if (ZiplineCoolChangeOption && IsFungleMap())
        {
            float cooldownTime = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                cooldownTime = ZiplineImpostorCoolTimeOption;
            
            if (cooldownTime <= 0f)
                cooldownTime = 0f;
            
            // 現在のクールダウンが設定値と異なる場合、修正する
            if (Mathf.Abs(__instance.CoolDown - cooldownTime) > 0.01f)
            {
                __instance.CoolDown = cooldownTime;
            }
            
            if (__instance.destination != null && Mathf.Abs(__instance.destination.CoolDown - cooldownTime) > 0.01f)
            {
                __instance.destination.CoolDown = cooldownTime;
            }
        }
        
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
            
            // 0秒以下の場合は0にする
            if (__result <= 0f)
                __result = 0f;
            
            return false;
        }
        return true;
    }


    [HarmonyPatch(nameof(ZiplineConsole.Use)), HarmonyPostfix]
    public static void ZiplineConsoleUsePostfix(ZiplineConsole __instance)
    {
        if (ZiplineCoolChangeOption)
        {
            float cooldownTime = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                cooldownTime = ZiplineImpostorCoolTimeOption;
            
            // 0秒の場合は即座に再利用可能にする
            if (cooldownTime <= 0f)
            {
                __instance.CoolDown = 0f;
                // 次のフレームで再度0に設定（確実にするため）
                new LateTask(() => {
                    if (__instance != null)
                        __instance.CoolDown = 0f;
                }, 0.01f, "No Name Task");
                
                // さらに確実にするため、0.1秒後にもう一度設定
                new LateTask(() => {
                    if (__instance != null)
                        __instance.CoolDown = 0f;
                }, 0.1f, "No Name Task");
            }
            else
            {
                __instance.CoolDown = cooldownTime;
                
                // 少し遅延してもう一度設定（確実にするため）
                new LateTask(() => {
                    if (__instance != null)
                        __instance.CoolDown = cooldownTime;
                }, 0.01f, "No Name Task");
            }
            
            // ログ出力してクールダウンが正しく設定されたことを確認
            Logger.Info($"Zipline cooldown set to {cooldownTime}s (current: {__instance.CoolDown}s)");
        }
    }

    [HarmonyPatch(nameof(ZiplineConsole.SetDestinationCooldown)), HarmonyPostfix]
    public static void ZiplineConsoleSetDestinationCooldownPostfix(ZiplineConsole __instance)
    {
        if (ZiplineCoolChangeOption)
        {
            float cooldownTime = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                cooldownTime = ZiplineImpostorCoolTimeOption;
            
            // 0秒の場合は即座に再利用可能にする
            if (cooldownTime <= 0f)
            {
                if (__instance.destination != null)
                {
                    __instance.destination.CoolDown = 0f;
                    // 次のフレームで再度0に設定（確実にするため）
                    new LateTask(() => {
                        if (__instance != null && __instance.destination != null)
                            __instance.destination.CoolDown = 0f;
                    }, 0.01f, "No Name Task");
                    
                    // さらに確実にするため、0.1秒後にもう一度設定
                    new LateTask(() => {
                        if (__instance != null && __instance.destination != null)
                            __instance.destination.CoolDown = 0f;
                    }, 0.1f, "No Name Task");
                }
            }
            else
            {
                if (__instance.destination != null)
                {
                    __instance.destination.CoolDown = cooldownTime;
                    
                    // 少し遅延してもう一度設定（確実にするため）
                    new LateTask(() => {
                        if (__instance != null && __instance.destination != null)
                            __instance.destination.CoolDown = cooldownTime;
                    }, 0.01f, "No Name Task");
                }
            }
            
            // ログ出力してクールダウンが正しく設定されたことを確認
            Logger.Info($"Zipline destination cooldown set to {cooldownTime}s (current: {__instance.destination?.CoolDown}s)");
        }
    }

    [HarmonyPatch(nameof(ZiplineConsole.Update)), HarmonyPostfix]
    public static void ZiplineConsoleUpdatePostfix(ZiplineConsole __instance)
    {
        // カスタムクールダウンが有効な場合のみ処理
        if (!ZiplineCoolChangeOption) return;
        
        // The Fungleマップでのみ処理
        if (!IsFungleMap()) return;
        
        // クールダウンを手動で減らす処理（元のUpdate処理を補完）
        if (__instance.CoolDown > 0f)
        {
            __instance.CoolDown = Mathf.Max(__instance.CoolDown - Time.deltaTime, 0f);
        }
        
        // 目的地のクールダウンも手動で減らす処理
        if (__instance.destination != null && __instance.destination.CoolDown > 0f)
        {
            __instance.destination.CoolDown = Mathf.Max(__instance.destination.CoolDown - Time.deltaTime, 0f);
        }
    }

}