using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.InnerNet;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Safety.Patches;

internal static class SnrLobbyReportContext
{
    public static string Name = string.Empty;
    public static int ClientId = -1;
    public static int GameId;
    public static string PublicId = string.Empty;
    public static string SessionId = string.Empty;
    public static string RegionId = string.Empty;

    public static void Capture(string name, int clientId = -1, int gameId = 0, string publicId = null, string sessionId = null, string regionId = null)
    {
        Name = name ?? string.Empty;
        ClientId = clientId;
        GameId = gameId;
        PublicId = publicId ?? SafetyParticipantIds.Get(gameId, clientId) ?? string.Empty;
        SessionId = string.IsNullOrEmpty(sessionId) ? SafetyParticipantIds.GetSessionId(gameId) ?? string.Empty : sessionId;
        RegionId = string.IsNullOrEmpty(regionId) ? SafetyParticipantIds.GetRegionId(gameId) : regionId;
    }

    public static void Clear()
    {
        Name = string.Empty;
        ClientId = -1;
        GameId = 0;
        PublicId = string.Empty;
        SessionId = string.Empty;
        RegionId = string.Empty;
    }

    public static void CaptureFromBar(LobbyPlayerBar bar)
    {
        ClientData live = SnrLobbyTargeting.FromBar(bar);
        FriendsListManager.RecentPlayedWithPlayer recent = SnrLobbyTargeting.ReadRecent(bar);
        string name = SnrLobbyTargeting.ReadName(bar);
        int gameId = live == null ? recent?.roomID ?? 0 : AmongUsClient.Instance?.GameId ?? 0;
        int clientId = live?.Id ?? recent?.clientId ?? -1;
        string publicId = SnrRecentPublicIds.Get(recent) ?? SafetyParticipantIds.Get(gameId, clientId);
        Capture(name, clientId, gameId, publicId, SnrRecentPublicIds.GetSessionId(recent), SnrRecentPublicIds.GetRegionId(recent));
    }
}

internal static class SnrRecentPublicIds
{
    private static readonly Dictionary<IntPtr, string> ByRecentPlayer = new();
    private static readonly Dictionary<IntPtr, (string SessionId, string RegionId)> Metadata = new();

    public static void Clear() { ByRecentPlayer.Clear(); Metadata.Clear(); }

    public static void Set(FriendsListManager.RecentPlayedWithPlayer recent, string publicId)
    {
        if (recent == null || recent.Pointer == IntPtr.Zero || string.IsNullOrEmpty(publicId)) return;
        ByRecentPlayer[recent.Pointer] = publicId;
    }

    public static void Set(FriendsListManager.RecentPlayedWithPlayer recent, string publicId, string sessionId, string regionId)
    {
        Set(recent, publicId);
        if (recent != null && recent.Pointer != IntPtr.Zero)
            Metadata[recent.Pointer] = (sessionId ?? string.Empty, regionId ?? string.Empty);
    }

    public static string Get(FriendsListManager.RecentPlayedWithPlayer recent)
    {
        return recent != null && recent.Pointer != IntPtr.Zero
            && ByRecentPlayer.TryGetValue(recent.Pointer, out string publicId)
            ? publicId
            : null;
    }

    public static string GetSessionId(FriendsListManager.RecentPlayedWithPlayer recent)
        => recent != null && recent.Pointer != IntPtr.Zero && Metadata.TryGetValue(recent.Pointer, out var value) ? value.SessionId : string.Empty;

    public static string GetRegionId(FriendsListManager.RecentPlayedWithPlayer recent)
        => recent != null && recent.Pointer != IntPtr.Zero && Metadata.TryGetValue(recent.Pointer, out var value) ? value.RegionId : string.Empty;
}

internal static class SnrBlockedPlayerIds
{
    private const string Prefix = "snr-identity-block:";

    public static string Encode(string rowId) => Prefix + (rowId ?? string.Empty);

    public static bool TryDecode(string value, out string rowId)
    {
        if (!string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            rowId = value[Prefix.Length..];
            return !string.IsNullOrEmpty(rowId);
        }
        rowId = string.Empty;
        return false;
    }
}

