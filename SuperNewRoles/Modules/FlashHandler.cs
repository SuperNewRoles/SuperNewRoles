using System;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class FlashHandler
{
    public class FlashHandle
    {
        internal SpriteRenderer Renderer;
        internal HudManager Hud;
        internal Coroutine Coroutine;
        internal Action OnFlashEnd;
        private bool _destroyed;

        public void Stop(float duration = 0.5f)
        {
            if (_destroyed) return;
            if (Hud == null || Renderer == null)
            {
                DestroyInternal();
                return;
            }

            if (Coroutine != null)
            {
                Hud.StopCoroutine(Coroutine);
                Coroutine = null;
            }

            if (duration <= 0f)
            {
                DestroyInternal();
                return;
            }

            float startAlpha = Renderer.color.a;
            Coroutine = Hud.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                if (_destroyed) return;
                if (Renderer == null)
                {
                    DestroyInternal();
                    return;
                }

                var c = Renderer.color;
                c.a = Mathf.Lerp(startAlpha, 0f, p);
                Renderer.color = c;

                if (p >= 1f) DestroyInternal();
            })));
        }

        internal void FinishFromCoroutine()
        {
            if (_destroyed) return;
            _destroyed = true;
            if (Renderer != null) GameObject.Destroy(Renderer.gameObject);
            Renderer = null;
            Coroutine = null;
            Hud = null;
            OnFlashEnd?.Invoke();
            OnFlashEnd = null;
        }

        private void DestroyInternal()
        {
            if (_destroyed) return;
            _destroyed = true;
            if (Hud != null && Coroutine != null)
            {
                Hud.StopCoroutine(Coroutine);
            }
            if (Renderer != null) GameObject.Destroy(Renderer.gameObject);
            Renderer = null;
            Coroutine = null;
            Hud = null;
            OnFlashEnd?.Invoke();
            OnFlashEnd = null;
        }
    }

    public static FlashHandle ShowFlashHandle(Color color, float duration = 1f, Action OnFlashEnd = null)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return null;

        var FullScreenRenderer = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, FastDestroyableSingleton<HudManager>.Instance.transform);
        var Renderer = FastDestroyableSingleton<HudManager>.Instance;
        FlashHandle handle = new()
        {
            Renderer = FullScreenRenderer,
            Hud = Renderer,
            OnFlashEnd = OnFlashEnd
        };
        Coroutine FlashCoroutine = null;
        FullScreenRenderer.gameObject.SetActive(true);
        FullScreenRenderer.enabled = true;

        if (Renderer == null || FullScreenRenderer == null)
        {
            Logger.Error("FlashHandler: Renderer or FullScreenRenderer is null");
            return null;
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
                handle.FinishFromCoroutine();
            }
        })));

        handle.Coroutine = FlashCoroutine;
        return handle;
    }

    public static void ShowFlash(Color color, float duration = 1f, Action OnFlashEnd = null)
    {
        ShowFlashHandle(color, duration, OnFlashEnd);
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
