using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

// partialキーワードを最初のクラス定義に追加
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static partial class KillTimerPatch
{

    public static void Postfix(PlayerControl __instance, float time)
    {
        __instance.killTimer = time;
        DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
    }
}