internal static class SnrRecentPlayerIds
{
    private const string Prefix = "snr-identity-recent:";

    public static string Encode(int gameId, int clientId, string sessionId = null) => $"{Prefix}{gameId}:{clientId}:{sessionId ?? string.Empty}";

    public static bool IsSynthetic(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);
    }
}

internal static class SnrLobbyTargeting
{
    private static readonly FieldInfo RecentPlayerField = AccessTools.Field(typeof(LobbyPlayerBar), "recentPlayer");
    private static readonly FieldInfo PlayerNameField = AccessTools.Field(typeof(LobbyPlayerBar), "playerName");

    public static ClientData FromBar(LobbyPlayerBar bar)
    {
        ClientData cached = LobbyBarClientCache.Get(bar);
        if (cached != null)
            return cached;

        FriendsListManager.RecentPlayedWithPlayer recent = ReadRecent(bar);
        return PlayerSafetyActions.FindInCurrentMatch(
            recent?.roomID ?? 0,
            recent?.clientId ?? -1,
            ReadName(bar, recent));
    }

    public static ClientData FromReportInfo(FriendsListManager manager)
    {
        if (SnrLobbyReportContext.ClientId >= 0)
        {
            ClientData captured = PlayerSafetyActions.FindByClientId(SnrLobbyReportContext.ClientId);
            if (captured != null)
                return captured;
        }

        var info = manager?.GetReportInfo();
        if (info == null)
            return null;
        return PlayerSafetyActions.FindInCurrentMatch(
            info.roomID,
            info.clientID,
            SnrLobbyReportContext.Name);
    }

    public static bool TryGetRecentReportTarget(FriendsListManager manager, out int gameId, out int clientId, out string publicId, out string name, out string sessionId, out string regionId)
    {
        var info = manager?.GetReportInfo();
        gameId = SnrLobbyReportContext.GameId != 0 ? SnrLobbyReportContext.GameId : info?.roomID ?? 0;
        clientId = SnrLobbyReportContext.ClientId >= 0 ? SnrLobbyReportContext.ClientId : info?.clientID ?? -1;
        publicId = !string.IsNullOrEmpty(SnrLobbyReportContext.PublicId)
            ? SnrLobbyReportContext.PublicId
            : SafetyParticipantIds.Get(gameId, clientId);
        name = SnrLobbyReportContext.Name ?? string.Empty;
        sessionId = string.IsNullOrEmpty(SnrLobbyReportContext.SessionId)
            ? SafetyParticipantIds.GetSessionId(gameId)
            : SnrLobbyReportContext.SessionId;
        regionId = string.IsNullOrEmpty(SnrLobbyReportContext.RegionId)
            ? SafetyParticipantIds.GetRegionId(gameId)
            : SnrLobbyReportContext.RegionId;
        return gameId != 0 && clientId >= 0 && !string.IsNullOrEmpty(publicId);
    }

    public static string ReadName(LobbyPlayerBar bar, FriendsListManager.RecentPlayedWithPlayer recent = null)
    {
        recent ??= ReadRecent(bar);
        string name = PlayerNameField?.GetValue(bar) as string;
        if (!string.IsNullOrEmpty(name))
            return name;
        if (bar?.InGameName != null && !string.IsNullOrEmpty(bar.InGameName.text))
            return bar.InGameName.text;
        return recent?.PlayerName;
    }

    public static FriendsListManager.RecentPlayedWithPlayer ReadRecent(LobbyPlayerBar bar)
    {
        object raw = RecentPlayerField?.GetValue(bar);
        if (raw is FriendsListManager.RecentPlayedWithPlayer direct)
            return direct;
        if (raw is Il2CppObjectBase il2)
            return il2.TryCast<FriendsListManager.RecentPlayedWithPlayer>();
        return null;
    }
}

internal static class LobbyBarClientCache
{
    private static readonly Dictionary<int, int> ClientIds = new();

    public static void Set(LobbyPlayerBar bar, ClientData client)
    {
        if (bar == null) return;
        int key = bar.GetInstanceID();
        if (client == null)
        {
            ClientIds.Remove(key);
            return;
        }
        ClientIds[key] = client.Id;
    }

