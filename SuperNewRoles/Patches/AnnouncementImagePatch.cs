using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnouncementText))]
    public static class AnnouncementPopUpUpdateAnnouncementTextPatch
    {
        public static void Postfix(AnnouncementPopUp __instance, int id, bool previewOnly)
        {
            if (__instance == null)
                return;

            var bodyText = __instance.AnnouncementBodyText;
            if (bodyText == null)
                return;

            var renderer = __instance.GetComponent<AnnouncementImageRenderer>();
            if (renderer == null)
                renderer = __instance.gameObject.AddComponent<AnnouncementImageRenderer>();

            renderer.Initialize(bodyText);
            renderer.ShowImages(id, previewOnly);
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.OnDisable))]
    public static class AnnouncementPopUpOnDisableImagePatch
    {
        public static void Postfix(AnnouncementPopUp __instance)
        {
            var renderer = __instance != null ? __instance.GetComponent<AnnouncementImageRenderer>() : null;
            if (renderer != null)
                renderer.ClearImages();
        }
    }

    [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.Update))]
    public static class AnnouncementPopUpUpdateImagePatch
    {
        public static void Postfix(AnnouncementPopUp __instance)
        {
            if (__instance == null)
                return;

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
    }
}
