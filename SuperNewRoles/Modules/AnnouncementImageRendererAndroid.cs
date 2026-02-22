using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

// Android-side minimal renderer: image-only, no video and no loading spinner.
public class AnnouncementImageRendererAndroid : MonoBehaviour
{
    private const float MaxImageWidth = 9.88f;
    private const float MaxImageHeight = 7.54f;
    private const float MaxImageScale = 4.68f;
    private const float ImageLeftPadding = 0f;
    private const string PlaceholderPrefix = "[[img:";
    private const string PlaceholderSuffix = "]]";

    private readonly Dictionary<int, SpriteRenderer> _imageRenderers = new();
    private TextMeshPro _bodyText;
    private Vector3? _defaultBodyTextPos;
    private int _requestToken;
    private float _extraHeight;

    public bool HasImages => _imageRenderers.Count > 0;

    [HideFromIl2Cpp]
    public void Initialize(TextMeshPro text)
    {
        _bodyText = text;
        if (_bodyText != null && !_defaultBodyTextPos.HasValue)
            _defaultBodyTextPos = _bodyText.transform.localPosition;
    }

    [HideFromIl2Cpp]
    public void ShowImages(int announcementNumber, bool previewOnly)
    {
        if (_bodyText != null && _defaultBodyTextPos.HasValue)
        {
            if (announcementNumber >= 1000000000)
                _bodyText.transform.localPosition = _defaultBodyTextPos.Value + new Vector3(0f, 0.4f, 0f);
            else
                _bodyText.transform.localPosition = _defaultBodyTextPos.Value;
        }

        if (previewOnly || _bodyText == null)
        {
            ClearImages();
            return;
        }

        if (!AnnouncementImageCache.TryGetImages(announcementNumber, out var images) || images.Count == 0)
        {
            ClearImages();
            return;
        }

        _requestToken++;
        ClearImagesInternal();
        StartCoroutine(LoadImages(images, _requestToken).WrapToIl2Cpp());
    }

    public void ClearImages()
    {
        _requestToken++;
        ClearImagesInternal();
    }

    public float GetExtraScrollHeight()
    {
        return _extraHeight;
    }

    public float GetTextHeight()
    {
        if (_bodyText == null)
            return 0f;
        return Mathf.Abs(_bodyText.GetNotDumbRenderedHeight());
    }

    [HideFromIl2Cpp]
    private IEnumerator LoadImages(List<AnnouncementImageInfo> images, int token)
    {
        foreach (var image in images)
        {
            if (image.MediaType != AnnouncementMediaType.Image)
                continue;

            Sprite sprite = null;
            yield return AnnouncementImageCache.LoadSprite(image.Url, result => sprite = result);
            if (token != _requestToken)
                yield break;

            if (sprite == null)
                continue;

            var renderer = CreateRenderer(sprite);
            if (renderer == null)
                continue;

            _imageRenderers[image.Index] = renderer;
            LayoutImages();
        }

        LayoutImages();
    }

