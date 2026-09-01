using System;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

/// <summary>
/// 報告スレッド本文の TMP リンクをホバーで緑にし、クリックで開く。
/// </summary>
public class ChatMessageLinkHandler : MonoBehaviour
{
    private const float MaxClickDriftPixels = 12f;

    private TextMeshPro tmp;
    private string[] urls = Array.Empty<string>();
    private string baseText;
    private int hoveredLinkIndex = -1;
    private int pressedLinkIndex = -1;
    private Vector3 pressPosition;

    public void Init(TextMeshPro target, string[] linkUrls)
    {
        tmp = target;
        urls = linkUrls ?? Array.Empty<string>();
        baseText = target != null ? target.text : string.Empty;
    }

    public void Update()
    {
        if (tmp == null || urls.Length == 0 || !tmp.gameObject.activeInHierarchy)
            return;

        int linkIndex = FindLinkUnderPointer();

        if (Input.GetMouseButtonDown(0))
        {
            pressedLinkIndex = linkIndex;
            pressPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (pressedLinkIndex >= 0
                && pressedLinkIndex == linkIndex
                && (Input.mousePosition - pressPosition).sqrMagnitude <= MaxClickDriftPixels * MaxClickDriftPixels)
            {
                TryOpenLink(pressedLinkIndex);
            }
            pressedLinkIndex = -1;
        }

        if (linkIndex == hoveredLinkIndex)
            return;

        hoveredLinkIndex = linkIndex;
        tmp.text = ChatMessageRichText.WithHoveredLinkColor(baseText, hoveredLinkIndex);
        tmp.ForceMeshUpdate();
    }

    private void TryOpenLink(int linkIndex)
    {
        int urlIndex = linkIndex;
        if (tmp.textInfo?.linkInfo != null && linkIndex >= 0 && linkIndex < tmp.textInfo.linkInfo.Length)
        {
            string linkId = tmp.textInfo.linkInfo[linkIndex].GetLinkID();
            if (int.TryParse(linkId, out int parsed) && parsed >= 0 && parsed < urls.Length)
                urlIndex = parsed;
        }

        if (urlIndex < 0 || urlIndex >= urls.Length)
            return;

        string url = urls[urlIndex];
        if (!ChatMessageRichText.TryNormalizeHttpUrl(url, out url))
            return;

        Constants.OpenURL(url);
    }

    private int FindLinkUnderPointer()
    {
        Camera uiCamera = ResolveCamera();
        int linkIndex = FindLinkWithCamera(uiCamera);
        if (linkIndex >= 0)
            return linkIndex;

        Camera main = Camera.main;
        if (main != null && main != uiCamera)
        {
            linkIndex = FindLinkWithCamera(main);
            if (linkIndex >= 0)
                return linkIndex;
        }

        return FindLinkByWorldBounds(uiCamera ?? main);
    }

    private int FindLinkWithCamera(Camera camera)
    {
        if (camera == null || tmp.textInfo == null || tmp.textInfo.linkCount <= 0)
            return -1;
        return TMP_TextUtilities.FindIntersectingLink(tmp, Input.mousePosition, camera);
    }

    private int FindLinkByWorldBounds(Camera camera)
    {
        if (camera == null || tmp.textInfo == null || tmp.textInfo.linkCount <= 0)
            return -1;

        const float padding = 0.08f;
        float z = camera.WorldToScreenPoint(tmp.transform.position).z;
        Vector3 mouseWorld = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));

        for (int i = 0; i < tmp.textInfo.linkCount; i++)
        {
            TMP_LinkInfo linkInfo = tmp.textInfo.linkInfo[i];
            for (int j = 0; j < linkInfo.linkTextLength; j++)
            {
                int charIndex = linkInfo.linkTextfirstCharacterIndex + j;
                if (charIndex < 0 || charIndex >= tmp.textInfo.characterCount)
                    continue;

                TMP_CharacterInfo charInfo = tmp.textInfo.characterInfo[charIndex];
                if (!charInfo.isVisible)
                    continue;

                Vector3 bottomLeft = tmp.transform.TransformPoint(charInfo.bottomLeft);
                Vector3 topLeft = tmp.transform.TransformPoint(new Vector3(charInfo.bottomLeft.x, charInfo.topRight.y, charInfo.bottomLeft.z));
                Vector3 topRight = tmp.transform.TransformPoint(charInfo.topRight);
                Vector3 bottomRight = tmp.transform.TransformPoint(new Vector3(charInfo.topRight.x, charInfo.bottomLeft.y, charInfo.topRight.z));
                if (PointInAabbXY(mouseWorld, bottomLeft, topLeft, topRight, bottomRight, padding))
                    return i;
            }
        }

        return -1;
    }

    private static bool PointInAabbXY(Vector3 point, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float padding)
    {
        float minX = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x)) - padding;
        float maxX = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x)) + padding;
        float minY = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y)) - padding;
        float maxY = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y)) + padding;
        return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
    }

    private static Camera ResolveCamera()
    {
        if (HudManager.InstanceExists && HudManager.Instance.UICamera != null)
            return HudManager.Instance.UICamera;
        return Camera.main;
    }
}
