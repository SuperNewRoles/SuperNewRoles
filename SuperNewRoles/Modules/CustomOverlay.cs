using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    [Harmony]
    public class CustomOverlays
    {
        public static Sprite helpButton;
        private static Sprite colorBG;
        private static SpriteRenderer meetingUnderlay;
        private static SpriteRenderer infoUnderlay;
        private static TMPro.TextMeshPro infoOverlayRules;
        private static TMPro.TextMeshPro infoOverlayRoles;
        public static bool overlayShown = false;

        public static void ResetOverlays()
        {
            HideBlackBG();
            HideInfoOverlay();
            UnityEngine.Object.Destroy(meetingUnderlay);
            UnityEngine.Object.Destroy(infoUnderlay);
            UnityEngine.Object.Destroy(infoOverlayRules);
            UnityEngine.Object.Destroy(infoOverlayRoles);
            meetingUnderlay = infoUnderlay = null;
            infoOverlayRules = infoOverlayRoles = null;
            overlayShown = false;
        }

        public static bool InitializeOverlays()
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (helpButton == null)
            {
                helpButton = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HelpButton.png", 115f);
            }

            if (colorBG == null)
            {
                colorBG = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.White.png", 100f);
            }

            if (meetingUnderlay == null)
            {
                meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
            }

            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
            }

            if (infoOverlayRules == null)
            {
                infoOverlayRules = UnityEngine.Object.Instantiate(hudManager.TaskText, hudManager.transform);
                infoOverlayRules.fontSize = infoOverlayRules.fontSizeMin = infoOverlayRules.fontSizeMax = 1.15f;
                infoOverlayRules.autoSizeTextContainer = false;
                infoOverlayRules.enableWordWrapping = false;
                infoOverlayRules.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayRules.transform.position = Vector3.zero;
                infoOverlayRules.transform.localPosition = new Vector3(-2.5f, 1.15f, -910f);
                infoOverlayRules.transform.localScale = Vector3.one;
                infoOverlayRules.color = Palette.White;
                infoOverlayRules.enabled = false;
            }

            if (infoOverlayRoles == null)
            {
                infoOverlayRoles = UnityEngine.Object.Instantiate(infoOverlayRules, hudManager.transform);
                infoOverlayRoles.maxVisibleLines = 28;
                infoOverlayRoles.fontSize = infoOverlayRoles.fontSizeMin = infoOverlayRoles.fontSizeMax = 1.15f;
                infoOverlayRoles.outlineWidth += 0.02f;
                infoOverlayRoles.autoSizeTextContainer = false;
                infoOverlayRoles.enableWordWrapping = false;
                infoOverlayRoles.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayRoles.transform.position = Vector3.zero;
                infoOverlayRoles.transform.localPosition = infoOverlayRules.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);
                infoOverlayRoles.transform.localScale = Vector3.one;
                infoOverlayRoles.color = Palette.White;
                infoOverlayRoles.enabled = false;
            }
            return true;
        }

        public static void ShowBlackBG()
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null) return;
            if (!InitializeOverlays()) return;

            meetingUnderlay.sprite = colorBG;
            meetingUnderlay.enabled = true;
            meetingUnderlay.transform.localScale = new Vector3(20f, 20f, 1f);
            var clearBlack = new Color32(0, 0, 0, 0);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                meetingUnderlay.color = Color.Lerp(clearBlack, Palette.Black, t);
            })));
        }

        public static void HideBlackBG()
        {
            if (meetingUnderlay == null) return;
            meetingUnderlay.enabled = false;
        }

        public static void ShowInfoOverlay()
        {
            if (overlayShown) return;

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if ((MapUtilities.CachedShipStatus == null || PlayerControl.LocalPlayer == null || hudManager == null || FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || PlayerControl.LocalPlayer.CanMove) && MeetingHud.Instance != null)
                return;

            if (!InitializeOverlays()) return;

            if (MapBehaviour.Instance != null)
                MapBehaviour.Instance.Close();

            hudManager.SetHudActive(false);

            overlayShown = true;

            Transform parent = MeetingHud.Instance != null ? MeetingHud.Instance.transform : hudManager.transform;
            infoUnderlay.transform.parent = parent;
            infoOverlayRules.transform.parent = parent;
            infoOverlayRoles.transform.parent = parent;

            infoUnderlay.sprite = colorBG;
            infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
            infoUnderlay.enabled = true;

            SuperNewRolesPlugin.optionsPage = 0;
            GameOptionsData o = PlayerControl.GameOptions;
            List<string> gameOptions = o.ToString().Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList().GetRange(2, 17);
            string text = "";
            text = GameOptionsDataPatch.ResultData();
            infoOverlayRules.text = text;
            infoOverlayRules.enabled = true;

            string rolesText = "";

            infoOverlayRoles.text = rolesText;
            infoOverlayRoles.enabled = true;

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                infoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                infoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        public static void HideInfoOverlay()
        {
            if (!overlayShown) return;

            if (MeetingHud.Instance == null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

            overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (infoUnderlay != null)
                {
                    infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) infoUnderlay.enabled = false;
                }

                if (infoOverlayRules != null)
                {
                    infoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayRules.enabled = false;
                }

                if (infoOverlayRoles != null)
                {
                    infoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayRoles.enabled = false;
                }
            })));
        }

        public static void YoggleInfoOverlay()
        {
            if (overlayShown)
                HideInfoOverlay();
            else
                ShowInfoOverlay();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class CustomOverlayKeybinds
        {
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (HudManager.Instance.Chat.IsOpen && overlayShown)
                    HideInfoOverlay();
                if (Input.GetKeyDown(KeyCode.H) && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    YoggleInfoOverlay();
                }
            }
        }
    }
}