using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.HelpMenus;
using SuperNewRoles.Patches;
using SuperNewRoles.RequestInGame;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SuperNewRoles.Modules;

public static class VersionUpdatesUI
{
    public const string ApiUrl = SNRURLs.UpdateURL;
    public const string VersionListUrl = ApiUrl + "versions20";
    // FilePath is now managed by VersionConfigManager
    // private static string FilePath = Path.Combine(BepInEx.Paths.PatcherPluginPath, "snrupdate.json");
    public static Color loadedButtonColor = new Color32(44, 74, 68, 255);
    public static Color notLoadedButtonColor = new Color32(74, 128, 123, 255);
    public static Color mouseOverButtonColor = Color.green;
    public static Color defaultButtonColor = Color.white;

    private const string VersionSelectButtonPrefab = "VersionSelectButton";
    private const string VersionUpBGPrefab = "VersionUpBG";
    private const string VersionContainerPrefab = "VersionContainer";
    private const string VersionSelecterPrefab = "VersionSelecter";

    private const float VersionSelectButtonScale = 0.4f;
    private const float VersionSelectBgScale = 0.58f;
    private const float VersionSelectBgZ = -100f;
    private const float VersionContainerZ = -0.1f;
    public const float FadeDuration = 0.115f;
    private const float VersionObjectInitialY = 1.5f;
    private const float VersionObjectX = -0.05f;
    private const float VersionObjectYOffset = -1.2f;

