using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch]
    public static class VanillaOptionsPatch
    {
        private static GameObject popUp;
        private static TextMeshPro titleText;

        private static ToggleButtonBehaviour moreOptions;
        private static List<ToggleButtonBehaviour> modButtons;
        private static TextMeshPro titleTextTitle;

        public static ToggleButtonBehaviour buttonPrefab;
        public static Vector3? _origin;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManager_StartPostfix(MainMenuManager __instance)
        {
            // Prefab for the title
            var tmp = __instance.Announcement.transform.Find("Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            titleText = Object.Instantiate(tmp);
            Object.Destroy(titleText.GetComponent<TextTranslatorTMP>());
            titleText.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(titleText);
        }
        public static Vector3? origin;
        public static float xOffset = 1.75f;
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Update))]
        class OptionsUpdate
        {
            public static void Postfix(OptionsMenuBehaviour __instance)
            {
                if (__instance.CensorChatButton != null) __instance.CensorChatButton.gameObject.SetActive(false);
                if (__instance.EnableFriendInvitesButton != null) __instance.EnableFriendInvitesButton.gameObject.SetActive(false);
                if (__instance.StreamerModeButton != null) __instance.StreamerModeButton.gameObject.SetActive(false);
                if (__instance.ColorBlindButton != null) __instance.ColorBlindButton.gameObject.SetActive(false);
            }
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

            SetUpOptions(__instance);
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
                if (gObj.name != "Background" && gObj.name != "CloseButton")
                    Object.Destroy(gObj);
            }
            popUp.SetActive(false);
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
            moreOptions.Text.text = ModTranslation.GetString("vanillaOptionsText");
            var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action)(() =>
            {
                if (!popUp) return;
                if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform)
                {
                    popUp.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    popUp.transform.localPosition = new Vector3(0, 0, -800f);
                }
                else
                {
                    popUp.transform.SetParent(null);
                    Object.DontDestroyOnLoad(popUp);
                }
                CheckSetTitle();
                RefreshOpen(__instance);
            }));
        }

        private static void RefreshOpen(OptionsMenuBehaviour __instance)
        {
            popUp.gameObject.SetActive(false);
            popUp.gameObject.SetActive(true);
            SetUpOptions(__instance);
        }

        private static void CheckSetTitle()
        {
            if (!popUp || popUp.GetComponentInChildren<TextMeshPro>() || !titleText) return;

            var title = titleTextTitle = Object.Instantiate(titleText, popUp.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.text = ModTranslation.GetString("vanillaOptionsText");
            title.name = "TitleText";
        }

        private static void SetUpOptions(OptionsMenuBehaviour __instance)
        {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) return;

            modButtons = new List<ToggleButtonBehaviour>();
            for (var i = 0; i < 4; i++)
            {
                ToggleButtonBehaviour mainbutton = null;
                switch (i)
                {
                    case 0:
                        mainbutton = __instance.CensorChatButton;
                        break;
                    case 1:
                        mainbutton = __instance.EnableFriendInvitesButton;
                        break;
                    case 2:
                        mainbutton = __instance.StreamerModeButton;
                        break;
                    case 3:
                        mainbutton = __instance.ColorBlindButton;
                        break;
                }
                var button = Object.Instantiate(buttonPrefab, popUp.transform);
                var pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 1.3f - i / 2 * 0.8f, -.5f);

                var transform = button.transform;
                transform.localPosition = pos;

                button.onState = mainbutton.onState;
                button.Background.color = mainbutton.onState ? Color.green : Palette.ImpostorRed;
                try
                {
                    switch (i)
                    {
                        case 0:
                            button.Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsCensorChat);
                            break;
                        case 1:
                            button.Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsEnableFriendInvites);
                            break;
                        case 2:
                            button.Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsStreamerMode);
                            break;
                        case 3:
                            button.Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsColorblind);
                            break;
                    }
                }
                catch
                {
                    switch (i)
                    {
                        case 0:
                            button.Text.text = __instance.CensorChatButton.Text.text;
                            break;
                        case 1:
                            button.Text.text = __instance.EnableFriendInvitesButton.Text.text;
                            break;
                        case 2:
                            button.Text.text = __instance.StreamerModeButton.Text.text;
                            break;
                        case 3:
                            button.Text.text = __instance.ColorBlindButton.Text.text;
                            break;
                    }
                }
                button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
                button.Text.font = Object.Instantiate(titleText.font);
                button.Text.GetComponent<RectTransform>().sizeDelta = new Vector2(2, 2);

                button.name = mainbutton.name;
                button.gameObject.SetActive(true);

                var passiveButton = button.GetComponent<PassiveButton>();
                var colliderButton = button.GetComponent<BoxCollider2D>();

                colliderButton.size = new Vector2(2.2f, .7f);

                passiveButton.OnClick = mainbutton.GetComponent<PassiveButton>().OnClick;
                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    button.onState = !button.onState;
                    button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
                }));
                passiveButton.OnMouseOver = mainbutton.GetComponent<PassiveButton>().OnMouseOver;
                passiveButton.OnMouseOut = mainbutton.GetComponent<PassiveButton>().OnMouseOut;

                passiveButton.OnMouseOver.AddListener((Action)(() => button.Background.color = new Color32(34, 139, 34, byte.MaxValue)));
                passiveButton.OnMouseOut.AddListener((Action)(() => button.Background.color = button.onState ? Color.green : Palette.ImpostorRed));

                foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
                    spr.size = new Vector2(2.2f, .7f);
                modButtons.Add(button);
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
                titleTextTitle.text = ModTranslation.GetString("vanillaOptionsText");

            if (moreOptions)
                moreOptions.Text.text = ModTranslation.GetString("vanillaOptionsText");
            try
            {
                modButtons[0].Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsCensorChat);
                modButtons[1].Text.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsEnableFriendInvites);
            }
            catch { }
        }
    }
}