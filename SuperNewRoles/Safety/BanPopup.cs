using System.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class BanPopup
{
    private const string PrefabName = "AnalyticsBG";
    private static GameObject _popup;
    private static BanInfo _pending;
    private static bool _swallowDisconnect;
    private static bool _suppressAutoShow;
    private static bool _closing;

    public static bool IsOpen => _popup;
    public static bool SwallowDisconnect => _swallowDisconnect || _popup;
    public static bool WasDismissed { get; private set; }
    public static bool HasQueuedBan => _pending != null;

    public static void Queue(BanInfo info)
    {
        if (info == null) return;
        _pending = info;
        _suppressAutoShow = false;
        WasDismissed = false;
    }

    public static void ClearPending()
    {
        if (_popup) return;
        _pending = null;
        _suppressAutoShow = false;
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
        _suppressAutoShow = false;
        if (_popup || _closing) return;
        BanInfo info = _pending;
        if (info == null && ConductGate.Last?.Banned == true)
            info = ConductGate.Last.Ban ?? new BanInfo();
        if (info == null) return;
        _pending = info;
        Show(info, runner);
    }

    public static bool TryHandle(MainMenuManager manager, ref GameObject currentPopup)
    {
        if (!_popup) return false;
        currentPopup = _popup;
        return true;
    }

    public static void HideVanillaDisconnect()
    {
        DisconnectPopup instance = DisconnectPopup.Instance;
        if (instance != null)
            instance.gameObject.SetActive(false);
    }

    public static void MarkSwallowDisconnect()
    {
        _swallowDisconnect = true;
    }

    public static void ClearSwallowDisconnect()
    {
        _swallowDisconnect = false;
    }

    public static bool ShouldSuppressAutoShow => _suppressAutoShow;

    public static void RebindToCamera()
    {
        if (WasDismissed || ShouldSuppressAutoShow)
        {
            if (_popup)
            {
                SafetyPopupUi.PlaceInFront(_popup);
                HideVanillaDisconnect();
            }
            return;
        }
        if (!_popup)
        {
            if (_pending == null || _closing) return;
            ShowNow(SafetyPopupUi.EnsureHost());
            return;
        }
        SafetyPopupUi.PlaceInFront(_popup);
        HideVanillaDisconnect();
    }

    private static void Show(BanInfo info, MonoBehaviour runner)
    {
        HideVanillaDisconnect();
        MonoBehaviour host = SafetyPopupUi.EnsureHost();
        _popup = AssetManager.Instantiate(PrefabName, host.transform);
        _popup.SetActive(true);
        SafetyPopupUi.PlaceInFront(_popup);

        Transform title = _popup.transform.Find("Title");
        if (title != null)
            title.GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyBanTitle");
        Transform text = _popup.transform.Find("Text");
        if (text != null)
        {
            TextMeshPro tmp = text.GetComponent<TextMeshPro>();
            if (tmp != null)
            {
                tmp.richText = true;
                tmp.enableWordWrapping = true;
                tmp.text = BanNotice.BuildBody(info);
            }
        }

        GameObject ok = _popup.transform.Find("AnalyticsButton")?.gameObject;
        if (ok != null)
        {
            Vector3 okPos = ok.transform.localPosition;
            ok.transform.localPosition = new Vector3(okPos.x, okPos.y, -5f);
            SafetyPopupUi.WireButton(ok, "SafetyBanOk", OnOk);
        }

        Transform close = _popup.transform.Find("CloseButton");
        if (close != null)
            close.gameObject.SetActive(false);

        SafetyPopupUi.WireBackdropAndCloseBox(_popup, OnOk);
        SafetyPopupUi.PlayOpenAnimation(_popup, host);
    }

    private static void OnOk()
    {
        _suppressAutoShow = true;
        _swallowDisconnect = true;
        CloseVisible();
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

    private static void FinishClose()
    {
        if (_popup)
            Object.Destroy(_popup);
        _popup = null;
        _closing = false;
        WasDismissed = true;
        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected)
            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
    }
}
