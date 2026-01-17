using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    internal sealed class AnnouncementImageInfo
    {
        public string Url { get; }
        public string AltText { get; }
        public int Index { get; }

        public AnnouncementImageInfo(string url, string altText, int index)
        {
            Url = url ?? string.Empty;
            AltText = altText ?? string.Empty;
            Index = index;
        }
    }

    internal static class AnnouncementImageCache
    {
        private const float SpritePixelsPerUnit = 100f;
        private const int PlaceholderLineCount = 6;
        private const string PlaceholderPrefix = "[[img:";
        private const string PlaceholderSuffix = "]]";
        private const string PlaceholderAlphaStart = "<alpha=#00>";
        private const string PlaceholderAlphaEnd = "<alpha=#FF>";
        private static readonly Dictionary<int, List<AnnouncementImageInfo>> ImagesByNumber = new();
        private static readonly Dictionary<string, Sprite> SpriteCache = new(StringComparer.Ordinal);
        private static readonly Regex MarkdownImageRegex = new Regex("!\\[(?<alt>[^\\]]*)\\]\\((?<url>[^\\)]+)\\)", RegexOptions.Compiled);

        public static string StripMarkdownImages(string markdown, List<AnnouncementImageInfo> images)
        {
            if (string.IsNullOrEmpty(markdown))
                return string.Empty;

            if (images == null)
                return markdown;

            int index = 0;
            return MarkdownImageRegex.Replace(markdown, match =>
            {
                string alt = match.Groups["alt"].Value;
                string url = ExtractUrl(match.Groups["url"].Value);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    images.Add(new AnnouncementImageInfo(url, alt, index));
                    return BuildPlaceholderBlock(index++);
                }
                return string.Empty;
            });
        }

        public static void SetImages(int announcementNumber, List<AnnouncementImageInfo> images)
        {
            if (images == null || images.Count == 0)
            {
                ImagesByNumber.Remove(announcementNumber);
                return;
            }

            ImagesByNumber[announcementNumber] = images;
        }

        public static bool TryGetImages(int announcementNumber, out List<AnnouncementImageInfo> images)
        {
            if (ImagesByNumber.TryGetValue(announcementNumber, out images) && images != null && images.Count > 0)
                return true;

            images = null;
            return false;
        }

        public static IEnumerator LoadSprite(string url, Action<Sprite> callback)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                callback?.Invoke(null);
                yield break;
            }

            if (SpriteCache.TryGetValue(url, out var cached))
            {
                callback?.Invoke(cached);
                yield break;
            }

            if (TryLoadAssetSprite(url, out var assetSprite))
            {
                SpriteCache[url] = assetSprite;
                callback?.Invoke(assetSprite);
                yield break;
            }

            if (!IsWebUrl(url))
            {
                callback?.Invoke(null);
                yield break;
            }

            var client = SNRHttpClient.Get(url);
            client.ignoreSslErrors = true;
            yield return client.SendWebRequest();
            if (!string.IsNullOrWhiteSpace(client.error))
            {
                callback?.Invoke(null);
                yield break;
            }

            var data = client.downloadHandler != null ? client.downloadHandler.data : null;
            if (data == null || data.Length == 0)
            {
                callback?.Invoke(null);
                yield break;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, data, false))
            {
                callback?.Invoke(null);
                yield break;
            }

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), SpritePixelsPerUnit);
            SpriteCache[url] = sprite;
            callback?.Invoke(sprite);
        }

        private static string BuildPlaceholderBlock(int index)
        {
            string token = $"{PlaceholderPrefix}{index}{PlaceholderSuffix}";
            string invisibleToken = $"{PlaceholderAlphaStart}{token}{PlaceholderAlphaEnd}";
            var builder = new StringBuilder();
            builder.Append('\n');
            for (int i = 0; i < PlaceholderLineCount; i++)
            {
                builder.Append(invisibleToken);
                if (i < PlaceholderLineCount - 1)
                    builder.Append('\n');
            }
            builder.Append('\n');
            return builder.ToString();
        }

        private static string ExtractUrl(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string trimmed = raw.Trim();
            int spaceIndex = trimmed.IndexOf(' ');
            if (spaceIndex > 0)
                trimmed = trimmed.Substring(0, spaceIndex);

            return trimmed.Trim();
        }

        private static bool IsWebUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryLoadAssetSprite(string url, out Sprite sprite)
        {
            sprite = null;
            string path = null;

            if (url.StartsWith("asset:", StringComparison.OrdinalIgnoreCase))
            {
                path = url.Substring("asset:".Length);
            }
            else if (url.StartsWith("snr:", StringComparison.OrdinalIgnoreCase))
            {
                path = url.Substring("snr:".Length);
            }
            else if (url.StartsWith("asset://", StringComparison.OrdinalIgnoreCase))
            {
                path = url.Substring("asset://".Length);
            }

            if (string.IsNullOrWhiteSpace(path))
                return false;

            path = path.Trim().TrimStart('/', '\\');
            sprite = AssetManager.GetAsset<Sprite>(path);
            return sprite != null;
        }
    }

    internal sealed class AnnouncementImageRenderer : MonoBehaviour
    {
        private const float MaxImageWidth = 6.76f;
        private const float MaxImageHeight = 4.94f;
        private const float MaxImageScale = 2.5f;
        private const float ImageLeftPadding = 0f;
        private const float SpinnerSize = 0.7f;
        private const float SpinnerExtraSpacing = 0.2f;
        private const string PlaceholderPrefix = "[[img:";
        private const string PlaceholderSuffix = "]]";

        private readonly Dictionary<int, SpriteRenderer> imageRenderers = new();
        private readonly Dictionary<int, SpinnerEntry> loadingSpinners = new();
        private TextMeshPro bodyText;
        private int requestToken;
        private float extraHeight;

        public bool HasImages => imageRenderers.Count > 0;

        public void Initialize(TextMeshPro text)
        {
            bodyText = text;
        }

        public void ShowImages(int announcementNumber, bool previewOnly)
        {
            if (previewOnly || bodyText == null)
            {
                ClearImages();
                return;
            }

            if (!AnnouncementImageCache.TryGetImages(announcementNumber, out var images) || images.Count == 0)
            {
                ClearImages();
                return;
            }

            requestToken++;
            ClearImagesInternal();
            CreateLoadingSpinners(images);
            StartCoroutine(LoadImages(images, requestToken).WrapToIl2Cpp());
        }

        public void ClearImages()
        {
            requestToken++;
            ClearImagesInternal();
        }

        public float GetExtraScrollHeight()
        {
            return extraHeight;
        }

        public float GetTextHeight()
        {
            if (bodyText == null)
                return 0f;
            return Mathf.Abs(bodyText.GetNotDumbRenderedHeight());
        }

        private IEnumerator LoadImages(List<AnnouncementImageInfo> images, int token)
        {
            foreach (var image in images)
            {
                Sprite sprite = null;
                yield return AnnouncementImageCache.LoadSprite(image.Url, result => sprite = result);
                if (token != requestToken)
                    yield break;

                RemoveSpinner(image.Index);

                if (sprite == null)
                    continue;

                var renderer = CreateRenderer(sprite);
                imageRenderers[image.Index] = renderer;
                LayoutImages();
            }

            LayoutImages();
        }

        private void ClearImagesInternal()
        {
            extraHeight = 0f;
            foreach (var renderer in imageRenderers.Values)
            {
                if (renderer != null)
                    Destroy(renderer.gameObject);
            }
            imageRenderers.Clear();
            foreach (var spinner in loadingSpinners.Values)
            {
                if (spinner.GameObject != null)
                    Destroy(spinner.GameObject);
            }
            loadingSpinners.Clear();
        }

        private SpriteRenderer CreateRenderer(Sprite sprite)
        {
            var go = new GameObject("AnnouncementImage");
            go.transform.SetParent(bodyText.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            SyncSorting(renderer);
            return renderer;
        }

        private void SyncSorting(SpriteRenderer renderer)
        {
            var meshRenderer = bodyText != null ? bodyText.GetComponent<MeshRenderer>() : null;
            if (meshRenderer == null)
                return;

            renderer.sortingLayerID = meshRenderer.sortingLayerID;
            renderer.sortingOrder = meshRenderer.sortingOrder;
        }

        private void LayoutImages()
        {
            if (bodyText == null || (imageRenderers.Count == 0 && loadingSpinners.Count == 0))
            {
                extraHeight = 0f;
                return;
            }

            bodyText.ForceMeshUpdate();
            var blocks = GetPlaceholderBlocks(bodyText.textInfo);
            float availableWidth = GetAvailableWidth();
            float maxWidth = Mathf.Min(availableWidth, MaxImageWidth);
            float leftEdge = GetLeftEdge();
            float totalExtra = 0f;
            if (maxWidth <= 0f)
            {
                extraHeight = 0f;
                return;
            }
            float fallbackY = bodyText.textBounds.min.y - 0.2f;

            foreach (var entry in imageRenderers)
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

            LayoutSpinners(blocks, leftEdge, ref fallbackY, ref totalExtra);
            extraHeight = totalExtra;
        }

        private void LayoutSpinners(Dictionary<int, PlaceholderBlock> blocks, float leftEdge, ref float fallbackY, ref float totalExtra)
        {
            if (loadingSpinners.Count == 0)
                return;

            foreach (var entry in loadingSpinners)
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
            float left = bodyText.textBounds.size.x > 0f ? bodyText.textBounds.min.x : bodyText.rectTransform.rect.min.x;
            return left + ImageLeftPadding;
        }

        private void CreateLoadingSpinners(List<AnnouncementImageInfo> images)
        {
            if (images == null || bodyText == null)
                return;

            foreach (var image in images)
            {
                if (loadingSpinners.ContainsKey(image.Index))
                    continue;

                var spinner = CreateSpinner();
                if (spinner.GameObject != null)
                    loadingSpinners[image.Index] = spinner;
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
            go.transform.SetParent(bodyText.transform, false);
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedSprite;
            if (selectedMaterial != null)
                renderer.sharedMaterial = selectedMaterial;

            SyncSorting(renderer);
            var spinner = go.AddComponent<AnnouncementImageSpinner>();
            spinner.Initialize(renderer.transform);
            return new SpinnerEntry(go, renderer);
        }

        private void RemoveSpinner(int index)
        {
            if (!loadingSpinners.TryGetValue(index, out var spinner))
                return;

            if (spinner.GameObject != null)
                Destroy(spinner.GameObject);
            loadingSpinners.Remove(index);
        }

        private Dictionary<int, PlaceholderBlock> GetPlaceholderBlocks(TMP_TextInfo info)
        {
            var blocks = new Dictionary<int, PlaceholderBlock>();
            if (info == null || info.characterCount == 0 || bodyText == null)
                return blocks;

            string raw = bodyText.text;
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
                width = bodyText.rectTransform.rect.width;
            }
            catch
            {
            }

            if (width <= 0f)
                width = bodyText.textBounds.size.x;
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

    internal sealed class AnnouncementImageSpinner : MonoBehaviour
    {
        private const float SpinSpeed = 175f;
        private Transform rotateTarget;

        public void Initialize(Transform target)
        {
            rotateTarget = target;
        }

        private void Update()
        {
            if (rotateTarget != null)
                rotateTarget.Rotate(0f, 0f, SpinSpeed * Time.deltaTime);
        }
    }
}
