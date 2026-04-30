using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Modules;

/// <summary>
/// オンボーディング本文の多段レイアウト。将来 3 列や 2×2 に拡張する際は <see cref="Step.BodyColumnKeys"/> の順と組み合わせる。
/// </summary>
public enum OnboardingColumnLayout
{
    None = 0,
    /// <summary>横に 2 列（現在の FAQ など）。キーは左→右の順で 2 個。</summary>
    Columns2 = 2,
    /// <summary>横に 3 列。キーは左→中→右の順で 3 個。</summary>
    Columns3 = 3,
    /// <summary>2 行×2 列。キーは左上→右上→左下→右下の順で 4 個。</summary>
    Grid2x2 = 4,
}

// バグ報告ページのスクショ用スプライト名: OnboardingBugReportStep1 / Step2 / Step3（AssetBundleType.Sprite）。
// 推奨テクスチャ: 横 480px × 縦 320px（3:2）程度を 1 枚あたり用意する。Pixels Per Unit = 200 を想定。
// 表示サイズは BugReportImageDisplayScale で最終調整できる。
public static class OnboardingPopup
{
    // 既存のアナリティクス許可ポップアップ prefab を、オンボーディング用の器として再利用する。
    private const string PrefabName = "AnalyticsBG";
    private const float PopupScale = 0.58f;
    private const float NextButtonCooldownSeconds = 0.5f;
    private const float PageTransitionSeconds = 0.24f;
    private const float PageTransitionSlideDistance = 1.15f;
    private const float PopupCloseSeconds = 0.22f;
    private const float PopupCloseScaleMultiplier = 0.92f;
    private const float PopupCloseYOffset = 0.16f;
    private const float RoleIconCarouselSeconds = 1.8f;
    private const float RoleIconFadeSeconds = 0.35f;
    private const float RoleIconBaseScale = 0.14f;
    private const float RoleIconPulseScale = 0.02f;
    private const float WelcomeLogoScale = 0.28f;
    private const int WelcomeStepIndex = 0;
    private const int RoleCarouselStepIndex = 1;
    private const int PlayStepIndex = 2;
    private const int BugReportStepIndex = 3;
    private const int AnalyticsStepIndex = 4;
    private const string DiscordLinkId = "discord";

    // 汎用カラム（FAQ の 2 列、将来 3 列 / 2×2）。最大 4 セル。
    private const float FlexColumnTopY = 0.35f;
    private const float FlexColumnWidth = 6.2f;
    private const float FlexColumnHeight = 4.2f;
    private const float FlexColumnOffsetX2 = 3.5f;
    private const float FlexColumnSpacingX3 = 3.15f;
    private const float FlexGridRowDeltaY = 2.1f;

    // 「人の集め方」2カラム用の固定座標（_popup ローカル）。
    // タイトルが y≒2.4 付近、Back/Next ボタンが y=-3.2 にあるため、その間 (約 -2.6〜+1.7) に収まるようにする。
    private const float PlayColumnOffsetX = 3.6f;
    private const float PlayColumnTopY = 0.4f;
    private const float PlayColumnWidth = 6.5f;
    private const float PlayColumnHeight = 4.4f;
    private const float PlayFootY = -2.15f;

    // バグ報告ページ。スクショ枠 → キャプション（縦並び）。
    private const float BugReportImageLocalY = 1.45f;
    private const float BugReportImageSpacingX = 4.7f;
    private const float BugReportImageDisplayScale = 0.28f;
    private const float BugReportImageWidth = 4.0f;
    private const float BugReportImageHeight = 2.4f;
    private const float BugReportCaptionLocalY = -0.45f;
    private const float BugReportFooterLocalY = -2f;

    // Among Us のメニュー UI は z が小さいほど手前に描画されるため、_popup より負方向へ出す。
    private const float PopupForegroundZOffset = -0.1f;

    // ナビゲーションボタンは prefab 由来の位置ではなく、オンボーディング専用の固定位置に置く。
    private static readonly Vector3 BackButtonLocalPosition = new(-5f, -3.2f, -0.1f);
    private static readonly Vector3 NextButtonLocalPosition = new(5f, -3.2f, -0.1f);
    private static readonly Vector3 NavigationButtonLocalScale = Vector3.one * 0.8f;

    // 1ページ分の表示内容。文字列は TranslationData.csv のキーを参照する。
    public sealed class Step
    {
        public string TitleKey { get; init; }
        public string BodyKey { get; init; }
        public float BodyFontScale { get; init; } = 1f;
        public float BodyYOffset { get; init; }
        public bool CenterBody { get; init; }
        public float LinkButtonY { get; init; } = -1.95f;
        public LinkSpec[] Links { get; init; } = Array.Empty<LinkSpec>();
        /// <summary>単一本文の代わりに複数カラムで表示する場合。指定時は通常の <see cref="BodyKey"/> は使わない（null 推奨）。</summary>
        public OnboardingColumnLayout ColumnLayout { get; init; } = OnboardingColumnLayout.None;
        /// <summary>カラムごとの翻訳キー。順序は <see cref="ColumnLayout"/> の説明に従う（最大 4 キー）。</summary>
        public string[] BodyColumnKeys { get; init; }
    }

    // ページ下部に表示する外部リンクボタンの定義。
    public sealed class LinkSpec
    {
        public string LabelKey { get; init; }
        public string Url { get; init; }
    }

