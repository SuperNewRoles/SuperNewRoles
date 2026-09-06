using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class HiddenLightRenderingPatch
{
    private static LightSource skippedLight;

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(LightSourceGpuRenderer), nameof(LightSourceGpuRenderer.Render));
        yield return AccessTools.Method(typeof(LightSourceRaycastRenderer), nameof(LightSourceRaycastRenderer.Render));
    }

    public static bool Prefix(LightSourceRenderer __instance)
    {
        var player = PlayerControl.LocalPlayer;
        var client = AmongUsClient.Instance;
        if (player == null || client == null || client.GameState != InnerNetClient.GameStates.Started ||
            GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal ||
            __instance.lightSource != player.lightSource)
            return true;

        var hud = HudManager.Instance;
        if (hud == null || hud.ShadowQuad == null || hud.ShadowQuad.gameObject.activeSelf)
        {
            skippedLight = null;
            return true;
        }

        // 死亡・ホーク等で暗がり自体を隠している間は、影用メッシュを生成しない。
        // LightSource.Update の位置・懐中電灯入力・マテリアル更新は継続する。
        skippedLight = player.lightSource;
        return false;
    }

    [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
    public static class RefreshRestoredLight
    {
        public static void Postfix()
        {
            if (skippedLight == null)
                return;

            var player = PlayerControl.LocalPlayer;
            if (player == null || player.lightSource != skippedLight)
            {
                skippedLight = null;
                return;
            }

            var hud = HudManager.Instance;
            if (hud == null || hud.ShadowQuad == null || !hud.ShadowQuad.gameObject.activeSelf)
                return;

            // Updateの途中で視界を戻した場合も、描画前に移動先の影を一度作り直す。
            var light = skippedLight;
            skippedLight = null;
            if (light.isActiveAndEnabled)
                light.Update();
        }
    }
}
