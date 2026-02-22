using System;
using System.Text.RegularExpressions;
using AmongUs.Data;
using HarmonyLib;
using TMPro;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnouncementText))]
    public static class AnnouncementPopUpUpdateAnnouncementTextPatch
    {
        public static void Postfix(AnnouncementPopUp __instance, int id, bool previewOnly)
        {
            try
            {
                if (!SuperNewRolesPlugin.IsAnnouncementImageSupported)
                    return;

                if (__instance == null)
                    return;

                var bodyText = __instance.AnnouncementBodyText;
                if (bodyText == null)
                    return;

                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(AnnouncementImageRenderer)))
                {
                    SuperNewRolesPlugin.DisableAnnouncementImageSupport("Announcement image renderer type is not registered in Il2Cpp.");
                    return;
                }

                var announcements = DataManager.Player?.Announcements?.AllAnnouncements;
                if (announcements != null)
                {
                    for (int i = 0; i < announcements.Count; i++)
                    {
                        var announcement = announcements[i];
                        if (announcement != null && announcement.Number == id)
                        {
                            AnnouncementImageCache.SetAnnouncementId(id, announcement.Id);
                            AnnouncementImageCache.EnsureImages(id, announcement.Id);
                            break;
                        }
                    }
                }

                var renderer = __instance.GetComponent<AnnouncementImageRenderer>();
                if (renderer == null)
                    renderer = __instance.gameObject.AddComponent<AnnouncementImageRenderer>();

                renderer.Initialize(bodyText);
                renderer.ShowImages(id, previewOnly);
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpUpdateAnnouncementTextPatch failed: {ex}");
                SuperNewRolesPlugin.DisableAnnouncementImageSupport(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.OnDisable))]
    public static class AnnouncementPopUpOnDisableImagePatch
    {
        public static void Postfix(AnnouncementPopUp __instance)
        {
            try
            {
                if (!SuperNewRolesPlugin.IsAnnouncementImageSupported)
                    return;

                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(AnnouncementImageRenderer)))
                {
                    SuperNewRolesPlugin.DisableAnnouncementImageSupport("Announcement image renderer type is not registered in Il2Cpp.");
                    return;
                }

                var renderer = __instance != null ? __instance.GetComponent<AnnouncementImageRenderer>() : null;
                if (renderer != null)
                    renderer.ClearImages();
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpOnDisableImagePatch failed: {ex}");
                SuperNewRolesPlugin.DisableAnnouncementImageSupport(ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Update))]
    public static class AnnouncementPopUpUpdateImagePatch
    {
        private static float _timestampUpdateTimer;
        private static readonly Regex DynamicTimestampRegex = new(
            @"<size=0><alpha=#00><t-dynamic:(\d+):R></alpha></size><size=100%><alpha=#FF>(.*?)<size=0><alpha=#00><t-end:R></alpha></size><size=100%><alpha=#FF>",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public static void Postfix(AnnouncementPopUp __instance)
        {
            try
            {
                if (!SuperNewRolesPlugin.IsAnnouncementImageSupported)
                    return;

                if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(AnnouncementImageRenderer)))
                {
                    SuperNewRolesPlugin.DisableAnnouncementImageSupport("Announcement image renderer type is not registered in Il2Cpp.");
                    return;
                }

                if (__instance == null)
                    return;

                // 1. タイムスタンプの更新 (1秒おきに実行)
                _timestampUpdateTimer -= Time.deltaTime;
                if (_timestampUpdateTimer <= 0f)
                {
                    _timestampUpdateTimer = 1f;
                    UpdateDynamicTimestamps(__instance);
                }

                var renderer = __instance.GetComponent<AnnouncementImageRenderer>();
                if (renderer == null || !renderer.HasImages)
                    return;

                var scroller = __instance.TextScroller;
                if (scroller == null)
                    return;

                float extra = renderer.GetExtraScrollHeight();
                if (extra <= 0f)
                    return;

                float textHeight = renderer.GetTextHeight();
                scroller.SetBoundsMax(textHeight + extra, 0f);
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogWarning($"AnnouncementPopUpUpdateImagePatch failed: {ex}");
                SuperNewRolesPlugin.DisableAnnouncementImageSupport(ex.Message);
            }
        }

        private static void UpdateDynamicTimestamps(AnnouncementPopUp popup)
        {
            if (popup.AnnouncementBodyText == null) return;
            string text = popup.AnnouncementBodyText.text;
            if (string.IsNullOrEmpty(text) || !text.Contains("<t-dynamic:")) return;

            bool changed = false;
            // <alpha=#00><t-dynamic:UNIX:R></alpha><alpha=#FF>{text}<alpha=#00><t-end:R></alpha><alpha=#FF>
            string newText = DynamicTimestampRegex.Replace(text, match =>
            {
                if (long.TryParse(match.Groups[1].Value, out long unixSeconds))
                {
                    string oldRelative = match.Groups[2].Value;
                    string newRelative = MarkdownToUnityTag.FormatUnixTimestamp(unixSeconds, "R");
                    if (oldRelative != newRelative)
                    {
                        changed = true;
                        return $"<size=0><alpha=#00><t-dynamic:{unixSeconds}:R></alpha></size><size=100%><alpha=#FF>{newRelative}<size=0><alpha=#00><t-end:R></alpha></size><size=100%><alpha=#FF>";
                    }
                }
                return match.Value;
            });

            if (changed)
            {
                popup.AnnouncementBodyText.text = newText;
            }
        }
    }
}