    // オンボーディングの全ページ定義。Render() は _currentStep を使ってこの配列から表示内容を引く。
    private static readonly Step[] Steps = new[]
    {
        new Step { TitleKey = "OnboardingWelcomeTitle", BodyKey = "OnboardingWelcomeBody", BodyFontScale = 0.88f, BodyYOffset = -0.35f },
        new Step { TitleKey = "OnboardingAboutTitle",   BodyKey = "OnboardingAboutBody", BodyFontScale = 0.76f, BodyYOffset = -0.05f },
        new Step { TitleKey = "OnboardingPlayTitle", BodyKey = null, BodyFontScale = 0.7f, BodyYOffset = 0f },
        new Step { TitleKey = "OnboardingBugReportTitle", BodyKey = null, BodyFontScale = 0.7f, BodyYOffset = 0f },
        new Step
        {
            TitleKey = "OnboardingAnalyticsTitle",
            BodyKey  = "OnboardingAnalyticsBody",
            BodyFontScale = 0.78f,
            BodyYOffset = 0.25f,
            LinkButtonY = -2.55f,
            Links = new[]
            {
                new LinkSpec { LabelKey = "OnboardingLinkAnalyticsDetail", Url = "https://supernewroles.com/analytics" },
            }
        },
        new Step
        {
            TitleKey = "OnboardingLinksTitle",
            BodyKey  = "OnboardingLinksBody",
            BodyFontScale = 0.75f,
            BodyYOffset = 0.35f,
            LinkButtonY = -1.8f,
            Links = new[]
            {
                new LinkSpec { LabelKey = "OnboardingLinkDiscord", Url = SocialLinks.DiscordServer },
                new LinkSpec { LabelKey = "OnboardingLinkTwitter", Url = SocialLinks.TwitterSnrOfficials },
                new LinkSpec { LabelKey = "OnboardingLinkWiki",    Url = "https://wiki.supernewroles.com" },
            }
        },
        new Step
        {
            TitleKey = "OnboardingFaqTitle",
            BodyKey = null,
            BodyFontScale = 0.58f,
            BodyYOffset = 0f,
            ColumnLayout = OnboardingColumnLayout.Columns2,
            BodyColumnKeys = new[] { "OnboardingFaqBodyCol1", "OnboardingFaqBodyCol2" },
            LinkButtonY = -2.55f,
            Links = new[]
            {
                new LinkSpec { LabelKey = "OnboardingLinkFaq", Url = "https://supernewroles.com/q-a" },
            }
        },
        new Step { TitleKey = "OnboardingEnjoyTitle", BodyKey = "OnboardingEnjoyBody", BodyFontScale = 1.35f, CenterBody = true },
    };

    // ポップアップ全体の状態。MainMenuManager.LateUpdate から毎フレーム TryHandle() が呼ばれる。
    private static GameObject _popup;
    private static int _currentStep;
    private static bool _isShown;
    private static float _nextButtonUnlockTime;
    private static Vector3 _popupBaseLocalPosition;
    private static Vector3 _popupBaseLocalScale;

    // prefab 内の固定テキストと、動的に生成するボタン類への参照。
    private static TextMeshPro _titleTmp;
    private static TextMeshPro _bodyTmp;
    private static TextMeshPro _stepIndicatorTmp;
    private static Vector3 _titleBaseLocalPosition;
    private static Vector3 _bodyBaseLocalPosition;
    private static Vector3 _stepIndicatorBaseLocalPosition;
    private static float _bodyBaseFontSize;
    private static TextAlignmentOptions _bodyBaseAlignment;

    // 「人の集め方」2カラム・バグ報告スクショ用（prefab の Text を複製した TMP）。
    private static TextMeshPro _playColumnLeftTmp;
    private static TextMeshPro _playColumnRightTmp;
    private static TextMeshPro _playFootTmp;
    private static TextMeshPro _bugReportFooterTmp;
    private static TextMeshPro[] _bugReportCaptionTmps = Array.Empty<TextMeshPro>();
    private static GameObject[] _bugReportImageObjects = Array.Empty<GameObject>();
    private static SpriteRenderer[] _bugReportImageRenderers = Array.Empty<SpriteRenderer>();
    private static TextMeshPro[] _flexColumnTmps = Array.Empty<TextMeshPro>();

    // AnalyticsButton は実ボタンとして使わず、Next/Back/Link ボタンのテンプレートとしてだけ使う。
    private static GameObject _buttonTemplate;
    private static GameObject _nextButton;
    private static GameObject _backButton;
    private static GameObject _closeButton;
    private static TextMeshPro _nextLabel;
    private static readonly List<GameObject> _linkButtons = new();
    private static readonly List<Transform> _animatedContentTransforms = new();
    private static readonly List<Vector3> _animatedContentBasePositions = new();
    private static readonly List<TextMeshPro> _popupAlphaTexts = new();
    private static readonly List<float> _popupAlphaTextBaseAlphas = new();
    private static readonly List<SpriteRenderer> _popupAlphaRenderers = new();
    private static readonly List<float> _popupAlphaRendererBaseAlphas = new();
    private static readonly List<Sprite> _roleIconSprites = new();
    private static readonly List<SpriteRenderer> _roleIconRenderers = new();
    private static readonly List<GameObject> _roleIconObjects = new();
    private static GameObject _welcomeLogoObject;
    private static readonly Vector3 WelcomeLogoBaseLocalPosition = new(0f, 1.15f, 0f);

    // テンプレートボタンの元位置と元スケール。リンクボタン配置の基準に使う。
    private static Vector3 _buttonAnchor;
    private static Vector3 _buttonAnchorScale;

    private static bool _isPageTransitioning;
    private static int _transitionDirection;
    private static float _pageTransitionStartTime;
    private static bool _isClosing;
    private static float _closeStartTime;
    private static float _roleIconCarouselStartTime;
    private static int _roleIconCarouselPage = -1;
    private static bool _hasAcceptedAnalytics;

    // 他のポップアップと競合しない時だけオンボーディングを表示する。
    public static bool TryHandle(MainMenuManager instance, ref GameObject currentPopup)
    {
        if (_isClosing)
        {
            UpdateCloseTransition();
            return true;
        }

        if (_isShown) return false;

        // 設定に完了済みと残っている場合は、この起動中も再表示しない。
        if (ConfigRoles.IsOnboardingViewd.Value)
        {
            _isShown = true;
            return false;
        }

        // Android notice や analytics popup など、別の currentPopup がある間は何もしない。
        if (currentPopup != null && currentPopup != _popup) return false;

        if (_popup == null)
        {
            Build();
            currentPopup = _popup;
        }
        UpdatePageTransition();
        UpdateRoleIconCarousel();
        HandleBodyTextLinks();
        return true;
    }

