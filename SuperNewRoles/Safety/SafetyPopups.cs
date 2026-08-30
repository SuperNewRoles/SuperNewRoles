using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Safety;

public static class SafetyPopupUi
{
    public const float PopupScale = 0.58f;
    public const float ForegroundZ = -100f;
    public const float HudOverlayZ = -500f;
    public const float OpenAnimationDuration = 0.15f;
    public const float CloseAnimationDuration = 0.15f;

    private static SafetyPopupHost _host;

    public static MonoBehaviour EnsureHost()
    {
        if (_host)
            return _host;
        GameObject root = new GameObject("SnrSafetyPopupHost");
        UnityEngine.Object.DontDestroyOnLoad(root);
        _host = root.AddComponent<SafetyPopupHost>();
        return _host;
    }

    public static GameObject Create(string title, string body)
    {
        GameObject popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
        popup.SetActive(true);
        popup.transform.localScale = Vector3.one * PopupScale;
        popup.transform.Find("Title").GetComponent<TextMeshPro>().text = title;
        popup.transform.Find("Text").GetComponent<TextMeshPro>().text = body;
        return popup;
    }

    public static void PlaceInFront(GameObject popup)
    {
        if (popup == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        popup.transform.SetPositionAndRotation(
            cam.transform.TransformPoint(new Vector3(0f, 0f, ForegroundZ)),
            cam.transform.rotation);
    }

    public static void PlaceUnderCamera(GameObject popup)
    {
        if (popup == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        Transform camTransform = cam.transform;
        if (popup.transform.parent != camTransform)
            popup.transform.SetParent(camTransform, false);
        popup.transform.localPosition = new Vector3(0f, 0f, ForegroundZ);
        popup.transform.localRotation = Quaternion.identity;
    }

    public static Transform ResolveWarningParent(Transform hostFallback)
    {
        bool hudAvailable = TryGetHudTransform(out Transform hud);
        Camera cam = Camera.main;
        switch (SafetyWarningLayout.ChooseParent(hudAvailable, cam != null))
        {
            case SafetyWarningLayout.ParentKind.Hud:
                return hud;
            case SafetyWarningLayout.ParentKind.Camera:
                return cam.transform;
            default:
                return hostFallback;
        }
    }

    public static void PlaceWarning(GameObject popup)
    {
        if (popup == null) return;
        if (TryGetHudTransform(out Transform hud))
        {
            if (popup.transform.parent != hud)
                popup.transform.SetParent(hud, false);
            popup.transform.localPosition = new Vector3(0f, 0f, HudOverlayZ);
            popup.transform.localRotation = Quaternion.identity;
            ApplyUiLayer(popup);
            return;
        }
        PlaceUnderCamera(popup);
    }

    public static bool TryGetHudTransform(out Transform hud)
    {
        hud = null;
        if (!DestroyableSingleton<HudManager>.InstanceExists)
            return false;
        HudManager instance = FastDestroyableSingleton<HudManager>.Instance;
        if (instance == null)
            return false;
        hud = instance.transform;
        return hud != null;
    }

    internal static void ApplyUiLayer(GameObject root)
    {
        int layer = LayerExpansion.GetUILayer();
        if (layer < 0 || root == null)
            return;
        SetLayerRecursively(root, layer);
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        Transform transform = obj.transform;
        for (int i = 0; i < transform.childCount; i++)
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
    }

    public static void WireClickCatcher(GameObject target, Action onClick)
    {
        if (target == null) return;
        PassiveButton passive = target.GetComponent<PassiveButton>() ?? target.AddComponent<PassiveButton>();
        BoxCollider2D box = target.GetComponent<BoxCollider2D>();
        if (box != null)
            passive.Colliders = new Collider2D[] { box };
        passive.OnClick = new();
        if (onClick != null)
            passive.OnClick.AddListener((UnityAction)(() => onClick()));
        passive.OnMouseOver = new();
        passive.OnMouseOut = new();
    }

    public static void WireBackdropAndCloseBox(GameObject popup, Action close)
    {
        if (popup == null) return;
        WireClickCatcher(popup, null);
        Transform closeBox = popup.transform.Find("CloseBox");
        if (closeBox != null)
            WireClickCatcher(closeBox.gameObject, close);
    }

    private static GameObject _closingPopup;

    public static void PlayOpenAnimation(GameObject popup, MonoBehaviour runner, float targetScale = PopupScale)
    {
        if (popup == null) return;
        popup.transform.localScale = Vector3.zero;
        if (runner == null)
        {
            popup.transform.localScale = Vector3.one * targetScale;
            return;
        }
        runner.StartCoroutine(CoOpenScale(popup, targetScale).WrapToIl2Cpp());
    }

    public static void PlayCloseAnimation(GameObject popup, MonoBehaviour runner, Action onComplete)
    {
        if (popup == null)
        {
            onComplete?.Invoke();
            return;
        }
        _closingPopup = popup;
        if (runner == null)
        {
            onComplete?.Invoke();
            return;
        }
        runner.StartCoroutine(CoCloseScale(popup, onComplete).WrapToIl2Cpp());
    }

    private static IEnumerator CoOpenScale(GameObject popup, float targetScale)
    {
        float timer = 0f;
        Vector3 finalScale = Vector3.one * targetScale;
        while (timer < OpenAnimationDuration)
        {
            if (!popup || popup == _closingPopup) yield break;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / OpenAnimationDuration);
            popup.transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);
            yield return null;
        }
        if (popup && popup != _closingPopup)
            popup.transform.localScale = finalScale;
    }