    public static ClientData Get(LobbyPlayerBar bar)
    {
        if (bar == null) return null;
        if (!ClientIds.TryGetValue(bar.GetInstanceID(), out int id))
            return null;
        return PlayerSafetyActions.FindByClientId(id);
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.RefreshRecentlyPlayed))]
public static class FriendsListRecentPlayersOverridePatch
{
    private static bool _loading;
    private static bool _applying;

    public static bool Prefix(FriendsListUI __instance)
    {
        if (!CustomServerFriendsListDisplay.ShouldUseSnrMode() || __instance == null)
            return true;
        if (!_applying && !_loading)
        {
            _loading = true;
            __instance.StartCoroutine(CoLoad(__instance).WrapToIl2Cpp());
        }
        // Skip vanilla's refresh while the API data is loading.  The coroutine
        // invokes the method again with _applying=true after replacing the list,
        // allowing the normal row renderer to run on the safe synthetic ids.
        return _applying;
    }

    private static IEnumerator CoLoad(FriendsListUI ui)
    {
        List<RecentPlayerRow> rows = null;
        yield return PlayerSafetyApiClient.ListRecentPlayers(value => rows = value).WrapToIl2Cpp();
        _loading = false;
        if (rows == null || ui == null) yield break;

        try
        {
            FriendsListManager manager = DestroyableSingleton<FriendsListManager>.Instance;
            if (manager == null) yield break;
            var recentPlayers = manager.RecentlyPlayedWith
                ?? new Il2CppSystem.Collections.Generic.List<FriendsListManager.RecentPlayedWithPlayer>();
            recentPlayers.Clear();
            SnrRecentPublicIds.Clear();
            foreach (RecentPlayerRow row in rows)
            {
                if (row == null || row.GameId == 0 || row.ClientId < 0 || string.IsNullOrEmpty(row.PublicId))
                    continue;
                IntPtr pointer = IL2CPP.il2cpp_object_new(
                    Il2CppClassPointerStore<FriendsListManager.RecentPlayedWithPlayer>.NativeClassPtr);
                var recent = new FriendsListManager.RecentPlayedWithPlayer(pointer)
                {
                    PlayerName = row.Name ?? string.Empty,
                    // Never leave this empty: vanilla's EOS lookup rejects empty product ids.
                    // GetAndSetPlatform is skipped for this sentinel below.
                    Puid = SnrRecentPlayerIds.Encode(row.GameId, row.ClientId, row.SessionId),
                    FriendCode = string.Empty,
                    clientId = row.ClientId,
                    roomID = row.GameId,
                    gameServerID = string.Empty,
                    isReported = false,
                };
                recentPlayers.Add(recent);
                SnrRecentPublicIds.Set(recent, row.PublicId, row.SessionId, row.RegionId);
            }
            manager.RecentlyPlayedWith = recentPlayers;
            _applying = true;
            ui.RefreshRecentlyPlayed();
        }
        catch (Exception error)
        {
            Logger.Error($"Failed to apply Identity recent players: {error}");
        }
        finally
        {
            _applying = false;
        }
    }
}

// In SNR mode the vanilla AU Friends/Platform Friends panes are hidden.  Their
// refresh methods still run when the Friends List opens, however, and can issue
// EOS lookups with an empty product id (or leave the hidden pane selected).  Do
// not run those vanilla requests while SNR owns the list; vanilla mode keeps the
// original implementation untouched.
[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.RefreshFriends))]
public static class FriendsListFriendsContentPatch
{
    public static bool Prefix()
    {
        return !CustomServerFriendsListDisplay.ShouldUseSnrMode();
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.RefreshPlatformFriends))]
public static class FriendsListPlatformFriendsContentPatch
{
    public static bool Prefix()
    {
        return !CustomServerFriendsListDisplay.ShouldUseSnrMode();
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.RefreshBlockedPlayers))]
public static class FriendsListBlockedPlayersOverridePatch
{
    private static readonly FieldInfo FriendBarsField = AccessTools.Field(typeof(FriendsListUI), "friendBars");
    private static bool _loading;
    private static bool _applying;
    private static int _loadGeneration;
    private static bool _reloadRequested;

