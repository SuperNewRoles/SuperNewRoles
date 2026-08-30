using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Identity;
using SuperNewRoles.Safety;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Safety.Api;

public static class PlayerSafetyApiClient
{
    private const int RequestTimeoutSeconds = 5;

    public const string ConductGet = "conduct.get";
    public const string ConductConsent = "conduct.consent";
    public const string BlocksList = "blocks.list";
    public const string BlocksCreate = "blocks.create";
    public const string BlocksUpdate = "blocks.update";
    public const string BlocksDelete = "blocks.delete";
    public const string GamesList = "games.list";
    public const string ReportsCreate = "reports.create";
    public const string NoticesGet = "notices.get";
    public const string NoticesAck = "notices.ack";
    public const string WarningsAck = "warnings.ack";
    public const string RecentPlayersList = "recent_players.list";

    public static IEnumerator GetConduct(Action<ConductResponse> callback)
    {
        yield return Send("GET", "/v1/conduct?lang=" + ConductLanguageQuery(), ConductGet, null, text =>
        {
            callback(ParseConduct(text));
        });
    }

    public static IEnumerator Consent(int version, Action<bool> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object> { ["version"] = version });
        yield return Send("POST", "/v1/conduct/consent?lang=" + ConductLanguageQuery(), ConductConsent, json, text => callback(text != null));
    }

    public static IEnumerator AckWarning(string warningId, Action<bool> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object> { ["id"] = warningId ?? string.Empty });
        yield return Send("POST", "/v1/warnings/ack", WarningsAck, json, text => callback(text != null));
    }

    public static IEnumerator ListBlocks(Action<List<BlockRow>> callback)
    {
        yield return Send("GET", "/v1/blocks", BlocksList, null, text =>
        {
            if (text == null)
            {
                callback(null);
                return;
            }
            var rows = new List<BlockRow>();
            if (JsonParser.Parse(text) is Dictionary<string, object> root
                && root.TryGetValue("blocks", out var list) && list is List<object> blocks)
            {
                foreach (var item in blocks)
                {
                    if (item is Dictionary<string, object> row)
                        rows.Add(BlockRow.From(row));
                }
            }
            callback(rows);
        });
    }

    public static IEnumerator CreateBlock(
        string gameCode,
        int gameId,
        int clientId,
        string publicId,
        string note,
        bool recent,
        Action<BlockCreateResult> callback,
        Action<string> failureCallback = null)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object>
        {
            ["game_code"] = gameCode,
            ["game_id"] = gameId,
            ["client_id"] = clientId,
            ["public_id"] = publicId ?? string.Empty,
            ["note"] = note ?? string.Empty,
            ["recent"] = recent,
        });
        yield return Send("POST", "/v1/blocks", BlocksCreate, json, text =>
        {
            callback(BlockCreateResult.From(text));
        }, failureCallback: failureCallback);
    }

    public static IEnumerator UpdateBlockNote(
        string rowId,
        string note,
        Action<bool> callback,
        Action<string> failureCallback = null)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object> { ["note"] = note ?? string.Empty });
        yield return Send(
            "PATCH",
            "/v1/blocks/" + rowId,
            BlocksUpdate,
            json,
            text => callback(text != null),
            failureCallback: failureCallback);
    }

    public static IEnumerator DeleteBlock(string rowId, Action<bool> callback, Action<string> failureCallback = null)
    {
        yield return Send(
            "DELETE",
            "/v1/blocks/" + rowId,
            BlocksDelete,
            null,
            text => callback(text != null),
            acceptEmpty: true,
            failureCallback: failureCallback);
    }

    public static IEnumerator ListGames(Action<List<PublicGameRow>> callback)
    {
        yield return Send("GET", "/v1/games", GamesList, null, text =>
        {
            var rows = new List<PublicGameRow>();
            if (text != null && JsonParser.Parse(text) is Dictionary<string, object> root
                && root.TryGetValue("games", out var list) && list is List<object> games)
            {
                foreach (var item in games)
                {
                    if (item is Dictionary<string, object> row)
                        rows.Add(PublicGameRow.From(row));
                }
            }
            callback(rows);
        });
    }

    public static IEnumerator CreateReport(
        string gameCode,
        int clientId,
        string category,
        string comment,
        Action<bool> callback,
        int? gameId = null,
        bool recent = false,
        string publicId = null,
        Action<string> failureCallback = null)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object>
        {
            ["game_code"] = gameCode,
            ["game_id"] = gameId ?? AmongUsClient.Instance.GameId,
            ["client_id"] = clientId,
            ["public_id"] = publicId ?? string.Empty,
            ["category"] = category ?? "other",
            ["comment"] = comment ?? string.Empty,
            ["recent"] = recent,
        });
        yield return Send(
            "POST",
            "/v1/reports",
            ReportsCreate,
            json,
            text => callback(text != null),
            failureCallback: failureCallback);
    }

    public static IEnumerator GetNotices(Action<SafetyNoticeInbox> callback)
    {
        yield return Send("GET", "/v1/notices", NoticesGet, null, text =>
        {
            callback(text == null ? null : SafetyNoticeInbox.From(text));
        });
    }

    public static IEnumerator AckNotices(Action<bool> callback)
    {
        yield return Send("POST", "/v1/notices/ack", NoticesAck, "{}", text => callback(text != null));
    }

    public static IEnumerator ListRecentPlayers(Action<List<RecentPlayerRow>> callback)
    {
        yield return Send("GET", "/v1/recent-players", RecentPlayersList, null, text =>
        {
            if (text == null || JsonParser.Parse(text) is not Dictionary<string, object> root
                || !root.TryGetValue("players", out object list) || list is not List<object> players)
            {
                callback(null);
                return;
            }

            var rows = new List<RecentPlayerRow>();
            foreach (object item in players)
            {
                if (item is Dictionary<string, object> row)
                    rows.Add(RecentPlayerRow.From(row));
            }
            callback(rows);
        });
    }

    private static string ConductLanguageQuery()
    {
        try
        {
            var lang = FastDestroyableSingleton<TranslationController>.Instance?.currentLanguage?.languageID
                ?? AmongUs.Data.DataManager.Settings.Language.CurrentLanguage;
            return lang switch
            {
                SupportedLangs.English => "en",
                SupportedLangs.SChinese => "zh-CN",
                SupportedLangs.TChinese => "zh-TW",
                SupportedLangs.Japanese => "ja",
                _ => "ja"
            };
        }
        catch
        {
            return "ja";
        }
    }

    private static IEnumerator Send(
        string method,
        string path,
        string action,
        string jsonBody,
        Action<string> callback,
        bool acceptEmpty = false,
        Action<string> failureCallback = null)
    {
        byte[] bodyBytes = jsonBody == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(jsonBody);
        PlayerIdentityProof proof = PlayerIdentityStore.CreateProof(action, bodyBytes);
        var request = new UnityWebRequest(SNRURLs.IdentityAPI + path, method)
        {
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = RequestTimeoutSeconds,
        };
        if (bodyBytes.Length > 0)
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-SNR-Public-Key", proof.PublicKeyBase64);
        request.SetRequestHeader("X-SNR-Timestamp", proof.TimestampUnix.ToString());
        request.SetRequestHeader("X-SNR-Nonce", proof.Nonce);
        request.SetRequestHeader("X-SNR-Signature", proof.SignatureBase64);
        request.SetRequestHeader("X-SNR-Action", proof.Action);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success && (request.responseCode is >= 200 and < 300))
        {
            string text = request.downloadHandler?.text;
            callback?.Invoke(acceptEmpty ? (text ?? string.Empty) : text);
        }
        else
        {
            Logger.Error($"PlayerSafety API {path} failed: {request.responseCode} {request.error} {request.downloadHandler?.text}");
            failureCallback?.Invoke(BuildFailureReason(request));
            callback?.Invoke(null);
        }
        request.Dispose();
    }

    private static string BuildFailureReason(UnityWebRequest request)
    {
        string bodyReason = null;
        string body = request?.downloadHandler?.text;
        try
        {
            if (!string.IsNullOrWhiteSpace(body)
                && JsonParser.Parse(body) is Dictionary<string, object> root)
            {
                if (root.TryGetValue("error", out object error) && !string.IsNullOrWhiteSpace(error?.ToString()))
                    bodyReason = error.ToString();
                else if (root.TryGetValue("message", out object message) && !string.IsNullOrWhiteSpace(message?.ToString()))
                    bodyReason = message.ToString();
            }
        }
        catch
        {
            // A proxy can return an HTML/plain-text error page.  Keep the
            // transport/status reason instead of losing the failure callback to
            // a JSON parse exception.
        }

        string status = request != null && request.responseCode > 0
            ? $"HTTP {request.responseCode}"
            : string.Empty;
        string transport = request?.error;
        if (!string.IsNullOrWhiteSpace(bodyReason))
            return string.IsNullOrEmpty(status) ? bodyReason : $"{status}: {bodyReason}";
        if (!string.IsNullOrWhiteSpace(transport))
            return string.IsNullOrEmpty(status) ? transport : $"{status}: {transport}";
        return string.IsNullOrEmpty(status) ? "network error" : status;
    }

    private static ConductResponse ParseConduct(string text)
    {
        return ConductResponse.Parse(text);
    }
}

