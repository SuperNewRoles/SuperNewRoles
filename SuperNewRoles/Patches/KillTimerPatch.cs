using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

// partialキーワードを最初のクラス定義に追加
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static partial class KillTimerPatch
{

    public static void Postfix(PlayerControl __instance, float time)
    {
        float timer = time;
        if (GameSettingOptions.ImmediateKillCooldown && Mathf.Approximately(time, 10f))
        {
            timer = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }
        __instance.killTimer = timer;
        DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(timer, GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
    }
}