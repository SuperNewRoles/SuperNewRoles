using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

// partialキーワードを最初のクラス定義に追加
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static partial class KillTimerPatch
{
    // ゲーム開始時に初手キルクールタイムを10秒に設定するためのフラグ
    private static bool isFirstKillCooldown = true;

    public static void Postfix(PlayerControl __instance, float time)
    {
        float @float = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        if (!(@float <= 0f))
        {
            // ゲーム開始時は初手キルクールタイムを10秒に設定
            if (isFirstKillCooldown && time == @float)
            {
                isFirstKillCooldown = false;
                __instance.killTimer = 10f;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(10f, @float);
            }
            else
            {
                __instance.killTimer = time;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, @float);
            }
        }
    }

    // 同じクラス内にResetFirstKillCooldownメソッドを移動
    public static void ResetFirstKillCooldown()
    {
        isFirstKillCooldown = true;
    }
}

// ゲーム終了時にフラグをリセットする
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class ResetKillCooldownPatch
{
    public static void Postfix()
    {
        KillTimerPatch.ResetFirstKillCooldown();
    }
}

// ロビーに戻ったときもリセットする
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class ResetKillCooldownGameStartManagetStartPatch
{
    public static void Postfix()
    {
        KillTimerPatch.ResetFirstKillCooldown();
    }
}