public sealed class WarningInfo
{
    public string Id;
    public string Body;

    public static WarningInfo From(Dictionary<string, object> row)
    {
        if (row == null) return null;
        return new WarningInfo
        {
            Id = row.TryGetValue("id", out var id) ? id?.ToString() ?? string.Empty : string.Empty,
            Body = row.TryGetValue("body", out var body) && body is string text ? text : body?.ToString() ?? string.Empty,
        };
    }
}

public sealed class ConductResponse
{
    public ConductResponse(int version, string body, bool consented, bool banned, BanInfo ban = null, WarningInfo warning = null)
    {
        Version = version;
        Body = body;
        Consented = consented;
        Banned = banned;
        Ban = ban;
        Warning = warning;
    }

    public int Version { get; }
    public string Body { get; }
    public bool Consented { get; }
    public bool Banned { get; }
    public BanInfo Ban { get; }
    public WarningInfo Warning { get; }
    public bool HasUnackedWarning => Warning != null && !string.IsNullOrEmpty(Warning.Id);

    public static ConductResponse Parse(string text)
    {
        if (text == null || JsonParser.Parse(text) is not Dictionary<string, object> root)
            return null;
        int version = root.TryGetValue("version", out var v) && v is long l ? (int)l : Convert.ToInt32(v ?? 0);
        string body = root.TryGetValue("body", out var b) && b is string s ? s : string.Empty;
        bool consented = root.TryGetValue("consented", out var c) && c is bool ok && ok;
        bool banned = root.TryGetValue("banned", out var ban) && ban is bool bannedOk && bannedOk;
        BanInfo banInfo = null;
        if (root.TryGetValue("ban", out var banRaw) && banRaw is Dictionary<string, object> banRow)
            banInfo = BanInfo.From(banRow);
        WarningInfo warning = null;
        if (root.TryGetValue("warning", out var warnRaw) && warnRaw is Dictionary<string, object> warnRow)
            warning = WarningInfo.From(warnRow);
        return new ConductResponse(version, body, consented, banned, banInfo, warning);
    }
}

