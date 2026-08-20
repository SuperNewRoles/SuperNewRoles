using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Identity;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Safety.Api;

public static class PlayerSafetyApiClient
{
    public const string ConductGet = "conduct.get";
    public const string ConductConsent = "conduct.consent";
    public const string BlocksList = "blocks.list";
    public const string BlocksCreate = "blocks.create";
    public const string BlocksUpdate = "blocks.update";
    public const string BlocksDelete = "blocks.delete";
    public const string GamesList = "games.list";
    public const string ReportsCreate = "reports.create";

    public static IEnumerator GetConduct(Action<ConductResponse> callback)
    {
        yield return Send("GET", "/v1/conduct", ConductGet, null, text =>
        {
            callback(ParseConduct(text));
        });
    }

    public static IEnumerator Consent(int version, Action<bool> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object> { ["version"] = version });
        yield return Send("POST", "/v1/conduct/consent", ConductConsent, json, text => callback(text != null));
    }

    public static IEnumerator ListBlocks(Action<List<BlockRow>> callback)
    {
        yield return Send("GET", "/v1/blocks", BlocksList, null, text =>
        {
            var rows = new List<BlockRow>();
            if (text != null && JsonParser.Parse(text) is Dictionary<string, object> root
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

    public static IEnumerator CreateBlock(string gameCode, int clientId, string note, Action<BlockCreateResult> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object>
        {
            ["game_code"] = gameCode,
            ["client_id"] = clientId,
            ["note"] = note ?? string.Empty,
        });
        yield return Send("POST", "/v1/blocks", BlocksCreate, json, text =>
        {
            callback(BlockCreateResult.From(text));
        });
    }

    public static IEnumerator UpdateBlockNote(string rowId, string note, Action<bool> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object> { ["note"] = note ?? string.Empty });
        yield return Send("PATCH", "/v1/blocks/" + rowId, BlocksUpdate, json, text => callback(text != null));
    }

    public static IEnumerator DeleteBlock(string rowId, Action<bool> callback)
    {
        yield return Send("DELETE", "/v1/blocks/" + rowId, BlocksDelete, null, text => callback(true), acceptEmpty: true);
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

    public static IEnumerator CreateReport(string gameCode, int clientId, string category, string comment, string chat, Action<bool> callback)
    {
        string json = JsonParser.Serialize(new Dictionary<string, object>
        {
            ["game_code"] = gameCode,
            ["client_id"] = clientId,
            ["category"] = category ?? "other",
            ["comment"] = comment ?? string.Empty,
            ["chat"] = chat ?? string.Empty,
        });
        yield return Send("POST", "/v1/reports", ReportsCreate, json, text => callback(text != null));
    }

    private static IEnumerator Send(string method, string path, string action, string jsonBody, Action<string> callback, bool acceptEmpty = false)
    {
        byte[] bodyBytes = jsonBody == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(jsonBody);
        PlayerIdentityProof proof = PlayerIdentityStore.CreateProof(action, bodyBytes);
        var request = new UnityWebRequest(SNRURLs.IdentityAPI + path, method)
        {
            downloadHandler = new DownloadHandlerBuffer(),
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
            callback(acceptEmpty ? (text ?? string.Empty) : text);
        }
        else
        {
            Logger.Error($"PlayerSafety API {path} failed: {request.responseCode} {request.error} {request.downloadHandler?.text}");
            callback(null);
        }
        request.Dispose();
    }

    private static ConductResponse ParseConduct(string text)
    {
        if (text == null || JsonParser.Parse(text) is not Dictionary<string, object> root)
            return null;
        int version = root.TryGetValue("version", out var v) && v is long l ? (int)l : Convert.ToInt32(v ?? 0);
        string body = root.TryGetValue("body", out var b) && b is string s ? s : string.Empty;
        bool consented = root.TryGetValue("consented", out var c) && c is bool ok && ok;
        bool banned = root.TryGetValue("banned", out var ban) && ban is bool bannedOk && bannedOk;
        return new ConductResponse(version, body, consented, banned);
    }
}

public sealed class ConductResponse
{
    public ConductResponse(int version, string body, bool consented, bool banned)
    {
        Version = version;
        Body = body;
        Consented = consented;
        Banned = banned;
    }

    public int Version { get; }
    public string Body { get; }
    public bool Consented { get; }
    public bool Banned { get; }
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
    public bool KickTarget;
    public bool LeaveSelf;

    public static BlockCreateResult From(string text)
    {
        var result = new BlockCreateResult();
        if (text != null && JsonParser.Parse(text) is Dictionary<string, object> root)
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

    public static PublicGameRow From(Dictionary<string, object> row)
    {
        return new PublicGameRow
        {
            Code = row.TryGetValue("code", out var code) ? code?.ToString() : string.Empty,
            HostName = row.TryGetValue("host_name", out var host) ? host?.ToString() : string.Empty,
            PlayerCount = ToInt(row, "player_count"),
            MaxPlayers = ToInt(row, "max_players"),
            HasBlockedPlayer = row.TryGetValue("has_blocked_player", out var flag) && flag is bool b && b,
        };
    }

    private static int ToInt(Dictionary<string, object> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value == null) return 0;
        return Convert.ToInt32(value);
    }
}