    public static bool Prefix(FriendsListUI __instance)
    {
        if (!CustomServerFriendsListDisplay.ShouldUseSnrMode() || __instance == null)
            return true;
        if (!_applying && !_loading)
            BeginLoad(__instance);
        // See the recent-player patch above: only the synchronous rebuild from
        // our coroutine should enter vanilla's row renderer in SNR mode.
        // Vanilla RefreshBlockedPlayers instantiates new BlockedPlayerBar rows
        // but never destroys the previous ones when the list is empty, so the
        // last blocked player remains visible after a successful unblock.
        if (_applying)
            ClearExistingBlockedPlayerBars(__instance);
        return _applying;
    }

    private static void ClearExistingBlockedPlayerBars(FriendsListUI ui)
    {
        if (ui?.BlockedArea == null) return;

        foreach (BlockedPlayerBar bar in ui.BlockedArea.GetComponentsInChildren<BlockedPlayerBar>(true))
        {
            if (bar == null || bar == ui.BlockedPlayerBar) continue;
            if (bar.ControllerSelectable != null && ControllerManager.Instance != null)
            {
                foreach (PassiveButton button in bar.ControllerSelectable)
                {
                    if (button != null)
                        ControllerManager.Instance.RemoveSelectableUiElement(button);
                }
            }
            UnityEngine.Object.DestroyImmediate(bar.gameObject);
        }

        Il2CppSystem.Collections.Generic.List<FriendsListBar> friendBars = ReadFriendBars(ui);
        if (friendBars == null) return;
        for (int i = friendBars.Count - 1; i >= 0; i--)
        {
            if (friendBars[i] == null)
                friendBars.RemoveAt(i);
        }
    }

    private static Il2CppSystem.Collections.Generic.List<FriendsListBar> ReadFriendBars(FriendsListUI ui)
    {
        object raw = FriendBarsField?.GetValue(ui);
        if (raw is Il2CppSystem.Collections.Generic.List<FriendsListBar> direct)
            return direct;
        if (raw is Il2CppObjectBase il2)
            return il2.TryCast<Il2CppSystem.Collections.Generic.List<FriendsListBar>>();
        return null;
    }

    public static void ForceReload(FriendsListUI ui)
    {
        if (_applying || !CustomServerFriendsListDisplay.ShouldUseSnrMode())
            return;

        // Keep the request when the list UI is not currently instantiated.  The
        // next blocked-tab/open callback will consume it, so a block performed
        // from the recent-player or report UI is visible when the list is opened.
        _reloadRequested = true;
        if (ui == null || !ui.isActiveAndEnabled)
            return;

        // A delete can finish while an initial refresh is still in flight.  Start
        // a new generation instead of relying on the old request's completion;
        // the older coroutine will be discarded when it observes the generation
        // mismatch.  This makes a successful unblock deterministic in the open UI.
        _reloadRequested = false;
        BeginLoad(ui);
    }

    public static void OnBlockedTabOpened(FriendsListUI ui)
    {
        if (_applying || !CustomServerFriendsListDisplay.ShouldUseSnrMode() || ui == null)
            return;

        // A block can complete while Friends List is closed, so consume the
        // pending mutation when the user actually opens the blocked pane.
        if (_reloadRequested)
            ForceReload(ui);
    }

    private static void BeginLoad(FriendsListUI ui)
    {
        _loadGeneration++;
        _loading = true;
        ui.StartCoroutine(CoLoad(ui, _loadGeneration).WrapToIl2Cpp());
    }

