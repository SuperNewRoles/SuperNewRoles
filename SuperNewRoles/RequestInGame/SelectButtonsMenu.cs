using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class SelectButtonsMenu
{
    public static SelectButtonsMenuCloseAnimation closeAnimator;
    public static void GenerateButtonsMenu()
    {
        Transform parent = DestroyableSingleton<HudManager>.InstanceExists ? DestroyableSingleton<HudManager>.Instance.transform : null;
        GameObject background = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BugReportUI_bg"), parent);
        background.transform.localPosition = new(0f, 0f, -100f);
        background.transform.localScale = Vector3.zero;

        PassiveButton closeButton = background.transform.Find("closeButton").gameObject.AddComponent<PassiveButton>();
        closeButton.Colliders = new Collider2D[] { closeButton.GetComponent<BoxCollider2D>() };
        closeButton.OnClick = new();
        closeButton.OnMouseOut = new();
        closeButton.OnMouseOver = new();

        PassiveButton returnButton = background.transform.Find("ReturnButton").gameObject.AddComponent<PassiveButton>();
        returnButton.Colliders = new Collider2D[] { returnButton.GetComponent<BoxCollider2D>() };
        returnButton.OnClick = new();
        returnButton.OnMouseOut = new();
        returnButton.OnMouseOver = new();

        PassiveButton backButton = background.transform.Find("back").gameObject.AddComponent<PassiveButton>();
        backButton.Colliders = new Collider2D[] { backButton.GetComponent<BoxCollider2D>() };
        backButton.OnClick = new();
        backButton.OnMouseOut = new();
        backButton.OnMouseOver = new();

        PassiveButton passiveforback = background.transform.Find("passiveforback").gameObject.AddComponent<PassiveButton>();
        passiveforback.Colliders = new Collider2D[] { passiveforback.GetComponent<BoxCollider2D>() };
        passiveforback.OnClick = new();
        passiveforback.OnMouseOut = new();
        passiveforback.OnMouseOver = new();

        closeAnimator = background.AddComponent<SelectButtonsMenuCloseAnimation>();
        closeAnimator.menu = background;
        closeButton.OnClick.AddListener((UnityAction)(() => closeAnimator.Close()));
        backButton.OnClick.AddListener((UnityAction)(() => closeAnimator.Close()));
        var openAnimator = background.AddComponent<SelectButtonsMenuOpenAnimation>();
        openAnimator.menu = background;
        openAnimator.targetScale = Vector3.one * 0.5f;
        openAnimator.onOpen = () =>
        {
            SetScreenMaskActive(false);
        };
        closeAnimator.onClose = () =>
        {
            SetScreenMaskActive(true);
        };
        openAnimator.Open();

        ActionOnEsc actionOnEsc = background.AddComponent<ActionOnEsc>();
        actionOnEsc.Init(() => closeAnimator.Close());
        ShowMainMenuUI(background);
    }

    public static void SetReturnButtonNonActive(GameObject background)
    {
        background.transform.Find("ReturnButton").gameObject.SetActive(false);
    }
    public static void SetReturnButtonActive(GameObject background, Action onClick)
    {
        background.transform.Find("ReturnButton").gameObject.SetActive(true);
        PassiveButton returnButton = background.transform.Find("ReturnButton").GetComponent<PassiveButton>();
        returnButton.OnClick = new();
        returnButton.OnClick.AddListener(onClick);
    }

    public static void ShowMainMenuUI(GameObject background)
    {
        GameObject top = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BugReport_Top"), background.transform);
        top.transform.localPosition = new(0f, -0.7f, -10f);
        top.transform.localScale = Vector3.one * 1f;
        UpdateTranslations(top);
        UpdateButtons(top);
        SetReturnButtonNonActive(background);
    }

    public static void UpdateTranslations(GameObject top)
    {
        top.transform.Find("DiscordMessage").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameDiscordText");
        top.transform.Find("Buttons/Button_MessageBox/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameMailBox");
        top.transform.Find("Texts/Text_01").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameUpperText");
        var buttons = top.transform.Find("Buttons");
        foreach (RequestInGameType requestInGameType in Enum.GetValues(typeof(RequestInGameType)))
        {
            GameObject button = buttons.transform.Find("Button_" + requestInGameType).gameObject;
            button.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameType." + requestInGameType);
        }
    }
    private static void SetScreenMaskActive(bool active)
    {
        GameObject.Find("MainMenuManager/MainUI/AspectScaler/RightPanel/ScreenMask")?.SetActive(active);
        if (MeetingHud.Instance != null)
            ModHelpers.UpdateMeetingHudMaskAreas(active);
        RoleOptionMenu.UpdateHostInfoMaskArea(active);
    }

    private static void UpdateButtons(GameObject top)
    {
        GameObject buttons = top.transform.Find("Buttons").gameObject;
        foreach (string buttonName in Enum.GetNames(typeof(RequestInGameType)))
        {
            GameObject button = buttons.transform.Find("Button_" + buttonName).gameObject;
            SetupButton(button, () => { ReportUIMenu.ShowReportUIMenu(top, (RequestInGameType)Enum.Parse(typeof(RequestInGameType), buttonName)); GameObject.Destroy(top); });
        }
        SetupButton(buttons.transform.Find("Button_Discord").gameObject, () => { Application.OpenURL(SocialLinks.DiscordServer); });
        SetupButton(buttons.transform.Find("Button_MessageBox").gameObject, () => { MessageListUI.ShowMessageListUI(top.transform.parent); GameObject.Destroy(top); });
    }
    private static void SetupButton(GameObject button, Action onClick)
    {
        PassiveButton passiveButton = button.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { passiveButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() => { button.transform.Find("Selected").gameObject.SetActive(false); }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() => { button.transform.Find("Selected").gameObject.SetActive(true); }));
        passiveButton.OnClick.AddListener(onClick);
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class KeyboardJoystickUpdatePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            // Overlayが非表示なら処理を省略する
            if (!DestroyableSingleton<HudManager>.InstanceExists || closeAnimator == null)
            {
                return;
            }

            // チャットがアクティブ、またはEsc, Tab, Hキーのいずれかが押された場合、
            // Overlayが表示されているなら非表示に切り替える
            bool isChatActive = FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening;
            if (isChatActive || FastDestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)
            {
                closeAnimator.Close();
            }
        }
    }
}