using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
class LightPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] NetworkedPlayerInfo player, ref float __result)
    {
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        // 電気系システムから明るさ補正値を計算
        float num = 1f;
        if (__instance.Systems.ContainsKey(SystemTypes.Electrical))
        {
            SwitchSystem switchSystem = __instance.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            if (switchSystem != null)
            {
                num = switchSystem.Value / 255f;
            }
        }
        ExPlayerControl exPlayer = (ExPlayerControl)player;
        // プレイヤーが null または死亡状態の場合は最大光量を返す
        if (player == null || player.IsDead)
        {
            __result = __instance.MaxLightRadius;
        }
        // インポスターまたはインポスター用の光量が有効な場合
        else if (exPlayer.HasImpostorVision())
        {
            __result = GetNeutralLightRadius(__instance, true);
        }
        // その他の場合は標準の中立光量を設定
        else
        {
            __result = GetNeutralLightRadius(__instance, false);
        }
        __result = ShipStatusLightEvent.Invoke(player, __result);
        return false;
    }

    public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor, float? timer = null)
    {
        if (isImpostor) return shipStatus.MaxLightRadius * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);

        float lerpValue = 1;
        if (timer.HasValue) lerpValue = timer.Value;
        else if (shipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elec)) lerpValue = elec.TryCast<SwitchSystem>().Value / 255f;
        var LocalPlayer = PlayerControl.LocalPlayer;
        // 反転
        // lerpValue = 1 - lerpValue;
        return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
    }
}