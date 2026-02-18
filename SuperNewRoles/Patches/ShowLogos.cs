using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

// メインメニューのロゴ表示処理を担当するクラス
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuLogos
{
    private const string DefaultBannerAssetName = "banner";
    private const string NewYearBannerAssetName = "banner_NewYear";
    private static readonly TimeSpan JstOffset = TimeSpan.FromHours(9);

    // Transform拡張メソッド：位置とスケールを同時に設定する
    /// <summary>
    /// Transformの位置とスケールを一度に設定する拡張メソッド
    /// </summary>
    /// <param name="position">ローカル座標での位置</param>
    /// <param name="scale">適用するスケール値</param>
    public static void SetPositionAndScale(this Transform transform, Vector3 position, Vector3 scale)
    {
        transform.localPosition = position;
        transform.localScale = scale;
    }
    public static void Postfix()
    {
        ShowModStamp();
        CreateMainMenuLogo();
    }

    private static void ShowModStamp() =>
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

    private static void CreateMainMenuLogo()
    {
        // アセットバンドルからロゴ画像を読み込み
        var bannerAssetName = GetBannerAssetName();
        var logo = AssetManager.GetAsset<Sprite>(bannerAssetName, AssetManager.AssetBundleType.Sprite);
        // フォールバック
        if (logo == null && bannerAssetName != DefaultBannerAssetName)
        {
            logo = AssetManager.GetAsset<Sprite>(DefaultBannerAssetName, AssetManager.AssetBundleType.Sprite);
        }
        if (logo == null)
        {
            Logger.Error("ロゴ画像の読み込みに失敗しました");
            return;
        }

        // メインメニュー用ロゴオブジェクトの作成
        var logoObject = new GameObject("MainMenuLogo");
        logoObject.transform.SetPositionAndScale(
            new(1.85f, 0.5f, 0),  // 右側に配置
            Vector3.one * 0.55f  // 適切なサイズに縮小
        );

        logoObject.AddComponent<SpriteRenderer>().sprite = logo;
    }

    internal static string GetBannerAssetName() =>
        IsNewYearBannerActive() ? NewYearBannerAssetName : DefaultBannerAssetName;

    private static bool IsNewYearBannerActive()
    {
        var nowJst = DateTimeOffset.UtcNow.ToOffset(JstOffset);
        var endJst = new DateTimeOffset(nowJst.Year, 1, 7, 17, 0, 0, JstOffset);
        return nowJst <= endJst;
    }
}

