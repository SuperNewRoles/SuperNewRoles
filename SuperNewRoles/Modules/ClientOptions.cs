using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Modules;

public static class ClientOptions
{
    private const string DialogObjectName = "SNR_ClientOptionsDialog";
    private const string MenuObjectName = "SNR_ClientOptionsMenu";
    private const string MainMenuScreenMaskPath = "MainMenuManager/MainUI/AspectScaler/RightPanel/ScreenMask";
    private const string StandardOptionMenuAssetName = "StandardOptionMenu";
    private const string CategoryButtonAssetName = "RoleDetailButton";
    private const string CheckOptionAssetName = "StandardOption_Check";
    private const string SelectOptionAssetName = "StandardOption_Select";
    private const string CloseButtonAssetName = "closeButton";
    private const string TitleKey = "ClientOptions.Title";

    private const float CategoryButtonSpacing = 0.6125f;
    private const float CategoryButtonScale = 0.48f;
    private const float CategoryInitialY = 1.6f;
    private const float CategoryX = -3.614f;
    private const float OptionInitialY = 1.5f;
    private const float OptionX = 3.42f;
    private const float OptionSpacing = 0.7f;
    private const float OptionScale = 0.4f;
    private const int RightVisibleRows = 6;
    private const float DialogWidth = 10.3f;
    private const float DialogHeight = 5.7f;
    private const float DialogScale = 0.9f;
    private const float DialogAnimationDuration = 0.1f;
    private const float BackdropWidth = 22f;
    private const float BackdropHeight = 13f;
    private const float BackdropAlpha = 0.72f;

    private static readonly Vector3 DialogCloseButtonPosition = new(-5f, 2.8f, -1f);
    private static readonly Vector3 DialogCloseButtonScale = Vector3.one * 0.65f;
    private static readonly Color32 HoverColor = new(45, 235, 198, 255);

    private static GameObject _dialogObject;
    private static GameObject _menuObject;
    private static Transform _leftAreaInner;
    private static Transform _rightAreaInner;
    private static Scroller _leftScroller;
    private static Scroller _rightScroller;
    private static GameObject _currentSelectedCategory;
    private static GameObject _mainMenuScreenMask;
    private static ClientOptionCategory _currentCategory;
    private static OptionsMenuBehaviour _animationOwner;
    private static Coroutine _animationCoroutine;
    private static bool _mainMenuScreenMaskWasActive;
    private static bool _mainMenuScreenMaskDisabledByDialog;
    private static bool _isClosing;
    private static readonly Dictionary<Material, Material> UnmaskedTextMaterials = new();
    private static readonly HashSet<Transform> ClippedScrollerInners = new();