    private static IEnumerator CoLoad(FriendsListUI ui, int generation)
    {
        List<BlockRow> rows = null;
        yield return PlayerSafetyApiClient.ListBlocks(value => rows = value).WrapToIl2Cpp();
        if (generation != _loadGeneration) yield break;
        _loading = false;
        if (rows == null || ui == null || !CustomServerFriendsListDisplay.ShouldUseSnrMode()) yield break;

        try
        {
            FriendsListManager manager = DestroyableSingleton<FriendsListManager>.Instance;
            if (manager == null) yield break;
            // FriendsListManager.BlockedPlayers is also used by vanilla join handling.  Keeping
            // Identity row ids in it can send a synthetic PUID into the platform API when somebody
            // joins, so only swap the list for the synchronous UI rebuild below.
            var originalBlockedPlayers = manager.BlockedPlayers;
            var blockedPlayers = new Il2CppSystem.Collections.Generic.List<ResponseBlockedPlayer>();
            foreach (BlockRow row in rows)
            {
                if (row == null || string.IsNullOrEmpty(row.Id)) continue;
                blockedPlayers.Add(new ResponseBlockedPlayer
                {
                    FriendCode = row.Name ?? string.Empty,
                    FriendPuid = SnrBlockedPlayerIds.Encode(row.Id),
                });
            }
            try
            {
                manager.BlockedPlayers = blockedPlayers;
                _applying = true;
                // Prefix destroys leftover BlockedPlayerBar rows before this
                // vanilla rebuild, including when the fetch result is empty.
                ui.RefreshBlockedPlayers();

                // RefreshBlockedPlayers rebuilds the rows synchronously, but the
                // selected pane can be lost when the list was refreshed from a
                // row callback.  Re-open the blocked pane only when it was already
                // selected so that an unblock never unexpectedly changes tabs.
                if (ui.CurrentTab == FriendsListUI.FriendsListTab.Blocked)
                    ui.OpenTab((int)FriendsListUI.FriendsListTab.Blocked);
            }
            finally
            {
                manager.BlockedPlayers = originalBlockedPlayers;
                _applying = false;
            }
        }
        catch (Exception error)
        {
            Logger.Error($"Failed to apply Identity blocked players: {error}");
        }
        finally
        {
            _applying = false;
        }
    }
}

[HarmonyPatch(typeof(FriendsListBar), nameof(FriendsListBar.GetAndSetPlatform))]
public static class IdentityBlockedPlayerPlatformPatch
{
    public static bool Prefix(FriendsListBar __instance)
    {
        if (!CustomServerFriendsListDisplay.ShouldUseSnrMode() || __instance == null)
            return true;
        return !SnrBlockedPlayerIds.TryDecode(__instance.puid, out _)
            && !SnrRecentPlayerIds.IsSynthetic(__instance.puid);
    }
}

[HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.UnblockPlayer))]
public static class FriendsListIdentityUnblockPatch
{
    public static bool Prefix(FriendsListManager __instance, string recipientPuid)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()
            || !SnrBlockedPlayerIds.TryDecode(recipientPuid, out string rowId))
            return true;

        MonoBehaviour runner = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (runner == null)
        {
            Logger.Error($"Safety unblock rejected before send row={rowId}: coroutine runner missing");
            PlayerSafetyActions.NotifyFailure(
                ModTranslation.GetString("SafetyBlockFailed"),
                "ブロック解除を実行できる状態ではありません。");
            return false;
        }

        runner.StartCoroutine(CoUnblock(__instance, rowId).WrapToIl2Cpp());
        return false;
    }

    private static IEnumerator CoUnblock(FriendsListManager manager, string rowId)
    {
        bool ok = false;
        string failureReason = null;
        yield return PlayerSafetyApiClient.DeleteBlock(
            rowId,
            value => ok = value,
            reason => failureReason = reason).WrapToIl2Cpp();
        Logger.Info($"Safety friends-list unblock row={rowId} ok={ok} reason={failureReason}");
        if (ok)
            PlayerSafetyActions.Notify(
                ModTranslation.GetString("SafetyUnblocked"),
                FriendListNotification.IconType.Block);
        else
            PlayerSafetyActions.NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), failureReason);
        if (ok)
            FriendsListBlockedPlayersOverridePatch.ForceReload(FindFriendsListUi(manager));
    }

    private static FriendsListUI FindFriendsListUi(FriendsListManager manager)
    {
        return manager?.Ui ?? UnityEngine.Object.FindObjectOfType<FriendsListUI>();
    }
}

