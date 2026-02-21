using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TMPro;
using SuperNewRoles;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;

namespace SuperNewRoles.Modules
{
    internal enum AnnouncementMediaType
    {
        Image,
        Video
    }

    internal sealed class AnnouncementImageInfo
    {
        public string Url { get; }
        public string AltText { get; }
        public int Index { get; }
        public AnnouncementMediaType MediaType { get; }

        public AnnouncementImageInfo(string url, string altText, int index, AnnouncementMediaType mediaType)
        {
            Url = url ?? string.Empty;
            AltText = altText ?? string.Empty;
            Index = index;
            MediaType = mediaType;
        }
    }

    internal static class AnnouncementImageCache
    {
        private const float SpritePixelsPerUnit = 100f;
        private const int PlaceholderLineCount = 10;
        private const string PlaceholderPrefix = "[[img:";
        private const string PlaceholderSuffix = "]]";
        private const string PlaceholderAlphaStart = "<alpha=#00>";
        private const string PlaceholderAlphaEnd = "<alpha=#FF>";
        private static readonly string MediaCacheDirectory = Path.Combine(SuperNewRolesPlugin.BaseDirectory, "AnnounceCache", "Media");
        private static readonly Dictionary<int, List<AnnouncementImageInfo>> ImagesByNumber = new();
        private static readonly Dictionary<int, string> IdByNumber = new();
        private static readonly Dictionary<string, Sprite> SpriteCache = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> MediaPathCache = new(StringComparer.Ordinal);
        private static readonly Regex MarkdownImageRegex = new Regex("!\\[(?<alt>[^\\]]*)\\]\\((?<url>[^\\)]+)\\)", RegexOptions.Compiled);
        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".mov",
            ".m4v",
            ".webm",
            ".ogv"
        };

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
                    var mediaType = IsVideoUrl(url) ? AnnouncementMediaType.Video : AnnouncementMediaType.Image;
                    images.Add(new AnnouncementImageInfo(url, alt, index, mediaType));
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

        public static void SetAnnouncementId(int announcementNumber, string announcementId)
        {
            if (string.IsNullOrWhiteSpace(announcementId))
            {
                IdByNumber.Remove(announcementNumber);
                return;
            }

            IdByNumber[announcementNumber] = announcementId;
        }

        public static bool TryGetImages(int announcementNumber, out List<AnnouncementImageInfo> images)
        {
            if (ImagesByNumber.TryGetValue(announcementNumber, out images) && images != null && images.Count > 0)
                return true;

            if (IdByNumber.TryGetValue(announcementNumber, out var announcementId))
            {
                EnsureImages(announcementNumber, announcementId);
                if (ImagesByNumber.TryGetValue(announcementNumber, out images) && images != null && images.Count > 0)
                    return true;
            }

            images = null;
            return false;
        }

        public static void EnsureImages(int announcementNumber, string announcementId)
        {
            if (string.IsNullOrWhiteSpace(announcementId))
                return;

            IdByNumber[announcementNumber] = announcementId;

            if (ImagesByNumber.ContainsKey(announcementNumber))
                return;

            string lang = GetApiLanguage();
            if (string.IsNullOrWhiteSpace(lang))
                return;

            var cached = AnnounceCache.GetArticle(announcementId, lang);
            string body = cached?.Article?.Body;
            if (string.IsNullOrWhiteSpace(body))
                return;

            var images = new List<AnnouncementImageInfo>();
            StripMarkdownImages(body, images);
            SetImages(announcementNumber, images);
        }