    private void ClearImagesInternal()
    {
        _extraHeight = 0f;
        foreach (var renderer in _imageRenderers.Values)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }
        _imageRenderers.Clear();
    }

    [HideFromIl2Cpp]
    private SpriteRenderer CreateRenderer(Sprite sprite)
    {
        if (_bodyText == null)
            return null;

        var go = new GameObject("AnnouncementImage");
        go.transform.SetParent(_bodyText.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        SyncSorting(renderer);
        ApplyMask(renderer);
        return renderer;
    }

    [HideFromIl2Cpp]
    private void SyncSorting(SpriteRenderer renderer)
    {
        var meshRenderer = _bodyText != null ? _bodyText.GetComponent<MeshRenderer>() : null;
        if (meshRenderer == null)
            return;

        renderer.sortingLayerID = meshRenderer.sortingLayerID;
        renderer.sortingOrder = meshRenderer.sortingOrder;
    }

    [HideFromIl2Cpp]
    private void ApplyMask(SpriteRenderer renderer)
    {
        if (renderer == null)
            return;

        renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    private void LayoutImages()
    {
        if (_bodyText == null || _imageRenderers.Count == 0)
        {
            _extraHeight = 0f;
            return;
        }

        _bodyText.ForceMeshUpdate();
        var blocks = GetPlaceholderBlocks(_bodyText.textInfo);
        float availableWidth = GetAvailableWidth();
        float maxWidth = Mathf.Min(availableWidth, MaxImageWidth);
        float leftEdge = GetLeftEdge();
        float totalExtra = 0f;
        if (maxWidth <= 0f)
        {
            _extraHeight = 0f;
            return;
        }

        float fallbackY = _bodyText.textBounds.min.y - 0.2f;

        foreach (var entry in _imageRenderers)
        {
            var renderer = entry.Value;
            if (renderer == null || renderer.sprite == null)
                continue;

            float spriteWidth = renderer.sprite.bounds.size.x;
            float spriteHeight = renderer.sprite.bounds.size.y;
            if (spriteWidth <= 0f)
                continue;

            if (blocks.TryGetValue(entry.Key, out var block))
            {
                float maxHeight2 = Mathf.Min(block.Height, MaxImageHeight);
                if (maxHeight2 <= 0f || spriteHeight <= 0f)
                    continue;

                float scale2 = Mathf.Min(maxWidth / spriteWidth, maxHeight2 / spriteHeight);
                scale2 = Mathf.Min(scale2, MaxImageScale);
                renderer.transform.localScale = new Vector3(scale2, scale2, 1f);
                float scaledWidth2 = spriteWidth * scale2;
                float centerY = (block.Top + block.Bottom) * 0.5f;
                float x2 = leftEdge + scaledWidth2 * 0.5f;
                renderer.transform.localPosition = new Vector3(x2, centerY, 0f);
                continue;
            }

            float maxHeight = Mathf.Min(MaxImageHeight, spriteHeight);
            if (maxHeight <= 0f || spriteHeight <= 0f)
                continue;

            float scale = Mathf.Min(maxWidth / spriteWidth, maxHeight / spriteHeight);
            scale = Mathf.Min(scale, MaxImageScale);
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
            float height = spriteHeight * scale;
            float scaledWidth = spriteWidth * scale;
            float x = leftEdge + scaledWidth * 0.5f;
            renderer.transform.localPosition = new Vector3(x, fallbackY - height * 0.5f, 0f);
            fallbackY -= height + 0.2f;
            totalExtra += height + 0.2f;
        }

        _extraHeight = totalExtra;
    }

    private float GetLeftEdge()
    {
        float left = _bodyText.textBounds.size.x > 0f ? _bodyText.textBounds.min.x : _bodyText.rectTransform.rect.min.x;
        return left + ImageLeftPadding;
    }

    [HideFromIl2Cpp]
    private Dictionary<int, PlaceholderBlock> GetPlaceholderBlocks(TMP_TextInfo info)
    {
        var blocks = new Dictionary<int, PlaceholderBlock>();
        if (info == null || info.characterCount == 0 || _bodyText == null)
            return blocks;

        string raw = _bodyText.text;
        if (string.IsNullOrEmpty(raw))
            return blocks;

        int searchIndex = 0;
        while (searchIndex < raw.Length)
        {
            int start = raw.IndexOf(PlaceholderPrefix, searchIndex, StringComparison.Ordinal);
            if (start < 0)
                break;

            int cursor = start + PlaceholderPrefix.Length;
            int index = 0;
            int digits = 0;
            while (cursor < raw.Length)
            {
                char ch = raw[cursor];
                if (!char.IsDigit(ch))
                    break;
                index = (index * 10) + (ch - '0');
                digits++;
                cursor++;
            }

            if (digits == 0 || cursor >= raw.Length)
            {
                searchIndex = start + PlaceholderPrefix.Length;
                continue;
            }

            if (!raw.AsSpan(cursor).StartsWith(PlaceholderSuffix, StringComparison.Ordinal))
            {
                searchIndex = start + PlaceholderPrefix.Length;
                continue;
            }

            int end = cursor + PlaceholderSuffix.Length - 1;
            int startChar = FindCharInfoIndex(info, start);
            int endChar = FindCharInfoIndex(info, end);
            if (startChar >= 0 && endChar >= 0)
            {
                int minLine = info.characterInfo[startChar].lineNumber;
                int maxLine = info.characterInfo[endChar].lineNumber;
                if (blocks.TryGetValue(index, out var existing))
                {
                    existing.MinLine = Math.Min(existing.MinLine, minLine);
                    existing.MaxLine = Math.Max(existing.MaxLine, maxLine);
                    blocks[index] = existing;
                }
                else
                {
                    blocks[index] = new PlaceholderBlock(minLine, maxLine);
                }
            }

            searchIndex = end + 1;
        }

        if (blocks.Count == 0)
            MergePlaceholderBlocksFromCharacters(info, blocks);

        if (info.lineCount == 0)
            return blocks;

        var keys = new List<int>(blocks.Keys);
        foreach (var key in keys)
        {
            var block = blocks[key];
            int minLine = Mathf.Clamp(block.MinLine, 0, info.lineCount - 1);
            int maxLine = Mathf.Clamp(block.MaxLine, 0, info.lineCount - 1);
            block.Top = info.lineInfo[minLine].ascender;
            block.Bottom = info.lineInfo[maxLine].descender;
            blocks[key] = block;
        }

        return blocks;
    }

    [HideFromIl2Cpp]
    private void MergePlaceholderBlocksFromCharacters(TMP_TextInfo info, Dictionary<int, PlaceholderBlock> blocks)
    {
        int count = info.characterCount;
        for (int i = 0; i < count; i++)
        {
            if (info.characterInfo[i].character != '[')
                continue;

            if (!MatchLiteral(info, i, PlaceholderPrefix))
                continue;

            int cursor = i + PlaceholderPrefix.Length;
            int index = 0;
            int digits = 0;
            while (cursor < count)
            {
                char ch = info.characterInfo[cursor].character;
                if (!char.IsDigit(ch))
                    break;
                index = (index * 10) + (ch - '0');
                digits++;
                cursor++;
            }

            if (digits == 0)
                continue;

            if (!MatchLiteral(info, cursor, PlaceholderSuffix))
                continue;

            int endIndex = cursor + PlaceholderSuffix.Length - 1;
            int minLine = info.characterInfo[i].lineNumber;
            int maxLine = info.characterInfo[endIndex].lineNumber;
            if (blocks.TryGetValue(index, out var existing))
            {
                existing.MinLine = Math.Min(existing.MinLine, minLine);
                existing.MaxLine = Math.Max(existing.MaxLine, maxLine);
                blocks[index] = existing;
            }
            else
            {
                blocks[index] = new PlaceholderBlock(minLine, maxLine);
            }

            i = endIndex;
        }
    }

    [HideFromIl2Cpp]
    private bool MatchLiteral(TMP_TextInfo info, int startIndex, string literal)
    {
        int count = info.characterCount;
        if (startIndex < 0 || startIndex + literal.Length > count)
            return false;

        for (int i = 0; i < literal.Length; i++)
        {
            if (info.characterInfo[startIndex + i].character != literal[i])
                return false;
        }

        return true;
    }

    [HideFromIl2Cpp]
    private int FindCharInfoIndex(TMP_TextInfo info, int stringIndex)
    {
        int count = info.characterCount;
        for (int i = 0; i < count; i++)
        {
            int index = info.characterInfo[i].index;
            if (index == stringIndex)
                return i;
            if (index > stringIndex)
                break;
        }

        return -1;
    }

    private float GetAvailableWidth()
    {
        float width = _bodyText.rectTransform.rect.width;

        if (width <= 0f)
            width = _bodyText.textBounds.size.x;
        if (width <= 0f)
            width = MaxImageWidth;
        return width;
    }

    private struct PlaceholderBlock
    {
        public int MinLine;
        public int MaxLine;
        public float Top;
        public float Bottom;

        public float Height => Top - Bottom;

        public PlaceholderBlock(int minLine, int maxLine)
        {
            MinLine = minLine;
            MaxLine = maxLine;
            Top = 0f;
            Bottom = 0f;
        }
    }
}
