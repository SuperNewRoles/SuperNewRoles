using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using SuperNewRoles.Patch;
using Newtonsoft.Json.Linq;
using SuperNewRoles.CustomCosmetics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Twitch;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch
    {
        public const string snrdiscordserver = "https://discord.gg/6DjxfaDsAj";
        private static void Prefix(MainMenuManager __instance)
        {
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var buttonDiscord = UnityEngine.Object.Instantiate(template, null);
            if (File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"))) buttonDiscord.transform.localPosition = new Vector3(buttonDiscord.transform.localPosition.x, buttonDiscord.transform.localPosition.y + 0.6f, buttonDiscord.transform.localPosition.z);
            else buttonDiscord.transform.localPosition = new Vector3(buttonDiscord.transform.localPosition.x, buttonDiscord.transform.localPosition.y + 1.2f, buttonDiscord.transform.localPosition.z);

            var textDiscord = buttonDiscord.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                textDiscord.SetText("Discord");
            })));

            PassiveButton passiveButtonDiscord = buttonDiscord.GetComponent<PassiveButton>();
            SpriteRenderer buttonSpriteDiscord = buttonDiscord.GetComponent<SpriteRenderer>();

            passiveButtonDiscord.OnClick = new Button.ButtonClickedEvent();
            passiveButtonDiscord.OnClick.AddListener((System.Action)(() => Application.OpenURL(snrdiscordserver)));

            Color discordColor = new Color32(88, 101, 242, byte.MaxValue);
            buttonSpriteDiscord.color = textDiscord.color = discordColor;
            passiveButtonDiscord.OnMouseOut.AddListener((System.Action)delegate
            {
                buttonSpriteDiscord.color = textDiscord.color = discordColor;
            });


            var buttonTwitter = UnityEngine.Object.Instantiate(template, null);
            if (File.Exists(Assembly.GetExecutingAssembly().Location.Replace("SuperNewRoles.dll", "Submerged.dll"))) buttonTwitter.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x, buttonTwitter.transform.localPosition.y + 1.2f, buttonTwitter.transform.localPosition.z);
            else buttonTwitter.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x, buttonTwitter.transform.localPosition.y + 1.8f, buttonTwitter.transform.localPosition.z);

            var textTwitter = buttonTwitter.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                textTwitter.SetText("Twitter");
            })));

            PassiveButton passiveButtonTwitter = buttonTwitter.GetComponent<PassiveButton>();
            SpriteRenderer buttonSpriteTwitter = buttonTwitter.GetComponent<SpriteRenderer>();

            passiveButtonTwitter.OnClick = new Button.ButtonClickedEvent();

            Color TwitterColor = new Color32(29, 155, 240, byte.MaxValue);
            buttonSpriteTwitter.color = textTwitter.color = TwitterColor;
            passiveButtonTwitter.OnMouseOut.AddListener((System.Action)delegate
            {
                buttonSpriteTwitter.color = textTwitter.color = TwitterColor;
            });


            var buttonTwitterSNRDevs = UnityEngine.Object.Instantiate(template, null);
            buttonTwitterSNRDevs.SetActive(false);

            var buttonTwitterSuperNewRoles = UnityEngine.Object.Instantiate(template, null);
            buttonTwitterSuperNewRoles.SetActive(false);

            passiveButtonTwitter.OnClick.AddListener((System.Action)(() =>
            {
                buttonTwitterSNRDevs.SetActive(true);
                var textTwitterSNRDevs = buttonTwitterSNRDevs.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    textTwitterSNRDevs.SetText("SuperNewRolesの人");
                })));
                buttonTwitterSNRDevs.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x + 1.1f, buttonTwitter.transform.localPosition.y + -0.3f, buttonTwitter.transform.localPosition.z);
                PassiveButton passivebuttonTwitterSNRDevs = buttonTwitterSNRDevs.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteTwitterSNRDevs = buttonTwitterSNRDevs.GetComponent<SpriteRenderer>();
                passivebuttonTwitterSNRDevs.OnClick = new Button.ButtonClickedEvent();
                passivebuttonTwitterSNRDevs.OnClick.AddListener((System.Action)(() => Application.OpenURL("https://twitter.com/SNRDevs")));

                buttonTwitterSuperNewRoles.SetActive(true);
                var textTwitterSuperNewRoles = buttonTwitterSuperNewRoles.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    textTwitterSuperNewRoles.SetText("Super New Roles");
                })));
                buttonTwitterSuperNewRoles.transform.localPosition = new Vector3(buttonTwitter.transform.localPosition.x + 1.1f, buttonTwitter.transform.localPosition.y + 0.3f, buttonTwitter.transform.localPosition.z);
                PassiveButton passivebuttonTwitterSuperNewRoles = buttonTwitterSuperNewRoles.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteTwitterSuperNewRoles = buttonTwitterSuperNewRoles.GetComponent<SpriteRenderer>();
                passivebuttonTwitterSuperNewRoles.OnClick = new Button.ButtonClickedEvent();
                passivebuttonTwitterSuperNewRoles.OnClick.AddListener((System.Action)(() => Application.OpenURL("https://twitter.com/SuperNewRoles")));                
            }));
        }
    }
}