    // prefab を生成し、固定 UI とナビゲーションボタンを初期化する。
    private static void Build()
    {
        _popup = AssetManager.Instantiate(PrefabName, Camera.main.transform);
        _popup.SetActive(true);
        _popup.transform.localScale = Vector3.one * PopupScale;
        _popupBaseLocalPosition = _popup.transform.localPosition;
        _popupBaseLocalScale = _popup.transform.localScale;

        // prefab 由来の Title/Text は背景より手前に出してから内容を差し替える。
        _titleTmp = _popup.transform.Find("Title").GetComponent<TextMeshPro>();
        _bodyTmp = _popup.transform.Find("Text").GetComponent<TextMeshPro>();
        MoveInFrontOfPopup(_titleTmp.transform);
        MoveInFrontOfPopup(_bodyTmp.transform);
        _titleBaseLocalPosition = _titleTmp.transform.localPosition;
        _bodyBaseLocalPosition = _bodyTmp.transform.localPosition;
        _bodyBaseFontSize = _bodyTmp.fontSize;
        _bodyBaseAlignment = _bodyTmp.alignment;

        // 元の OK ボタンは clone 元にする。直接使うと PassiveButton の複製/破棄が不安定になりやすい。
        _buttonTemplate = _popup.transform.Find("AnalyticsButton").gameObject;
        _buttonAnchor = WithPopupForegroundZ(_buttonTemplate.transform.localPosition);
        _buttonAnchorScale = _buttonTemplate.transform.localScale;
        _buttonTemplate.SetActive(false);

        // 背景にも PassiveButton を付けて、裏側の UI にクリックが抜けにくいようにする。
        AddColliders(_popup);

        _nextButton = CloneButton("OnboardingNextButton",
            NextButtonLocalPosition,
            NavigationButtonLocalScale,
            "OnboardingNext",
            GoNext);
        _nextLabel = _nextButton.transform.Find("Text").GetComponent<TextMeshPro>();

        _backButton = CloneButton("OnboardingBackButton",
            BackButtonLocalPosition,
            NavigationButtonLocalScale,
            "OnboardingBack",
            GoBack);

        AttachCloseButton();

        // 現在のページ番号。本文より少し手前に置き、描画順のちらつきを避ける。
        var indicatorObj = new GameObject("OnboardingStepIndicator");
        indicatorObj.transform.SetParent(_popup.transform, false);
        indicatorObj.transform.localPosition = new Vector3(6.9f, 3.6f, -0.11f);
        indicatorObj.transform.localScale = Vector3.one;
        indicatorObj.AddComponent<MeshRenderer>();
        _stepIndicatorTmp = indicatorObj.AddComponent<TextMeshPro>();
        _stepIndicatorTmp.fontSize = 5.2f;
        _stepIndicatorTmp.alignment = TextAlignmentOptions.Center;
        _stepIndicatorTmp.color = new Color(0.78f, 0.78f, 0.78f, 1f);
        _stepIndicatorTmp.font = _titleTmp.font;
        _stepIndicatorTmp.fontSharedMaterial = _titleTmp.fontSharedMaterial;
        _stepIndicatorTmp.enableWordWrapping = false;
        _stepIndicatorTmp.rectTransform.sizeDelta = new Vector2(3f, 1f);
        _stepIndicatorBaseLocalPosition = indicatorObj.transform.localPosition;

        CreateOnboardingExtraTexts();

        _currentStep = 0;
        _nextButtonUnlockTime = 0f;
        _isPageTransitioning = false;
        _isClosing = false;
        _hasAcceptedAnalytics = ConfigRoles.IsSendAnalyticsPopupViewd.Value;
        Render();
        SetContentPose(0f, 1f);
    }

    // 右上のバツは「案内を閉じる」だけにする。
    // 解析に未同意なら、閉じた直後に Analytics.cs 側の従来ポップアップが単体で表示される。
    private static void AttachCloseButton()
    {
        var closeButton = _popup.transform.Find("CloseButton");
        if (closeButton == null) return;

        _closeButton = closeButton.gameObject;
        MoveInFrontOfPopup(closeButton);
        AttachButton(_closeButton, CloseOnboarding);
    }

    // _popup より手前に出す z を返す。additionalOffset を足すほどさらに手前へ移動する。
    private static float GetPopupForegroundZ(float additionalOffset = 0f)
    {
        return _popup.transform.localPosition.z + PopupForegroundZOffset - additionalOffset;
    }

    // 既存の x/y は維持し、z だけオンボーディング用の前面 z に差し替える。
    private static Vector3 WithPopupForegroundZ(Vector3 position, float additionalOffset = 0f)
    {
        return new Vector3(position.x, position.y, GetPopupForegroundZ(additionalOffset));
    }

    // prefab 内の既存要素を背景より前面に移動する。
    private static void MoveInFrontOfPopup(Transform target, float additionalOffset = 0f)
    {
        target.localPosition = WithPopupForegroundZ(target.localPosition, additionalOffset);
    }

