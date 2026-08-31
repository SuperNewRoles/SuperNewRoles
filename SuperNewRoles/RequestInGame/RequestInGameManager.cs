using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using SuperNewRoles.Safety.Identity;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.IO.Compression;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

public enum RequestInGameType
{
    Bug,
    Question,
    Request,
    Thanks,
    Other,
}
public class RequestInGameManager
{
    public class Thread
    {
        public string thread_id { get; }
        public string title { get; }
        public string first_message { get; }
        public DateTime created_at { get; }
        public DateTime updated_at { get; }
        public bool unread { get; }
        public bool isSafetyNotice { get; }
        public IReadOnlyList<DateTime> safetyEvents { get; }
        public StatusData currentStatus { get; }
        public Thread(string thread_id, string title, string first_message, string created_at, bool unread, string status, string color, string mark, string updated_at = null, bool isSafetyNotice = false, IReadOnlyList<DateTime> safetyEvents = null)
        {
            this.thread_id = thread_id;
            this.title = title;
            this.first_message = first_message;
            this.created_at = ParseThreadTime(created_at, DateTime.UtcNow);
            this.updated_at = ParseThreadTime(updated_at, this.created_at);
            this.unread = unread;
            this.isSafetyNotice = isSafetyNotice;
            this.safetyEvents = safetyEvents ?? Array.Empty<DateTime>();
            // 何もなかったら黄緑色●のオープン表記
            this.currentStatus = new StatusData(status, string.IsNullOrEmpty(color) ? "#32CD32" : color, string.IsNullOrEmpty(mark) ? "●" : mark);
        }
    }
    public abstract class MessageBase
    {
        public abstract MessageType MsgType { get; }
        public virtual string message_id { get; }
        public virtual DateTime created_at { get; }
        public MessageBase(string message_id, string created_at)
        {
            this.message_id = message_id;
            this.created_at = DateTime.Parse(created_at);
        }
    }
    public enum MessageType
    {
        normal,
        status,
    }
    public struct StatusData
    {
        public string status { get; }
        public string color { get; }
        public string mark { get; }
        public StatusData(string status, string color, string mark)
        {
            this.status = status;
            this.color = color;
            this.mark = mark;
        }
    }
    public class StatusUpdate : MessageBase
    {
        public override MessageType MsgType => MessageType.status;
        public StatusData status { get; }
        public StatusUpdate(string message_id, string created_at, string status, string color, string mark) : base(message_id, created_at)
        {
            this.status = new StatusData(status, color, mark);
        }
    }
    public class Message : MessageBase
    {
        public override MessageType MsgType => MessageType.normal;
        public string content { get; }
        public string sender { get; }
        public Message(string message_id, string created_at, string content, string sender) : base(message_id, created_at)
        {
            this.content = content;
            this.sender = sender;
        }
    }
    private static DateTime ParseThreadTime(string raw, DateTime fallback)
    {
        // RequestInGame API は naive UTC（末尾 Z なし）を返すことがある。ローカル時刻扱いすると受信箱の時系列が崩れる。
        if (!string.IsNullOrEmpty(raw) &&
            DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset parsed))
            return parsed.UtcDateTime;
        return fallback;
    }

    private static T Get<T>(Dictionary<string, object> dict, string key, T defaultValue = default)
    {
        if (dict == null)
            return defaultValue;
        if (dict.TryGetValue(key, out var value))
        {
            if (value is T result)
                return result;
        }
        return defaultValue;
    }

    private static string Token = string.Empty;
    private static bool ValidatedToken = false;
    private static string FilePath = Path.Combine(SuperNewRolesPlugin.SecretDirectory, "RequestInGame.token");
    public const string StatusUpdater = "StatusUpdater";
    internal static bool ShouldRecreateTokenAfterValidationFailure(UnityWebRequest.Result result, long responseCode, bool createIfMissing)
    {
        return createIfMissing && result == UnityWebRequest.Result.ProtocolError && responseCode is 401 or 403;
    }

    public static void Load()
    {
        if (File.Exists(FilePath))
        {
            Token = File.ReadAllText(FilePath);
            ValidatedToken = false;
        }
    }
    public static IEnumerator GetOrCreateToken(Action<string> callback, bool createIfMissing = true)
    {
        if (!string.IsNullOrEmpty(Token))
        {
            if (!ValidatedToken)
            {
                var validateUrl = $"{SNRURLs.ReportInGameAPI}/validateToken/";
                var validateRequest = UnityWebRequest.Get(validateUrl);
                validateRequest.SetRequestHeader("Authorization", $"Bearer {Token}");
                yield return validateRequest.SendWebRequest();
                if (validateRequest.result == UnityWebRequest.Result.Success && validateRequest.responseCode == 200)
                {
                    ValidatedToken = true;
                }
                else
                {
                    Logger.Error($"Failed to validate token: {validateRequest.responseCode} {validateRequest.error}");
                    if (!ShouldRecreateTokenAfterValidationFailure(validateRequest.result, validateRequest.responseCode, createIfMissing))
                    {
                        if (validateRequest.result == UnityWebRequest.Result.ProtocolError && validateRequest.responseCode is 401 or 403)
                        {
                            Logger.Info("Stored RequestInGame token was rejected, and this path does not create new tokens.");
                            validateRequest.Dispose();
                            callback(null);
                            yield break;
                        }
                        Logger.Info("Keeping stored RequestInGame token because validation failure was not an authentication rejection.");
                        validateRequest.Dispose();
                        callback(Token);
                        yield break;
                    }
                    Logger.Info("Stored RequestInGame token was rejected by the server. Attempting to create a new account.");
                }
                validateRequest.Dispose();
            }
            if (ValidatedToken)
            {
                callback(Token);
                yield break;
            }
        }

        if (!createIfMissing)
        {
            callback(null);
            yield break;
        }

        var createUrl = $"{SNRURLs.ReportInGameAPI}/createAccount/";
        var createRequest = new UnityWebRequest(createUrl, "POST");
        createRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return createRequest.SendWebRequest();
        if (createRequest.result == UnityWebRequest.Result.Success && createRequest.responseCode == 200)
        {
            var createContent = createRequest.downloadHandler.text;
            var jsonObj = JsonParser.Parse(createContent) as Dictionary<string, object>;
            string createdToken = (jsonObj != null && jsonObj.TryGetValue("token", out var tokenVal) && tokenVal is string tokenStr) ? tokenStr : string.Empty;
            if (string.IsNullOrEmpty(createdToken))
            {
                Logger.Error("Failed to create account: response did not include a token.");
                callback(null);
                createRequest.Dispose();
                yield break;
            }

            Token = createdToken;
            File.WriteAllText(FilePath, Token);
            ValidatedToken = true;
            callback(Token);
        }
        else
        {
            Logger.Error($"Failed to create account: {createRequest.responseCode}");
            callback(null);
        }
        createRequest.Dispose();
    }
    // unreadonly not working
    public static IEnumerator GetThreads(Action<List<Thread>> callback, bool unreadOnly = false)
    {
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t, createIfMissing: false);
        if (string.IsNullOrEmpty(token))
        {
            callback(new List<Thread>());
            yield break;
        }

        string url = $"{SNRURLs.ReportInGameAPI}/getThreads/";
        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        List<Thread> threads = new List<Thread>();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Failed to get threads: {request.responseCode}");
            callback(null);
        }
        else
        {
            var content = request.downloadHandler.text;
            var root = JsonParser.Parse(content) as Dictionary<string, object>;
            if (root != null && root.TryGetValue("threads", out var threadsValue) && threadsValue is List<object> threadsList)
            {
                foreach (var threadObj in threadsList)
                {
                    if (threadObj is Dictionary<string, object> threadDict)
                    {
                        string title = Get<string>(threadDict, "title");
                        string threadId = Get<string>(threadDict, "thread_id");
                        string firstMessage = Get<string>(threadDict, "message");
                        string createdAt = Get<string>(threadDict, "created_at");
                        string updatedAt = Get<string>(threadDict, "last_updated");
                        bool unread = Get<bool>(threadDict, "has_unread_messages");
                        Dictionary<string, object> statusDict = Get<Dictionary<string, object>>(threadDict, "status");
                        string status = Get<string>(statusDict, "status");
                        // 16進数をColor32に変換
                        string color = Get<string>(statusDict, "color");
                        string mark = Get<string>(statusDict, "mark");
                        threads.Add(new Thread(threadId, title, firstMessage, createdAt, unread, status, color, mark, updatedAt));
                    }
                }
            }
            callback(threads);
        }
        request.Dispose();
    }
    public static IEnumerator GetMessages(string thread_id, Action<List<MessageBase>> callback)
    {
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t, createIfMissing: false);
        if (string.IsNullOrEmpty(token))
        {
            callback(null);
            yield break;
        }

        string url = $"{SNRURLs.ReportInGameAPI}/getMessages/{thread_id}";
        var request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        List<MessageBase> messages = new();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.Error($"Failed to get messages: {request.responseCode}");
            callback(null);
        }
        else
        {
            var content = request.downloadHandler.text;
            var root2 = JsonParser.Parse(content) as Dictionary<string, object>;
            if (root2 != null && root2.TryGetValue("messages", out var msgsValue) && msgsValue is List<object> msgsList)
            {
                foreach (var msgObj in msgsList)
                {
                    if (msgObj is Dictionary<string, object> msgDict)
                    {
                        string messageId = msgDict.TryGetValue("message_id", out var midVal) && midVal is string midStr ? midStr : string.Empty;
                        string createdAt = msgDict.TryGetValue("created_at", out var caVal) && caVal is string caStr ? caStr : string.Empty;
                        switch (msgDict.TryGetValue("type", out var typeVal) && typeVal is string typeStr ? typeStr : string.Empty)
                        {
                            case nameof(MessageType.normal):
                                string msgContent2 = msgDict.TryGetValue("content", out var contVal) && contVal is string contStr ? contStr : string.Empty;
                                string sender = msgDict.TryGetValue("sender", out var senVal) && senVal is string senStr ? senStr : string.Empty;
                                messages.Add(new Message(messageId, createdAt, msgContent2, sender));
                                break;
                            case nameof(MessageType.status):
                                Logger.Debug(string.Join(", ", msgDict.Select(kv => $"{kv.Key}: {kv.Value}")));
                                string status = msgDict.TryGetValue("content", out var statusVal) && statusVal is string statusStr ? statusStr : string.Empty;
                                string color = msgDict.TryGetValue("color", out var colorVal) && colorVal is string colorStr ? colorStr : string.Empty;
                                string mark = msgDict.TryGetValue("mark", out var markVal) && markVal is string markStr ? markStr : string.Empty;
                                messages.Add(new StatusUpdate(messageId, createdAt, status, color, mark));
                                break;
                        }
                    }
                }
            }
            callback(messages);
        }
        request.Dispose();
    }
    public static IEnumerator SendMessage(string thread_id, string text, Action<bool> callback)
    {
        Dictionary<string, string> data = new()
        {
            { "thread_id", thread_id },
            { "content", text }
        };
        var bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data.Wrap()));
        string url = $"{SNRURLs.ReportInGameAPI}/sendMessage/{thread_id}";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyBytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t, createIfMissing: false);
        if (string.IsNullOrEmpty(token))
        {
            request.Dispose();
            callback(false);
            yield break;
        }
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();
        bool success = false;
        if (request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 300)
        {
            Logger.Info($"Message sent: {text}");
            success = true;
        }
        else
        {
            var errorDetail = request.error ?? request.downloadHandler.text;
            Logger.Error($"Message failed: {request.responseCode} - {errorDetail} - {text}");
        }
        request.Dispose();
        callback(success);
    }
    public static IEnumerator SendReport(string description, string title, string type, Dictionary<string, string> additionalInfo, Action<bool> callback, Action<float> progressCallback = null)
    {
        additionalInfo["message"] = description;
        additionalInfo["title"] = title;
        additionalInfo["version"] = "SNR:" + Statics.VersionString + "&AmongUs:" + Constants.GetBroadcastVersion();
        additionalInfo["platform"] = Constants.GetPurchasingPlatformType();
        string data = JsonConvert.SerializeObject(additionalInfo.Wrap());

        string url = $"{SNRURLs.ReportInGameAPI}/sendRequest/{type}";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.chunkedTransfer = true;
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t, createIfMissing: true);
        if (string.IsNullOrEmpty(token))
        {
            Logger.Error($"Failed to get token for report: {title}");
            request.Dispose();
            callback(false);
            yield break;
        }
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            Logger.Info($"Sending report progress: {request.uploadProgress * 100f:F1}%");
            progressCallback?.Invoke(request.uploadProgress * 100);
            yield return null;
        }
        progressCallback?.Invoke(request.uploadProgress * 100); // Ensure final progress is reported

        bool success = false;
        if (request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 300)
        {
            Logger.Info($"Report sent: {title} - {description}");
            success = true;
        }
        else
        {
            var errorDetail = request.error ?? request.downloadHandler.text;
            Logger.Error($"Report failed: {request.responseCode} - {errorDetail} - {title} - {description}");
        }
        request.Dispose();
        callback(success);
    }

    public static IEnumerator hasNotifications(Action<bool> callback)
    {
        bool hasNotifications = false;
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t, createIfMissing: false);
        if (!string.IsNullOrEmpty(token))
        {
            string url = $"{SNRURLs.ReportInGameAPI}/getNotification/";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var root = JsonParser.Parse(request.downloadHandler.text) as Dictionary<string, object>;
                if (root != null && root.TryGetValue("notification", out var notificationVal) && notificationVal is bool notificationBool)
                    hasNotifications = notificationBool;
            }
            request.Dispose();
        }
        if (hasNotifications)
        {
            callback(true);
            yield break;
        }
        bool identityUnread = false;
        if (PlayerIdentityStore.HasKey())
            yield return PlayerSafetyApiClient.GetNotices(inbox => identityUnread = inbox != null && inbox.Unread);
        callback(identityUnread);
    }
}
