using System.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Safety;

public static class ConductPlayDecision
{
    public enum Result
    {
        Allow,
        Fetch,
        ShowConduct,
        ShowBan,
        ShowWarning,
    }

    public static Result Decide(bool identityEnabled, bool fetched, bool banned, bool consented, bool unackedWarning = false)
    {
        if (!identityEnabled) return Result.Allow;
        if (!fetched) return Result.Fetch;
        if (banned) return Result.ShowBan;
        if (unackedWarning) return Result.ShowWarning;
        if (consented) return Result.Allow;
        return Result.ShowConduct;
    }

    public static bool CanPlayOfficial(bool banned, bool consented, bool unackedWarning)
    {
        return Decide(true, true, banned, consented, unackedWarning) == Result.Allow;
    }
}

public static class ConductGate
{
    private static bool _canPlayOfficial = true;

    public static bool Fetched { get; private set; }
    public static bool CanPlayOfficial
    {
        get => _canPlayOfficial && !HasUnackedWarning;
        private set => _canPlayOfficial = value;
    }
    public static ConductResponse Last { get; private set; }
    public static bool IsBannedNow => Last?.Banned == true || BanPopup.HasQueuedBan;
    public static bool HasUnackedWarning
    {
        get
        {
            if (IsBannedNow) return false;
            if (Last != null && Last.HasUnackedWarning) return true;
            return WarningPopup.HasQueuedWarning;
        }
    }

    public static void Apply(ConductResponse response)
    {
        if (response == null)
        {
            CanPlayOfficial = true;
            return;
        }

        Last = response;
        Fetched = true;
        CanPlayOfficial = !response.Banned && response.Consented;
        ConductPlayDecision.Result decision = ConductPlayDecision.Decide(
            identityEnabled: true,
            fetched: true,
            banned: response.Banned,
            consented: response.Consented,
            unackedWarning: response.HasUnackedWarning);

        switch (decision)
        {
            case ConductPlayDecision.Result.ShowBan:
                WarningPopup.CloseVisible();
                BanPopup.Queue(response.Ban ?? new BanInfo());
                return;
            case ConductPlayDecision.Result.ShowWarning:
                BanPopup.ClearPending();
                WarningPopup.Queue(response.Warning);
                if (!response.Consented)
                    ConductPopup.Queue(response);
                return;
            case ConductPlayDecision.Result.ShowConduct:
                BanPopup.ClearPending();
                WarningPopup.ClearPending();
                ConductPopup.Queue(response);
                return;
            default:
                BanPopup.ClearPending();
                WarningPopup.ClearPending();
                return;
        }
    }

    public static void MarkConsented()
    {
        CanPlayOfficial = Last == null || !Last.Banned;
        ConductPopup.WasDeclined = false;
        ConductPopup.CloseVisible();
    }

    public static void MarkWarningAcked()
    {
        if (Last != null && Last.Warning != null)
            Last = new ConductResponse(Last.Version, Last.Body, Last.Consented, Last.Banned, Last.Ban, null);
        WarningPopup.CloseVisible();
    }

    public static void InvalidateFetched()
    {
        Fetched = false;
    }
}

public static class ConductPopup
{
    private const string PrefabName = "AnalyticsBG";
    private static GameObject _popup;
    private static GameObject _decline;
    private static ConductResponse _pending;
    private static bool _busy;
    private static bool _fetching;
    private static bool _closing;
    private static MonoBehaviour _runner;

    public static ConductResponse Pending => _pending;
    public static bool IsBusy => _busy && _popup;
    public static bool IsOpen => IsBusy;
    public static bool WasDeclined { get; set; }

    public static void Queue(ConductResponse response)
    {
        if (response == null) return;
        _pending = response;
        WasDeclined = false;
    }

    public static void ShowFetching(MonoBehaviour runner)
    {
        _fetching = true;
        WasDeclined = false;
        _runner = runner;
        EnsurePopup();
        SetBody(ModTranslation.GetString("SafetyConductFetching"));
        SetActionsVisible(false);
        RebindToCamera();
    }

    public static void ShowNow(MonoBehaviour runner)
    {
        if (_pending == null && ConductGate.Last != null && !ConductGate.Last.Consented && !ConductGate.Last.Banned)
            _pending = ConductGate.Last;
        if (_pending == null) return;
        _fetching = false;
        _runner = runner;
        WasDeclined = false;
        EnsurePopup();
        SetBody(_pending.Body);
        SetActionsVisible(true);
        RebindToCamera();
    }

