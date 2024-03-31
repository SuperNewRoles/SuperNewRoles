using System;
using System.Linq;
using Cpp2IL.Core.InstructionSets;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerParticles), nameof(PlayerParticles.Start))]
public class MainMenuStartPatcha
{
    private static void Postfix(PlayerParticles __instance)
    {
        //とりあえず僕の誕生日終わるまで出しとく
        if (DateTime.UtcNow < new DateTime(2023, 11, 4, 15, 0, 0) &&
            !AprilFoolsMode.ShouldHorseAround())
            return;
        foreach (var item in __instance.pool.activeChildren)
        {
            PlayerMaterial.SetColors(ModHelpers.GetRandomIndex(Palette.PlayerColors.ToList()), item.TryCast<PlayerParticle>().myRend);
        }
    }
}
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class MainMenuStartPatch
{
    private static void Prefix(MainMenuManager __instance)
    {
        var template = GameObject.Find("ExitGameButton");
        if (template == null) return;

        var buttonDiscord = UnityEngine.Object.Instantiate(template, null);
        GameObject.Destroy(buttonDiscord.GetComponent<AspectPosition>());
        buttonDiscord.transform.localPosition = new(1f, -2.04f, 0f);
        buttonDiscord.transform.localScale = Vector3.one * 2.3f;
        buttonDiscord.GetComponent<BoxCollider2D>().offset = new(0, 0.0377f);

        var textDiscord = buttonDiscord.GetComponentInChildren<TextMeshPro>();
        textDiscord.transform.localPosition = new(0, 0.035f, -2);
        textDiscord.GetComponent<AspectPosition>().anchorPoint = new(0.5162f, 0.5f);
        textDiscord.transform.localScale = Vector3.one * 1.25f;
        textDiscord.alignment = TextAlignmentOptions.Center;
        textDiscord.enableWordWrapping = false;
        textDiscord.GetComponent<TextTranslatorTMP>().enabled = false;
        textDiscord.SetText(ModTranslation.GetString("MainMenuDiscordText"));
        textDiscord.m_minFontSize = 1.15f;

        PassiveButton passiveButtonDiscord = buttonDiscord.GetComponent<PassiveButton>();
        SpriteRenderer buttonSpriteDiscord = buttonDiscord.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();

        passiveButtonDiscord.OnClick = new Button.ButtonClickedEvent();
        passiveButtonDiscord.OnClick.AddListener((System.Action)(() => Application.OpenURL(SuperNewRolesPlugin.DiscordServer)));

        Color discordColor = new Color32(88, 101, 242, byte.MaxValue);
        buttonSpriteDiscord.color = textDiscord.color = discordColor;
        passiveButtonDiscord.OnMouseOut.AddListener((System.Action)delegate
        {
            buttonSpriteDiscord.color = textDiscord.color = discordColor;
        });

        var buttonTwitter = GameObject.Instantiate(template, null);
        GameObject.Destroy(buttonTwitter.GetComponent<AspectPosition>());
        buttonTwitter.transform.localPosition = new(4.3f, -2, 0);
        buttonTwitter.GetComponent<BoxCollider2D>().offset = new(0, 0.0377f);

        var textTwitter = buttonTwitter.GetComponentInChildren<TextMeshPro>();
        textTwitter.transform.localPosition = new(0, 0.035f, -2);
        textTwitter.alignment = TextAlignmentOptions.Right;
        textTwitter.GetComponent<TextTranslatorTMP>().enabled = false;
        textTwitter.SetText("Twitter");

        PassiveButton passiveButtonTwitter = buttonTwitter.GetComponent<PassiveButton>();
        SpriteRenderer buttonSpriteTwitter = buttonTwitter.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();

        passiveButtonTwitter.OnClick = new Button.ButtonClickedEvent();

        Color TwitterColor = new Color32(29, 155, 240, byte.MaxValue);
        buttonSpriteTwitter.color = textTwitter.color = TwitterColor;
        passiveButtonTwitter.OnMouseOut.AddListener((System.Action)delegate
        {
            buttonSpriteTwitter.color = textTwitter.color = TwitterColor;
        });

        var buttonTwitterSNRDevs = GameObject.Instantiate(template, null);
        GameObject.Destroy(buttonTwitterSNRDevs.GetComponent<AspectPosition>());
        buttonTwitterSNRDevs.transform.localPosition = new(4.3f, -1.5f, 0);
        buttonTwitterSNRDevs.SetActive(false);
        buttonTwitterSNRDevs.GetComponent<BoxCollider2D>().offset = new(0, 0.0377f);

        var buttonTwitterSuperNewRoles = GameObject.Instantiate(template, null);
        GameObject.Destroy(buttonTwitterSuperNewRoles.GetComponent<AspectPosition>());
        buttonTwitterSuperNewRoles.transform.localPosition = new(4.3f, -1.1f, 0);
        buttonTwitterSuperNewRoles.SetActive(false);
        buttonTwitterSuperNewRoles.GetComponent<BoxCollider2D>().offset = new(0, 0.0377f);

        passiveButtonTwitter.OnClick.AddListener((System.Action)(() =>
        {
            if (buttonTwitterSNRDevs.active)
            {
                buttonTwitterSNRDevs.SetActive(false);
                buttonTwitterSuperNewRoles.SetActive(false);
            }
            else
            {
                buttonTwitterSNRDevs.SetActive(true);
                var textTwitterSNRDevs = buttonTwitterSNRDevs.GetComponentInChildren<TextMeshPro>();
                textTwitterSNRDevs.transform.localPosition = new(0, 0.035f, -2);
                textTwitterSNRDevs.alignment = TextAlignmentOptions.Right;
                textTwitterSNRDevs.GetComponent<TextTranslatorTMP>().enabled = false;
                textTwitterSNRDevs.SetText("SuperNewRolesの人");
                //buttonTwitterSNRDevs.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x + 1.1f, buttonTwitter.transform.localPosition.y + -0.3f, buttonTwitter.transform.localPosition.z);
                PassiveButton passivebuttonTwitterSNRDevs = buttonTwitterSNRDevs.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteTwitterSNRDevs = buttonTwitterSNRDevs.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
                passivebuttonTwitterSNRDevs.OnClick = new Button.ButtonClickedEvent();
                passivebuttonTwitterSNRDevs.OnClick.AddListener((System.Action)(() => Application.OpenURL(SuperNewRolesPlugin.Twitter1)));

                buttonTwitterSuperNewRoles.SetActive(true);
                var textTwitterSuperNewRoles = buttonTwitterSuperNewRoles.GetComponentInChildren<TextMeshPro>();
                textTwitterSuperNewRoles.transform.localPosition = new(0, 0.035f, -2);
                textTwitterSuperNewRoles.alignment = TextAlignmentOptions.Right;
                textTwitterSuperNewRoles.GetComponent<TextTranslatorTMP>().enabled = false;
                textTwitterSuperNewRoles.SetText("Super New Roles");
                //buttonTwitterSuperNewRoles.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x + 1.1f, buttonTwitter.transform.localPosition.y + 0.3f, buttonTwitter.transform.localPosition.z);
                PassiveButton passivebuttonTwitterSuperNewRoles = buttonTwitterSuperNewRoles.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteTwitterSuperNewRoles = buttonTwitterSuperNewRoles.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
                passivebuttonTwitterSuperNewRoles.OnClick = new Button.ButtonClickedEvent();
                passivebuttonTwitterSuperNewRoles.OnClick.AddListener((System.Action)(() => Application.OpenURL(SuperNewRolesPlugin.Twitter2)));
            }
        }));
    }
}