    private static readonly ClientOptionCategory[] Categories =
    [
        new(
            "ClientOptions.Category.General",
            [
                ClientOptionEntry.Toggle(
                    "ClientOptions.AutoCopyGameCode",
                    () => ConfigRoles.AutoCopyGameCode.Value,
                    value => ConfigRoles.AutoCopyGameCode.Value = value),
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsSendAnalytics",
                    () => ConfigRoles.IsSendAnalytics.Value,
                    value => ConfigRoles.IsSendAnalytics.Value = value),
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsVersionErrorView",
                    () => ConfigRoles.IsVersionErrorView.Value,
                    value => ConfigRoles.IsVersionErrorView.Value = value),
            ]),
        new(
            "ClientOptions.Category.Display",
            [
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsModCosmeticsAreNotLoaded",
                    () => ConfigRoles.IsModCosmeticsAreNotLoaded.Value,
                    value => ConfigRoles.IsModCosmeticsAreNotLoaded.Value = value),
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsNotUsingBlood",
                    () => ConfigRoles.IsNotUsingBlood.Value,
                    value =>
                    {
                        ConfigRoles.IsNotUsingBlood.Value = value;
                        BloodStain.RefreshAllColors();
                    }),
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsLightAndDarker",
                    () => ConfigRoles.IsLightAndDarker.Value,
                    value =>
                    {
                        ConfigRoles.IsLightAndDarker.Value = value;
                        DarkLightPlayerVoteAreaPatch.ApplyCurrentSettingToMeeting();
                    }),
                ClientOptionEntry.Toggle(
                    "ClientOptions.IsMuteLobbyBGM",
                    () => ConfigRoles.IsMuteLobbyBGM.Value,
                    value =>
                    {
                        ConfigRoles.IsMuteLobbyBGM.Value = value;
                        LobbyBehaviourPatch.ApplyCurrentLobbyBgmSetting();
                    }),
            ]),
        new(
            "ClientOptions.Category.Network",
            [
                ClientOptionEntry.Toggle(
                    "ClientOptions.CanUseDataConnection",
                    () => ConfigRoles.CanUseDataConnection.Value,
                    value => ConfigRoles.CanUseDataConnection.Value = value),
            ]),
        new(
            "ClientOptions.Category.Performance",
            [
                ClientOptionEntry.Toggle(
                    "ClientOptions.CPUProcessorAffinity",
                    () => ConfigRoles._isCPUProcessorAffinity.Value,
                    value =>
                    {
                        ConfigRoles._isCPUProcessorAffinity.Value = value;
                        SuperNewRolesPlugin.UpdateCPUProcessorAffinity();
                    }),
                ClientOptionEntry.Select(
                    "ClientOptions.ProcessorAffinityMask",
                    GetProcessorAffinityMaskSelection,
                    SetProcessorAffinityMaskSelection,
                    [
                        new("ClientOptions.ProcessorAffinityMask.OneCore"),
                        new("ClientOptions.ProcessorAffinityMask.TwoCores"),
                    ],
                    () => ConfigRoles._isCPUProcessorAffinity.Value),
            ]),
    ];

    public static void Toggle(OptionsMenuBehaviour owner)
    {
        if (_dialogObject != null && _dialogObject.activeSelf)
        {
            Close();
            return;
        }

        Open(owner);
    }

    public static void Open(OptionsMenuBehaviour owner)
    {
        if (owner == null)
            return;

        if (_dialogObject == null)
            InitializeMenu(owner);
        else
            ReparentDialog(owner);

        if (_leftAreaInner == null || _rightAreaInner == null)
            return;

        DisableMainMenuScreenMask();
        StopDialogAnimation();
        _dialogObject.SetActive(true);
        _dialogObject.transform.localScale = Vector3.zero;
        ShowCategory(_currentCategory ?? Categories[0], _currentSelectedCategory);
        DisableSpriteMasksTemporarily();
        StartDialogAnimation(Vector3.zero, Vector3.one * DialogScale, null);
    }

    public static void Close()
    {
        if (_dialogObject == null || !_dialogObject.activeSelf || _isClosing)
            return;

        _isClosing = true;
        StartDialogAnimation(_dialogObject.transform.localScale, Vector3.zero, () =>
        {
            if (_dialogObject != null)
            {
                _dialogObject.SetActive(false);
                _dialogObject.transform.localScale = Vector3.one * DialogScale;
            }

            _isClosing = false;
            RestoreMainMenuScreenMask();
        });
    }

    public static void CloseImmediate()
    {
        StopDialogAnimation();
        _isClosing = false;

        if (_dialogObject != null)
        {
            _dialogObject.SetActive(false);
            _dialogObject.transform.localScale = Vector3.one * DialogScale;
        }

        RestoreMainMenuScreenMask();
    }

    public static bool IsCurrentOwner(OptionsMenuBehaviour owner)
    {
        return owner != null && _animationOwner == owner;
    }

    public static void DestroyMenu()
    {
        StopDialogAnimation();
        RestoreMainMenuScreenMask();

        if (_dialogObject != null)
            GameObject.Destroy(_dialogObject);

        DestroyUnmaskedTextMaterials();

        _dialogObject = null;
        _menuObject = null;
        _leftAreaInner = null;
        _rightAreaInner = null;
        _leftScroller = null;
        _rightScroller = null;
        _currentSelectedCategory = null;
        _mainMenuScreenMask = null;
        _currentCategory = null;
        _animationOwner = null;
        _animationCoroutine = null;
        ClippedScrollerInners.Clear();
        _isClosing = false;
    }

    private static void InitializeMenu(OptionsMenuBehaviour owner)
    {
        _dialogObject = new GameObject(DialogObjectName);
        ReparentDialog(owner);
        CreateBlackBackdrop(owner);
        CreateDialogBackground(owner);
        CreateDialogCloseButton();

        _menuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(StandardOptionMenuAssetName), _dialogObject.transform);
        _menuObject.name = MenuObjectName;
        _menuObject.transform.localPosition = new Vector3(0f, 0f, -0.5f);
        _menuObject.transform.localScale = Vector3.one;
        _menuObject.transform.localRotation = Quaternion.identity;

        _leftScroller = _menuObject.transform.Find("LeftArea/Scroller")?.GetComponent<Scroller>();
        _rightScroller = _menuObject.transform.Find("RightArea/Scroller")?.GetComponent<Scroller>();
        _leftAreaInner = _menuObject.transform.Find("LeftArea/Scroller/Inner");
        _rightAreaInner = _menuObject.transform.Find("RightArea/Scroller/Inner");
        ConfigureMenuVisuals();

        if (_leftAreaInner == null || _rightAreaInner == null)
        {
            Logger.Error("ClientOptions: StandardOptionMenu layout is not available.");
            _dialogObject.SetActive(false);
            return;
        }

        ClearChildren(_leftAreaInner);
        ClearChildren(_rightAreaInner);
        ConfigureMenuText();
        GenerateCategoryButtons();
        SetScrollerBounds(_leftScroller, Categories.Length, 4, CategoryButtonSpacing);
        _dialogObject.SetActive(false);
    }

    private static void ReparentDialog(OptionsMenuBehaviour owner)
    {
        _animationOwner = owner;
        _dialogObject.transform.SetParent(owner.transform, false);
        _dialogObject.transform.localPosition = new Vector3(0f, 0f, -12f);
        _dialogObject.transform.localScale = Vector3.one * DialogScale;
        _dialogObject.transform.localRotation = Quaternion.identity;
    }

    private static void StartDialogAnimation(Vector3 fromScale, Vector3 toScale, Action onComplete)
    {
        StopDialogAnimation();

        if (_dialogObject == null)
            return;

        if (_animationOwner == null)
        {
            _dialogObject.transform.localScale = toScale;
            onComplete?.Invoke();
            return;
        }

        bool completed = false;
        _animationCoroutine = _animationOwner.StartCoroutine(Effects.Lerp(DialogAnimationDuration, new Action<float>((progress) =>
        {
            if (_dialogObject == null)
                return;

            float t = Mathf.SmoothStep(0f, 1f, progress);
            _dialogObject.transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);

            if (completed || progress < 1f)
                return;

            completed = true;
            _animationCoroutine = null;
            onComplete?.Invoke();
        })));
    }

    private static void StopDialogAnimation()
    {
        if (_animationCoroutine == null || _animationOwner == null)
        {
            _animationCoroutine = null;
            return;
        }

        _animationOwner.StopCoroutine(_animationCoroutine);
        _animationCoroutine = null;
    }

    private static void DisableMainMenuScreenMask()
    {
        if (_mainMenuScreenMaskDisabledByDialog)
            return;

        _mainMenuScreenMask = GameObject.Find(MainMenuScreenMaskPath);
        if (_mainMenuScreenMask == null)
            return;

        _mainMenuScreenMaskWasActive = _mainMenuScreenMask.activeSelf;
        _mainMenuScreenMask.SetActive(false);
        _mainMenuScreenMaskDisabledByDialog = true;
    }

    private static void RestoreMainMenuScreenMask()
    {
        if (!_mainMenuScreenMaskDisabledByDialog)
            return;

        if (_mainMenuScreenMask != null)
            _mainMenuScreenMask.SetActive(_mainMenuScreenMaskWasActive);

        _mainMenuScreenMask = null;
        _mainMenuScreenMaskWasActive = false;
        _mainMenuScreenMaskDisabledByDialog = false;
    }

    private static void CreateDialogBackground(OptionsMenuBehaviour owner)
    {
        if (owner.Background == null)
            return;

        var background = GameObject.Instantiate(owner.Background.gameObject, _dialogObject.transform);
        background.name = "DialogBackground";
        background.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        background.transform.localScale = Vector3.one;
        background.transform.localRotation = Quaternion.identity;
        ClearChildren(background.transform);

        var backgroundRenderer = background.GetComponent<SpriteRenderer>();
        if (backgroundRenderer == null)
            return;

        backgroundRenderer.drawMode = SpriteDrawMode.Sliced;
        backgroundRenderer.size = new Vector2(DialogWidth, DialogHeight);
        backgroundRenderer.color = Color.white;
        ConfigureEmptyPassiveButton(background, backgroundRenderer.size, null);
    }

    private static void CreateBlackBackdrop(OptionsMenuBehaviour owner)
    {
        if (owner.Background == null)
            return;

        var backdrop = GameObject.Instantiate(owner.Background.gameObject, _dialogObject.transform);
        backdrop.name = "DialogBlackBackdrop";
        backdrop.transform.localPosition = new Vector3(0f, 0f, 1.2f);
        backdrop.transform.localScale = Vector3.one;
        backdrop.transform.localRotation = Quaternion.identity;
        ClearChildren(backdrop.transform);

        var backdropRenderer = backdrop.GetComponent<SpriteRenderer>();
        if (backdropRenderer == null)
            return;

        backdropRenderer.drawMode = SpriteDrawMode.Sliced;
        backdropRenderer.size = new Vector2(BackdropWidth, BackdropHeight);
        backdropRenderer.color = new Color(0f, 0f, 0f, BackdropAlpha);
        ConfigureEmptyPassiveButton(backdrop, backdropRenderer.size, (UnityAction)Close);
    }

    private static void CreateDialogCloseButton()
    {
        var closeButtonPrefab = AssetManager.GetAsset<GameObject>(CloseButtonAssetName);
        if (closeButtonPrefab == null)
            return;

        var closeButtonObject = GameObject.Instantiate(closeButtonPrefab, _dialogObject.transform);
        closeButtonObject.name = "DialogCloseButton";
        closeButtonObject.transform.localPosition = DialogCloseButtonPosition;
        closeButtonObject.transform.localScale = DialogCloseButtonScale;
        closeButtonObject.transform.localRotation = Quaternion.identity;

        var aspectPosition = closeButtonObject.GetComponent<AspectPosition>();
        if (aspectPosition != null)
            GameObject.Destroy(aspectPosition);

        var passiveButton = closeButtonObject.GetComponent<PassiveButton>();
        if (passiveButton == null)
            passiveButton = closeButtonObject.AddComponent<PassiveButton>();

        var collider = closeButtonObject.GetComponent<Collider2D>();
        if (collider != null)
            passiveButton.Colliders = new Collider2D[] { collider };

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)Close);
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();
    }

    private static void ConfigureEmptyPassiveButton(GameObject obj, Vector2 colliderSize, UnityAction onClick)
    {
        var boxCollider = obj.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
            boxCollider = obj.AddComponent<BoxCollider2D>();

        boxCollider.size = colliderSize;
        boxCollider.offset = Vector2.zero;

        var passiveButton = obj.GetComponent<PassiveButton>();
        if (passiveButton == null)
            passiveButton = obj.AddComponent<PassiveButton>();

        passiveButton.Colliders = new Collider2D[] { boxCollider };
        passiveButton.OnClick = new();
        if (onClick != null)
            passiveButton.OnClick.AddListener(onClick);
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();
    }

    private static void ConfigureMenuVisuals()
    {
        ConfigureAreaBackground(_menuObject.transform.Find("LeftArea"));
        ConfigureAreaBackground(_menuObject.transform.Find("RightArea"));
        ConfigureLeftArea();
        ConfigureRightArea();
        ConfigureMenuLine();
    }

    // なんかいろいろめんどくさくなったからマスクなしにする。スクロールなしだからとりあえず。
    private static void DisableSpriteMasksTemporarily()
    {
        // Temporary workaround for the client options dialog mask issue. Remove this method when the UI layering is finalized.
        if (_dialogObject == null)
            return;

        foreach (var renderer in _dialogObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (IsUnderScrollerInner(renderer.transform))
                continue;

            renderer.maskInteraction = SpriteMaskInteraction.None;
        }

        foreach (var text in _dialogObject.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (IsUnderClippedScrollerInner(text.transform))
                continue;

            var material = text.fontSharedMaterial;
            if (material == null)
                continue;

            material = GetOrCreateUnmaskedTextMaterial(material);
            text.fontSharedMaterial = material;
            text.UpdateMeshPadding();
        }
    }

    private static bool IsUnderScrollerInner(Transform transform)
    {
        for (var current = transform; current != null && current != _dialogObject.transform; current = current.parent)
        {
            if (current.name == "Inner" && current.parent != null && current.parent.name == "Scroller")
                return true;
        }

        return false;
    }

    private static bool IsUnderClippedScrollerInner(Transform transform)
    {
        for (var current = transform; current != null && current != _dialogObject.transform; current = current.parent)
        {
            if (ClippedScrollerInners.Contains(current))
                return true;
        }

        return false;
    }

    private static Material GetOrCreateUnmaskedTextMaterial(Material sourceMaterial)
    {
        const string materialSuffix = " (SNR ClientOptions)";
        if (sourceMaterial.name.EndsWith(materialSuffix))
        {
            sourceMaterial.SetFloat("_StencilComp", 8f);
            sourceMaterial.SetFloat("_Stencil", 0f);
            return sourceMaterial;
        }

        if (!UnmaskedTextMaterials.TryGetValue(sourceMaterial, out Material material) || material == null)
        {
            material = new Material(sourceMaterial)
            {
                name = sourceMaterial.name + materialSuffix,
            };
            material.SetFloat("_StencilComp", 8f);
            material.SetFloat("_Stencil", 0f);
            UnmaskedTextMaterials[sourceMaterial] = material;
        }

        return material;
    }

    private static void DestroyUnmaskedTextMaterials()
    {
        foreach (var material in UnmaskedTextMaterials.Values)
        {
            if (material != null)
                GameObject.Destroy(material);
        }

        UnmaskedTextMaterials.Clear();
    }

    private static void ConfigureLeftArea()
    {
        var leftArea = _menuObject.transform.Find("LeftArea");
        if (leftArea != null)
            leftArea.localPosition = new Vector3(-0.1f, 0f, 0f);
    }

    private static void ConfigureRightArea()
    {
        var rightArea = _menuObject.transform.Find("RightArea");
        if (rightArea == null)
            return;

        rightArea.localPosition = new Vector3(0.2f, 0f, -6f);
        rightArea.localScale = Vector3.one * 1.1f;
    }

    private static void ConfigureMenuLine()
    {
        var mainLine = FindChildByName(_menuObject.transform, "Line");
        if (mainLine != null)
        {
            mainLine.localPosition = new Vector3(-2.3f, 0f, 0f);
            mainLine.localScale = new Vector3(0.01f, 5.3f, 1f);
        }

        var horizontalLine = FindChildByName(_menuObject.transform, "Line (1)");
        if (horizontalLine != null)
            horizontalLine.localScale = new Vector3(10f, 6f, 1f);
    }

    private static void ConfigureAreaBackground(Transform area)
    {
        if (area == null)
            return;

        var inner = area.Find("Scroller/Inner");
        foreach (var renderer in area.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (inner != null && renderer.transform.IsChildOf(inner))
                continue;

            if (renderer.transform == area || renderer.name.Contains("Back") || renderer.name.Contains("BG"))
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
        }
    }

    private static void GenerateCategoryButtons()
    {
        for (int i = 0; i < Categories.Length; i++)
        {
            var category = Categories[i];
            var button = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(CategoryButtonAssetName), _leftAreaInner);
            button.name = $"ClientOptionCategory_{category.TitleKey}";
            button.transform.localScale = Vector3.one * CategoryButtonScale;
            button.transform.localPosition = new Vector3(CategoryX, CategoryInitialY - (i * CategoryButtonSpacing), -0.21f);

            UIHelper.SetText(button, ModTranslation.GetString(category.TitleKey));
            ConfigureCategoryButton(button, category);

            if (i == 0)
                _currentSelectedCategory = button.transform.Find("Selected")?.gameObject;
        }
    }

    private static void ConfigureMenuText()
    {
        var titleText = _menuObject.transform.Find("TitleText")?.GetComponent<TextMeshPro>();
        if (titleText != null)
            titleText.text = ModTranslation.GetString(TitleKey);
    }

    private static void ConfigureCategoryButton(GameObject buttonObj, ClientOptionCategory category)
    {
        var passiveButton = buttonObj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { buttonObj.GetComponent<BoxCollider2D>() };
        var selectedObject = buttonObj.transform.Find("Selected")?.gameObject;
        if (selectedObject != null)
            selectedObject.SetActive(false);

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() => ShowCategory(category, selectedObject)));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() => selectedObject?.SetActive(true)));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (_currentSelectedCategory != selectedObject)
                selectedObject?.SetActive(false);
        }));
    }

    private static void ShowCategory(ClientOptionCategory category, GameObject selectedObject)
    {
        if (_rightAreaInner == null)
            return;

        if (_currentSelectedCategory != null && _currentSelectedCategory != selectedObject)
            _currentSelectedCategory.SetActive(false);

        _currentCategory = category;
        _currentSelectedCategory = selectedObject;
        _currentSelectedCategory?.SetActive(true);

        ClearChildren(_rightAreaInner);

        int activeCount = 0;
        foreach (var option in category.Options)
        {
            if (!option.IsVisible())
                continue;

            var optionObject = option.Kind == ClientOptionKind.Toggle
                ? GenerateToggleOption(option)
                : GenerateSelectOption(option);

            optionObject.transform.localPosition = new Vector3(OptionX, OptionInitialY - (activeCount * OptionSpacing), -0.21f);
            activeCount++;
        }

        SetScrollerBounds(_rightScroller, activeCount, RightVisibleRows, OptionSpacing);
        _rightScroller?.ScrollToTop();
        DisableSpriteMasksTemporarily();
    }

    private static GameObject GenerateToggleOption(ClientOptionEntry option)
    {
        var optionObject = InstantiateOptionObject(CheckOptionAssetName, option.TitleKey);
        var checkMark = optionObject.transform.Find("CheckMark")?.gameObject;
        checkMark?.SetActive(option.GetBool());

        var passiveButton = optionObject.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { optionObject.GetComponent<BoxCollider2D>() };
        ConfigureOptionButton(passiveButton, optionObject.GetComponent<SpriteRenderer>(), () =>
        {
            bool newValue = !option.GetBool();
            option.SetBool(newValue);
            checkMark?.SetActive(newValue);
            RefreshCurrentCategory();
        });

        return optionObject;
    }

    private static GameObject GenerateSelectOption(ClientOptionEntry option)
    {
        var optionObject = InstantiateOptionObject(SelectOptionAssetName, option.TitleKey);
        var selectedText = optionObject.transform.Find("SelectedText")?.GetComponent<TextMeshPro>();
        UpdateSelectText(option, selectedText);

        ConfigureSelectButton(optionObject, "Button_Minus", option, selectedText, isIncrement: false);
        ConfigureSelectButton(optionObject, "Button_Plus", option, selectedText, isIncrement: true);

        return optionObject;
    }

    private static GameObject InstantiateOptionObject(string assetName, string titleKey)
    {
        var optionObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>(assetName), _rightAreaInner);
        optionObject.name = $"ClientOption_{titleKey}";
        optionObject.transform.localScale = Vector3.one * OptionScale;
        UIHelper.SetText(optionObject, ModTranslation.GetString(titleKey));
        BringOptionTextToFront(optionObject);
        return optionObject;
    }

    private static void BringOptionTextToFront(GameObject optionObject)
    {
        foreach (var text in optionObject.GetComponentsInChildren<TextMeshPro>(true))
        {
            text.sortingOrder = 1;
            var position = text.transform.localPosition;
            text.transform.localPosition = new Vector3(position.x, position.y, -0.2f);
        }
    }

    private static void ConfigureSelectButton(
        GameObject optionObject,
        string buttonName,
        ClientOptionEntry option,
        TextMeshPro selectedText,
        bool isIncrement)
    {
        var buttonObject = optionObject.transform.Find(buttonName)?.gameObject;
        if (buttonObject == null)
            return;

        var passiveButton = buttonObject.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { buttonObject.GetComponent<BoxCollider2D>() };
        ConfigureOptionButton(passiveButton, buttonObject.GetComponent<SpriteRenderer>(), () =>
        {
            int count = option.Choices.Length;
            if (count == 0)
                return;

            int newSelection = option.GetSelection() + (isIncrement ? 1 : -1);
            if (newSelection < 0)
                newSelection = count - 1;
            else if (newSelection >= count)
                newSelection = 0;

            option.SetSelection(newSelection);
            UpdateSelectText(option, selectedText);
            RefreshCurrentCategory();
        });
    }

    private static void ConfigureOptionButton(PassiveButton button, SpriteRenderer spriteRenderer, Action onClick)
    {
        button.OnClick = new();
        button.OnClick.AddListener((UnityAction)(() => onClick()));
        button.OnMouseOver = new();
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = HoverColor;
        }));
        button.OnMouseOut = new();
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }));
    }

    private static void UpdateSelectText(ClientOptionEntry option, TextMeshPro selectedText)
    {
        if (selectedText == null || option.Choices.Length == 0)
            return;

        int selection = Mathf.Clamp(option.GetSelection(), 0, option.Choices.Length - 1);
        selectedText.text = ModTranslation.GetString(option.Choices[selection].TitleKey);
    }

    private static void RefreshCurrentCategory()
    {
        if (_currentCategory != null)
            ShowCategory(_currentCategory, _currentSelectedCategory);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.gameObject.SetActive(false);
            GameObject.Destroy(child.gameObject);
        }
    }

    private static Transform FindChildByName(Transform parent, string name)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
                return child;

            var nested = FindChildByName(child, name);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private static void SetScrollerBounds(Scroller scroller, int itemCount, int visibleRows, float spacing)
    {
        if (scroller == null)
            return;

        scroller.SetYBoundsMax(itemCount <= visibleRows ? 0f : (itemCount - visibleRows) * spacing);

        var inner = scroller.transform.Find("Inner");
        if (inner == null)
            return;

        if (itemCount > visibleRows)
            ClippedScrollerInners.Add(inner);
        else
            ClippedScrollerInners.Remove(inner);
    }

    private static int GetProcessorAffinityMaskSelection()
    {
        return ConfigRoles._ProcessorAffinityMask.Value == 1UL ? 0 : 1;
    }

    private static void SetProcessorAffinityMaskSelection(int selection)
    {
        ConfigRoles._ProcessorAffinityMask.Value = selection == 0 ? 1UL : 3UL;
        SuperNewRolesPlugin.UpdateCPUProcessorAffinity();
    }

    private sealed class ClientOptionCategory
    {
        public string TitleKey { get; }
        public ClientOptionEntry[] Options { get; }

        public ClientOptionCategory(string titleKey, ClientOptionEntry[] options)
        {
            TitleKey = titleKey;
            Options = options;
        }
    }

    private sealed class ClientOptionEntry
    {
        public string TitleKey { get; }
        public ClientOptionKind Kind { get; }
        public ClientOptionChoice[] Choices { get; }
        private readonly Func<bool> _getBool;
        private readonly Action<bool> _setBool;
        private readonly Func<int> _getSelection;
        private readonly Action<int> _setSelection;
        private readonly Func<bool> _isVisible;

        private ClientOptionEntry(
            string titleKey,
            ClientOptionKind kind,
            Func<bool> getBool,
            Action<bool> setBool,
            Func<int> getSelection,
            Action<int> setSelection,
            ClientOptionChoice[] choices,
            Func<bool> isVisible)
        {
            TitleKey = titleKey;
            Kind = kind;
            _getBool = getBool;
            _setBool = setBool;
            _getSelection = getSelection;
            _setSelection = setSelection;
            Choices = choices ?? Array.Empty<ClientOptionChoice>();
            _isVisible = isVisible ?? (() => true);
        }

        public static ClientOptionEntry Toggle(
            string titleKey,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<bool> isVisible = null)
        {
            return new ClientOptionEntry(titleKey, ClientOptionKind.Toggle, getValue, setValue, null, null, null, isVisible);
        }

        public static ClientOptionEntry Select(
            string titleKey,
            Func<int> getSelection,
            Action<int> setSelection,
            ClientOptionChoice[] choices,
            Func<bool> isVisible = null)
        {
            return new ClientOptionEntry(titleKey, ClientOptionKind.Select, null, null, getSelection, setSelection, choices, isVisible);
        }

        public bool IsVisible() => _isVisible();
        public bool GetBool() => _getBool();
        public void SetBool(bool value) => _setBool(value);
        public int GetSelection() => _getSelection();
        public void SetSelection(int selection) => _setSelection(selection);
    }

    private readonly record struct ClientOptionChoice(string TitleKey);

    private enum ClientOptionKind
    {
        Toggle,
        Select,
    }
}

