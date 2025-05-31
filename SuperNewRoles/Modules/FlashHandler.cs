using System;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class FlashHandler
{
    public static void ShowFlash(Color color, float duration = 1f, Action OnFlashEnd = null)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

        var FullScreenRenderer = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, FastDestroyableSingleton<HudManager>.Instance.transform);
        var Renderer = FastDestroyableSingleton<HudManager>.Instance;
        Coroutine FlashCoroutine = null;
        FullScreenRenderer.gameObject.SetActive(true);
        FullScreenRenderer.enabled = true;

        if (Renderer == null || FullScreenRenderer == null)
        {
            Logger.Error("FlashHandler: Renderer or FullScreenRenderer is null");
            return;
        }

        FlashCoroutine = Renderer.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            if (p < 0.5f)
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f) * color.a);
                }
            }
            else
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f) * color.a);
                }
            }
            p *= duration;
            if (p >= duration && FullScreenRenderer != null)
            {
                GameObject.Destroy(FullScreenRenderer.gameObject);
                OnFlashEnd?.Invoke();
            }
        })));
    }
    [CustomRPC]
    public static void RpcShowFlash(this PlayerControl player, Color color, float duration = 1f)
    {
        if (PlayerControl.LocalPlayer != player) return;

        ShowFlash(color, duration);
    }
    [CustomRPC]
    public static void RpcShowFlashAll(Color color, float duration = 1f)
    {
        ShowFlash(color, duration);
    }
}
