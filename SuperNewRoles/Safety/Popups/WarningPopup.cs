using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class WarningPopup
{
    private const string PrefabName = "AnalyticsBG";
    private static GameObject _popup;
    private static WarningInfo _pending;
    private static bool _closing;
    private static bool _acking;
    private static bool _showFailed;
    private static bool _deferredShowStarted;
    private static bool _ackReady;

    public static bool IsOpen => _popup;
    public static bool HasQueuedWarning => _pending != null;
    public static bool WasDismissed { get; private set; }

    public static void Queue(WarningInfo info)
    {
        if (info == null) return;
        if (ConductGate.IsBannedNow) return;
        _pending = info;
        _showFailed = false;
        WasDismissed = false;
    }

    public static void ReceiveFromServer(WarningInfo info)
    {
        Queue(info);
        if (_pending == null) return;
        MonoBehaviour host = SafetyPopupUi.EnsureHost();
        if (host == null || _deferredShowStarted) return;
        _deferredShowStarted = true;
        host.StartCoroutine(CoShowNextFrame(host).WrapToIl2Cpp());
    }

    public static void ClearPending()
    {
        if (_popup) return;
        _pending = null;
        WasDismissed = false;
    }

    public static void PrepareToShow()
    {
        WasDismissed = false;
    }

    public static IEnumerator WaitUntilDismissed(MonoBehaviour runner)
    {
        PrepareToShow();
        while (!WasDismissed)
        {
            if (!IsOpen)
                ShowNow(SafetyPopupUi.EnsureHost());
            RebindToCamera();
            yield return null;
        }
    }

    public static void ShowNow(MonoBehaviour runner)
    {
        if (ConductGate.IsBannedNow) return;
        if (_showFailed || _popup || _closing) return;
        WarningInfo info = _pending;
        if (info == null && ConductGate.Last?.HasUnackedWarning == true)
            info = ConductGate.Last.Warning;
        if (info == null) return;
        _pending = info;
        Show(info);
    }

    public static void RebindToCamera()
    {
        if (ConductGate.IsBannedNow)
        {
            if (_popup)
                CloseVisible();
            return;
        }
        if (_showFailed)
            return;
        if (WasDismissed)
        {
            if (_popup)
                SafetyPopupUi.PlaceWarning(_popup);
            return;
        }
        if (!_popup)
        {
            if (_pending == null || _closing) return;
            ShowNow(SafetyPopupUi.EnsureHost());
            return;
        }
        SafetyPopupUi.PlaceWarning(_popup);
    }

    public static void CloseVisible()
    {
        if (_closing)
            return;
        if (!_popup)
        {
            FinishClose();
            return;
        }
        _closing = true;
        SafetyPopupUi.PlayCloseAnimation(_popup, SafetyPopupUi.EnsureHost(), FinishClose);
    }

    private static IEnumerator CoShowNextFrame(MonoBehaviour host)
    {
        yield return null;
        _deferredShowStarted = false;
        ShowNow(host);
    }

    private static void Show(WarningInfo info)
    {
        try
        {
            MonoBehaviour host = SafetyPopupUi.EnsureHost();
            Transform parent = SafetyPopupUi.ResolveWarningParent(host.transform);
            _popup = AssetManager.Instantiate(PrefabName, parent);
            _popup.SetActive(true);
            SafetyPopupUi.PlaceWarning(_popup);

            Transform title = _popup.transform.Find("Title");
            if (title != null)
            {
                TextMeshPro titleTmp = title.GetComponent<TextMeshPro>();
                if (titleTmp != null)
                    titleTmp.text = ModTranslation.GetString("SafetyWarningTitle");
            }
            Transform text = _popup.transform.Find("Text");
            if (text != null)
            {
                TextMeshPro tmp = text.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.richText = true;
                    tmp.enableWordWrapping = true;
                    tmp.text = MarkdownToUnityTag.ConvertForSafetyPopup(info?.Body ?? string.Empty);
                    SafetyMarkdownLinks.Bind(tmp);
                }
            }

            GameObject ok = _popup.transform.Find("AnalyticsButton")?.gameObject;
            _ackReady = ok == null;
            if (ok != null)
            {
                Vector3 okPos = ok.transform.localPosition;
                ok.transform.localPosition = new Vector3(okPos.x, okPos.y, -5f);
                SafetyPopupUi.WireButton(ok, "SafetyWarningAck", OnAck);
                host.StartCoroutine(CoAckCountdown(ok, ModTranslation.GetString("SafetyWarningAck")).WrapToIl2Cpp());
            }

            Transform close = _popup.transform.Find("CloseButton");
            if (close != null)
                close.gameObject.SetActive(false);

            SafetyPopupUi.WireBackdropAndCloseBox(_popup, null);
            SafetyPopupUi.PlayOpenAnimation(_popup, host);
        }
        catch (Exception error)
        {
            Logger.Error(error, "WarningPopup");
            _showFailed = true;
            _deferredShowStarted = false;
            _ackReady = false;
            if (_popup)
                UnityEngine.Object.Destroy(_popup);
            _popup = null;
            _closing = false;
            WasDismissed = true;
        }
    }

    private static IEnumerator CoAckCountdown(GameObject button, string ackLabel)
    {
        float elapsed = 0f;
        int lastRemaining = -1;
        SpriteRenderer[] sprites = CollectButtonSprites(button);
        Color[] originalColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            originalColors[i] = sprites[i].color;

        SetAckButtonEnabled(button, false);
        ApplySpriteColors(sprites, originalColors, waiting: true);
        while (elapsed < WarningAckCountdown.DelaySeconds)
        {
            if (!_popup || !button)
                yield break;
            int remaining = WarningAckCountdown.RemainingDisplay(elapsed);
            if (remaining != lastRemaining)
            {
                SetAckButtonLabel(button, WarningAckCountdown.ButtonLabel(elapsed, ackLabel));
                lastRemaining = remaining;
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (!_popup || !button)
            yield break;
        SetAckButtonLabel(button, ackLabel);
        ApplySpriteColors(sprites, originalColors, waiting: false);
        SetAckButtonEnabled(button, true);
        _ackReady = true;
    }

    private static SpriteRenderer[] CollectButtonSprites(GameObject button)
    {
        Transform text = button.transform.Find("Text");
        List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        foreach (SpriteRenderer renderer in button.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (text != null && renderer.transform.IsChildOf(text))
                continue;
            sprites.Add(renderer);
        }
        return sprites.ToArray();
    }

    private static void ApplySpriteColors(SpriteRenderer[] sprites, Color[] originals, bool waiting)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (!sprites[i])
                continue;
            sprites[i].color = waiting ? WarningAckCountdown.Dim(originals[i]) : originals[i];
        }
    }

    private static void SetAckButtonLabel(GameObject button, string label)
    {
        Transform text = button.transform.Find("Text");
        if (text == null)
            return;
        TextMeshPro tmp = text.GetComponent<TextMeshPro>();
        if (tmp != null)
            tmp.text = label;
    }

    private static void SetAckButtonEnabled(GameObject button, bool enabled)
    {
        PassiveButton passive = button.GetComponent<PassiveButton>();
        if (passive != null)
            passive.enabled = enabled;
        Transform selected = button.transform.Find("Selected");
        if (selected != null && !enabled)
            selected.gameObject.SetActive(false);
    }

    private static void OnAck()
    {
        if (!_ackReady || _acking || _closing) return;
        WarningInfo info = _pending;
        if (info == null || string.IsNullOrEmpty(info.Id)) return;

        MonoBehaviour host = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (host == null)
            host = SafetyPopupUi.EnsureHost();
        if (host == null) return;

        _acking = true;
        host.StartCoroutine(PlayerSafetyApiClient.AckWarning(info.Id, ok =>
        {
            _acking = false;
            if (ok)
                ConductGate.MarkWarningAcked();
        }).WrapToIl2Cpp());
    }

    private static void FinishClose()
    {
        if (_popup)
            UnityEngine.Object.Destroy(_popup);
        _popup = null;
        _pending = null;
        _closing = false;
        _acking = false;
        _ackReady = false;
        _deferredShowStarted = false;
        WasDismissed = true;
    }
}

public static class WarningAckCountdown
{
    public const float DelaySeconds = 5f;
    public const float DimMultiplier = 0.55f;

    public static Color Dim(Color original)
    {
        return new Color(
            original.r * DimMultiplier,
            original.g * DimMultiplier,
            original.b * DimMultiplier,
            original.a);
    }

    public static bool IsReady(float elapsedSeconds) => elapsedSeconds >= DelaySeconds;

    public static int RemainingDisplay(float elapsedSeconds)
    {
        if (IsReady(elapsedSeconds))
            return 0;
        if (elapsedSeconds < 0f)
            elapsedSeconds = 0f;
        int remaining = (int)DelaySeconds - (int)elapsedSeconds;
        return remaining < 1 ? 1 : remaining;
    }

    public static string ButtonLabel(float elapsedSeconds, string ackLabel)
    {
        int remaining = RemainingDisplay(elapsedSeconds);
        return remaining > 0 ? remaining.ToString() : ackLabel ?? string.Empty;
    }
}
