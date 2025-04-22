using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class SelectButtonsMenu
{
    public static void GenerateButtonsMenu()
    {
        Logger.Info("a");
        GameObject background = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BugReportUI_bg"), null);
        background.transform.localPosition = new(0f, 0f, -10f);
        background.transform.localScale = Vector3.zero;
        PassiveButton closeButton = background.transform.Find("closeButton").gameObject.AddComponent<PassiveButton>();
        closeButton.Colliders = new Collider2D[] { closeButton.GetComponent<BoxCollider2D>() };
        closeButton.OnClick = new();
        closeButton.OnMouseOut = new();
        closeButton.OnMouseOver = new();
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
        var closeAnimator = background.AddComponent<SelectButtonsMenuCloseAnimation>();
        closeAnimator.menu = background;
        closeButton.OnClick.AddListener((UnityAction)(() => closeAnimator.Close()));
        backButton.OnClick.AddListener((UnityAction)(() => closeAnimator.Close()));
        GameObject top = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("BugReport_Top"), background.transform);
        top.transform.localPosition = new(0f, -0.7f, -10f);
        top.transform.localScale = Vector3.one * 1f;
        UpdateTranslations(top);
        var openAnimator = background.AddComponent<SelectButtonsMenuOpenAnimation>();
        openAnimator.menu = background;
        openAnimator.targetScale = Vector3.one * 0.5f;
        openAnimator.Open();
        UpdateButtons(top);
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
    private static void UpdateButtons(GameObject top)
    {
        GameObject buttons = top.transform.Find("Buttons").gameObject;
        foreach (string buttonName in Enum.GetNames(typeof(RequestInGameType)))
        {
            GameObject button = buttons.transform.Find("Button_" + buttonName).gameObject;
            SetupButton(button, () => { GameObject.Instantiate(AssetManager.GetAsset<GameObject>("ReportUI"), top.transform.parent).transform.localPosition = new(0f, 0f, -10f); GameObject.Destroy(top); });
        }
        SetupButton(buttons.transform.Find("Button_Discord").gameObject, () => { Application.OpenURL("https://supernewroles.com/discord"); });
        SetupButton(buttons.transform.Find("Button_MessageBox").gameObject, () => { Logger.Info("clicked!:MessageBox"); });
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
}