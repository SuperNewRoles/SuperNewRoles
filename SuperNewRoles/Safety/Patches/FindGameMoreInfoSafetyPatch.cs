using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Safety.Patches;

[HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.HandleList))]
public static class FindAGameManagerHandleListSafetyPatch
{
    public static void Postfix(FindAGameManager __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled() || __instance == null || !__instance.isActiveAndEnabled)
            return;
        __instance.StartCoroutine(SafetyBlockedOccupantCache.Refresh().WrapToIl2Cpp());
    }
}

[HarmonyPatch(typeof(FindGameMoreInfoPopup), nameof(FindGameMoreInfoPopup.SetupInfo))]
public static class FindGameMoreInfoPopupSetupInfoPatch
{
    private const string NoticeName = "BlockedPlayerNotice";

    public static void Postfix(FindGameMoreInfoPopup __instance)
    {
        if (__instance == null)
            return;
        if (!OfficialSnrServer.IsIdentityEnabled())
        {
            SetNotice(__instance, null);
            return;
        }

        GameListing listing = __instance.gameListing;
        string code = GameCode.IntToGameName(listing.GameId);
        ApplyCached(__instance, listing, code);
        __instance.StartCoroutine(CoRefresh(__instance, listing.GameId, code).WrapToIl2Cpp());
    }

    private static IEnumerator CoRefresh(FindGameMoreInfoPopup popup, int gameId, string code)
    {
        bool miss = !SafetyBlockedOccupantCache.TryGet(code, out _);
        yield return SafetyBlockedOccupantCache.Refresh(force: miss).WrapToIl2Cpp();
        if (popup == null || popup.gameListing.GameId != gameId)
            yield break;
        ApplyCached(popup, popup.gameListing, code);
    }

    private static void ApplyCached(FindGameMoreInfoPopup popup, GameListing listing, string code)
    {
        if (SafetyBlockedOccupantCache.TryGet(code, out PublicGameRow row))
        {
            if (row.HasBlockedPlayer)
                SetNotice(popup, SafetyBlockedOccupantNotice.Format(row.BlockedPlayerNames));
            else
                SetNotice(popup, null);
            return;
        }

        if (SafetyBlockedOccupantNotice.HostNameLooksWarned(listing.HostName) ||
            SafetyBlockedOccupantNotice.HostNameLooksWarned(listing.TrueHostName))
        {
            SetNotice(popup, SafetyBlockedOccupantNotice.Format(null));
            return;
        }

        SetNotice(popup, null);
    }

    private static void SetNotice(FindGameMoreInfoPopup popup, string text)
    {
        TextMeshPro notice = EnsureNotice(popup);
        if (notice == null)
            return;
        bool show = !string.IsNullOrEmpty(text);
        notice.gameObject.SetActive(show);
        if (show)
            notice.text = text;
    }

    private static TextMeshPro EnsureNotice(FindGameMoreInfoPopup popup)
    {
        TextMeshPro template = popup.languageText != null ? popup.languageText : popup.tagText;
        if (template == null)
            template = popup.capacity;
        if (template == null)
            return null;

        Transform parent = popup.transform;
        Transform existing = parent.Find(NoticeName);
        if (existing == null && template.transform.parent != null && template.transform.parent != parent)
            existing = template.transform.parent.Find(NoticeName);

        TextMeshPro notice;
        if (existing != null)
        {
            notice = existing.GetComponent<TextMeshPro>();
            if (existing.parent != parent)
                existing.SetParent(parent, false);
        }
        else
        {
            notice = Object.Instantiate(template, parent);
            notice.name = NoticeName;
            notice.color = new Color(1f, 0.82f, 0.2f);
            notice.enableWordWrapping = true;
            notice.overflowMode = TextOverflowModes.Overflow;
            notice.alignment = TextAlignmentOptions.Center;
        }

        notice.transform.localPosition = new Vector3(0.9f, -0.88f, -0.1f);
        return notice;
    }
}
