using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Safety;

/// <summary>
/// 行動規範・警告ポップアップ本文の TMP リンク。ホバーで緑、クリックで URL を開く。
/// </summary>
public sealed class SafetyMarkdownLinks : MonoBehaviour
{
    public const string HoverColorHex = "2EE86A";

    private static readonly Regex LinkTagRegex = new Regex(
        @"<link=""([^""]*)"">(.*?)</link>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private TextMeshPro _tmp;
    private string _baseText = string.Empty;
    private int _hoveredLink = -1;

    public static void Bind(TextMeshPro tmp)
    {
        if (tmp == null)
            return;
        SafetyMarkdownLinks existing = tmp.GetComponent<SafetyMarkdownLinks>();
        if (existing != null)
        {
            existing._tmp = tmp;
            existing._hoveredLink = -1;
            existing._baseText = tmp.text ?? string.Empty;
            return;
        }
        SafetyMarkdownLinks added = tmp.gameObject.AddComponent<SafetyMarkdownLinks>();
        added._tmp = tmp;
        added._baseText = tmp.text ?? string.Empty;
    }

    public static bool TryGetOpenableUrl(string raw, out string url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        if (!Uri.TryCreate(raw.Trim(), UriKind.Absolute, out Uri uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        url = uri.AbsoluteUri;
        return true;
    }

    public static string ColorNthLink(string source, int linkIndex)
    {
        if (string.IsNullOrEmpty(source) || linkIndex < 0)
            return source ?? string.Empty;
        int found = -1;
        return LinkTagRegex.Replace(source, match =>
        {
            found++;
            if (found != linkIndex)
                return match.Value;
            return $"<link=\"{match.Groups[1].Value}\"><color=#{HoverColorHex}>{match.Groups[2].Value}</color></link>";
        });
    }

    private void Awake()
    {
        if (_tmp == null)
            _tmp = GetComponent<TextMeshPro>();
    }

    private void LateUpdate()
    {
        TextMeshPro tmp = _tmp != null ? _tmp : GetComponent<TextMeshPro>();
        _tmp = tmp;
        if (tmp == null || !tmp.gameObject.activeInHierarchy)
            return;

        if (_hoveredLink < 0)
            _baseText = tmp.text ?? string.Empty;

        int linkIndex = FindIntersectingLink(tmp);
        if (linkIndex != _hoveredLink)
        {
            string next = ColorNthLink(_baseText, linkIndex);
            if (tmp.text != next)
                tmp.text = next;
            _hoveredLink = linkIndex;
        }

        if (linkIndex >= 0 && Input.GetMouseButtonUp(0))
            TryOpenLink(tmp, linkIndex);
    }

    private static int FindIntersectingLink(TextMeshPro tmp)
    {
        Vector3 mouse = Input.mousePosition;
        Camera camera = ResolveCamera(tmp);
        int index = TMP_TextUtilities.FindIntersectingLink(tmp, mouse, camera);
        if (index >= 0)
            return index;
        Camera main = Camera.main;
        if (main != null && main != camera)
        {
            index = TMP_TextUtilities.FindIntersectingLink(tmp, mouse, main);
            if (index >= 0)
                return index;
        }
        return TMP_TextUtilities.FindIntersectingLink(tmp, mouse, null);
    }

    private static Camera ResolveCamera(TextMeshPro tmp)
    {
        if (tmp != null && DestroyableSingleton<HudManager>.InstanceExists)
        {
            HudManager hud = FastDestroyableSingleton<HudManager>.Instance;
            if (hud != null && hud.UICamera != null && tmp.transform.IsChildOf(hud.transform))
                return hud.UICamera;
        }
        return Camera.main;
    }

    private static void TryOpenLink(TextMeshPro tmp, int linkIndex)
    {
        TMP_TextInfo textInfo = tmp.textInfo;
        if (textInfo?.linkInfo == null || linkIndex < 0 || linkIndex >= textInfo.linkInfo.Length)
            return;
        string raw = textInfo.linkInfo[linkIndex].GetLinkID();
        if (!TryGetOpenableUrl(raw, out string url))
            return;
        Constants.OpenURL(url);
    }
}
