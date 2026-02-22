using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

// Android-specific image-only renderer to avoid VideoPlayer-related IL2CPP injection issues.
public class AnnouncementImageRendererAndroid : MonoBehaviour
{
    private const float MaxImageWidth = 9.88f;
    private const float MaxImageHeight = 7.54f;
    private const float MaxImageScale = 4.68f;
    private const float ImageLeftPadding = 0f;
    private const float SpinnerSize = 0.7f;
    private const string PlaceholderPrefix = "[[img:";
    private const string PlaceholderSuffix = "]]";

    private readonly Dictionary<int, SpriteRenderer> _imageRenderers = new();
    private readonly Dictionary<int, SpinnerEntry> _loadingSpinners = new();
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
        {
            _defaultBodyTextPos = _bodyText.transform.localPosition;
        }
    }

    [HideFromIl2Cpp]
    public void ShowImages(int announcementNumber, bool previewOnly)
    {
        try
        {
            if (_bodyText != null && _defaultBodyTextPos.HasValue)
            {
                if (announcementNumber >= 1000000000)
                {
                    _bodyText.transform.localPosition = _defaultBodyTextPos.Value + new Vector3(0f, 0.4f, 0f);
                }
                else
                {
                    _bodyText.transform.localPosition = _defaultBodyTextPos.Value;
                }
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
            CreateLoadingSpinners(images);
            StartCoroutine(LoadImages(images, _requestToken).WrapToIl2Cpp());
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementImageRendererAndroid.ShowImages failed: {ex}");
            SuperNewRolesPlugin.DisableAnnouncementImageSupport(ex.Message);
            ClearImagesInternal();
        }
    }

    public void ClearImages()
    {
        _requestToken++;
        ClearImagesInternal();
    }

    public float GetExtraScrollHeight()
    {
        try
        {
            return _extraHeight;
        }
        catch
        {
            SuperNewRolesPlugin.DisableAnnouncementImageSupport("AnnouncementImageRendererAndroid.GetExtraScrollHeight failed");
            return 0f;
        }
    }

    public float GetTextHeight()
    {
        try
        {
            if (_bodyText == null)
                return 0f;
            return Mathf.Abs(_bodyText.GetNotDumbRenderedHeight());
        }
        catch
        {
            SuperNewRolesPlugin.DisableAnnouncementImageSupport("AnnouncementImageRendererAndroid.GetTextHeight failed");
            return 0f;
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator LoadImages(List<AnnouncementImageInfo> images, int token)
    {
        foreach (var image in images)
        {
            if (image.MediaType != AnnouncementMediaType.Image)
            {
                RemoveSpinner(image.Index);
                continue;
            }

            Sprite sprite = null;
            yield return AnnouncementImageCache.LoadSprite(image.Url, result => sprite = result);
            if (token != _requestToken)
                yield break;

            RemoveSpinner(image.Index);

            if (sprite == null)
                continue;

            var renderer = CreateRenderer(sprite);
            if (renderer != null)
            {
                _imageRenderers[image.Index] = renderer;
                LayoutImages();
            }
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

        foreach (var spinner in _loadingSpinners.Values)
        {
            if (spinner.GameObject != null)
                Destroy(spinner.GameObject);
        }
        _loadingSpinners.Clear();
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
        if (_bodyText == null || (_imageRenderers.Count == 0 && _loadingSpinners.Count == 0))
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

        LayoutSpinners(blocks, leftEdge);
        _extraHeight = totalExtra;
    }

    [HideFromIl2Cpp]
    private void LayoutSpinners(Dictionary<int, PlaceholderBlock> blocks, float leftEdge)
    {
        if (_loadingSpinners.Count == 0)
            return;

        foreach (var entry in _loadingSpinners)
        {
            var spinnerEntry = entry.Value;
            var renderer = spinnerEntry.Renderer;
            if (renderer == null || renderer.sprite == null)
                continue;

            float spriteWidth = renderer.sprite.bounds.size.x;
            float spriteHeight = renderer.sprite.bounds.size.y;
            if (spriteWidth <= 0f || spriteHeight <= 0f)
                continue;

            if (blocks.TryGetValue(entry.Key, out var block))
            {
                float maxHeight = Mathf.Max(block.Height, SpinnerSize);
                float scale = Mathf.Min(SpinnerSize / spriteWidth, maxHeight / spriteHeight);
                renderer.transform.localScale = new Vector3(scale, scale, 1f);
                float scaledWidth = spriteWidth * scale;
                float centerY = (block.Top + block.Bottom) * 0.5f;
                float x = leftEdge + scaledWidth * 0.5f;
                renderer.transform.localPosition = new Vector3(x, centerY, 0f);
                if (spinnerEntry.GameObject != null && !spinnerEntry.GameObject.activeSelf)
                    spinnerEntry.GameObject.SetActive(true);
                continue;
            }

            if (spinnerEntry.GameObject != null && spinnerEntry.GameObject.activeSelf)
                spinnerEntry.GameObject.SetActive(false);
        }
    }

    private float GetLeftEdge()
    {
        float left = _bodyText.textBounds.size.x > 0f ? _bodyText.textBounds.min.x : _bodyText.rectTransform.rect.min.x;
        return left + ImageLeftPadding;
    }

    [HideFromIl2Cpp]
    private void CreateLoadingSpinners(List<AnnouncementImageInfo> images)
    {
        if (images == null || _bodyText == null)
            return;

        foreach (var image in images)
        {
            if (image.MediaType != AnnouncementMediaType.Image || _loadingSpinners.ContainsKey(image.Index))
                continue;

            var spinner = CreateSpinner();
            if (spinner.GameObject != null)
                _loadingSpinners[image.Index] = spinner;
        }

        LayoutImages();
    }

    private SpinnerEntry CreateSpinner()
    {
        var prefab = AssetManager.GetAsset<GameObject>("LoadingUI");
        if (prefab == null)
            return default;

        var temp = Instantiate(prefab);
        var spriteRenderers = temp.GetComponentsInChildren<SpriteRenderer>(true);
        Sprite selectedSprite = null;
        Material selectedMaterial = null;
        float bestArea = float.MaxValue;

        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                continue;

            var rect = spriteRenderer.sprite.rect;
            float area = rect.width * rect.height;
            if (area < bestArea)
            {
                bestArea = area;
                selectedSprite = spriteRenderer.sprite;
                selectedMaterial = spriteRenderer.sharedMaterial;
            }
        }

        Destroy(temp);

        if (selectedSprite == null)
            return default;

        var go = new GameObject("AnnouncementImageSpinner");
        go.transform.SetParent(_bodyText.transform, false);
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        go.SetActive(false);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = selectedSprite;
        if (selectedMaterial != null)
            renderer.sharedMaterial = selectedMaterial;

        SyncSorting(renderer);
        ApplyMask(renderer);
        return new SpinnerEntry(go, renderer);
    }

    [HideFromIl2Cpp]
    private void RemoveSpinner(int index)
    {
        if (!_loadingSpinners.TryGetValue(index, out var spinner))
            return;

        if (spinner.GameObject != null)
            Destroy(spinner.GameObject);
        _loadingSpinners.Remove(index);
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
            var top = info.lineInfo[minLine].ascender;
            var bottom = info.lineInfo[maxLine].descender;
            block.Top = top;
            block.Bottom = bottom;
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
        float width = 0f;
        try
        {
            width = _bodyText.rectTransform.rect.width;
        }
        catch
        {
        }

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

    private readonly struct SpinnerEntry
    {
        public GameObject GameObject { get; }
        public SpriteRenderer Renderer { get; }

        public SpinnerEntry(GameObject gameObject, SpriteRenderer renderer)
        {
            GameObject = gameObject;
            Renderer = renderer;
        }
    }
}