public static class ClientOptionsPatches
{
    private const string ButtonObjectName = "SNR_ClientOptionsButton";
    private static readonly Vector3 ReturnToGamePosition = new(1.35f, -2.321f, -1f);
    private static readonly Vector3 LeaveGameButtonPosition = new(-1.35f, -2.321f, -1f);
    private static readonly Vector3 ClientOptionButtonPosition = new(0f, -1.47f, -1f);

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenuBehaviourStartPatch
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (__instance.CensorChatButton == null)
                return;

            ConfigureGeneralTabButtonPositions(__instance);

            if (FindClientOptionButton(__instance) != null)
                return;

            var parent = GetClientOptionButtonParent(__instance);
            var buttonObject = GameObject.Instantiate(__instance.CensorChatButton.gameObject, parent);
            buttonObject.name = ButtonObjectName;
            buttonObject.transform.localPosition = ClientOptionButtonPosition;
            buttonObject.SetActive(true);

            var aspectPosition = buttonObject.GetComponent<AspectPosition>();
            if (aspectPosition != null)
                GameObject.Destroy(aspectPosition);

            var toggleButton = buttonObject.GetComponent<ToggleButtonBehaviour>();
            if (toggleButton != null)
            {
                if (toggleButton.Background != null)
                    toggleButton.Background.color = Color.white;
                if (toggleButton.Rollover != null)
                    toggleButton.Rollover.ChangeOutColor(Color.white);
                GameObject.Destroy(toggleButton);
            }

