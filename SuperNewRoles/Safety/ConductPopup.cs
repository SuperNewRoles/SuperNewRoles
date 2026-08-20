using System;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Safety;

public static class ConductPopup
{
    private const string PrefabName = "AnalyticsBG";
    private static GameObject _popup;
    private static ConductResponse _pending;

    public static ConductResponse Pending => _pending;

    public static bool TryHandle(MainMenuManager manager, ref GameObject currentPopup)
    {
        if (_pending == null || _popup != null) return _popup != null;
        Show(manager, _pending);
        currentPopup = _popup;
        return true;
    }

    public static void Queue(ConductResponse response)
    {
        _pending = response;
    }

    private static void Show(MainMenuManager manager, ConductResponse response)
    {
        _popup = AssetManager.Instantiate(PrefabName, Camera.main.transform);
        _popup.SetActive(true);
        _popup.transform.localScale = Vector3.one * 0.58f;
        _popup.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("SafetyConductTitle");
        _popup.transform.Find("Text").GetComponent<TextMeshPro>().text = response.Body;
        Transform agree = _popup.transform.Find("AgreeButton") ?? _popup.transform.Find("Button");
        var buttons = _popup.GetComponentsInChildren<PassiveButton>(true);
        if (buttons.Length > 0)
        {
            buttons[0].OnClick = new();
            buttons[0].OnClick.AddListener((UnityAction)(() =>
            {
                manager.StartCoroutine(PlayerSafetyApiClient.Consent(response.Version, ok =>
                {
                    if (ok) Close();
                }).WrapToIl2Cpp());
            }));
        }
        Transform title = _popup.transform.Find("Title");
        var close = _popup.GetComponentsInChildren<PassiveButton>(true).LastOrDefault();
        if (close != null && buttons.Length > 1)
        {
            buttons[1].OnClick = new();
            buttons[1].OnClick.AddListener((UnityAction)Close);
        }
    }

    private static void Close()
    {
        if (_popup != null) UnityEngine.Object.Destroy(_popup);
        _popup = null;
        _pending = null;
    }
}