// バージョン情報表示のハンドリングクラス
public static class VersionTextHandler
{
    private const string ModColor = "#a6d289";
    private const float VersionTextScale = 1.5f;
    private const float CredentialsTextScale = 2.0f;
    private const string SocialIconsRootName = "SNRMainMenuSocialIcons";
    private const string DiscordIconAssetName = "SNR_DiscordIcon_MainMenu.png";
    private const string XIconAssetName = "SNR_XIcon_MainMenu.png";
    private const float SocialIconsXOffset = 0.35f;
    private const float SocialIconHoverScale = 1.08f;
    private const int MainMenuUiLayer = 5;
    private static readonly Color DiscordBlurple = new(88f / 255f, 101f / 255f, 242f / 255f, 0.95f);
    private static readonly Color SocialIconBackdropOuterColor = new(0f, 0f, 0f, 0.55f);
    private static readonly Color SocialIconBackdropInnerColor = new(1f, 1f, 1f, 0.92f);
    private static readonly Vector3 SocialIconScale = Vector3.one * 0.25f;
    private static Sprite _roundedRectSprite;

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    private static class VersionShowerPatch
    {
        static void Postfix(VersionShower __instance)
        {
            if (GameObject.FindObjectOfType<MainMenuManager>() == null) return;
            CreateCredentialsText(__instance);
            var versionText = CreateVersionText(__instance);
            CreateSocialIcons(versionText);
        }

        // クレジットテキスト生成処理
        /// <summary>
        /// 開発者クレジットテキストを作成し画面に表示する
        /// </summary>
        private static void CreateCredentialsText(VersionShower instance)
        {
            var credentials = GameObject.Instantiate(instance.text);
            credentials.transform.SetPositionAndScale(
                new Vector3(2, -0.3f, 0),
                Vector3.one * CredentialsTextScale
            );

            credentials.SetText(GetCredentialsText());
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.9f;  // 文字サイズ微調整
        }

        // バージョン情報テキスト生成処理
        /// <summary>
        /// モッドのバージョン情報テキストを作成し表示する
        /// </summary>
        private static TMPro.TextMeshPro CreateVersionText(VersionShower instance)
        {
            var version = GameObject.Instantiate(instance.text);
            version.name = "SNRVersionText";
            version.transform.SetPositionAndScale(
                new Vector3(2, -0.65f, 0),  // クレジットテキストの直下
                Vector3.one * VersionTextScale
            );
            version.SetText($"{Statics.ModName} v{Statics.VersionString}");
            return version;
        }

        private static void CreateSocialIcons(TMPro.TextMeshPro versionText)
        {
            if (versionText.transform.Find(SocialIconsRootName) != null)
                return;

            var socialIconsRoot = new GameObject(SocialIconsRootName);
            socialIconsRoot.layer = MainMenuUiLayer;
            socialIconsRoot.transform.SetParent(versionText.transform, false);
            // Keep icon size/offset stable while still inheriting version text as parent.
            socialIconsRoot.transform.localScale = Vector3.one * 0.6f;
            socialIconsRoot.transform.localPosition = new Vector3(-0.03f, -0.31f, 0);

            CreateSocialIcon(
                socialIconsRoot.transform,
                new Vector3(-SocialIconsXOffset, 0, 0f),
                DiscordIconAssetName,
                SocialLinks.DiscordServer
            );
            CreateSocialIcon(
                socialIconsRoot.transform,
                new Vector3(SocialIconsXOffset, 0, 0f),
                XIconAssetName,
                SocialLinks.XSnrOfficials
            );
        }

        private static void CreateSocialIcon(Transform parent, Vector3 localPosition, string assetName, string targetUrl)
        {
            var iconSprite = AssetManager.GetAsset<Sprite>(assetName);
            if (iconSprite == null)
            {
                Logger.Warning($"SNS icon asset was not found: {assetName}", nameof(VersionTextHandler));
                return;
            }

            var iconObject = new GameObject(assetName);
            iconObject.layer = MainMenuUiLayer;
            iconObject.transform.SetParent(parent, false);
            iconObject.transform.localPosition = localPosition;
            iconObject.transform.localScale = SocialIconScale;

            var iconRenderer = iconObject.AddComponent<SpriteRenderer>();
            iconRenderer.sprite = iconSprite;
            iconRenderer.sortingOrder = 2;
            bool isDiscordIcon = assetName == DiscordIconAssetName;
            if (isDiscordIcon)
                iconRenderer.color = Color.white;

            CreateIconBackdrop(iconObject.transform, iconSprite, 1.25f, SocialIconBackdropOuterColor, 0);
            CreateIconBackdrop(
                iconObject.transform,
                iconSprite,
                1.12f,
                isDiscordIcon ? DiscordBlurple : SocialIconBackdropInnerColor,
                1
            );

            var boxCollider = iconObject.AddComponent<BoxCollider2D>();
            boxCollider.size = iconSprite.bounds.size;
            boxCollider.offset = iconSprite.bounds.center;

            var passiveButton = iconObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { boxCollider };
            passiveButton.OnClick = new();
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOver = new();

            passiveButton.OnClick.AddListener((UnityAction)(() => Application.OpenURL(targetUrl)));
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                iconObject.transform.localScale = SocialIconScale * SocialIconHoverScale;
            }));
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                iconObject.transform.localScale = SocialIconScale;
            }));
        }

        private static void CreateIconBackdrop(Transform iconParent, Sprite iconSprite, float sizeMultiplier, Color color, int sortingOrder)
        {
            var backdrop = new GameObject("Backdrop");
            backdrop.layer = MainMenuUiLayer;
            backdrop.transform.SetParent(iconParent, false);
            backdrop.transform.localPosition = Vector3.zero;

            var renderer = backdrop.AddComponent<SpriteRenderer>();
            renderer.sprite = GetRoundedRectSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            var iconSize = iconSprite.bounds.size;
            var side = Mathf.Max(iconSize.x, iconSize.y) * sizeMultiplier;
            backdrop.transform.localScale = new Vector3(side, side, 1f);
        }

        private static Sprite GetRoundedRectSprite()
        {
            if (_roundedRectSprite != null)
                return _roundedRectSprite;

            const int size = 256;
            const float radius = 56f;
            const float antiAliasWidth = 2f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs((x + 0.5f) - half);
                    float py = Mathf.Abs((y + 0.5f) - half);
                    float dx = Mathf.Max(px - (half - radius), 0f);
                    float dy = Mathf.Max(py - (half - radius), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    float t = Mathf.Clamp01((dist + antiAliasWidth) / (antiAliasWidth * 2f));
                    float alpha = 1f - t;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.Apply();
            _roundedRectSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return _roundedRectSprite;
        }

        /// <summary>
        /// Gitブランチ情報を含むクレジットテキストを生成
        /// </summary>
        /// <returns>フォーマット済みクレジット文字列</returns>
        private static string GetCredentialsText()
        {
            // ベータ版の場合のみブランチ情報を表示
            var branchInfo = Statics.IsBeta
                ? $"\r\n<color={ModColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>"
                : "";

            return branchInfo + ModTranslation.GetString("creditsMain");
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class ShowInGameLogosPatch
    {
        public static AspectPosition _aspectPosition { get; private set; }
        public static void Postfix(HudManager __instance)
        {
            GameObject _logoObject = new("Logo");
            _logoObject.layer = 5;
            _logoObject.transform.SetParent(__instance.transform);
            _logoObject.transform.localScale = Vector3.one * 0.2f;
            _logoObject.transform.localPosition = new(-3.8f, 2.5f, -1f);
            var _logoRenderer = _logoObject.AddComponent<SpriteRenderer>();
            // Hudの方はサイズ調整めんどくさかったのでパス
            _logoRenderer.sprite = AssetManager.GetAsset<Sprite>(
                "banner",
                AssetManager.AssetBundleType.Sprite
            );
            // バージョンテキスト
            var versionText = GameObject.Instantiate(__instance.roomTracker.text);
            versionText.name = "VersionText";
            GameObject.Destroy(versionText.GetComponent<RoomTracker>());
            versionText.transform.SetParent(_logoObject.transform);
            versionText.transform.localScale = Vector3.one * 2.55f;
            versionText.transform.localPosition = new(-3.45f, -0.88f, 0f);
            versionText.SetText($"<color=#ffa500>v{VersionInfo.VersionString}</color>");
            _aspectPosition = _logoObject.AddComponent<AspectPosition>();
            _aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            _aspectPosition.DistanceFromEdge = new(1, 0.27f);
            _aspectPosition.OnEnable();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipStatusPatch
    {
        public static void Postfix()
        {
            ShowInGameLogosPatch._aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
            ShowInGameLogosPatch._aspectPosition.DistanceFromEdge = new(4.13f, 0.5f);
            ShowInGameLogosPatch._aspectPosition.OnEnable();
        }
    }
}