    // 背景用の空イベント PassiveButton。クリックを処理するボタンではなく、入力を受け止めるために付ける。
    private static void AddColliders(GameObject bg)
    {
        PassiveButton passiveButton = bg.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { bg.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();
    }

    // テンプレートから独立したボタンを作る。Next/Back/Link を同じ経路にして PassiveButton の差をなくす。
    private static GameObject CloneButton(string name, Vector3 localPos, Vector3 localScale, string labelKey, Action onClick)
    {
        GameObject clone = GameObject.Instantiate(_buttonTemplate, _buttonTemplate.transform.parent);
        clone.name = name;
        clone.SetActive(true);
        clone.transform.localPosition = localPos;
        clone.transform.localScale = localScale;

        // clone 元に PassiveButton が付いている場合は破棄し、下で新しい PassiveButton を必ず付け直す。
        var oldPb = clone.GetComponent<PassiveButton>();
        if (oldPb != null) GameObject.Destroy(oldPb);

        var label = clone.transform.Find("Text").GetComponent<TextMeshPro>();
        label.text = ModTranslation.GetString(labelKey);

        AttachButton(clone, onClick, true);
        return clone;
    }

    // PassiveButton のクリック/ホバー処理を設定する。
    private static void AttachButton(GameObject button, Action onClick, bool forceNewPassiveButton = false)
    {
        var selected = button.transform.Find("Selected")?.gameObject;
        if (selected != null)
            selected.SetActive(false);
        var collider = button.GetComponent<Collider2D>() ?? button.GetComponentInChildren<Collider2D>();

        // clone 直後は Destroy 予約済みの PassiveButton を再利用しないよう、明示的に新規追加できる。
        var pb = forceNewPassiveButton ? button.AddComponent<PassiveButton>() : button.GetComponent<PassiveButton>() ?? button.AddComponent<PassiveButton>();
        if (collider != null)
            pb.Colliders = new Collider2D[] { collider };
        pb.OnClick = new();
        pb.OnClick.AddListener((UnityAction)(() => onClick()));
        pb.OnMouseOver = new();
        pb.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (selected != null)
                selected.SetActive(true);
        }));
        pb.OnMouseOut = new();
        pb.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (selected != null)
                selected.SetActive(false);
        }));
    }

    // 現在ページのタイトル・本文・ページ番号・リンクボタンを反映する。
    private static void Render()
    {
        var step = Steps[_currentStep];
        _titleTmp.text = ModTranslation.GetString(step.TitleKey);
        _stepIndicatorTmp.text = $"{_currentStep + 1} / {Steps.Length}";

        bool useStandardBody = !string.IsNullOrEmpty(step.BodyKey);
        Vector3 bodyBasePosition;
        if (useStandardBody)
        {
            _bodyTmp.gameObject.SetActive(true);
            _bodyTmp.text = ModTranslation.GetString(step.BodyKey);
            _bodyTmp.fontSize = _bodyBaseFontSize * step.BodyFontScale;
            _bodyTmp.alignment = step.CenterBody ? TextAlignmentOptions.Center : _bodyBaseAlignment;
            bodyBasePosition = _bodyBaseLocalPosition + new Vector3(0f, step.BodyYOffset, 0f);
            _bodyTmp.transform.localPosition = bodyBasePosition;
        }
        else
        {
            _bodyTmp.text = string.Empty;
            _bodyTmp.gameObject.SetActive(false);
            bodyBasePosition = _bodyBaseLocalPosition;
        }

        HideOnboardingExtraLayouts();

        _animatedContentTransforms.Clear();
        _animatedContentBasePositions.Clear();
        RegisterAnimatedContent(_titleTmp.transform, _titleBaseLocalPosition);
        if (useStandardBody)
            RegisterAnimatedContent(_bodyTmp.transform, bodyBasePosition);
        RegisterAnimatedContent(_stepIndicatorTmp.transform, _stepIndicatorBaseLocalPosition);

        if (_currentStep == PlayStepIndex)
            ApplyPlayTwoColumnLayout();
        else if (_currentStep == BugReportStepIndex)
            ApplyBugReportScreenshotLayout();
        else if (step.ColumnLayout != OnboardingColumnLayout.None)
            ApplyFlexibleColumnLayout(step);

        bool isFirst = _currentStep == WelcomeStepIndex;
        bool isLast = _currentStep == Steps.Length - 1;
        _backButton.SetActive(!isFirst);
        _nextLabel.text = ModTranslation.GetString(isLast ? "OnboardingStart" : _currentStep == AnalyticsStepIndex && !_hasAcceptedAnalytics ? "OnboardingAgreeNext" : "OnboardingNext");

        // ページごとにリンク数が違うため、リンクボタンは Render() のたびに作り直す。
        foreach (var lb in _linkButtons)
            if (lb != null) GameObject.Destroy(lb);
        _linkButtons.Clear();

        if (step.Links != null && step.Links.Length > 0)
        {
            int n = step.Links.Length;
            // Back/Next ボタン (x = ±5) と被らないように、リンクの x 範囲を ±3.6 に収める。
            float maxSpan = 7.2f;
            float spacing = n > 1 ? Mathf.Min(3.4f, maxSpan / (n - 1)) : 0f;
            float startX = -(n - 1) * spacing / 2f;
            for (int i = 0; i < n; i++)
            {
                var link = step.Links[i];
                string url = link.Url;
                // url をローカル変数に逃がして、ラムダがループ変数を誤って参照しないようにする。
                var linkButton = CloneButton(
                    $"OnboardingLink_{i}",
                    new Vector3(startX + i * spacing, step.LinkButtonY, _buttonAnchor.z),
                    _buttonAnchorScale * 0.62f,
                    link.LabelKey,
                    () => Application.OpenURL(url));
                _linkButtons.Add(linkButton);
                RegisterAnimatedContent(linkButton.transform, linkButton.transform.localPosition);
            }
        }

        if (_currentStep == WelcomeStepIndex)
            EnsureWelcomeLogo();
        else
            DestroyWelcomeLogo();

        if (_currentStep == RoleCarouselStepIndex)
            EnsureRoleIconCarousel();
        else
            DestroyRoleIconCarousel();
    }

    // prefab の本文 Text を複製して、2カラムやバグ報告キャプションなどに使う。
    private static TextMeshPro CreateBodyTextSibling(string name)
    {
        var clone = GameObject.Instantiate(_bodyTmp.gameObject, _bodyTmp.transform.parent);
        clone.name = name;
        var tmp = clone.GetComponent<TextMeshPro>();
        MoveInFrontOfPopup(clone.transform);
        tmp.text = string.Empty;
        clone.SetActive(false);
        return tmp;
    }

    private static void CreateOnboardingExtraTexts()
    {
        _playColumnLeftTmp = CreateBodyTextSibling("OnboardingPlayColumnLeft");
        _playColumnRightTmp = CreateBodyTextSibling("OnboardingPlayColumnRight");
        _playFootTmp = CreateBodyTextSibling("OnboardingPlayFoot");
        _bugReportFooterTmp = CreateBodyTextSibling("OnboardingBugReportFooter");
        _bugReportCaptionTmps = new TextMeshPro[3];
        for (int i = 0; i < 3; i++)
            _bugReportCaptionTmps[i] = CreateBodyTextSibling($"OnboardingBugCaption{i + 1}");

        _bugReportImageObjects = new GameObject[3];
        _bugReportImageRenderers = new SpriteRenderer[3];
        float imgZ = GetPopupForegroundZ(0.04f);
        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject($"OnboardingBugReportImage{i + 1}");
            go.transform.SetParent(_popup.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, imgZ);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            _bugReportImageObjects[i] = go;
            _bugReportImageRenderers[i] = sr;
            go.SetActive(false);
        }

        _flexColumnTmps = new TextMeshPro[4];
        for (int i = 0; i < 4; i++)
            _flexColumnTmps[i] = CreateBodyTextSibling($"OnboardingFlexColumn{i}");
    }

    private static void HideOnboardingExtraLayouts()
    {
        if (_playColumnLeftTmp != null) _playColumnLeftTmp.gameObject.SetActive(false);
        if (_playColumnRightTmp != null) _playColumnRightTmp.gameObject.SetActive(false);
        if (_playFootTmp != null) _playFootTmp.gameObject.SetActive(false);
        if (_bugReportFooterTmp != null) _bugReportFooterTmp.gameObject.SetActive(false);
        if (_bugReportCaptionTmps != null)
        {
            foreach (var c in _bugReportCaptionTmps)
                if (c != null) c.gameObject.SetActive(false);
        }
        if (_bugReportImageObjects != null)
        {
            foreach (var go in _bugReportImageObjects)
                if (go != null) go.SetActive(false);
        }
        if (_flexColumnTmps != null)
        {
            foreach (var t in _flexColumnTmps)
                if (t != null) t.gameObject.SetActive(false);
        }
    }

    // 3ページ目「人の集め方」: 左右2カラムだけで構成。本文 (_bodyTmp) と Foot は使わない。
    private static void ApplyPlayTwoColumnLayout()
    {
        if (_playColumnLeftTmp == null || _playColumnRightTmp == null || _playFootTmp == null) return;

        _playColumnLeftTmp.text = ModTranslation.GetString("OnboardingPlayBodyLeft");
        _playColumnRightTmp.text = ModTranslation.GetString("OnboardingPlayBodyRight");
        _playFootTmp.text = ModTranslation.GetString("OnboardingPlayBodyFoot");

        float colScale = 0.62f;
        _playColumnLeftTmp.fontSize = _bodyBaseFontSize * colScale;
        _playColumnRightTmp.fontSize = _bodyBaseFontSize * colScale;
        _playColumnLeftTmp.alignment = TextAlignmentOptions.Top;
        _playColumnRightTmp.alignment = TextAlignmentOptions.Top;
        _playColumnLeftTmp.enableWordWrapping = true;
        _playColumnRightTmp.enableWordWrapping = true;
        _playColumnLeftTmp.rectTransform.sizeDelta = new Vector2(PlayColumnWidth, PlayColumnHeight);
        _playColumnRightTmp.rectTransform.sizeDelta = new Vector2(PlayColumnWidth, PlayColumnHeight);

        _playFootTmp.fontSize = _bodyBaseFontSize * 0.55f;
        _playFootTmp.alignment = TextAlignmentOptions.Center;
        _playFootTmp.enableWordWrapping = false;
        _playFootTmp.rectTransform.sizeDelta = new Vector2(13.5f, 1.6f);

        float z = _bodyBaseLocalPosition.z;
        var leftPos = new Vector3(-PlayColumnOffsetX, PlayColumnTopY, z);
        var rightPos = new Vector3(PlayColumnOffsetX, PlayColumnTopY, z);
        var footPos = new Vector3(0f, PlayFootY, z);

        _playColumnLeftTmp.transform.localPosition = leftPos;
        _playColumnRightTmp.transform.localPosition = rightPos;
        _playFootTmp.transform.localPosition = footPos;
        _playColumnLeftTmp.gameObject.SetActive(true);
        _playColumnRightTmp.gameObject.SetActive(true);
        _playFootTmp.gameObject.SetActive(true);

        RegisterAnimatedContent(_playColumnLeftTmp.transform, leftPos);
        RegisterAnimatedContent(_playColumnRightTmp.transform, rightPos);
        RegisterAnimatedContent(_playFootTmp.transform, footPos);
    }

    /// <summary>
    /// <see cref="Step.ColumnLayout"/> に応じて汎用カラム（最大 4）を配置する。キー順は enum の XML コメント参照。
    /// </summary>
    private static void ApplyFlexibleColumnLayout(Step step)
    {
        if (_flexColumnTmps == null || _flexColumnTmps.Length < 4) return;
        var keys = step.BodyColumnKeys;
        if (keys == null || keys.Length == 0) return;

        float z = _bodyBaseLocalPosition.z;
        float fs = _bodyBaseFontSize * step.BodyFontScale;

        for (int i = 0; i < 4; i++)
        {
            var tmp = _flexColumnTmps[i];
            tmp.fontSize = fs;
            tmp.alignment = TextAlignmentOptions.Top;
            tmp.enableWordWrapping = true;
            tmp.rectTransform.sizeDelta = new Vector2(FlexColumnWidth, FlexColumnHeight);

            bool active = false;
            Vector3 pos = default;

            switch (step.ColumnLayout)
            {
                case OnboardingColumnLayout.Columns2:
                    if (i < 2 && i < keys.Length)
                    {
                        active = true;
                        tmp.text = ModTranslation.GetString(keys[i]);
                        float x = i == 0 ? -FlexColumnOffsetX2 : FlexColumnOffsetX2;
                        pos = new Vector3(x, FlexColumnTopY, z);
                    }
                    break;
                case OnboardingColumnLayout.Columns3:
                    if (i < 3 && i < keys.Length)
                    {
                        active = true;
                        tmp.text = ModTranslation.GetString(keys[i]);
                        float x = (i - 1) * FlexColumnSpacingX3;
                        pos = new Vector3(x, FlexColumnTopY, z);
                    }
                    break;
                case OnboardingColumnLayout.Grid2x2:
                    if (i < 4 && i < keys.Length)
                    {
                        active = true;
                        tmp.text = ModTranslation.GetString(keys[i]);
                        int col = i % 2;
                        int row = i / 2;
                        float x = col == 0 ? -FlexColumnOffsetX2 : FlexColumnOffsetX2;
                        float y = FlexColumnTopY - row * FlexGridRowDeltaY;
                        pos = new Vector3(x, y, z);
                    }
                    break;
            }

            if (!active)
            {
                tmp.text = string.Empty;
                tmp.gameObject.SetActive(false);
                continue;
            }

            tmp.transform.localPosition = pos;
            tmp.gameObject.SetActive(true);
            RegisterAnimatedContent(tmp.transform, pos);
        }
    }

    // 4ページ目「質問・バグ報告」: スクショ枠 (3つ横並び) + その下にステップキャプション + 最下部に注記。本文 (_bodyTmp) は使わない。
    private static void ApplyBugReportScreenshotLayout()
    {
        if (_bugReportFooterTmp == null || _bugReportCaptionTmps == null || _bugReportImageObjects == null) return;

        _bugReportFooterTmp.text = ModTranslation.GetString("OnboardingBugReportBodyFooter");
        _bugReportCaptionTmps[0].text = ModTranslation.GetString("OnboardingBugReportStep1Caption");
        _bugReportCaptionTmps[1].text = ModTranslation.GetString("OnboardingBugReportStep2Caption");
        _bugReportCaptionTmps[2].text = ModTranslation.GetString("OnboardingBugReportStep3Caption");

        float zText = _bodyBaseLocalPosition.z;
        float imgZ = GetPopupForegroundZ(0.04f);

        for (int i = 0; i < 3; i++)
        {
            float xPos = (i - 1) * BugReportImageSpacingX;
            var sprite = AssetManager.GetAsset<Sprite>($"OnboardingBugReportStep{i + 1}", AssetManager.AssetBundleType.Sprite);
            var imgGo = _bugReportImageObjects[i];
            var sr = _bugReportImageRenderers[i];
            imgGo.SetActive(true);
            imgGo.transform.localPosition = new Vector3(xPos, BugReportImageLocalY, imgZ);
            imgGo.transform.localScale = Vector3.one * BugReportImageDisplayScale;
            sr.sprite = sprite;
            sr.enabled = sprite != null;
            sr.color = Color.white;
            RegisterAnimatedContent(imgGo.transform, imgGo.transform.localPosition);

            var cap = _bugReportCaptionTmps[i];
            var capPos = new Vector3(xPos, BugReportCaptionLocalY, zText);
            cap.transform.localPosition = capPos;
            cap.fontSize = _bodyBaseFontSize * 0.5f;
            cap.alignment = TextAlignmentOptions.Top;
            cap.enableWordWrapping = true;
            cap.rectTransform.sizeDelta = new Vector2(BugReportImageWidth + 0.4f, 1.7f);
            cap.gameObject.SetActive(true);
            RegisterAnimatedContent(cap.transform, capPos);
        }

        var footerPos = new Vector3(0f, BugReportFooterLocalY, zText);
        _bugReportFooterTmp.transform.localPosition = footerPos;
        _bugReportFooterTmp.fontSize = _bodyBaseFontSize * 0.5f;
        _bugReportFooterTmp.alignment = TextAlignmentOptions.Center;
        _bugReportFooterTmp.enableWordWrapping = true;
        _bugReportFooterTmp.rectTransform.sizeDelta = new Vector2(13.5f, 1.8f);
        _bugReportFooterTmp.gameObject.SetActive(true);
        RegisterAnimatedContent(_bugReportFooterTmp.transform, footerPos);
    }

    private static void RegisterAnimatedContent(Transform target, Vector3 baseLocalPosition)
    {
        if (target == null) return;
        _animatedContentTransforms.Add(target);
        _animatedContentBasePositions.Add(baseLocalPosition);
    }

    // 本文内の <link="..."> タグを毎フレーム監視する。
    // TextMeshPro の本文はボタンではないため、PassiveButton ではなく TMP のリンク判定を直接使う。
    private static void HandleBodyTextLinks()
    {
        if (_bodyTmp == null || _isPageTransitioning || _isClosing) return;
        if (!Input.GetMouseButtonUp(0)) return;

        if (TryOpenDiscordLinkFromTmp(_bodyTmp)) return;
        if (_currentStep == PlayStepIndex && _playColumnRightTmp != null && _playColumnRightTmp.gameObject.activeSelf)
        {
            if (TryOpenDiscordLinkFromTmp(_playColumnRightTmp)) return;
        }
        if (_flexColumnTmps != null)
        {
            foreach (var t in _flexColumnTmps)
            {
                if (t != null && t.gameObject.activeSelf && TryOpenDiscordLinkFromTmp(t))
                    return;
            }
        }
    }

    private static bool TryOpenDiscordLinkFromTmp(TextMeshPro tmp)
    {
        if (tmp == null) return false;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmp, Input.mousePosition, Camera.main);
        if (linkIndex < 0) return false;
        if (tmp.textInfo?.linkInfo == null || linkIndex >= tmp.textInfo.linkInfo.Length) return false;

        var linkInfo = tmp.textInfo.linkInfo[linkIndex];
        if (linkInfo.GetLinkID() != DiscordLinkId) return false;
        Application.OpenURL(SocialLinks.DiscordServer);
        return true;
    }

    private static void StartPageTransition(int targetStep, int direction)
    {
        if (_isPageTransitioning) return;

        _currentStep = targetStep;
        _transitionDirection = direction;
        _pageTransitionStartTime = Time.realtimeSinceStartup;
        _isPageTransitioning = true;
        _nextButtonUnlockTime = Time.realtimeSinceStartup + NextButtonCooldownSeconds;
        SetNavigationInputEnabled(false);
        Render();
        SetContentPose(_transitionDirection * PageTransitionSlideDistance, 0f);
    }

    private static void UpdatePageTransition()
    {
        if (!_isPageTransitioning || _popup == null) return;

        float elapsed = Time.realtimeSinceStartup - _pageTransitionStartTime;
        float progress = EaseOutCubic(elapsed / PageTransitionSeconds);
        SetContentPose(_transitionDirection * PageTransitionSlideDistance * (1f - progress), progress);

        if (elapsed < PageTransitionSeconds) return;

        _isPageTransitioning = false;
        SetContentPose(0f, 1f);
        SetNavigationInputEnabled(true);
    }

    private static void StartCloseTransition()
    {
        if (_isClosing) return;

        _isClosing = true;
        _isPageTransitioning = false;
        _closeStartTime = Time.realtimeSinceStartup;
        CapturePopupAlphaTargets();
        SetNavigationInputEnabled(false);
        SetButtonInputEnabled(_closeButton, false);
    }

    private static void UpdateCloseTransition()
    {
        if (_popup == null)
        {
            FinishCloseTransition();
            return;
        }

        float progress = EaseOutCubic((Time.realtimeSinceStartup - _closeStartTime) / PopupCloseSeconds);
        float alpha = 1f - progress;
        float scale = Mathf.Lerp(1f, PopupCloseScaleMultiplier, progress);

        _popup.transform.localScale = _popupBaseLocalScale * scale;
        _popup.transform.localPosition = _popupBaseLocalPosition + new Vector3(0f, PopupCloseYOffset * progress, 0f);
        SetPopupAlpha(alpha);

        if (progress >= 1f)
            FinishCloseTransition();
    }

    private static void FinishCloseTransition()
    {
        foreach (var lb in _linkButtons)
            if (lb != null) GameObject.Destroy(lb);
        _linkButtons.Clear();

        if (_popup != null)
            GameObject.Destroy(_popup);

        _popup = null;
        _buttonTemplate = null;
        _nextButton = null;
        _backButton = null;
        _closeButton = null;
        _nextLabel = null;
        _animatedContentTransforms.Clear();
        _animatedContentBasePositions.Clear();
        ClearPopupAlphaTargets();
        DestroyWelcomeLogo();
        DestroyRoleIconCarousel();
        _isPageTransitioning = false;
        _isClosing = false;
        _nextButtonUnlockTime = 0f;
    }

    private static void CapturePopupAlphaTargets()
    {
        ClearPopupAlphaTargets();
        if (_popup == null) return;

        var texts = _popup.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var text in texts)
        {
            if (text == null) continue;
            _popupAlphaTexts.Add(text);
            _popupAlphaTextBaseAlphas.Add(text.color.a);
        }

        var renderers = _popup.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            _popupAlphaRenderers.Add(renderer);
            _popupAlphaRendererBaseAlphas.Add(renderer.color.a);
        }
    }

    private static void ClearPopupAlphaTargets()
    {
        _popupAlphaTexts.Clear();
        _popupAlphaTextBaseAlphas.Clear();
        _popupAlphaRenderers.Clear();
        _popupAlphaRendererBaseAlphas.Clear();
    }

    private static void SetPopupAlpha(float alphaMultiplier)
    {
        alphaMultiplier = Mathf.Clamp01(alphaMultiplier);
        for (int i = 0; i < _popupAlphaTexts.Count; i++)
        {
            var text = _popupAlphaTexts[i];
            if (text == null) continue;
            var color = text.color;
            text.color = new Color(color.r, color.g, color.b, _popupAlphaTextBaseAlphas[i] * alphaMultiplier);
        }

        for (int i = 0; i < _popupAlphaRenderers.Count; i++)
        {
            var renderer = _popupAlphaRenderers[i];
            if (renderer == null) continue;
            var color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, _popupAlphaRendererBaseAlphas[i] * alphaMultiplier);
        }
    }

    private static void SetContentPose(float xOffset, float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        for (int i = 0; i < _animatedContentTransforms.Count; i++)
        {
            var target = _animatedContentTransforms[i];
            if (target == null) continue;

            target.localPosition = _animatedContentBasePositions[i] + new Vector3(xOffset, 0f, 0f);
            SetObjectAlpha(target.gameObject, alpha);
        }
    }

    private static void SetObjectAlpha(GameObject target, float alpha)
    {
        var texts = target.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var text in texts)
        {
            var color = text.color;
            text.color = new Color(color.r, color.g, color.b, alpha);
        }

        var renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in renderers)
        {
            var color = renderer.color;
            renderer.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    // 最初のページだけ SNR ロゴを出す。メインメニューと同じ banner アセットを使う。
    private static void EnsureWelcomeLogo()
    {
        if (_welcomeLogoObject != null)
        {
            RegisterAnimatedContent(_welcomeLogoObject.transform, _welcomeLogoObject.transform.localPosition);
            return;
        }

        var logo = AssetManager.GetAsset<Sprite>("banner", AssetManager.AssetBundleType.Sprite);
        if (logo == null) return;

        _welcomeLogoObject = new GameObject("OnboardingSNRLogo");
        _welcomeLogoObject.transform.SetParent(_popup.transform, false);
        _welcomeLogoObject.transform.localPosition = WithPopupForegroundZ(WelcomeLogoBaseLocalPosition, 0.05f);
        _welcomeLogoObject.transform.localScale = Vector3.one * WelcomeLogoScale;

        var renderer = _welcomeLogoObject.AddComponent<SpriteRenderer>();
        renderer.sprite = logo;
        renderer.color = Color.white;

        RegisterAnimatedContent(_welcomeLogoObject.transform, _welcomeLogoObject.transform.localPosition);
    }

    private static void DestroyWelcomeLogo()
    {
        if (_welcomeLogoObject != null)
            GameObject.Destroy(_welcomeLogoObject);
        _welcomeLogoObject = null;
    }

    // 2ページ目の飾り。RoleIcon は元画像サイズが大きいため、かなり小さいスケールで扱う。
    private static void EnsureRoleIconCarousel()
    {
        if (_roleIconObjects.Count > 0) return;

        LoadRoleIconSprites();
        if (_roleIconSprites.Count == 0) return;

        Vector3[] positions =
        {
            new(-6.15f, 1.55f, GetPopupForegroundZ(0.04f)),
            new(6.15f, 1.55f, GetPopupForegroundZ(0.04f)),
            new(-6.15f, -0.65f, GetPopupForegroundZ(0.04f)),
            new(6.15f, -0.65f, GetPopupForegroundZ(0.04f)),
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var iconObj = new GameObject($"OnboardingRoleIcon_{i}");
            iconObj.transform.SetParent(_popup.transform, false);
            iconObj.transform.localPosition = positions[i];
            iconObj.transform.localScale = Vector3.one * RoleIconBaseScale;

            var renderer = iconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = _roleIconSprites[i % _roleIconSprites.Count];
            renderer.color = new Color(1f, 1f, 1f, 0f);
            _roleIconObjects.Add(iconObj);
            _roleIconRenderers.Add(renderer);
        }

        _roleIconCarouselStartTime = Time.realtimeSinceStartup;
        _roleIconCarouselPage = -1;
    }

    private static void LoadRoleIconSprites()
    {
        if (_roleIconSprites.Count > 0) return;
        if (CustomRoleManager.AllRoles == null) return;

        // 役職辞典に出さない役職やバニラ役職は、オンボーディングの見せ場からも外す。
        foreach (var role in CustomRoleManager.AllRoles.Where(role => role != null && !role.IsVanillaRole && !role.HideInRoleDictionary))
        {
            var icon = role.RoleIcon;
            if (icon != null)
                _roleIconSprites.Add(icon);
        }
    }

    private static void UpdateRoleIconCarousel()
    {
        if (_currentStep != RoleCarouselStepIndex || _roleIconObjects.Count == 0 || _roleIconSprites.Count == 0) return;

        float elapsed = Time.realtimeSinceStartup - _roleIconCarouselStartTime;
        int page = Mathf.FloorToInt(elapsed / RoleIconCarouselSeconds);
        float phase = elapsed - page * RoleIconCarouselSeconds;

        if (page != _roleIconCarouselPage)
        {
            _roleIconCarouselPage = page;
            // 4つ同時に差し替え、各サイクルの前後だけフェードさせる。
            for (int i = 0; i < _roleIconRenderers.Count; i++)
                _roleIconRenderers[i].sprite = _roleIconSprites[(page * _roleIconRenderers.Count + i) % _roleIconSprites.Count];
        }

        float alpha = 1f;
        if (phase < RoleIconFadeSeconds)
            alpha = EaseOutCubic(phase / RoleIconFadeSeconds);
        else if (phase > RoleIconCarouselSeconds - RoleIconFadeSeconds)
            alpha = 1f - EaseOutCubic((phase - (RoleIconCarouselSeconds - RoleIconFadeSeconds)) / RoleIconFadeSeconds);

        for (int i = 0; i < _roleIconRenderers.Count; i++)
        {
            var renderer = _roleIconRenderers[i];
            if (renderer == null) continue;
            renderer.color = new Color(1f, 1f, 1f, alpha * 0.72f);
            renderer.transform.localScale = Vector3.one * (RoleIconBaseScale + RoleIconPulseScale * alpha);
        }
    }

    private static void DestroyRoleIconCarousel()
    {
        foreach (var iconObj in _roleIconObjects)
            if (iconObj != null) GameObject.Destroy(iconObj);
        _roleIconObjects.Clear();
        _roleIconRenderers.Clear();
        _roleIconCarouselPage = -1;
    }

    private static void SetNavigationInputEnabled(bool enabled)
    {
        SetButtonInputEnabled(_nextButton, enabled);
        SetButtonInputEnabled(_backButton, enabled);
        SetButtonInputEnabled(_closeButton, enabled);
        foreach (var linkButton in _linkButtons)
            SetButtonInputEnabled(linkButton, enabled);
    }

    private static void SetButtonInputEnabled(GameObject button, bool enabled)
    {
        if (button == null) return;
        var passiveButton = button.GetComponent<PassiveButton>();
        if (passiveButton != null)
            passiveButton.enabled = enabled;
    }

    private static float EaseOutCubic(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float inverse = 1f - progress;
        return 1f - inverse * inverse * inverse;
    }

    // 次ページへ進む。最後のページから進んだ時は完了扱いにする。
    private static void GoNext()
    {
        if (_isPageTransitioning || Time.realtimeSinceStartup < _nextButtonUnlockTime) return;

        if (_currentStep == AnalyticsStepIndex)
            AcceptAnalytics();

        int nextStep = _currentStep + 1;
        if (nextStep >= Steps.Length)
        {
            Complete();
            return;
        }

        StartPageTransition(nextStep, 1);
    }

    // 前ページへ戻る。最初のページでは何もしない。
    private static void GoBack()
    {
        if (_isPageTransitioning || Time.realtimeSinceStartup < _nextButtonUnlockTime) return;
        if (_currentStep <= 0) return;
        StartPageTransition(_currentStep - 1, -1);
    }

    // 解析同意はオンボーディングの既読とは別管理にする。
    // 途中で X を押された場合はここを通らず、Analytics.cs の単体ポップアップで同意を取る。
    private static void AcceptAnalytics()
    {
        if (_hasAcceptedAnalytics) return;

        ConfigRoles.IsSendAnalyticsPopupViewd.Value = true;
        ConfigRoles.IsSendAnalytics.Value = true;
        _hasAcceptedAnalytics = true;
    }

    // X や最終ページでオンボーディング自体を閉じる処理。解析同意の有無はここでは変えない。
    private static void CloseOnboarding()
    {
        ConfigRoles.IsOnboardingViewd.Value = true;
        _isShown = true;
        StartCloseTransition();
    }

    // 最終ページから進んだ場合は、念のため解析同意も確定してから完了にする。
    private static void Complete()
    {
        AcceptAnalytics();
        CloseOnboarding();
    }
}
