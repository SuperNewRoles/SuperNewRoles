using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class ReportUIMenu
{
    public static void ShowReportUIMenu(GameObject parent, RequestInGameType requestInGameType)
    {
        GameObject reportUIMenu = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("ReportUI"), parent.transform.parent);
        reportUIMenu.transform.localPosition = new(0f, 0f, -10f);
        GameObject Inner = reportUIMenu.transform.Find("Inner").gameObject;
        TextBoxTMP descriptionTextBox = Inner.transform.Find("InputBoxDescription").GetComponent<TextBoxTMP>();
        TextBoxTMP titleTextBox = Inner.transform.Find("InputBoxTitle").GetComponent<TextBoxTMP>();
        // Translation
        Inner.transform.Find("InputBoxDescription/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameSendDescriptionBack");
        Inner.transform.Find("InputBoxTitle/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameSendTitleBack");
        Inner.transform.Find("TextGrayTitle/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameSendTitleGray");
        Inner.transform.Find("TextGrayDescription/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameSendDescriptionGray");
        Inner.transform.Find("Button_Send/Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGameSendButton");
        GameObject agreement = Inner.transform.Find("AgreementText").gameObject;
        TextMeshPro agreementTMP = agreement.GetComponent<TextMeshPro>();
        agreementTMP.text = ModTranslation.GetString("RequestInGameAgreement");

        PassiveButton agreementButton = agreement.AddComponent<PassiveButton>();
        agreementButton.Colliders = new Collider2D[] { agreementButton.GetComponent<Collider2D>() };
        agreementButton.OnClick = new();
        agreementButton.OnClick.AddListener((UnityAction)(() =>
        {
            Application.OpenURL(SNRURLs.ReportInGameAgreement);
        }));
        agreementButton.OnMouseOver = new();
        agreementButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            agreementTMP.color = Color.green;
        }));
        agreementButton.OnMouseOut = new();
        agreementButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            agreementTMP.color = Color.white;
        }));

        ConfigureTextBox(descriptionTextBox);
        ConfigureTextBox(titleTextBox);

        GameObject Button_Send = Inner.transform.Find("Button_Send").gameObject;
        PassiveButton passiveButton = Button_Send.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { passiveButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (!ValidateReport(descriptionTextBox.text, titleTextBox.text, out string errorMessage))
            {
                Logger.Error($"Report validation failed: {errorMessage}");
            }
            else
            {
                Logger.Info($"Report sent: {titleTextBox.text} - {descriptionTextBox.text}");
                SendReport(reportUIMenu.transform, requestInGameType, descriptionTextBox.text, titleTextBox.text);
            }
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            Button_Send.transform.Find("Selected").gameObject.SetActive(true);
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            Button_Send.transform.Find("Selected").gameObject.SetActive(false);
        }));
    }
    private static bool ValidateReport(string description, string title, out string errorMessage)
    {
        if (string.IsNullOrEmpty(description))
        {
            errorMessage = "Description cannot be empty";
            return false;
        }
        if (string.IsNullOrEmpty(title))
        {
            errorMessage = "Title cannot be empty";
            return false;
        }
        if (title.Length <= 2)
        {
            errorMessage = "Title must be longer than 2 characters";
            return false;
        }
        if (description.Length <= 9)
        {
            errorMessage = "Description must be longer than 10 characters";
            return false;
        }
        errorMessage = null;
        return true;
    }
    private static void SendReport(Transform parent, RequestInGameType requestInGameType, string description, string title)
    {
        bool isActive = true;
        string text = ModTranslation.GetString("RequestInGameLoadingData");
        LoadingUI.ShowLoadingUI(parent, () => text, () => isActive);
        switch (requestInGameType)
        {
            case RequestInGameType.Bug:
                AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetOrCreateToken(token =>
                {
                    if (token == null)
                    {
                        Logger.Error($"Failed to get token");
                        isActive = false;
                    }
                    else
                    {
                        text = ModTranslation.GetString("RequestInGameSendingReportProgress", 0);
                        Dictionary<string, string> additionalInfo = new();
                        additionalInfo["version"] = "SNR:" + Statics.VersionString + "&AmongUs:" + Constants.GetBroadcastVersion();
                        additionalInfo["mode"] = Categories.ModeOption.ToString();
                        additionalInfo["log_compressed"] = LogCompression.CompressAndEncryptLog(SNRLogListener.Instance.logBuilder.ToString());

                        AmongUsClient.Instance.StartCoroutine(RequestInGameManager.SendReport(description, title, RequestInGameType.Bug.ToString(), additionalInfo, success =>
                        {
                            if (!success)
                            {
                                Logger.Error($"Failed to send report");
                            }
                            else
                            {
                                Logger.Info($"Report sent: {title} - {description}");
                                isActive = false;
                                new LateTask(() =>
                                {
                                    CreateSuccessUI(parent.parent);
                                    GameObject.Destroy(parent.gameObject);
                                }, 0f);
                            }
                        }, progress =>
                        {
                            text = ModTranslation.GetString("RequestInGameSendingReportProgress", progress);
                        }).WrapToIl2Cpp());
                    }
                }).WrapToIl2Cpp());
                break;
            default:
                AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetOrCreateToken(token =>
                {
                    if (token == null)
                    {
                        Logger.Error($"Failed to get token");
                        isActive = false;
                    }
                    else
                    {
                        text = ModTranslation.GetString("RequestInGameSendingReportProgress", 0);
                        Dictionary<string, string> additionalInfo = new();
                        AmongUsClient.Instance.StartCoroutine(RequestInGameManager.SendReport(description, title, requestInGameType.ToString(), additionalInfo, success =>
                        {
                            if (!success)
                            {
                                Logger.Error($"Failed to send report");
                            }
                            else
                            {
                                Logger.Info($"Report sent: {title} - {description}");
                                isActive = false;
                                new LateTask(() =>
                                {
                                    CreateSuccessUI(parent.parent);
                                    GameObject.Destroy(parent.gameObject);
                                }, 0f);
                            }
                        }, progress =>
                        {
                            text = ModTranslation.GetString("RequestInGameSendingReportProgress", progress);
                        }).WrapToIl2Cpp());
                    }
                }).WrapToIl2Cpp());
                break;
        }
    }
    private static void CreateSuccessUI(Transform parent)
    {
        GameObject successUI = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("SuccessReport"), parent);
        successUI.transform.localPosition = new(0f, 0f, -10f);
        successUI.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGame_SuccessReport");
        successUI.transform.Find("Description").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGame_SuccessReportText");
        GameObject returnButton = successUI.transform.Find("ReturnButton").gameObject;
        returnButton.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGame_SuccessReportReturnButton");
        PassiveButton passiveButton = returnButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { passiveButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            SelectButtonsMenu.ShowMainMenuUI(parent.gameObject);
            GameObject.Destroy(successUI);
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            returnButton.transform.Find("Selected").gameObject.SetActive(true);
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            returnButton.transform.Find("Selected").gameObject.SetActive(false);
        }));
    }
    public static void ConfigureTextBox(TextBoxTMP textBox)
    {
        PassiveButton passiveButton = textBox.gameObject.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { textBox.GetComponent<BoxCollider2D>() };
        string defaultText = textBox.outputText.text;

        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            textBox.GiveFocus();
            if (string.IsNullOrEmpty(textBox.text))
                textBox.outputText.text = "";
        }));

        passiveButton.OnMouseOver = new UnityEvent();
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!textBox.hasFocus)
                textBox.Background.color = Color.green;
        }));

        passiveButton.OnMouseOut = new UnityEvent();
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (!textBox.hasFocus)
                textBox.Background.color = Color.white;
        }));

        textBox.OnFocusLost = new();
        textBox.OnFocusLost.AddListener((UnityAction)(() =>
        {
            if (string.IsNullOrEmpty(textBox.text))
                textBox.outputText.text = defaultText;
        }));
    }
}
