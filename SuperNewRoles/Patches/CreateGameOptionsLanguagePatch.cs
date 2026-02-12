using System.Net.Mime;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
public class CreateGameOptionsStartPatch
{
    private static TextMeshPro[] TextMeshPros;
    private static TextMeshPro SetupClassicText(Transform classicOption, string stateName, bool hideCheckmark, GameObject languageOption)
    {
        var state = classicOption.Find(stateName);
        if (state == null)
        {
            Logger.Error(stateName + " not found.");
            Rollback(languageOption);
            return null;
        }

        if (hideCheckmark)
            state.Find("Checkmark")?.gameObject.SetActive(false);

        var classicText = state.Find("ClassicText");
        if (classicText == null)
        {
            Logger.Error("ClassicText not found.");
            Rollback(languageOption);
            return null;
        }

        var classicTextTTT = classicText.GetComponent<TextTranslatorTMP>();
        if (classicTextTTT != null)
            GameObject.Destroy(classicTextTTT);

        return classicText.GetComponent<TextMeshPro>();
    }
    public static void UpdateLanguage()
    {
        // 現在の言語を取得
        var currentLang = ModHelpers.GetCurrentLanguageName();
        if (TextMeshPros == null)
        {
            Logger.Error("TextMeshPros is null.");
            return;
        }
        foreach (var tmp in TextMeshPros)
        {
            if (tmp == null)
                continue;
            tmp.text = currentLang;
        }
    }
    private static void Rollback(GameObject languageOption)
    {
        GameObject.Destroy(languageOption);
    }
    public static void Postfix(CreateGameOptions __instance)
    {
        // GeneralTabを取得
        var general = __instance.transform.Find("ParentContent/Content/GeneralTab");
        if (general == null)
        {
            Logger.Error("GeneralTab not found.");
            return;
        }

        // 言語用の表示枠をモード設定を複製して作成
        var modeOptions = general.transform.Find("ModeOptions");
        var languageOption = GameObject.Instantiate(modeOptions, modeOptions.transform.parent);
        var languageText = languageOption.transform.Find("BlackSquare/ModeText");
        // テキストを言語に変更
        var languageTextTTT = languageText.GetComponent<TextTranslatorTMP>();
        languageTextTTT.TargetText = StringNames.SettingsLanguage;
        languageTextTTT.ResetText();

        // 不要なボタンを削除
        List<GameObject> destroyObjects = new();
        for (int i = 0; i < languageOption.childCount; i++)
        {
            var child = languageOption.GetChild(i);
            if (child.name != "ClassicOption" && child.name != "BlackSquare")
                destroyObjects.Add(child.gameObject);
        }
        foreach (var obj in destroyObjects)
        {
            Object.Destroy(obj);
        }

        var classicOption = languageOption.Find("ClassicOption");
        if (classicOption == null)
        {
            Logger.Error("ClassicOption not found.");
            Rollback(languageOption.gameObject);
            return;
        }
        string[] buttons = ["SelectedInactive", "SelectedHighlight", "Inactive", "Highlight"];
        TextMeshPros = new TextMeshPro[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            TextMeshPros[i] = SetupClassicText(classicOption, buttons[i], hideCheckmark: true, languageOption.gameObject);
            classicOption.transform.Find(buttons[i])?.gameObject.SetActive(false);
        }
        classicOption.transform.Find("Inactive")?.gameObject.SetActive(true);

        // ボタンの動作を上書き
        var classicButton = classicOption.GetComponent<PassiveButton>();
        classicButton.OnClick = new();
        classicButton.OnClick.AddListener((UnityAction)(() =>
        {
            __instance.OpenTab(isGeneral: false);
        }));
        classicButton.OnMouseOver = new();
        classicButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            __instance.SetLangTooltip(true);
        }));
        classicButton.OnMouseOut = new();
        classicButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            __instance.SetLangTooltip(false);
        }));

        classicButton.ReceiveMouseOut();

        // 位置を上書き
        languageOption.transform.localPosition = new Vector3(1.61f, -3.445f, 0f);

        UpdateLanguage();
    }
}
[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.OpenTab))]
public static class CreateGameOptionsOpenTabPatch
{
    public static void Postfix(bool isGeneral)
    {
        if (isGeneral)
            CreateGameOptionsStartPatch.UpdateLanguage();
    }
}