[HarmonyPatch(typeof(BlockedPlayerBar), nameof(BlockedPlayerBar.UnblockPlayer))]
public static class BlockedPlayerBarIdentityUnblockPatch
{
    public static bool Prefix(BlockedPlayerBar __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()
            || __instance == null
            || !SnrBlockedPlayerIds.TryDecode(__instance.puid, out string rowId))
            return true;

        FriendsListManager manager = DestroyableSingleton<FriendsListManager>.Instance;
        MonoBehaviour runner = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (runner == null)
        {
            Logger.Error($"Safety unblock rejected before send row={rowId}: coroutine runner missing");
            PlayerSafetyActions.NotifyFailure(
                ModTranslation.GetString("SafetyBlockFailed"),
                "ブロック解除を実行できる状態ではありません。");
            return false;
        }

        Logger.Info($"Safety blocked-player bar unblock selected row={rowId}");
        runner.StartCoroutine(CoUnblock(manager, rowId).WrapToIl2Cpp());
        return false;
    }

    private static IEnumerator CoUnblock(FriendsListManager manager, string rowId)
    {
        bool ok = false;
        string failureReason = null;
        yield return PlayerSafetyApiClient.DeleteBlock(
            rowId,
            value => ok = value,
            reason => failureReason = reason).WrapToIl2Cpp();
        Logger.Info($"Safety blocked-player bar unblock row={rowId} ok={ok} reason={failureReason}");
        if (ok)
            PlayerSafetyActions.Notify(
                ModTranslation.GetString("SafetyUnblocked"),
                FriendListNotification.IconType.Block);
        else
            PlayerSafetyActions.NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), failureReason);
        if (ok)
            FriendsListBlockedPlayersOverridePatch.ForceReload(manager?.Ui ?? UnityEngine.Object.FindObjectOfType<FriendsListUI>());
    }
}

[HarmonyPatch(typeof(ReportReasonScreen), nameof(ReportReasonScreen.Show), typeof(string), typeof(int), typeof(bool))]
public static class ReportReasonDestinationPatch
{
    public static void Postfix(ReportReasonScreen __instance, string playerName, int colorId, bool fromFriends)
    {
        _ = colorId;
        TMP_Text whyText = FindWhyText(__instance);
        if (whyText == null) return;
        var info = DestroyableSingleton<FriendsListManager>.Instance?.GetReportInfo();
        if (fromFriends && info != null && SnrLobbyReportContext.ClientId < 0)
            SnrLobbyReportContext.Capture(
                playerName,
                info.clientID,
                info.roomID,
                SafetyParticipantIds.Get(info.roomID, info.clientID));

        string dest = fromFriends && OfficialSnrServer.IsIdentityEnabled()
            ? ModTranslation.GetString("SafetyReportDestinationSnr")
            : ModTranslation.GetString("SafetyReportDestinationVanilla");
        // Keep the vanilla NameText for the reported player's name.  The
        // destination (SNR or Among Us) belongs in the reason/explanation text.
        // Show() can update the child text again after its postfix runs, so
        // apply the destination after the vanilla UI initialization completes.
        __instance.StartCoroutine(CoApplyDestination(__instance, dest).WrapToIl2Cpp());

        if (!fromFriends || !OfficialSnrServer.IsIdentityEnabled())
            return;

        PassiveButton block = FindBlockButton(__instance);
        if (block == null) return;
        block.OnClick = new();
        block.OnClick.AddListener((UnityAction)(() => SnrLobbyBlock.FromReportScreen(__instance)));
    }

    private static IEnumerator CoApplyDestination(ReportReasonScreen screen, string destination)
    {
        // Show() may initialize the prompt from a coroutine, so wait until
        // that setup has had a chance to complete before composing both lines.
        for (int i = 0; i < 10; i++)
            yield return null;
        TMP_Text whyText = FindWhyText(screen);
        if (whyText == null)
            yield break;

        string current = whyText.text ?? string.Empty;
        string prefix = (destination ?? string.Empty) + "\n";
        string body = GetLocalizedReportWhy();
        if (string.IsNullOrWhiteSpace(body))
            body = current;
        if (body.StartsWith(prefix, StringComparison.Ordinal))
            body = body.Substring(prefix.Length);
        if (body.Equals(destination, StringComparison.Ordinal))
            body = string.Empty;
        whyText.text = prefix + body;
    }