            var text = buttonObject.GetComponentInChildren<TextMeshPro>();
            if (text != null)
                text.text = ModTranslation.GetString("ClientOptionsMod");

            var passiveButton = buttonObject.GetComponent<PassiveButton>();
            if (passiveButton == null)
                return;

            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() => ClientOptions.Toggle(__instance)));
        }

        private static void ConfigureGeneralTabButtonPositions(OptionsMenuBehaviour optionsMenu)
        {
            var generalTab = FindChildByName(optionsMenu.transform, "GeneralTab");
            if (generalTab == null)
                return;

            SetChildLocalPosition(generalTab, "ReturnToGameButton", ReturnToGamePosition);
            SetChildLocalPosition(generalTab, "LeaveGameButton", LeaveGameButtonPosition);
        }

        private static Transform FindClientOptionButton(OptionsMenuBehaviour optionsMenu)
        {
            foreach (var transform in optionsMenu.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == ButtonObjectName)
                    return transform;
            }

            return null;
        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            if (parent == null)
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                    return child;

                var nested = FindChildByName(child, name);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static void SetChildLocalPosition(Transform parent, string name, Vector3 position)
        {
            var child = FindChildByName(parent, name);
            if (child != null)
                child.localPosition = position;
        }

        private static Transform GetClientOptionButtonParent(OptionsMenuBehaviour optionsMenu)
        {
            if (optionsMenu.EnableFriendInvitesButton != null)
                return optionsMenu.EnableFriendInvitesButton.transform.parent;

            return optionsMenu.CensorChatButton.transform.parent;
        }

    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Close))]
    public static class OptionsMenuBehaviourClosePatch
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (ClientOptions.IsCurrentOwner(__instance))
                ClientOptions.CloseImmediate();
        }
    }

    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.OnDestroy))]
    public static class OptionsMenuBehaviourOnDestroyPatch
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            if (ClientOptions.IsCurrentOwner(__instance))
                ClientOptions.DestroyMenu();
        }
    }
}
