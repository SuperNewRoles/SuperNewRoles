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
        if (Mathf.Approximately(time, 10f))
        {
            float defaultKillCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            switch (GameSettingOptions.InitialCooldown)
            {
                case InitialCooldownType.Immediate:
                    timer = defaultKillCooldown;
                    break;
                case InitialCooldownType.OneThird:
                    timer = defaultKillCooldown / 3f;
                    break;
                case InitialCooldownType.TenSeconds:
                    timer = 10f;
                    break;
            }
        }
        __instance.killTimer = timer;
        DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(timer, GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
    }
}