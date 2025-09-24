using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
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
        public bool unread { get; }
        public StatusData currentStatus { get; }
        public Thread(string thread_id, string title, string first_message, string created_at, bool unread, string status, string color, string mark)
        {
            this.thread_id = thread_id;
            this.title = title;
            this.first_message = first_message;
            this.created_at = DateTime.Parse(created_at);
            this.unread = unread;
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
    private static string Token = string.Empty;
    private static bool ValidatedToken = false;
    private static string FilePath = Path.Combine(SuperNewRolesPlugin.SecretDirectory, "RequestInGame.token");
    public const string StatusUpdater = "StatusUpdater";

    public static void Load()
    {
        if (File.Exists(FilePath))
        {
            Token = File.ReadAllText(FilePath);
            ValidatedToken = false;
        }
    }
    public static IEnumerator GetOrCreateToken(Action<string> callback)
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
                    ValidatedToken = true;
                else
                    Logger.Error($"Failed to validate token: {validateRequest.responseCode} {validateRequest.error}");
                validateRequest.Dispose();
            }
            if (ValidatedToken)
            {
                callback(Token);
                yield break;
            }
        }

        var createUrl = $"{SNRURLs.ReportInGameAPI}/createAccount/";
        var createRequest = new UnityWebRequest(createUrl, "POST");
        createRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return createRequest.SendWebRequest();
        if (createRequest.result == UnityWebRequest.Result.Success && createRequest.responseCode == 200)
        {
            var createContent = createRequest.downloadHandler.text;
            var jsonObj = JsonParser.Parse(createContent) as Dictionary<string, object>;
            Token = (jsonObj != null && jsonObj.TryGetValue("token", out var tokenVal) && tokenVal is string tokenStr) ? tokenStr : string.Empty;
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
        string url = $"{SNRURLs.ReportInGameAPI}/getThreads/";
        var request = UnityWebRequest.Get(url);
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t);
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
                        string title = threadDict.TryGetValue("title", out var titleVal) && titleVal is string titleStr ? titleStr : string.Empty;
                        string threadId = threadDict.TryGetValue("thread_id", out var idVal) && idVal is string idStr ? idStr : string.Empty;
                        string firstMessage = threadDict.TryGetValue("message", out var msgVal) && msgVal is string msgStr ? msgStr : string.Empty;
                        string createdAt = threadDict.TryGetValue("created_at", out var caVal) && caVal is string caStr ? caStr : string.Empty;
                        bool unread = threadDict.TryGetValue("has_unread_messages", out var unreadVal) && unreadVal is bool unreadBool ? unreadBool : false;
                        Dictionary<string, object> statusDict = threadDict.TryGetValue("status", out var statusVal) && statusVal is Dictionary<string, object> statusDict2 ? statusDict2 : null;
                        string status = statusDict != null && statusDict.TryGetValue("status", out var statusStr) && statusStr is string statusStr2 ? statusStr2 : string.Empty;
                        // 16進数をColor32に変換
                        string color = statusDict != null && statusDict.TryGetValue("color", out var colorVal) && colorVal is string colorStr ? colorStr : string.Empty;
                        string mark = statusDict != null && statusDict.TryGetValue("mark", out var markVal) && markVal is string markStr ? markStr : string.Empty;
                        threads.Add(new Thread(threadId, title, firstMessage, createdAt, unread, status, color, mark));
                    }
                }
            }
            callback(threads);
        }
        request.Dispose();
    }
    public static IEnumerator GetMessages(string thread_id, Action<List<MessageBase>> callback)
    {
        string url = $"{SNRURLs.ReportInGameAPI}/getMessages/{thread_id}";
        var request = UnityWebRequest.Get(url);
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t);
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
        yield return GetOrCreateToken(t => token = t);
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
        string data = JsonConvert.SerializeObject(additionalInfo.Wrap());

        string url = $"{SNRURLs.ReportInGameAPI}/sendRequest/{type}";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.chunkedTransfer = true;
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t);
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
        string url = $"{SNRURLs.ReportInGameAPI}/getNotification/";
        var request = UnityWebRequest.Get(url);
        string token = string.Empty;
        yield return GetOrCreateToken(t => token = t);
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return request.SendWebRequest();

        bool hasNotifications = false;
        if (request.result == UnityWebRequest.Result.Success)
        {
            var root = JsonParser.Parse(request.downloadHandler.text) as Dictionary<string, object>;
            if (root != null && root.TryGetValue("notification", out var notificationVal) && notificationVal is bool notificationBool)
                hasNotifications = notificationBool;
        }
        callback(hasNotifications);
    }
}