    private static string GetLocalizedReportWhy()
    {
        try
        {
            return FastDestroyableSingleton<TranslationController>.Instance?
                .GetString(StringNames.ReportWhy);
        }
        catch (Exception error)
        {
            Logger.Warning($"Failed to resolve localized report prompt: {error.Message}");
            return string.Empty;
        }
    }

    private static TMP_Text FindWhyText(ReportReasonScreen screen)
    {
        if (screen == null) return null;
        // WhyText is a direct child of the report screen in the vanilla prefab.
        // Resolve it by the concrete TMP component; enumerating Transform via
        // IL2CPP can omit inactive/prefab children on some game builds.
        Transform direct = screen.transform.Find("WhyText");
        TMP_Text directText = direct?.GetComponent<TextMeshPro>()
            ?? direct?.GetComponentInChildren<TextMeshPro>(true);
        if (directText != null)
            return directText;

        foreach (TextMeshPro child in screen.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (child != null && string.Equals(child.name, "WhyText", StringComparison.Ordinal))
                return child;
        }

        return null;
    }

    private static PassiveButton FindBlockButton(ReportReasonScreen screen)
    {
        foreach (string name in new[] { "BlockButton", "blockButton", "Block", "ConfirmBlock" })
        {
            var field = AccessTools.Field(typeof(ReportReasonScreen), name);
            object value = field?.GetValue(screen);
            if (value is PassiveButton direct)
                return direct;
            if (value is Component component)
            {
                PassiveButton nested = component.GetComponent<PassiveButton>();
                if (nested != null) return nested;
            }
        }

        foreach (PassiveButton button in screen.GetComponentsInChildren<PassiveButton>(true))
        {
            if (button != null && button.name.IndexOf("Block", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return button;
        }

        return null;
    }
}

[HarmonyPatch(typeof(LobbyPlayerBar), nameof(LobbyPlayerBar.SetInfoFromClient))]
public static class LobbyPlayerBarClientCachePatch
{
    public static void Postfix(LobbyPlayerBar __instance, ClientData clientData)
    {
        LobbyBarClientCache.Set(__instance, clientData);
    }
}

[HarmonyPatch(typeof(LobbyPlayerBar), nameof(LobbyPlayerBar.SetUp))]
public static class LobbyPlayerBarSetupPatch
{
    public static void Postfix(LobbyPlayerBar __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled() || __instance == null) return;
        if (__instance.BlockButton != null)
            __instance.BlockButton.gameObject.SetActive(true);
        if (__instance.ReportButton != null)
            __instance.ReportButton.gameObject.SetActive(true);
        CustomServerFriendsListDisplay.HideAddFriendControl(__instance);
    }
}

[HarmonyPatch(typeof(LobbyPlayerBar), nameof(LobbyPlayerBar.ReportPlayer))]
public static class LobbyPlayerBarReportPatch
{
    public static bool Prefix(LobbyPlayerBar __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return true;
        SnrLobbyReportContext.CaptureFromBar(__instance);
        var recent = SnrLobbyTargeting.ReadRecent(__instance);
        if (recent == null) return true;
        string name = SnrLobbyTargeting.ReadName(__instance, recent);
        DestroyableSingleton<FriendsListManager>.Instance.OpenReportScreen(name, 0, recent);
        return false;
    }
}

[HarmonyPatch(typeof(FriendsListManager), nameof(FriendsListManager.StartReport))]
public static class FriendsListStartReportPatch
{
    public static bool Prefix(FriendsListManager __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return true;
        var client = SnrLobbyTargeting.FromReportInfo(__instance);
        var runner = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        if (client == null && runner != null && SnrLobbyTargeting.TryGetRecentReportTarget(__instance, out int gameId, out int clientId, out string publicId, out string name, out string sessionId, out string regionId))
        {
            __instance.Ui?.Close();
            PlayerSafetyActions.ReportRecentClient(
                gameId,
                clientId,
                publicId,
                name,
                SafetyReportCategories.FromVanilla(__instance.GetReportInfo()?.reason ?? default),
                string.Empty,
                runner,
                sessionId,
                regionId);
            SnrLobbyReportContext.Clear();
            return false;
        }

        if (client == null || runner == null)
        {
            __instance.Ui?.Close();
            PlayerSafetyActions.NotifyFailure(
                ModTranslation.GetString("SafetyReportFailed"),
                ModTranslation.GetString("SafetyNotInThisMatch"));
            return false;
        }

        __instance.Ui?.Close();
        PlayerSafetyActions.ReportClient(client, SafetyReportCategories.FromVanilla(__instance.GetReportInfo()?.reason ?? default), string.Empty, runner);
        SnrLobbyReportContext.Clear();
        return false;
    }
}

[HarmonyPatch(typeof(LobbyPlayerBar), nameof(LobbyPlayerBar.BlockPlayer))]
public static class LobbyPlayerBarBlockPatch
{
    public static bool Prefix(LobbyPlayerBar __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return true;
        var runner = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        var client = SnrLobbyTargeting.FromBar(__instance);
        if (client != null)
        {
            SnrLobbyBlock.BlockClient(client, runner);
            return false;
        }

        SnrLobbyReportContext.CaptureFromBar(__instance);
        Logger.Info(
            $"Safety recent block selected name={SnrLobbyReportContext.Name} game_id={SnrLobbyReportContext.GameId} "
            + $"client_id={SnrLobbyReportContext.ClientId} public_id_empty={string.IsNullOrEmpty(SnrLobbyReportContext.PublicId)} "
            + $"runner_null={runner == null}");
        SnrLobbyBlock.BlockRecent(runner);
        return false;
    }
}

[HarmonyPatch]
public static class FriendsListStartBlockPatch
{
    public static bool Prepare() => AccessTools.Method(typeof(FriendsListManager), "StartBlock") != null;