    public static void CloseVisible()
    {
        Close(declined: false);
    }

    public static IEnumerator CoRefreshAndPresent(MonoBehaviour runner)
    {
        ShowFetching(runner);
        ConductResponse fetched = null;
        yield return PlayerSafetyApiClient.GetConduct(result => fetched = result).WrapToIl2Cpp();
        ConductGate.Apply(fetched);
        if (ConductGate.IsBannedNow)
        {
            CloseVisible();
            BanPopup.ShowNow(SafetyPopupUi.EnsureHost());
            yield break;
        }
        if (ConductGate.HasUnackedWarning)
        {
            CloseVisible();
            WarningPopup.ShowNow(SafetyPopupUi.EnsureHost());
            yield break;
        }
        if (ConductGate.CanPlayOfficial || WasDeclined)
        {
            CloseVisible();
            yield break;
        }
        ShowNow(runner);
    }

    public static void RebindToCamera()
    {
        if (!_busy) return;
        if (!_popup)
        {
            if (_fetching)
                ShowFetching(_runner);
            else if (_pending != null)
                ShowNow(_runner);
            return;
        }
        SafetyPopupUi.PlaceInFront(_popup);
        _popup.SetActive(true);
    }

    private static void EnsurePopup()
    {
        if (_closing)
            FinishClose();

        if (_popup)
        {
            _busy = true;
            return;
        }

        MonoBehaviour host = SafetyPopupUi.EnsureHost();
        _popup = AssetManager.Instantiate(PrefabName, host.transform);
        _popup.SetActive(true);
        SafetyPopupUi.PlaceInFront(_popup);
        _busy = true;

        Transform title = _popup.transform.Find("Title");
        if (title != null)
            title.GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyConductTitle");

        SafetyPopupUi.WireBackdropAndCloseBox(_popup, () => Close(declined: true));

        GameObject agree = _popup.transform.Find("AnalyticsButton")?.gameObject;
        if (agree != null)
        {
            Vector3 agreePos = agree.transform.localPosition;
            agree.transform.localPosition = new Vector3(3f, agreePos.y, -5f);
            SafetyPopupUi.WireButton(agree, "SafetyConductAgree", OnAgree);
            _decline = Object.Instantiate(agree, agree.transform.parent);
            _decline.name = "SafetyConductDecline";
            _decline.transform.localPosition = new Vector3(-3f, agreePos.y, -5f);
            SafetyPopupUi.WireButton(_decline, "SafetyConductDecline", () => Close(declined: true));
        }

        Transform close = _popup.transform.Find("CloseButton");
        if (close != null)
            SafetyPopupUi.WireButton(close.gameObject, null, () => Close(declined: true));

        SafetyPopupUi.PlayOpenAnimation(_popup, SafetyPopupUi.EnsureHost());
    }

    private static void SetBody(string body)
    {
        if (!_popup) return;
        Transform text = _popup.transform.Find("Text");
        if (text == null) return;
        TextMeshPro tmp = text.GetComponent<TextMeshPro>();
        if (tmp == null) return;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.text = MarkdownToUnityTag.Convert(body ?? string.Empty);
    }

    private static void SetActionsVisible(bool visible)
    {
        if (!_popup) return;
        GameObject agree = _popup.transform.Find("AnalyticsButton")?.gameObject;
        if (agree != null)
            agree.SetActive(visible);
        if (_decline)
            _decline.SetActive(visible);
    }

    private static void OnAgree()
    {
        ConductResponse response = _pending ?? ConductGate.Last;
        if (response == null) return;
        MonoBehaviour host = _runner != null ? _runner : SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (host == null)
            host = AmongUsClient.Instance;
        if (host == null) return;
        host.StartCoroutine(PlayerSafetyApiClient.Consent(response.Version, ok =>
        {
            if (ok) ConductGate.MarkConsented();
        }).WrapToIl2Cpp());
    }

    private static void Close(bool declined)
    {
        if (declined)
            WasDeclined = true;
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
        if (_popup) Object.Destroy(_popup);
        _popup = null;
        _decline = null;
        _pending = null;
        _busy = false;
        _fetching = false;
        _closing = false;
    }
}
