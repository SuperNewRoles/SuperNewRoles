using System;
using System.Collections.Generic;
using Epic.OnlineServices.Presence;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class ClientModOptionsPatch
{
    private static readonly SelectionBehaviour[] AllOptions = {
            new("CustomStremerMode", () => ConfigRoles.StreamerMode.Value = !ConfigRoles.StreamerMode.Value, ConfigRoles.StreamerMode.Value),
            new("CustomAutoUpdate", () => ConfigRoles.AutoUpdate.Value = !ConfigRoles.AutoUpdate.Value, ConfigRoles.AutoUpdate.Value),
            new("CustomAutoCopyGameCode", () => ConfigRoles.AutoCopyGameCode.Value = !ConfigRoles.AutoCopyGameCode.Value, ConfigRoles.AutoCopyGameCode.Value),
            new("CustomIsVersionErrorView", () => ConfigRoles.IsVersionErrorView.Value = !ConfigRoles.IsVersionErrorView.Value, ConfigRoles.IsVersionErrorView.Value),
            new("IsModCosmeticsAreNotLoaded", () => ConfigRoles.IsModCosmeticsAreNotLoaded.Value = !ConfigRoles.IsModCosmeticsAreNotLoaded.Value, ConfigRoles.IsModCosmeticsAreNotLoaded.Value),
            new("IsNotUsingBlood", () => ConfigRoles.IsNotUsingBlood.Value = !ConfigRoles.IsNotUsingBlood.Value, ConfigRoles.IsNotUsingBlood.Value),
            new("IsSendAnalytics", () => ConfigRoles.IsSendAnalytics.Value = !ConfigRoles.IsSendAnalytics.Value, ConfigRoles.IsSendAnalytics.Value),
            new("IsLightAndDarker", () => ConfigRoles.IsLightAndDarker.Value = !ConfigRoles.IsLightAndDarker.Value, ConfigRoles.IsLightAndDarker.Value),
            // new("ReplayOptions", OpenReplayWindow, true),
            new("IsMuteLobbyBGM", () => ConfigRoles.IsMuteLobbyBGM.Value = !ConfigRoles.IsMuteLobbyBGM.Value, ConfigRoles.IsMuteLobbyBGM.Value),
            new("IsSaveLogWhenEndGame", () => ConfigRoles.IsSaveLogWhenEndGame.Value = !ConfigRoles.IsSaveLogWhenEndGame.Value, ConfigRoles.IsSaveLogWhenEndGame.Value),
            new(ProcessorAffinityMaskTitle, OnProcessorAffinityMaskClick, ConfigRoles._ProcessorAffinityMask.Value == 3),
    };
    private static bool OnProcessorAffinityMaskClick(SelectionBehaviour button)
    {
        ConfigRoles._ProcessorAffinityMask.Value = ConfigRoles._ProcessorAffinityMask.Value == (ulong)3 ? (ulong)1 : (ulong)3;
        SuperNewRolesPlugin.UpdateCPUProcessorAffinity();
        button.Title = ProcessorAffinityMaskTitle;
        return ConfigRoles._ProcessorAffinityMask.Value == (ulong)3;
    }
    private static string ProcessorAffinityMaskTitle
    {
        get
        {
            ulong mask = ConfigRoles._ProcessorAffinityMask.Value;
            string showCore = "1";
            if (mask == (ulong)1)
                showCore = "1";
            else if (mask == (ulong)3)
                showCore = "2";
            else
                showCore = "?";
            return $"{ModTranslation.GetString("ProcessorAffinityMask")} {showCore} {ModTranslation.GetString("ProcessorAffinityMaskCoreName")}";
        }
    }
    private static GameObject popUp;
    private static TextMeshPro titleText;

    private static ToggleButtonBehaviour moreOptions;
    private static List<ToggleButtonBehaviour> modButtons;
    private static TextMeshPro titleTextTitle;
    public static Vector3? _origin;

    public static ToggleButtonBehaviour buttonPrefab;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static void MainMenuManager_StartPostfix(MainMenuManager __instance)
    {
        // Prefab for the title
        var tmp = __instance.announcementPopUp.Title;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.transform.localPosition += Vector3.left * 0.2f;
        titleText = Object.Instantiate(tmp);
        Object.Destroy(titleText.GetComponent<TextTranslatorTMP>());
        titleText.gameObject.SetActive(false);
        Object.DontDestroyOnLoad(titleText);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
    {
        if (!__instance.CensorChatButton) return;

        if (!popUp)
        {
            CreateCustom(__instance);
        }

        if (!buttonPrefab)
        {
            buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
            Object.DontDestroyOnLoad(buttonPrefab);
            buttonPrefab.name = "CensorChatPrefab";
            buttonPrefab.gameObject.SetActive(false);
        }

        SetUpOptions();
        ReplaySetUpOptions();
        InitializeMoreButton(__instance);
    }

    private static void CreateCustom(OptionsMenuBehaviour prefab)
    {
        popUp = Object.Instantiate(prefab.gameObject);
        Object.DontDestroyOnLoad(popUp);
        var transform = popUp.transform;
        var pos = transform.localPosition;
        pos.z = -810f;
        transform.localPosition = pos;

        Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
        foreach (var gObj in popUp.gameObject.GetAllChilds())
        {
            if (gObj.name is not "Background" and not "CloseButton")
                Object.Destroy(gObj);
        }
        popUp.SetActive(false);

        ReplayPopup = Object.Instantiate(prefab.gameObject);
        Object.DontDestroyOnLoad(ReplayPopup);
        transform = ReplayPopup.transform;
        pos = transform.localPosition;
        pos.z = -816.51f;
        transform.localPosition = pos;

        Object.Destroy(ReplayPopup.GetComponent<OptionsMenuBehaviour>());
        foreach (var gObj in ReplayPopup.gameObject.GetAllChilds())
        {
            if (gObj.name is not "Background" and not "CloseButton")
                Object.Destroy(gObj);
        }
        ReplayPopup.SetActive(false);
    }

    private static void InitializeMoreButton(OptionsMenuBehaviour __instance)
    {
        moreOptions = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
        var transform = __instance.CensorChatButton.transform;
        _origin ??= transform.localPosition;
        transform.localPosition = _origin.Value + Vector3.left * 2.6f;
        moreOptions.transform.localPosition = _origin.Value + Vector3.right * 2.6f;
        var trans = moreOptions.transform.localPosition;
        moreOptions.gameObject.SetActive(true);
        trans = moreOptions.transform.position;
        moreOptions.Text.text = ModTranslation.GetString("modOptionsText");
        var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
        moreOptionsButton.OnClick = new ButtonClickedEvent();
        moreOptionsButton.OnClick.AddListener((Action)(() =>
        {
            if (!popUp) return;
            if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
            {
                popUp.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                popUp.transform.localPosition = new Vector3(0, 0, -920f);
            }
            else
            {
                popUp.transform.SetParent(null);
                Object.DontDestroyOnLoad(popUp);
            }
            CheckSetTitle();
            RefreshOpen();
        }));
    }

    private static GameObject ReplayPopup;
    private static List<ToggleButtonBehaviour> ReplayButtons;
    public static bool OpenReplayWindow()
    {
        popUp.gameObject.SetActive(false);
        ReplayPopup.gameObject.SetActive(true);
        Transform obj = GameObject.FindObjectOfType<OptionsMenuBehaviour>()?.transform;
        if (obj?.parent && obj?.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
        {
            ReplayPopup.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
            ReplayPopup.transform.localPosition = new Vector3(0, 0, -920f);
        }
        else
        {
            ReplayPopup.transform.SetParent(null);
            Object.DontDestroyOnLoad(ReplayPopup);
        }
        ReplaySetUpOptions();
        return true;
    }

    private static void RefreshOpen()
    {
        popUp.gameObject.SetActive(false);
        ReplayPopup.gameObject.SetActive(false);
        popUp.gameObject.SetActive(true);
        SetUpOptions();
        ReplaySetUpOptions();
    }

    private static void CheckSetTitle()
    {
        if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;

        var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
        title.gameObject.SetActive(true);
        title.text = ModTranslation.GetString("modOptionsText");
        title.name = "TitleText";
    }

    private static void SetUpOptions()
    {
        if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

        modButtons = new List<ToggleButtonBehaviour>();

        for (var i = 0; i < AllOptions.Length; i++)
        {
            var info = AllOptions[i];

            var button = Object.Instantiate(buttonPrefab, popUp.transform);
            var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 2.1f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = ModTranslation.GetString(info.Title);
            button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
            button.Text.font = Object.Instantiate(titleText.font);
            button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

            button.name = info.Title.Replace(" ", "") + "Toggle";
            button.gameObject.SetActive(true);

            var passiveButton = button.GetComponent<PassiveButton>();
            var colliderButton = button.GetComponent<BoxCollider2D>();

            colliderButton.size = new Vector2(2.2f, .7f);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOver = new UnityEvent();

            passiveButton.OnClick.AddListener((Action)(() =>
            {
                button.onState = info.OnClick();
                button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                button.Text.text = ModTranslation.GetString(info.Title);
            }));

            passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
            passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

            foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                spr.size = new Vector2(2.2f, .7f);
            modButtons.Add(button);
        }
    }
    public const float ReplayQualityLowTime = 0.75f;
    public const float ReplayQualityMediumTime = 0.5f;
    public const float ReplayQualityHighTime = 0.25f;
    public static void UpdateState(ToggleButtonBehaviour button, bool state)
    {
        button.onState = state;
        button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
    }
    public static List<SelectionBehaviour> ReplayOptions = new() { new SelectionBehaviour("リプレイを収録する",()=>{
        foreach (GameObject obj in ReplayEnableObjects.AsSpan())
        {
            obj.SetActive(!ConfigRoles.ReplayEnable.Value);
        }
        return ConfigRoles.ReplayEnable.Value = !ConfigRoles.ReplayEnable.Value; },ConfigRoles.ReplayEnable.Value,pos: new(0, 1.8f, -0.5f)),
        new SelectionBehaviour("ReplayOptionsQualityLow",()=>UpdateReplayQuality(ReplayQualityLowTime),ConfigRoles.ReplayQualityTime.Value == ReplayQualityLowTime,pos:new(-1.75f, -0.1f, -0.5f),scale:Vector3.one*0.75f),
        new SelectionBehaviour("ReplayOptionsQualityMedium",()=>UpdateReplayQuality(ReplayQualityMediumTime),ConfigRoles.ReplayQualityTime.Value == ReplayQualityMediumTime,pos:new(0, -0.1f, -0.5f),scale:Vector3.one*0.75f),
        new SelectionBehaviour("ReplayOptionsQualityHigh",()=>UpdateReplayQuality(ReplayQualityHighTime),ConfigRoles.ReplayQualityTime.Value == ReplayQualityHighTime,pos:new(1.75f, -0.1f, -0.5f),scale:Vector3.one*0.75f)};

    static bool UpdateReplayQuality(float timer)
    {
        ConfigRoles.ReplayQualityTime.Value = timer;
        UpdateState(ReplayOptions[1].Button, timer == ReplayQualityLowTime);
        UpdateState(ReplayOptions[2].Button, timer == ReplayQualityMediumTime);
        UpdateState(ReplayOptions[3].Button, timer == ReplayQualityHighTime);
        return true;
    }
    static List<GameObject> ReplayEnableObjects;
    public static void ReplaySetUpOptions()
    {
        if (ReplayPopup.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;
        ReplayPopup.name = "ReplayPopup";
        ReplayEnableObjects = new();

        ReplayButtons = new List<ToggleButtonBehaviour>();

        TextMeshPro ReplayTMPTemplate = buttonPrefab.Text;
        TextMeshPro ReplayQualityTitle = Object.Instantiate(ReplayTMPTemplate, ReplayPopup.transform);
        ReplayQualityTitle.name = "QualityTitle";
        ReplayQualityTitle.text = ModTranslation.GetString("ReplayOptionsQualityTitle");
        ReplayQualityTitle.transform.localScale = Vector3.one * 2;
        ReplayQualityTitle.transform.localPosition = new(0, 1.1f, -1);
        TextMeshPro ReplayQualityDescription = Object.Instantiate(ReplayTMPTemplate, ReplayPopup.transform);
        ReplayQualityDescription.name = "QualityDescription";
        ReplayQualityDescription.text = ModTranslation.GetString("ReplayOptionsQualityDescription");
        ReplayQualityDescription.transform.localPosition = new(0, 0.55f, -1);
        ReplayQualityDescription.rectTransform.sizeDelta = new(4, 0.6f);

        ReplayEnableObjects.Add(ReplayQualityTitle.gameObject);
        ReplayEnableObjects.Add(ReplayQualityDescription.gameObject);
        for (var i = 0; i < ReplayOptions.Count; i++)
        {
            var info = ReplayOptions[i];

            var button = Object.Instantiate(buttonPrefab, ReplayPopup.transform);
            var pos = info.pos.HasValue ? info.pos.Value : new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

            var transform = button.transform;
            transform.localPosition = pos;
            if (info.scale.HasValue)
                transform.localScale = info.scale.Value;

            button.onState = info.DefaultValue;
            button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

            button.Text.text = ModTranslation.GetString(info.Title);
            button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
            button.Text.font = Object.Instantiate(titleText.font);
            button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

            button.name = info.Title.Replace(" ", "") + "Toggle";
            button.gameObject.SetActive(true);

            var passiveButton = button.GetComponent<PassiveButton>();
            var colliderButton = button.GetComponent<BoxCollider2D>();

            colliderButton.size = new Vector2(2.2f, .7f);

            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnMouseOut = new UnityEvent();
            passiveButton.OnMouseOver = new UnityEvent();

            passiveButton.OnClick.AddListener((Action)(() =>
            {
                UpdateState(button, info.OnClick());
                button.Text.text = ModTranslation.GetString(info.Title);
            }));

            passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
            passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

            foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                spr.size = new Vector2(2.2f, .7f);
            info.Button = button;
            ReplayButtons.Add(button);
            if (i > 0)
                ReplayEnableObjects.Add(button.gameObject);
        }
        foreach (GameObject obj in ReplayEnableObjects.AsSpan())
        {
            obj.SetActive(ConfigRoles.ReplayEnable.Value);
        }
    }
    private static IEnumerable<GameObject> GetAllChilds(this GameObject Go)
    {
        for (var i = 0; i < Go.transform.childCount; i++)
        {
            yield return Go.transform.GetChild(i).gameObject;
        }
    }

    public static void updateTranslations()
    {
        if (titleTextTitle)
            titleTextTitle.text = ModTranslation.GetString("modOptionsText");

        if (moreOptions)
            moreOptions.Text.text = ModTranslation.GetString("modOptionsText");

        for (int i = 0; i < AllOptions.Length; i++)
        {
            if (i >= modButtons.Count) break;
            modButtons[i].Text.text = ModTranslation.GetString(AllOptions[i].Title);
        }
    }

    public class SelectionBehaviour
    {
        public string Title;
        public Func<bool> OnClick;
        public bool DefaultValue;
        public Vector3? pos;
        public Vector3? scale;
        public ToggleButtonBehaviour Button;

        public SelectionBehaviour(string title, Func<bool> onClick, bool defaultValue, Vector3? pos = null, Vector3? scale = null)
        {
            Title = title;
            OnClick = onClick;
            DefaultValue = defaultValue;
            this.pos = pos;
            this.scale = scale;
        }
        public SelectionBehaviour(string title, Func<SelectionBehaviour, bool> onClick, bool defaultValue, Vector3? pos = null, Vector3? scale = null)
        {
            Title = title;
            OnClick = () => onClick(this);
            DefaultValue = defaultValue;
            this.pos = pos;
            this.scale = scale;
        }
    }
}

[HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
public static class HiddenTextPatch
{
    private static void Postfix(TextBoxTMP __instance)
    {
        bool flag = ConfigRoles.StreamerMode.Value && (__instance.name == "GameIdText" || __instance.name == "IpTextBox" || __instance.name == "PortTextBox");
        if (flag) __instance.outputText.text = new string('*', __instance.text.Length);
    }
}