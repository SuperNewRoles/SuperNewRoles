using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;

public static class AndroidStartupNoticePopup
{
    private const string AndroidNoticeTriggerText = "SHOW";

    private static bool _isFetchStarted;
    private static bool _isFetchCompleted;
    private static bool _shouldShowPopup;
    private static bool _isPopupViewed;

    public static bool TryHandle(MainMenuManager instance, ref GameObject currentPopup)
    {
        if (!ModHelpers.IsAndroid())
            return false;

        if (!_isFetchStarted)
        {
            _isFetchStarted = true;
            Logger.Info($"Start fetching Android startup notice from {SNRURLs.AndroidNoticeCheckURL}", "AndroidStartupNotice");
            instance.StartCoroutine(FetchNoticeFlag().WrapToIl2Cpp());
        }

        // Androidでは判定完了までAnalyticsポップアップ表示を待つ
        if (!_isFetchCompleted)
            return true;

        if (currentPopup == null && _shouldShowPopup && !_isPopupViewed)
        {
            ShowNoticePopup(ref currentPopup);
            return true;
        }

        return false;
    }

    private static IEnumerator FetchNoticeFlag()
    {
        UnityWebRequest request = UnityWebRequest.Get(SNRURLs.AndroidNoticeCheckURL);
        request.timeout = 5;

        yield return request.SendWebRequest();

        Logger.Info($"Android startup notice status code: {request.responseCode}", "AndroidStartupNotice");
        if (request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 300)
        {
            string responseText = request.downloadHandler?.text?.Trim() ?? "";
            _shouldShowPopup = responseText == AndroidNoticeTriggerText;
            Logger.Info($"Android startup notice match result: {_shouldShowPopup}", "AndroidStartupNotice");
        }
        else
        {
            _shouldShowPopup = false;
            string errorDetail = request.error ?? request.downloadHandler?.text;
            Logger.Warning($"Android startup notice fetch failed: {request.responseCode} - {errorDetail}", "AndroidStartupNotice");
        }

        _isFetchCompleted = true;
        request.Dispose();
    }

    private static void ShowNoticePopup(ref GameObject currentPopup)
    {
        GameObject popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
        popup.gameObject.SetActive(true);
        popup.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AndroidStartupNoticeTitle");
        popup.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AndroidStartupNoticeText");
        popup.transform.localScale = Vector3.one * 0.58f;

        GameObject okButton = popup.transform.Find("AnalyticsButton").gameObject;
        okButton.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AnalyticsOK");
        PassiveButton passiveButton = okButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { okButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            GameObject.Destroy(popup);
            _isPopupViewed = true;
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            okButton.transform.Find("Selected").gameObject.SetActive(true);
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            okButton.transform.Find("Selected").gameObject.SetActive(false);
        }));

        currentPopup = popup;
    }
}