public sealed class BlockRow
{
    public string Id;
    public string Name;
    public string Note;
    public string CreatedAt;

    public static BlockRow From(Dictionary<string, object> row)
    {
        return new BlockRow
        {
            Id = row.TryGetValue("id", out var id) ? id?.ToString() : string.Empty,
            Name = row.TryGetValue("name", out var name) ? name?.ToString() : string.Empty,
            Note = row.TryGetValue("note", out var note) ? note?.ToString() : string.Empty,
            CreatedAt = row.TryGetValue("created_at", out var created) ? created?.ToString() : string.Empty,
        };
    }
}

public sealed class BlockCreateResult
{
    public bool Ok;
    public bool KickTarget;
    public bool LeaveSelf;

    public static BlockCreateResult From(string text)
    {
        var result = new BlockCreateResult();
        if (text == null)
            return result;
        result.Ok = true;
        if (JsonParser.Parse(text) is Dictionary<string, object> root)
        {
            result.KickTarget = root.TryGetValue("kick_target", out var k) && k is bool kb && kb;
            result.LeaveSelf = root.TryGetValue("leave_self", out var l) && l is bool lb && lb;
        }
        return result;
    }
}

public sealed class PublicGameRow
{
    public string Code;
    public string HostName;
    public int PlayerCount;
    public int MaxPlayers;
    public bool HasBlockedPlayer;
    public string[] BlockedPlayerNames = Array.Empty<string>();