    private static IEnumerator CoCloseScale(GameObject popup, Action onComplete)
    {
        float timer = 0f;
        Vector3 start = popup.transform.localScale;
        while (timer < CloseAnimationDuration)
        {
            if (!popup)
            {
                if (_closingPopup == popup)
                    _closingPopup = null;
                onComplete?.Invoke();
                yield break;
            }
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / CloseAnimationDuration);
            popup.transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }
        if (popup)
            popup.transform.localScale = Vector3.zero;
        if (_closingPopup == popup)
            _closingPopup = null;
        onComplete?.Invoke();
    }

    public static GameObject TemplateButton(GameObject popup)
    {
        return popup.transform.Find("AnalyticsButton")?.gameObject;
    }

    public static void WireButton(GameObject button, string labelKey, System.Action onClick)
    {
        if (button == null) return;
        button.SetActive(true);
        Transform text = button.transform.Find("Text");
        if (text != null && !string.IsNullOrEmpty(labelKey))
            text.GetComponent<TextMeshPro>().text = ModTranslation.GetString(labelKey);

        var old = button.GetComponent<PassiveButton>();
        PassiveButton passive = old ?? button.AddComponent<PassiveButton>();
        Collider2D collider = button.GetComponent<Collider2D>() ?? button.GetComponentInChildren<Collider2D>();
        if (collider != null)
            passive.Colliders = new Collider2D[] { collider };
        passive.OnClick = new();
        passive.OnClick.AddListener((UnityAction)(() => onClick()));
        passive.OnMouseOver = new();
        passive.OnMouseOut = new();
        GameObject selected = button.transform.Find("Selected")?.gameObject;
        if (selected != null)
        {
            selected.SetActive(false);
            passive.OnMouseOver.AddListener((UnityAction)(() => selected.SetActive(true)));
            passive.OnMouseOut.AddListener((UnityAction)(() => selected.SetActive(false)));
        }
    }

    public static GameObject CloneButton(GameObject template, string name, Vector3 localPosition, string labelKey, System.Action onClick)
    {
        GameObject clone = UnityEngine.Object.Instantiate(template, template.transform.parent);
        clone.name = name;
        clone.transform.localPosition = localPosition;
        clone.transform.localScale = template.transform.localScale;
        WireButton(clone, labelKey, onClick);
        Transform text = clone.transform.Find("Text");
        if (text != null && string.IsNullOrEmpty(labelKey))
            text.GetComponent<TextMeshPro>().text = name;
        return clone;
    }
}

public static class BlockListPopup
{
    public static void Open(MonoBehaviour runner)
    {
        MonoBehaviour host = SafetyRuntime.FindCoroutineRunner(runner);
        if (host == null)
            return;
        host.StartCoroutine(PlayerSafetyApiClient.ListBlocks(rows => Show(rows, host)).WrapToIl2Cpp());
    }

    private static void Show(List<BlockRow> rows, MonoBehaviour runner)
    {
        var sb = new StringBuilder();
        if (rows == null || rows.Count == 0)
            sb.Append(ModTranslation.GetString("SafetyBlockListEmpty"));
        else
        {
            foreach (var row in rows)
            {
                sb.AppendLine($"{row.Name}  {row.CreatedAt}");
                if (!string.IsNullOrEmpty(row.Note))
                    sb.AppendLine($"  {row.Note}");
            }
        }

        GameObject popup = SafetyPopupUi.Create(ModTranslation.GetString("SafetyBlockListTitle"), sb.ToString());
        GameObject template = SafetyPopupUi.TemplateButton(popup);
        if (template == null) return;
        template.SetActive(false);

        Transform close = popup.transform.Find("CloseButton");
        if (close != null)
            SafetyPopupUi.WireButton(close.gameObject, null, () => UnityEngine.Object.Destroy(popup));

        if (rows == null) return;
        int shown = 0;
        foreach (var row in rows)
        {
            if (shown >= 6) break;
            int index = shown;
            BlockRow current = row;
            GameObject button = SafetyPopupUi.CloneButton(
                template,
                "Unblock" + index,
                template.transform.localPosition + new Vector3(-4.4f + (index % 2) * 4.4f, 1.15f + (index / 2) * 0.85f, 0f),
                null,
                () =>
                {
                    PlayerSafetyActions.Unblock(current.Id, runner);
                    UnityEngine.Object.Destroy(popup);
                });
            Transform label = button.transform.Find("Text");
            if (label != null)
                label.GetComponent<TextMeshPro>().text = string.Format(ModTranslation.GetString("SafetyUnblockNamed"), current.Name);
            shown++;
        }
    }
}

public static class SafetyWarningLayout
{
    public enum ParentKind
    {
        Hud,
        Camera,
        Host,
    }

    public static ParentKind ChooseParent(bool hudAvailable, bool cameraAvailable)
    {
        if (hudAvailable) return ParentKind.Hud;
        if (cameraAvailable) return ParentKind.Camera;
        return ParentKind.Host;
    }
}

public sealed class SafetyPopupHost : MonoBehaviour
{
    public void LateUpdate()
    {
        WarningPopup.RebindToCamera();
    }
}
