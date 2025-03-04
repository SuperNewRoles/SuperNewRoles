using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
public static class KillTimerPatch
{
    public static void Postfix(PlayerControl __instance, float time)
    {
        float @float = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        if (!(@float <= 0f))
        {
            __instance.killTimer = time;
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, @float);
        }
    }
}