    public static MethodBase TargetMethod() => AccessTools.Method(typeof(FriendsListManager), "StartBlock");

    public static bool Prefix(FriendsListManager __instance)
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return true;
        SnrLobbyBlock.FromFriendsList(__instance);
        return false;
    }
}

internal static class SnrLobbyBlock
{
    public static void FromReportScreen(ReportReasonScreen screen)
    {
        var manager = DestroyableSingleton<FriendsListManager>.Instance;
        manager?.Ui?.Close();
        screen?.gameObject.SetActive(false);
        FromFriendsList(manager);
    }

    public static void FromFriendsList(FriendsListManager manager)
    {
        var runner = SafetyRuntime.FindCoroutineRunner(AmongUsClient.Instance);
        var client = SnrLobbyTargeting.FromReportInfo(manager);
        if (client != null)
            SnrLobbyBlock.BlockClient(client, runner);
        else
            SnrLobbyBlock.BlockRecent(runner, manager);
        SnrLobbyReportContext.Clear();
        manager?.Ui?.Close();
    }

    public static void BlockRecent(MonoBehaviour runner, FriendsListManager manager = null)
    {
        manager ??= DestroyableSingleton<FriendsListManager>.Instance;
        if (!SnrLobbyTargeting.TryGetRecentReportTarget(manager, out int gameId, out int clientId, out string publicId, out string name, out string sessionId, out string regionId))
        {
            PlayerSafetyActions.NotifyFailure(
                ModTranslation.GetString("SafetyBlockFailed"),
                "最近のプレイヤー情報を取得できませんでした。");
            return;
        }

        PlayerSafetyActions.BlockRecentClient(gameId, clientId, publicId, name, string.Empty, runner, sessionId, regionId);
    }

    public static void BlockClient(ClientData client, MonoBehaviour runner)
    {
        if (runner == null || client == null)
        {
            PlayerSafetyActions.NotifyFailure(
                ModTranslation.GetString("SafetyBlockFailed"),
                ModTranslation.GetString("SafetyNotInThisMatch"));
            return;
        }

        Logger.Info($"Safety block sending name={client.PlayerName} client_id={client.Id} game_id={AmongUsClient.Instance?.GameId}");
        PlayerSafetyActions.BlockClient(client, string.Empty, runner);
    }
}