    public static PublicGameRow From(Dictionary<string, object> row)
    {
        string[] names = ReadStringList(row, "blocked_player_names");
        bool hasBlocked = row.TryGetValue("has_blocked_player", out var flag) && flag is bool b && b;
        return new PublicGameRow
        {
            Code = row.TryGetValue("code", out var code) ? code?.ToString() : string.Empty,
            HostName = row.TryGetValue("host_name", out var host) ? host?.ToString() : string.Empty,
            PlayerCount = ToInt(row, "player_count"),
            MaxPlayers = ToInt(row, "max_players"),
            HasBlockedPlayer = hasBlocked || names.Length > 0,
            BlockedPlayerNames = names,
        };
    }

    private static int ToInt(Dictionary<string, object> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value == null) return 0;
        return Convert.ToInt32(value);
    }

    private static string[] ReadStringList(Dictionary<string, object> row, string key)
    {
        if (row == null || !row.TryGetValue(key, out var value) || value is not List<object> list)
            return Array.Empty<string>();
        var names = new List<string>();
        foreach (var item in list)
        {
            string name = item?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
                names.Add(name.Trim());
        }
        return names.Count == 0 ? Array.Empty<string>() : names.ToArray();
    }
}

public sealed class RecentPlayerRow
{
    public string Name;
    public string PublicId;
    public int ClientId;
    public string GameCode;
    public int GameId;
    public string LastSeenAt;

    public static RecentPlayerRow From(Dictionary<string, object> row)
    {
        return new RecentPlayerRow
        {
            Name = ReadString(row, "name"),
            PublicId = ReadString(row, "public_id"),
            ClientId = ReadInt(row, "client_id"),
            GameCode = ReadString(row, "game_code"),
            GameId = ReadInt(row, "game_id"),
            LastSeenAt = ReadString(row, "last_seen_at"),
        };
    }

    private static string ReadString(Dictionary<string, object> row, string key)
    {
        return row.TryGetValue(key, out object value) ? value?.ToString() ?? string.Empty : string.Empty;
    }

    private static int ReadInt(Dictionary<string, object> row, string key)
    {
        return row.TryGetValue(key, out object value) && value != null ? Convert.ToInt32(value) : 0;
    }
}

public static class SafetyBlockLookup
{
    public static BlockRow Find(IEnumerable<BlockRow> rows, string rowIdOrName)
    {
        if (rows == null || string.IsNullOrWhiteSpace(rowIdOrName))
            return null;
        BlockRow byId = null;
        BlockRow byName = null;
        foreach (var row in rows)
        {
            if (row == null)
                continue;
            if (byId == null && string.Equals(row.Id, rowIdOrName, StringComparison.OrdinalIgnoreCase))
                byId = row;
            if (byName == null && string.Equals(row.Name, rowIdOrName, StringComparison.OrdinalIgnoreCase))
                byName = row;
        }
        return byId ?? byName;
    }
}
