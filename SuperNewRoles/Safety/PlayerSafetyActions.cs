using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Patches;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class PlayerSafetyActions
{
    public static ClientData FindByClientId(int clientId)
    {
        return AmongUsClient.Instance?.allClients?.ToArray()
            .FirstOrDefault(client => client != null && client.Id == clientId);
    }

    public static ClientData FindInCurrentMatch(int recordedGameId, int recordedClientId, string recordedName)
    {
        AmongUsClient auc = AmongUsClient.Instance;
        if (auc == null)
            return null;

        var live = auc.allClients?.ToArray()
            .Where(client => client != null)
            .Select(client => (client.Id, client.PlayerName ?? string.Empty))
            .ToList();
        int? id = SafetyLobbyTarget.ResolveClientId(
            auc.GameId,
            recordedGameId,
            recordedClientId,
            recordedName,
            live);
        return id == null ? null : FindByClientId(id.Value);
    }

    public static void BlockClient(ClientData client, string note, MonoBehaviour runner)
    {
        string code = CurrentGameCode();
        string publicId = SafetyParticipantIds.Get(AmongUsClient.Instance.GameId, client.Id);
        string sessionId = SafetyParticipantIds.GetSessionId(AmongUsClient.Instance.GameId);
        string failureReason = null;
        runner.StartCoroutine(PlayerSafetyApiClient.CreateBlock(code, AmongUsClient.Instance.GameId, client.Id, publicId, note, false, result =>
        {
            if (!result.Ok)
            {
                NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), failureReason);
                return;
            }
            Notify(
                string.Format(ModTranslation.GetString("SafetyBlocked"), client.PlayerName),
                FriendListNotification.IconType.Block);
            RefreshBlockedPlayers();
            if (result.KickTarget && AmongUsClient.Instance.AmHost)
                OnGameJoinedIdentityPatch.SendHostBlockKick(client.Id);
            if (result.LeaveSelf)
                AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
        }, reason => failureReason = reason, sessionId: sessionId).WrapToIl2Cpp());
    }

    public static void BlockRecentClient(int gameId, int clientId, string publicId, string playerName, string note, MonoBehaviour runner, string sessionId = null, string regionId = null)
    {
        if (string.IsNullOrEmpty(publicId) && runner != null)
        {
            runner.StartCoroutine(ResolveRecentThenBlock(gameId, clientId, playerName, note, runner).WrapToIl2Cpp());
            return;
        }

        string code = GameCode.IntToGameName(gameId)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(code) || gameId == 0 || clientId < 0 || string.IsNullOrEmpty(publicId) || runner == null)
        {
            Logger.Error(
                $"Safety recent block rejected before send name={playerName} game_id={gameId} client_id={clientId} "
                + $"code_empty={string.IsNullOrEmpty(code)} public_id_empty={string.IsNullOrEmpty(publicId)} runner_null={runner == null}");
            NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), ModTranslation.GetString("SafetyTargetInvalid"));
            return;
        }

        Logger.Info($"Safety recent block sending name={playerName} client_id={clientId} game_code={code} game_id={gameId}");
        string failureReason = null;
        runner.StartCoroutine(PlayerSafetyApiClient.CreateBlock(code, gameId, clientId, publicId, note, true, result =>
        {
            if (result.Ok)
            {
                Notify(
                    string.Format(ModTranslation.GetString("SafetyBlocked"), playerName),
                    FriendListNotification.IconType.Block);
                RefreshBlockedPlayers();
            }
            else
            {
                NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), failureReason);
            }
        }, reason => failureReason = reason, sessionId: sessionId, regionId: regionId).WrapToIl2Cpp());
    }

    private static IEnumerator ResolveRecentThenBlock(int gameId, int clientId, string playerName, string note, MonoBehaviour runner)
    {
        List<RecentPlayerRow> rows = null;
        yield return PlayerSafetyApiClient.ListRecentPlayers(value => rows = value).WrapToIl2Cpp();
        RecentPlayerRow match = rows?.FirstOrDefault(row =>
            row != null
            && !string.IsNullOrEmpty(row.PublicId)
            && (gameId == 0 || row.GameId == gameId)
            && (clientId < 0 || row.ClientId == clientId)
            && (string.IsNullOrEmpty(playerName) || string.Equals(row.Name, playerName, StringComparison.Ordinal)));
        if (match == null)
        {
            Logger.Error($"Safety recent block could not resolve public id name={playerName} game_id={gameId} client_id={clientId}");
            NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), ModTranslation.GetString("SafetyRecentPlayersUnavailable"));
            yield break;
        }

        BlockRecentClient(
            match.GameId,
            match.ClientId,
            match.PublicId,
            string.IsNullOrEmpty(match.Name) ? playerName : match.Name,
            note,
            runner,
            match.SessionId,
            match.RegionId);
    }

    public static void ReportClient(ClientData client, string category, string comment, MonoBehaviour runner)
    {
        string code = CurrentGameCode();
        string publicId = SafetyParticipantIds.Get(AmongUsClient.Instance.GameId, client.Id);
        string sessionId = SafetyParticipantIds.GetSessionId(AmongUsClient.Instance.GameId);
        Logger.Info(
            $"Safety report sending name={client.PlayerName} client_id={client.Id} self_client_id={AmongUsClient.Instance.ClientId} game_code={code} game_id={AmongUsClient.Instance.GameId}");
        string failureReason = null;
        runner.StartCoroutine(PlayerSafetyApiClient.CreateReport(
            code,
            client.Id,
            category,
            comment,
            ok =>
            {
                if (ok)
                {
                    Notify(
                        string.Format(ModTranslation.GetString("SafetyReported"), client.PlayerName),
                        FriendListNotification.IconType.Sent);
                }
                else
                {
                    NotifyFailure(ModTranslation.GetString("SafetyReportFailed"), failureReason);
                }
            },
            publicId: publicId,
            failureCallback: reason => failureReason = reason,
            sessionId: sessionId).WrapToIl2Cpp());
    }

    public static void ReportRecentClient(int gameId, int clientId, string publicId, string playerName, string category, string comment, MonoBehaviour runner, string sessionId = null, string regionId = null)
    {
        string code = GameCode.IntToGameName(gameId)?.ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(code) || gameId == 0 || clientId < 0 || string.IsNullOrEmpty(publicId) || runner == null)
        {
            NotifyFailure(ModTranslation.GetString("SafetyReportFailed"), ModTranslation.GetString("SafetyTargetInvalid"));
            return;
        }

        Logger.Info($"Safety recent report sending name={playerName} client_id={clientId} game_code={code} game_id={gameId}");
        string failureReason = null;
        runner.StartCoroutine(PlayerSafetyApiClient.CreateReport(
            code,
            clientId,
            category,
            comment,
            ok =>
            {
                if (ok)
                {
                    Notify(
                        string.Format(ModTranslation.GetString("SafetyReported"), playerName),
                        FriendListNotification.IconType.Sent);
                }
                else
                {
                    NotifyFailure(ModTranslation.GetString("SafetyReportFailed"), failureReason);
                }
            },
            gameId,
            recent: true,
            publicId: publicId,
            failureCallback: reason => failureReason = reason,
            sessionId: sessionId,
            regionId: regionId).WrapToIl2Cpp());
    }

    public static void Unblock(string rowIdOrName, MonoBehaviour runner)
    {
        MonoBehaviour host = SafetyRuntime.FindCoroutineRunner(runner);
        if (host == null)
        {
            NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), ModTranslation.GetString("SafetyUnblockUnavailable"));
            return;
        }

        Logger.Info($"Safety unblock start query={rowIdOrName} host={host.name}");
        host.StartCoroutine(UnblockRoutine(rowIdOrName, (ok, failureReason) =>
        {
            Logger.Info($"Safety unblock query={rowIdOrName} ok={ok}");
            if (ok)
            {
                Notify(ModTranslation.GetString("SafetyUnblocked"), FriendListNotification.IconType.Block);
                FriendsListBlockedPlayersOverridePatch.ForceReload(
                    DestroyableSingleton<FriendsListManager>.Instance?.Ui
                    ?? UnityEngine.Object.FindObjectOfType<FriendsListUI>());
            }
            else
                NotifyFailure(ModTranslation.GetString("SafetyBlockFailed"), failureReason);
        }).WrapToIl2Cpp());
    }

    private static IEnumerator UnblockRoutine(string rowIdOrName, Action<bool, string> callback)
    {
        if (Guid.TryParse(rowIdOrName, out _))
        {
            string failureReason = null;
            bool ok = false;
            yield return PlayerSafetyApiClient.DeleteBlock(rowIdOrName, value => ok = value, reason => failureReason = reason);
            callback(ok, failureReason);
            yield break;
        }

        List<BlockRow> rows = null;
        yield return PlayerSafetyApiClient.ListBlocks(list => rows = list);
        BlockRow match = SafetyBlockLookup.Find(rows, rowIdOrName);
        if (match == null || string.IsNullOrEmpty(match.Id))
        {
            callback(false, "ブロック対象が見つかりません。");
            yield break;
        }

        string deleteFailureReason = null;
        bool deleteOk = false;
        yield return PlayerSafetyApiClient.DeleteBlock(
            match.Id,
            value => deleteOk = value,
            reason => deleteFailureReason = reason);
        callback(deleteOk, deleteFailureReason);
    }

    public static void UpdateNote(string rowId, string note, MonoBehaviour runner)
    {
        MonoBehaviour host = SafetyRuntime.FindCoroutineRunner(runner);
        if (host == null)
        {
            NotifyFailure(ModTranslation.GetString("SafetyReportFailed"), ModTranslation.GetString("SafetyNoteSaveUnavailable"));
            return;
        }

        string failureReason = null;
        host.StartCoroutine(PlayerSafetyApiClient.UpdateBlockNote(rowId, note, ok =>
        {
            if (ok)
                Notify(ModTranslation.GetString("SafetyNoteSaved"), FriendListNotification.IconType.Sent);
            else
                NotifyFailure(ModTranslation.GetString("SafetyReportFailed"), failureReason);
        }, reason => failureReason = reason).WrapToIl2Cpp());
    }

    public static void NotifyBlockedJoinRejected(string name)
    {
        if (!ConfigRoles.NotifyHostWhenBlockedJoin.Value) return;
        if (string.IsNullOrWhiteSpace(name)) return;
        MonoBehaviour host = SafetyPopupUi.EnsureHost();
        if (host == null)
        {
            Logger.Info(FormatBlockedJoinRejected(name));
            return;
        }
        host.StartCoroutine(CoNotifyBlockedJoinRejected(name).WrapToIl2Cpp());
    }

    public static string FormatBlockedJoinRejected(string name)
    {
        return string.Format(ModTranslation.GetString("SafetyHostBlockRejected"), name ?? string.Empty);
    }

    private static IEnumerator CoNotifyBlockedJoinRejected(string name)
    {
        yield return null;
        Notify(FormatBlockedJoinRejected(name), FriendListNotification.IconType.Block);
    }

    public static void Notify(string text)
    {
        Notify(text, FriendListNotification.IconType.Friend);
    }

    public static void Notify(string text, FriendListNotification.IconType iconType)
    {
        if (string.IsNullOrEmpty(text))
            return;

        try
        {
            FriendsListManager manager = DestroyableSingleton<FriendsListManager>.Instance;
            FriendListNotification notification = manager?.FriendListNotification;
            if (notification != null)
            {
                notification.SetUp(text, iconType);
                return;
            }
        }
        catch (Exception error)
        {
            Logger.Warning($"FriendListNotification failed: {error.Message}");
        }

        // The game may not have created FriendsListManager yet (for example
        // during very early startup).  Do not fall back to chat: safety
        // actions must not inject messages into the room chat.  Keep a local
        // log entry so the failure is still diagnosable.
        Logger.Info(text);
    }

    public static void NotifyFailure(string message, string reason = null)
    {
        string suffix = NormalizeFailureReason(reason);
        Logger.Warning(string.IsNullOrEmpty(suffix) ? message : $"{message} {suffix}");
        Notify(
            string.IsNullOrEmpty(suffix) ? message : $"{message}\n{suffix}",
            FriendListNotification.IconType.Failed);
    }

    private static string NormalizeFailureReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return string.Empty;
        string normalized = reason.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return normalized.Length <= 180 ? normalized : normalized[..180];
    }

    private static void RefreshBlockedPlayers()
    {
        FriendsListBlockedPlayersOverridePatch.ForceReload(
            DestroyableSingleton<FriendsListManager>.Instance?.Ui
            ?? UnityEngine.Object.FindObjectOfType<FriendsListUI>());
    }

    private static string CurrentGameCode()
    {
        if (AmongUsClient.Instance == null) return string.Empty;
        string code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        return string.IsNullOrEmpty(code) ? string.Empty : code.ToUpperInvariant();
    }
}