    public static void InitializeMainMenuButton(MainMenuManager __instance)
    {
        GameObject versionSelectButton = AssetManager.Instantiate(VersionSelectButtonPrefab, null);
        versionSelectButton.transform.localPosition = Vector3.zero;
        versionSelectButton.transform.localScale = Vector3.one * VersionSelectButtonScale;
        PassiveButton passiveButton = versionSelectButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { versionSelectButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() => ShowVersionSelect()));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() => versionSelectButton.transform.Find("Selected").gameObject.SetActive(true)));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() => versionSelectButton.transform.Find("Selected").gameObject.SetActive(false)));
        AspectPosition aspectPosition = versionSelectButton.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.Bottom;
        // memo right: 3.9 0.83 -100
        aspectPosition.DistanceFromEdge = new(0, 0.83f, 0);
        aspectPosition.OnEnable();
    }
    public static void ShowVersionSelect()
    {
        GameObject versionSelect = AssetManager.Instantiate(VersionUpBGPrefab, null);
        versionSelect.transform.localPosition = new(0, 0, VersionSelectBgZ);
        versionSelect.transform.localScale = Vector3.one * VersionSelectBgScale;

        GameObject versionContainer = AssetManager.Instantiate(VersionContainerPrefab, versionSelect.transform);
        versionContainer.transform.localPosition = new(0, 0, VersionContainerZ);
        versionContainer.transform.localScale = Vector3.one;
        versionContainer.transform.localRotation = Quaternion.identity;

        GameObject updateTypeButton = versionContainer.transform.Find("AutoUpDateModeSelecter/UpdateTypeBox").gameObject;
        ConfigureUpdateTypeButton(updateTypeButton);

        GameObject versionListScroller = versionContainer.transform.Find("VersionListScroller").gameObject;
        ConfigureVersionListScroller(versionListScroller);

        // 後ろ触っても反応しないように
        PassiveButton passiveButton = versionSelect.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { versionSelect.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();

        var fadeCoroutine = versionSelect.AddComponent<FadeCoroutine>();
        fadeCoroutine.StartFadeIn(versionSelect, FadeDuration);

        VersionUpdatesComponent versionUpdatesComponent = versionSelect.AddComponent<VersionUpdatesComponent>();
        versionUpdatesComponent.StartCoroutine(GenerateVersionList(versionListScroller.GetComponent<Scroller>(), updateTypeButton.transform.Find("Text").GetComponent<TextMeshPro>()).WrapToIl2Cpp());

        // 閉じれるように

        PassiveButton closeBoxPassiveButton = versionSelect.transform.Find("CloseBox").gameObject.AddComponent<PassiveButton>();
        closeBoxPassiveButton.Colliders = new Collider2D[] { closeBoxPassiveButton.GetComponent<Collider2D>() };
        closeBoxPassiveButton.OnClick = new();
        closeBoxPassiveButton.OnClick.AddListener((UnityAction)(() => fadeCoroutine.StartFadeOut(versionSelect, FadeDuration, true)));
        closeBoxPassiveButton.OnMouseOver = new();
        closeBoxPassiveButton.OnMouseOut = new();

        PassiveButton closeButtonPassiveButton = versionSelect.transform.Find("CloseButton").gameObject.AddComponent<PassiveButton>();
        closeButtonPassiveButton.Colliders = new Collider2D[] { closeButtonPassiveButton.GetComponent<Collider2D>() };
        closeButtonPassiveButton.OnClick = new();
        closeButtonPassiveButton.OnClick.AddListener((UnityAction)(() => fadeCoroutine.StartFadeOut(versionSelect, FadeDuration, true)));
        closeButtonPassiveButton.OnMouseOver = new();
        closeButtonPassiveButton.OnMouseOut = new();
    }
    public static void ConfigureVersionListScroller(GameObject versionListScroller)
    {
        Scroller scroller = versionListScroller.GetComponent<Scroller>();
    }
    public static void ConfigureUpdateTypeButton(GameObject updateTypeButton)
    {
        string updateType = VersionConfigManager.GetVersionType();
        TextMeshPro tmp = updateTypeButton.transform.Find("Text").GetComponent<TextMeshPro>();
        UpdateText(tmp, updateType);
        ConfigurePlusOrMinusButton(updateTypeButton.transform.Find("uiButtonPlus"), () =>
        {
            int newIndex = VersionTypeList.IndexOf(updateType) + 1;
            if (newIndex >= VersionTypeList.Count || newIndex == -1)
                newIndex = 0;
            VersionConfigManager.SetVersionType(VersionTypeList[newIndex]);
            updateType = VersionTypeList[newIndex];
            UpdateText(tmp, updateType);
        });
        ConfigurePlusOrMinusButton(updateTypeButton.transform.Find("uiButtonMinus"), () =>
        {
            int newIndex = VersionTypeList.IndexOf(updateType) - 1;
            if (newIndex < 0)
                newIndex = VersionTypeList.Count - 1;
            VersionConfigManager.SetVersionType(VersionTypeList[newIndex]);
            updateType = VersionTypeList[newIndex];
            UpdateText(tmp, updateType);
        });
    }
    public static void UpdateText(TextMeshPro tmp, string updateType)
    {
        tmp.text = ModTranslation.TryGetString("VersionUpdateType." + updateType, out string translation) ? translation : "INVALID:" + updateType;
    }
    public static void ConfigurePlusOrMinusButton(Transform button, Action onClick)
    {
        PassiveButton passiveButton = button.gameObject.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { passiveButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            onClick();
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            button.GetComponent<SpriteRenderer>().color = mouseOverButtonColor;
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            button.GetComponent<SpriteRenderer>().color = defaultButtonColor;
        }));
    }
    private static IEnumerator GenerateVersionList(Scroller scroller, TextMeshPro selectText)
    {
        bool active = true;
        LoadingUI.ShowLoadingUI(scroller.Inner.transform, () => ModTranslation.GetString("VersionUpdateLoading"), () => active);
        UnityWebRequest request = UnityWebRequest.Get(VersionListUrl);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch version list: " + request.error);
            active = false;
            yield break;
        }
        active = false;
        string data = request.downloadHandler.text;
        // \n で分割してリストにする
        var lines = data.Split('\n');
        var versionPairs = new System.Collections.Generic.List<(string version, string hash)>();
        for (int i = 0; i < lines.Length / 2 && versionPairs.Count < 10; i++)
        {
            // Check if both lines exist before adding
            if (i * 2 + 1 < lines.Length)
            {
                versionPairs.Add((lines[i * 2], lines[i * 2 + 1]));
            }
        }
        int index = 0;
        string dllPath = SuperNewRolesPlugin.Assembly.Location;
        byte[] bytes = File.ReadAllBytes(dllPath);
        string currentHash = ModHelpers.HashSHA256(bytes);
        string currentVersion = VersionConfigManager.GetVersion();
        string currentType = VersionConfigManager.GetVersionType();
        SpriteRenderer currentSelectedButtonRenderer = null;
        TextMeshPro currentSelectedText = null;
        foreach ((string version, string hash) in versionPairs)
        {
            if (string.IsNullOrEmpty(version))
                continue;
            GameObject versionObject = AssetManager.Instantiate(VersionSelecterPrefab, scroller.Inner.transform);
            versionObject.transform.localPosition = new(VersionObjectX, VersionObjectInitialY + index * VersionObjectYOffset, 0);
            versionObject.transform.localScale = Vector3.one;

            TextMeshPro versionNameText = versionObject.transform.Find("ReleaseNameButton/Text").GetComponent<TextMeshPro>();
            SpriteRenderer getButtonRenderer = versionObject.transform.Find("GetButton").GetComponent<SpriteRenderer>();
            TextMeshPro getButtonText = versionObject.transform.Find("GetButton/Text").GetComponent<TextMeshPro>();
            GameObject getButtonSelectedHighlight = versionObject.transform.Find("GetButton/Selected").gameObject;
            GameObject getButtonGameObject = versionObject.transform.Find("GetButton").gameObject;

            versionNameText.text = version;

            // Add PassiveButton and mouse events to ReleaseNameButton
            GameObject releaseNameButtonGameObject = versionObject.transform.Find("ReleaseNameButton").gameObject;
            GameObject releaseNameButtonSelectedHighlight = versionObject.transform.Find("ReleaseNameButton/Selected").gameObject;
            PassiveButton releaseNamePassiveButton = releaseNameButtonGameObject.AddComponent<PassiveButton>();
            releaseNamePassiveButton.Colliders = new Collider2D[] { releaseNameButtonGameObject.GetComponent<Collider2D>() };
            releaseNamePassiveButton.OnClick = new();
            releaseNamePassiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info("ReleaseNameButton clicked: " + version);
                ShowReleaseNote(version);
            }));
            releaseNamePassiveButton.OnMouseOver = new();
            releaseNamePassiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                releaseNameButtonSelectedHighlight.SetActive(true);
            }));
            releaseNamePassiveButton.OnMouseOut = new();
            releaseNamePassiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                releaseNameButtonSelectedHighlight.SetActive(false);
            }));
            Logger.Info("currentHash: " + currentHash + " hash: " + hash);

            if (currentHash == hash)
            {
                getButtonRenderer.color = loadedButtonColor;
                getButtonText.text = ModTranslation.GetString("VersionLoaded"); // Assuming you have a key for this
            }
            else if (currentType == "static" && currentVersion == version)
            {
                getButtonRenderer.color = loadedButtonColor;
                currentSelectedButtonRenderer = getButtonRenderer;
                currentSelectedText = getButtonText;
                getButtonText.text = ModTranslation.GetString("VersionWillUpdate");
            }
            else
            {
                getButtonText.text = ModTranslation.GetString("VersionUpdateGet");
                getButtonRenderer.color = notLoadedButtonColor; // Ensure default color if not selected/loaded
            }

            PassiveButton passiveButton = getButtonGameObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { passiveButton.GetComponent<Collider2D>() };
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                if (getButtonRenderer.color == loadedButtonColor)
                {
                    Logger.Info("Already loaded version: " + version);
                    return;
                }
                //
                Logger.Info("Loading version: " + version);
                VersionConfigManager.SetVersionType("static");
                selectText.text = ModTranslation.GetString("VersionUpdateType.static");
                VersionConfigManager.SaveVersion(version);
                if (currentSelectedButtonRenderer != null && currentSelectedButtonRenderer != getButtonRenderer)
                {
                    currentSelectedButtonRenderer.color = notLoadedButtonColor;
                }
                currentSelectedButtonRenderer = getButtonRenderer;
                currentSelectedButtonRenderer.color = loadedButtonColor;
                getButtonSelectedHighlight.SetActive(false);
                getButtonText.text = ModTranslation.GetString("VersionWillUpdate");
            }));
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (currentSelectedButtonRenderer != getButtonRenderer)
                    getButtonSelectedHighlight.SetActive(true);
            }));
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (currentSelectedButtonRenderer != getButtonRenderer)
                    getButtonSelectedHighlight.SetActive(false);
            }));

            index++;
        }
        scroller.ContentYBounds.max = index <= 5 ? 0 : (index - 5) * 1.4f - 0.1f;
    }

    private static List<string> VersionTypeList = new()
    {
        "all",
        "stability",
        "disable",
    };


    private static void ShowReleaseNote(string version)
    {
        GameObject releaseNote = AssetManager.Instantiate("ReleaseNoteBG", null);
        releaseNote.transform.localPosition = new(0, 0, VersionSelectBgZ - 10);
        releaseNote.transform.localScale = Vector3.one * 0.55f;

        GameObject releaseNoteContainer = AssetManager.Instantiate("ReleaseNoteContainer", releaseNote.transform);

        ReleaseNoteComponent releaseNoteComponent = releaseNote.AddComponent<ReleaseNoteComponent>();

        // 後ろ触っても反応しないように
        PassiveButton passiveButton = releaseNote.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { releaseNote.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();

        var fadeCoroutine = releaseNote.AddComponent<FadeCoroutine>();
        fadeCoroutine.StartFadeIn(releaseNote, FadeDuration);

        PassiveButton closeBoxPassiveButton = releaseNote.transform.Find("CloseBox").gameObject.AddComponent<PassiveButton>();
        closeBoxPassiveButton.Colliders = new Collider2D[] { closeBoxPassiveButton.GetComponent<Collider2D>() };
        closeBoxPassiveButton.OnClick = new();
        closeBoxPassiveButton.OnClick.AddListener((UnityAction)(() => fadeCoroutine.StartFadeOut(releaseNote, FadeDuration, true)));
        closeBoxPassiveButton.OnMouseOver = new();
        closeBoxPassiveButton.OnMouseOut = new();

        PassiveButton closeButtonPassiveButton = releaseNote.transform.Find("CloseButton").gameObject.AddComponent<PassiveButton>();
        closeButtonPassiveButton.Colliders = new Collider2D[] { closeButtonPassiveButton.GetComponent<Collider2D>() };
        closeButtonPassiveButton.OnClick = new();
        closeButtonPassiveButton.OnClick.AddListener((UnityAction)(() => fadeCoroutine.StartFadeOut(releaseNote, FadeDuration, true)));
        closeButtonPassiveButton.OnMouseOver = new();
        closeButtonPassiveButton.OnMouseOut = new();

        releaseNoteComponent.StartCoroutine(GenerateReleaseNote(releaseNoteContainer.transform.Find("ReleaseNoteScroller").GetComponent<Scroller>(), version).WrapToIl2Cpp());
    }
    private static IEnumerator GenerateReleaseNote(Scroller scroller, string version)
    {
        bool active = true;
        scroller.Inner.transform.Find("ReleaseNoteText").GetComponent<TextMeshPro>().text = "";
        LoadingUI.ShowLoadingUI(scroller.Inner.transform, () => ModTranslation.GetString("VersionUpdateReleaseNoteLoading"), () => active);
        UnityWebRequest request = UnityWebRequest.Get(SNRURLs.GithubAPITags + "/" + version);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch version list: " + request.error);
            scroller.Inner.transform.Find("ReleaseNoteText").GetComponent<TextMeshPro>().text = ModTranslation.GetString(request.responseCode == 404 ? "VersionUpdateReleaseNoteNotFound" : "VersionUpdateReleaseNoteLoadingError");
            active = false;
            yield break;
        }
        Dictionary<string, object> releaseData = JsonParser.Parse(request.downloadHandler.text) as Dictionary<string, object>;
        if (releaseData == null)
        {
            Debug.LogError("Failed to parse release data: " + request.downloadHandler.text);
            active = false;
            yield break;
        }
        string body = releaseData["body"] as string;
        if (string.IsNullOrEmpty(body))
        {
            Debug.LogError("Failed to get release note: " + version);
            active = false;
            yield break;
        }
        active = false;
        string convertedUnityText = MarkdownToUnityTag.Convert(body);
        string wrappedUnityText = WrapTextByVisibleChars(convertedUnityText, 30); // Apply wrapping here

        string text = $"<size=150%>{releaseData["name"]}</size>\n{wrappedUnityText}";
        scroller.Inner.transform.Find("ReleaseNoteText").GetComponent<TextMeshPro>().text = text;
        string[] lines = text.Split('\n');
        if (lines.Length > 13)
            scroller.ContentYBounds.max = (lines.Length - 13) * 0.55f + 0.5f;
    }

    private static string WrapTextByVisibleChars(string inputText, int maxVisibleCharsPerLine)
    {
        if (string.IsNullOrEmpty(inputText) || maxVisibleCharsPerLine <= 0)
            return inputText;

        System.Text.StringBuilder resultBuilder = new System.Text.StringBuilder();
        int currentVisibleCharCount = 0;
        bool tagActive = false;

        for (int i = 0; i < inputText.Length; i++)
        {
            char currentChar = inputText[i];
            resultBuilder.Append(currentChar);

            if (currentChar == '<')
            {
                tagActive = true;
            }
            else if (currentChar == '>')
            {
                tagActive = false;
            }
            else if (currentChar == '\n')
            {
                currentVisibleCharCount = 0;
            }
            else if (!tagActive)
            {
                currentVisibleCharCount++;
                if (currentVisibleCharCount >= maxVisibleCharsPerLine)
                {
                    if (i < inputText.Length - 1 && inputText[i + 1] != '\n')
                    {
                        resultBuilder.Append('\n');
                    }
                    currentVisibleCharCount = 0;
                }
            }
        }
        return resultBuilder.ToString();
    }
}
public class VersionUpdatesComponent : MonoBehaviour
{
    public FadeCoroutine fadeCoroutine;
    public void Start()
    {
        fadeCoroutine = gameObject.GetComponent<FadeCoroutine>();
        fadeCoroutine.onFadeOut += OnFadeOut;
        GameObject.Find("MainMenuManager/MainUI/AspectScaler/RightPanel/ScreenMask")?.SetActive(false);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameObject.FindObjectOfType<ReleaseNoteComponent>() == null)
            fadeCoroutine.StartFadeOut(gameObject, VersionUpdatesUI.FadeDuration, true);
    }
    private void OnFadeOut()
    {
        GameObject.Find("MainMenuManager/MainUI/AspectScaler/RightPanel/ScreenMask")?.SetActive(true);
    }
}
public class ReleaseNoteComponent : MonoBehaviour
{
    public FadeCoroutine fadeCoroutine;
    public void Start()
    {
        fadeCoroutine = gameObject.GetComponent<FadeCoroutine>();
        fadeCoroutine.onFadeOut += OnFadeOut;
        Logger.Info("VersionUpdatesComponent Start");
        GameObject.Find("VersionUpBG(Clone)/VersionContainer(Clone)/VersionListMask")?.SetActive(false);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            fadeCoroutine.StartFadeOut(gameObject, VersionUpdatesUI.FadeDuration, true);
    }
    private void OnFadeOut()
    {
        GameObject.Find("VersionUpBG(Clone)/VersionContainer(Clone)/VersionListMask")?.SetActive(true);
    }
}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManager_Start
{
    public static void Postfix(MainMenuManager __instance)
    {
        VersionUpdatesUI.InitializeMainMenuButton(__instance);
    }
}