        public static IEnumerator LoadSprite(string url, Action<Sprite> callback)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                callback?.Invoke(null);
                yield break;
            }

            if (SpriteCache.TryGetValue(url, out var cached) && cached != null)
            {
                callback?.Invoke(cached);
                yield break;
            }

            if (cached != null)
                SpriteCache.Remove(url);

            if (TryLoadAssetSprite(url, out var assetSprite))
            {
                SpriteCache[url] = assetSprite;
                callback?.Invoke(assetSprite);
                yield break;
            }

            if (TryLoadCachedBytes(url, out var cachedBytes))
            {
                var cachedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(cachedTexture, cachedBytes, false))
                {
                    var cachedSprite = Sprite.Create(cachedTexture, new Rect(0f, 0f, cachedTexture.width, cachedTexture.height), new Vector2(0.5f, 0.5f), SpritePixelsPerUnit);
                    SpriteCache[url] = cachedSprite;
                    callback?.Invoke(cachedSprite);
                    yield break;
                }
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

            SaveMediaBytes(url, data);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), SpritePixelsPerUnit);
            SpriteCache[url] = sprite;
            callback?.Invoke(sprite);
        }

        public static IEnumerator LoadVideo(string url, Action<string> callback)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                callback?.Invoke(null);
                yield break;
            }

            if (IsWebUrl(url))
            {
                callback?.Invoke(url);
                yield break;
            }

            if (File.Exists(url))
            {
                callback?.Invoke(url);
                yield break;
            }

            callback?.Invoke(null);
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

        private static bool IsVideoUrl(string url)
        {
            string extension = GetUrlExtension(url);
            return !string.IsNullOrEmpty(extension) && VideoExtensions.Contains(extension);
        }

        private static string GetApiLanguage()
        {
            try
            {
                return DataManager.Settings.Language.CurrentLanguage switch
                {
                    SupportedLangs.Japanese => "ja",
                    SupportedLangs.SChinese => "zh-CN",
                    SupportedLangs.TChinese => "zh-TW",
                    _ => "en"
                };
            }
            catch
            {
                return "en";
            }
        }

        private static bool TryLoadCachedBytes(string url, out byte[] bytes)
        {
            bytes = null;
            if (!TryGetCachedMediaPath(url, out var path))
                return false;

            try
            {
                bytes = File.ReadAllBytes(path);
                return bytes != null && bytes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetCachedMediaPath(string url, out string path)
        {
            if (MediaPathCache.TryGetValue(url, out path) && File.Exists(path))
                return true;

            path = GetMediaCachePath(url);
            if (File.Exists(path))
            {
                MediaPathCache[url] = path;
                return true;
            }

            path = null;
            return false;
        }

        private static string SaveMediaBytes(string url, byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                EnsureMediaCacheDirectory();
                string path = GetMediaCachePath(url);
                File.WriteAllBytes(path, data);
                MediaPathCache[url] = path;
                return path;
            }
            catch
            {
                return null;
            }
        }

        private static void EnsureMediaCacheDirectory()
        {
            if (!Directory.Exists(MediaCacheDirectory))
                Directory.CreateDirectory(MediaCacheDirectory);
        }

        private static string GetMediaCachePath(string url)
        {
            string hash = ModHelpers.HashMD5(url ?? string.Empty);
            string extension = GetUrlExtension(url);
            if (string.IsNullOrEmpty(extension))
                extension = ".bin";
            return Path.Combine(MediaCacheDirectory, $"{hash}{extension}");
        }

        private static string GetUrlExtension(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return Path.GetExtension(uri.AbsolutePath);
            }
            catch
            {
            }

            return Path.GetExtension(url);
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
        private const float MaxImageWidth = 9.88f;
        private const float MaxImageHeight = 7.54f;
        private const float MaxImageScale = 4.68f;
        private const float ImageLeftPadding = 0f;
        private const float VideoPixelsPerUnit = 100f;
        private const float SpinnerSize = 0.7f;
        private const float VideoControlHeightRatio = 0.12f;
        private const float VideoControlMinHeight = 0.12f;
        private const float VideoControlMaxHeight = 0.32f;
        private const float VideoControlWidthRatio = 0.94f;
        private const float VideoControlPaddingRatio = 0.4f;
        private const float VideoControlButtonRatio = 1.6f;
        private const float VideoControlYOffsetRatio = 0.65f;
        private const string PlaceholderPrefix = "[[img:";
        private const string PlaceholderSuffix = "]]";
        private static readonly Color32 VideoControlBackgroundColor = new(0, 0, 0, 160);
        private static readonly Color32 VideoControlFillColor = new(90, 210, 220, 220);
        private static readonly Color32 VideoControlHoverColor = new(70, 240, 220, 255);

        private readonly Dictionary<int, SpriteRenderer> imageRenderers = new();
        private readonly Dictionary<int, VideoEntry> videoEntries = new();
        private readonly Dictionary<int, SpinnerEntry> loadingSpinners = new();
        private TextMeshPro bodyText;
        private Vector3? defaultBodyTextPos;
        private int requestToken;
        private float extraHeight;
        private static Sprite WhiteSprite;

        public bool HasImages => imageRenderers.Count > 0 || videoEntries.Count > 0;

        public void Initialize(TextMeshPro text)
        {
            bodyText = text;
            if (bodyText != null && !defaultBodyTextPos.HasValue)
            {
                defaultBodyTextPos = bodyText.transform.localPosition;
            }
        }

        public void ShowImages(int announcementNumber, bool previewOnly)
        {
            if (bodyText != null && defaultBodyTextPos.HasValue)
            {
                if (announcementNumber >= 1000000000)
                {
                    bodyText.transform.localPosition = defaultBodyTextPos.Value + new Vector3(0f, 0.4f, 0f);
                }
                else
                {
                    bodyText.transform.localPosition = defaultBodyTextPos.Value;
                }
            }

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

        private void Update()
        {
            if (videoEntries.Count == 0)
                return;

            List<int> prepared = null;
            List<int> resized = null;
            foreach (var entry in videoEntries)
            {
                var data = entry.Value;
                var player = data.Player;
                if (player == null)
                    continue;

                if (!player.isPrepared)
                    continue;

                if (!data.Prepared)
                {
                    prepared ??= new List<int>();
                    prepared.Add(entry.Key);
                    continue;
                }

                if (NeedsVideoResize(data, player))
                {
                    resized ??= new List<int>();
                    resized.Add(entry.Key);
                }

            }

            if (prepared != null)
            {
                foreach (var index in prepared)
                {
                    if (!videoEntries.TryGetValue(index, out var entry))
                        continue;

                    if (entry.Player != null && entry.Player.isPrepared && !entry.Prepared)
                        OnVideoPrepared(index, entry.Player);
                }
            }

            if (resized != null)
            {
                foreach (var index in resized)
                {
                    if (!videoEntries.TryGetValue(index, out var entry))
                        continue;

                    if (entry.Player != null && entry.Player.isPrepared)
                        OnVideoPrepared(index, entry.Player);
                }
            }

            foreach (var entry in videoEntries)
            {
                var data = entry.Value;
                if (!data.Prepared)
                    continue;

                UpdateVideoTexture(data);
                UpdateVideoControls(data);
            }
        }

        private IEnumerator LoadImages(List<AnnouncementImageInfo> images, int token)
        {
            foreach (var image in images)
            {
                if (image.MediaType == AnnouncementMediaType.Video)
                {
                    string path = null;
                    yield return AnnouncementImageCache.LoadVideo(image.Url, result => path = result);
                    if (token != requestToken)
                        yield break;

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        RemoveSpinner(image.Index);
                        continue;
                    }

                    var entry = CreateVideoRenderer(image.Index, path);
                    if (entry.Player != null)
                    {
                        videoEntries[image.Index] = entry;
                        try
                        {
                            entry.Player.Prepare();
                        }
                        catch (Exception ex)
                        {
                            OnVideoError(image.Index, ex.Message);
                        }
                        LayoutImages();
                    }
                    else
                    {
                        RemoveSpinner(image.Index);
                    }
                    continue;
                }

                Sprite sprite = null;
                yield return AnnouncementImageCache.LoadSprite(image.Url, result => sprite = result);
                if (token != requestToken)
                    yield break;

                RemoveSpinner(image.Index);

                if (sprite == null)
                    continue;

                var renderer = CreateRenderer(sprite);
                if (renderer != null)
                {
                    imageRenderers[image.Index] = renderer;
                    LayoutImages();
                }
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
            foreach (var entry in videoEntries.Values)
            {
                if (entry.Player != null)
                {
                    entry.Player.errorReceived -= (VideoPlayer.ErrorEventHandler)OnVideoPlayerErrorReceived;
                    entry.Player.Stop();
                }
                if (entry.RenderTexture != null)
                {
                    entry.RenderTexture.Release();
                    Destroy(entry.RenderTexture);
                }
                if (entry.VideoSprite != null)
                    Destroy(entry.VideoSprite);
                if (entry.VideoTexture != null)
                    Destroy(entry.VideoTexture);
                if (entry.GameObject != null)
                    Destroy(entry.GameObject);
            }
            videoEntries.Clear();
            foreach (var spinner in loadingSpinners.Values)
            {
                if (spinner.GameObject != null)
                    Destroy(spinner.GameObject);
            }
            loadingSpinners.Clear();
        }

        private SpriteRenderer CreateRenderer(Sprite sprite)
        {
            if (bodyText == null)
                return null;

            var go = new GameObject("AnnouncementImage");
            go.transform.SetParent(bodyText.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            SyncSorting(renderer);
            ApplyMask(renderer);
            return renderer;
        }

        private void SyncSorting(SpriteRenderer renderer)
        {
            SyncSorting(renderer, 0);
        }

        private void SyncSorting(SpriteRenderer renderer, int orderOffset)
        {
            var meshRenderer = bodyText != null ? bodyText.GetComponent<MeshRenderer>() : null;
            if (meshRenderer == null)
                return;

            renderer.sortingLayerID = meshRenderer.sortingLayerID;
            renderer.sortingOrder = meshRenderer.sortingOrder + orderOffset;
        }

        private void ApplyMask(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;

            renderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        private void LayoutImages()
        {
            if (bodyText == null || (imageRenderers.Count == 0 && videoEntries.Count == 0 && loadingSpinners.Count == 0))
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

            foreach (var entry in videoEntries)
            {
                var video = entry.Value;
                var renderer = video.Renderer;
                if (!video.Prepared || renderer == null)
                    continue;

                float spriteWidth = video.Width;
                float spriteHeight = video.Height;
                if (spriteWidth <= 0f || spriteHeight <= 0f)
                    continue;

                if (blocks.TryGetValue(entry.Key, out var block))
                {
                    float maxHeight2 = Mathf.Min(block.Height, MaxImageHeight);
                    if (maxHeight2 <= 0f)
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
                if (maxHeight <= 0f)
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
            extraHeight = totalExtra;
        }

        private void LayoutSpinners(Dictionary<int, PlaceholderBlock> blocks, float leftEdge)
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
            ApplyMask(renderer);
            var spinner = go.AddComponent<AnnouncementImageSpinner>();
            spinner.Initialize(renderer.transform);
            return new SpinnerEntry(go, renderer);
        }

        private VideoControls CreateVideoControls(Transform parent, int index)
        {
            var root = new GameObject($"AnnouncementVideoControls_{index}");
            root.transform.SetParent(parent, false);
            root.transform.localScale = Vector3.one;
            root.transform.localRotation = Quaternion.identity;
            root.SetActive(false);

            var playButton = new GameObject("PlayPauseButton");
            playButton.transform.SetParent(root.transform, false);
            var playRenderer = playButton.AddComponent<SpriteRenderer>();
            playRenderer.sprite = GetWhiteSprite();
            playRenderer.color = VideoControlBackgroundColor;
            SyncSorting(playRenderer, 1);
            ApplyMask(playRenderer);

            var playText = CreateControlText(playButton.transform);
            var playPauseButton = ConfigureControlButton(playButton, playRenderer, () => ToggleVideoPlayback(index));

            var progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(root.transform, false);
            var progressRenderer = progressBar.AddComponent<SpriteRenderer>();
            progressRenderer.sprite = GetWhiteSprite();
            progressRenderer.color = VideoControlBackgroundColor;
            SyncSorting(progressRenderer, 1);
            ApplyMask(progressRenderer);
            var progressButton = ConfigureControlButton(progressBar, progressRenderer, () => SeekVideoToPointer(index, progressRenderer));

            var progressFill = new GameObject("ProgressFill");
            progressFill.transform.SetParent(progressBar.transform, false);
            var fillRenderer = progressFill.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = GetWhiteSprite();
            fillRenderer.color = VideoControlFillColor;
            SyncSorting(fillRenderer, 2);
            ApplyMask(fillRenderer);

            return new VideoControls(root, playRenderer, playText, playPauseButton, progressRenderer, fillRenderer, progressButton);
        }

        private TextMeshPro CreateControlText(Transform parent)
        {
            var textObject = new GameObject("Label");
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.text = ">";
            text.color = Color.white;
            text.enableWordWrapping = false;
            text.richText = false;
            if (bodyText != null)
            {
                text.fontSize = bodyText.fontSize * 0.9f;
                var bodyRenderer = bodyText.GetComponent<MeshRenderer>();
                var renderer = text.GetComponent<MeshRenderer>();
                if (bodyRenderer != null && renderer != null)
                {
                    renderer.sortingLayerID = bodyRenderer.sortingLayerID;
                    renderer.sortingOrder = bodyRenderer.sortingOrder + 2;
                }
            }
            return text;
        }

        private PassiveButton ConfigureControlButton(GameObject target, SpriteRenderer renderer, Action onClick)
        {
            var button = target.AddComponent<PassiveButton>();
            var collider = target.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            button.Colliders = new Collider2D[] { collider };
            button.OnClick = new();
            button.OnMouseOver = new();
            button.OnMouseOut = new();
            if (onClick != null)
                button.OnClick.AddListener(onClick);
            if (renderer != null)
            {
                button.OnMouseOver.AddListener((Action)(() => renderer.color = VideoControlHoverColor));
                button.OnMouseOut.AddListener((Action)(() => renderer.color = VideoControlBackgroundColor));
            }
            return button;
        }

        private VideoEntry CreateVideoRenderer(int index, string path)
        {
            if (bodyText == null)
                return default;

            string videoUrl = ToFileUrl(path);
            if (string.IsNullOrWhiteSpace(videoUrl))
                return default;

            GameObject go = null;
            RenderTexture renderTexture = null;
            try
            {
                go = new GameObject("AnnouncementVideo");
                go.transform.SetParent(bodyText.transform, false);
                go.SetActive(false);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = GetWhiteSprite();
                renderer.enabled = false;
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    renderer.material = new Material(shader);
                SyncSorting(renderer);
                ApplyMask(renderer);

                renderTexture = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGB32);
                renderTexture.Create();

                var player = go.AddComponent<VideoPlayer>();
                player.playOnAwake = false;
                player.isLooping = true;
                player.audioOutputMode = VideoAudioOutputMode.None;
                player.source = VideoSource.Url;
                player.renderMode = VideoRenderMode.RenderTexture;
                player.waitForFirstFrame = true;
                player.skipOnDrop = true;
                player.targetTexture = renderTexture;
                player.url = videoUrl;
                player.errorReceived += (VideoPlayer.ErrorEventHandler)OnVideoPlayerErrorReceived;

                var clickCollider = go.AddComponent<BoxCollider2D>();
                clickCollider.size = Vector2.one;
                clickCollider.offset = Vector2.zero;

                var clickButton = go.AddComponent<PassiveButton>();
                clickButton.Colliders = new Collider2D[] { clickCollider };
                clickButton.OnClick = new();
                clickButton.OnMouseOver = new();
                clickButton.OnMouseOut = new();
                clickButton.OnClick.AddListener((Action)(() => ToggleVideoPlayback(index)));

                var controls = CreateVideoControls(go.transform, index);
                return new VideoEntry(go, renderer, player, false, 1f, 1f, renderTexture, null, null, controls, clickCollider, clickButton);
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogWarning($"Failed to create announcement video renderer: {ex.Message}");
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Destroy(renderTexture);
                }
                if (go != null)
                    Destroy(go);
                return default;
            }
        }

        private void OnVideoPrepared(int index, VideoPlayer player)
        {
            if (!videoEntries.TryGetValue(index, out var entry))
                return;

            int pixelWidth = player.width > 0 ? (int)player.width : 0;
            int pixelHeight = player.height > 0 ? (int)player.height : 0;

            if (pixelWidth > 0 && pixelHeight > 0 &&
                (entry.RenderTexture == null || entry.RenderTexture.width != pixelWidth || entry.RenderTexture.height != pixelHeight))
            {
                if (entry.RenderTexture != null)
                {
                    entry.RenderTexture.Release();
                    Destroy(entry.RenderTexture);
                }

                var renderTexture = new RenderTexture(pixelWidth, pixelHeight, 0, RenderTextureFormat.ARGB32);
                renderTexture.Create();
                player.targetTexture = renderTexture;
                entry.RenderTexture = renderTexture;
            }

            if (pixelWidth <= 0 && entry.RenderTexture != null)
                pixelWidth = entry.RenderTexture.width;
            if (pixelHeight <= 0 && entry.RenderTexture != null)
                pixelHeight = entry.RenderTexture.height;

            if (pixelWidth > 0 && pixelHeight > 0 &&
                (entry.VideoTexture == null || entry.VideoTexture.width != pixelWidth || entry.VideoTexture.height != pixelHeight))
            {
                if (entry.VideoSprite != null)
                    Destroy(entry.VideoSprite);
                if (entry.VideoTexture != null)
                    Destroy(entry.VideoTexture);

                var videoTexture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.ARGB32, false);
                videoTexture.wrapMode = TextureWrapMode.Clamp;
                videoTexture.filterMode = FilterMode.Bilinear;
                var videoSprite = Sprite.Create(videoTexture, new Rect(0f, 0f, pixelWidth, pixelHeight), new Vector2(0.5f, 0.5f), VideoPixelsPerUnit);
                entry.VideoTexture = videoTexture;
                entry.VideoSprite = videoSprite;
                if (entry.Renderer != null)
                    entry.Renderer.sprite = videoSprite;
            }

            float width = pixelWidth > 0 ? pixelWidth / VideoPixelsPerUnit : 1f;
            float height = pixelHeight > 0 ? pixelHeight / VideoPixelsPerUnit : 1f;
            entry.Width = width;
            entry.Height = height;
            entry.Prepared = true;
            LayoutVideoControls(entry);
            SetVideoControlsActive(entry.Controls, true);
            videoEntries[index] = entry;
            RemoveSpinner(index);
            if (entry.GameObject != null)
                entry.GameObject.SetActive(true);
            if (entry.Renderer != null)
                entry.Renderer.enabled = true;
            player.Play();
            LayoutImages();
        }

        private bool NeedsVideoResize(VideoEntry entry, VideoPlayer player)
        {
            int pixelWidth = player.width > 0 ? (int)player.width : 0;
            int pixelHeight = player.height > 0 ? (int)player.height : 0;
            if (pixelWidth <= 0 || pixelHeight <= 0)
                return false;

            if (entry.RenderTexture == null || entry.RenderTexture.width != pixelWidth || entry.RenderTexture.height != pixelHeight)
                return true;

            if (entry.VideoTexture == null || entry.VideoTexture.width != pixelWidth || entry.VideoTexture.height != pixelHeight)
                return true;

            float expectedWidth = pixelWidth / VideoPixelsPerUnit;
            float expectedHeight = pixelHeight / VideoPixelsPerUnit;
            return !Mathf.Approximately(entry.Width, expectedWidth) || !Mathf.Approximately(entry.Height, expectedHeight);
        }

        private void SetVideoControlsActive(VideoControls controls, bool active)
        {
            if (controls.Root != null && controls.Root.activeSelf != active)
                controls.Root.SetActive(active);
        }

        private void LayoutVideoControls(VideoEntry entry)
        {
            var controls = entry.Controls;
            if (controls.Root == null || entry.Width <= 0f || entry.Height <= 0f)
                return;

            float width = entry.Width;
            float height = entry.Height;
            float barHeight = Mathf.Clamp(height * VideoControlHeightRatio, VideoControlMinHeight, VideoControlMaxHeight);
            float totalWidth = Mathf.Max(0.1f, width * VideoControlWidthRatio);
            float padding = barHeight * VideoControlPaddingRatio;
            float buttonSize = barHeight * VideoControlButtonRatio;
            float barWidth = Mathf.Max(0.1f, totalWidth - buttonSize - padding);
            float leftEdge = -width * 0.5f + (width - totalWidth) * 0.5f;
            float y = -height * 0.5f + barHeight * VideoControlYOffsetRatio;

            controls.Root.transform.localPosition = new Vector3(0f, y, -0.01f);

            if (controls.PlayPauseRenderer != null)
            {
                controls.PlayPauseRenderer.transform.localPosition = new Vector3(leftEdge + buttonSize * 0.5f, 0f, 0f);
                controls.PlayPauseRenderer.transform.localScale = new Vector3(buttonSize, buttonSize, 1f);
            }

            if (controls.PlayPauseText != null)
            {
                controls.PlayPauseText.transform.localPosition = Vector3.zero;
                controls.PlayPauseText.transform.localScale = Vector3.one * 0.6f;
            }

            if (controls.ProgressBackground != null)
            {
                controls.ProgressBackground.transform.localPosition = new Vector3(leftEdge + buttonSize + padding + barWidth * 0.5f, 0f, 0f);
                controls.ProgressBackground.transform.localScale = new Vector3(barWidth, barHeight, 1f);
            }

            if (controls.ProgressFill != null)
            {
                controls.ProgressFill.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);
                controls.ProgressFill.transform.localScale = new Vector3(0f, 1f, 1f);
            }

            UpdateVideoCollider(entry, barHeight);
        }

        private void UpdateVideoCollider(VideoEntry entry, float barHeight)
        {
            if (entry.VideoCollider == null || entry.Width <= 0f || entry.Height <= 0f)
                return;

            float usableHeight = Mathf.Max(0.01f, entry.Height - barHeight);
            entry.VideoCollider.size = new Vector2(entry.Width, usableHeight);
            entry.VideoCollider.offset = new Vector2(0f, barHeight * 0.5f);
        }

        private void UpdateVideoTexture(VideoEntry entry)
        {
            if (entry.RenderTexture == null || entry.VideoTexture == null)
                return;

            try
            {
                if (SystemInfo.copyTextureSupport != CopyTextureSupport.None)
                {
                    Graphics.CopyTexture(entry.RenderTexture, entry.VideoTexture);
                    return;
                }
            }
            catch
            {
            }

            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = entry.RenderTexture;
                entry.VideoTexture.ReadPixels(new Rect(0f, 0f, entry.RenderTexture.width, entry.RenderTexture.height), 0, 0);
                entry.VideoTexture.Apply(false);
            }
            finally
            {
                RenderTexture.active = previous;
            }
        }

        private void UpdateVideoControls(VideoEntry entry)
        {
            var controls = entry.Controls;
            if (controls.Root == null || !controls.Root.activeSelf)
                return;

            var player = entry.Player;
            if (player == null)
                return;

            if (controls.PlayPauseText != null)
                controls.PlayPauseText.text = entry.IsPaused ? ">" : "||";

            float progress = 0f;
            if (entry.IsPaused)
            {
                double length = player.length;
                if (length > 0d && !double.IsInfinity(length) && !double.IsNaN(length))
                    progress = Mathf.Clamp01((float)(entry.SavedTime / length));
            }
            else
            {
                progress = GetVideoProgress(player);
            }
            UpdateProgressFill(controls, progress);
        }

        private float GetVideoProgress(VideoPlayer player)
        {
            if (player == null)
                return 0f;

            double length = player.length;
            if (length <= 0d || double.IsInfinity(length) || double.IsNaN(length))
                return 0f;

            return Mathf.Clamp01((float)(player.time / length));
        }

        private void UpdateProgressFill(VideoControls controls, float progress)
        {
            if (controls.ProgressFill == null)
                return;

            progress = Mathf.Clamp01(progress);
            bool show = progress > 0.001f;
            controls.ProgressFill.enabled = show;
            float width = Mathf.Max(0.001f, progress);
            controls.ProgressFill.transform.localScale = new Vector3(width, 1f, 1f);
            controls.ProgressFill.transform.localPosition = new Vector3(-0.5f + width * 0.5f, 0f, -0.01f);
        }

        private void ToggleVideoPlayback(int index)
        {
            if (!videoEntries.TryGetValue(index, out var entry))
                return;

            var player = entry.Player;
            if (player == null || !entry.Prepared)
                return;

            if (entry.IsPaused)
            {
                entry.IsPaused = false;
                player.time = entry.SavedTime;
                player.Play();
                videoEntries[index] = entry;
            }
            else if (player.isPlaying)
            {
                entry.SavedTime = player.time;
                entry.IsPaused = true;
                player.Stop();
                videoEntries[index] = entry;
            }
            else
            {
                TryResetVideoPosition(player);
                player.Play();
            }

            UpdateVideoControls(entry);
        }

        private void TryResetVideoPosition(VideoPlayer player)
        {
            if (player == null)
                return;

            double length = player.length;
            if (length > 0d && !double.IsInfinity(length) && !double.IsNaN(length))
            {
                if (player.time >= length - 0.05d)
                    player.time = 0d;
                return;
            }

            if (player.frameCount > 0 && player.frame >= (long)player.frameCount - 1)
                player.frame = 0;
        }

        private void SeekVideoToPointer(int index, SpriteRenderer barRenderer)
        {
            if (!videoEntries.TryGetValue(index, out var entry))
                return;

            var player = entry.Player;
            if (player == null || !entry.Prepared || !player.canSetTime)
                return;

            double length = player.length;
            if (length <= 0d || double.IsInfinity(length) || double.IsNaN(length))
                return;

            var camera = Camera.main;
            if (camera == null || barRenderer == null)
                return;

            var world = camera.ScreenToWorldPoint(Input.mousePosition);
            var bounds = barRenderer.bounds;
            float t = Mathf.InverseLerp(bounds.min.x, bounds.max.x, world.x);
            t = Mathf.Clamp01(t);
            double seekTime = length * t;

            if (entry.IsPaused)
            {
                entry.SavedTime = seekTime;
                videoEntries[index] = entry;
            }
            else
            {
                player.time = seekTime;
            }

            UpdateProgressFill(entry.Controls, t);
        }

        private void OnVideoPlayerErrorReceived(VideoPlayer player, string message)
        {
            if (player == null)
                return;

            int index = -1;
            foreach (var pair in videoEntries)
            {
                if (pair.Value.Player == player)
                {
                    index = pair.Key;
                    break;
                }
            }

            if (index >= 0)
                OnVideoError(index, message);
        }

        private void OnVideoError(int index, string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                SuperNewRolesPlugin.Logger.LogWarning($"Video error ({index}): {message}");

            RemoveSpinner(index);
            if (videoEntries.TryGetValue(index, out var entry))
            {
                if (entry.Player != null)
                {
                    entry.Player.errorReceived -= (VideoPlayer.ErrorEventHandler)OnVideoPlayerErrorReceived;
                    entry.Player.Stop();
                }
                if (entry.RenderTexture != null)
                {
                    entry.RenderTexture.Release();
                    Destroy(entry.RenderTexture);
                }
                if (entry.VideoSprite != null)
                    Destroy(entry.VideoSprite);
                if (entry.VideoTexture != null)
                    Destroy(entry.VideoTexture);
                if (entry.GameObject != null)
                    Destroy(entry.GameObject);
                videoEntries.Remove(index);
            }
        }

        private string ToFileUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
            {
                if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    return path;

                if (!uri.IsFile || !IsAllowedLocalVideoPath(uri.LocalPath))
                    return string.Empty;

                return new Uri(Path.GetFullPath(uri.LocalPath)).AbsoluteUri;
            }

            if (!IsAllowedLocalVideoPath(path))
                return string.Empty;

            return new Uri(Path.GetFullPath(path)).AbsoluteUri;
        }

        private bool IsAllowedLocalVideoPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                string fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath))
                    return false;

                string baseDirectory = Path.GetFullPath(SuperNewRolesPlugin.BaseDirectory);
                if (!baseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    baseDirectory += Path.DirectorySeparatorChar;

                return fullPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static Sprite GetWhiteSprite()
        {
            if (WhiteSprite != null)
                return WhiteSprite;

            WhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return WhiteSprite;
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

        private struct VideoControls
        {
            public GameObject Root;
            public SpriteRenderer PlayPauseRenderer;
            public TextMeshPro PlayPauseText;
            public PassiveButton PlayPauseButton;
            public SpriteRenderer ProgressBackground;
            public SpriteRenderer ProgressFill;
            public PassiveButton ProgressButton;

            public VideoControls(GameObject root, SpriteRenderer playPauseRenderer, TextMeshPro playPauseText, PassiveButton playPauseButton,
                SpriteRenderer progressBackground, SpriteRenderer progressFill, PassiveButton progressButton)
            {
                Root = root;
                PlayPauseRenderer = playPauseRenderer;
                PlayPauseText = playPauseText;
                PlayPauseButton = playPauseButton;
                ProgressBackground = progressBackground;
                ProgressFill = progressFill;
                ProgressButton = progressButton;
            }
        }

        private struct VideoEntry
        {
            public GameObject GameObject;
            public SpriteRenderer Renderer;
            public VideoPlayer Player;
            public bool Prepared;
            public float Width;
            public float Height;
            public RenderTexture RenderTexture;
            public Texture2D VideoTexture;
            public Sprite VideoSprite;
            public VideoControls Controls;
            public BoxCollider2D VideoCollider;
            public PassiveButton VideoButton;
            public bool IsPaused;
            public double SavedTime;

            public VideoEntry(GameObject gameObject, SpriteRenderer renderer, VideoPlayer player, bool prepared, float width, float height,
                RenderTexture renderTexture, Texture2D videoTexture, Sprite videoSprite, VideoControls controls, BoxCollider2D videoCollider, PassiveButton videoButton)
            {
                GameObject = gameObject;
                Renderer = renderer;
                Player = player;
                Prepared = prepared;
                Width = width;
                Height = height;
                RenderTexture = renderTexture;
                VideoTexture = videoTexture;
                VideoSprite = videoSprite;
                Controls = controls;
                VideoCollider = videoCollider;
                IsPaused = false;
                SavedTime = 0d;
                VideoButton = videoButton;
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
