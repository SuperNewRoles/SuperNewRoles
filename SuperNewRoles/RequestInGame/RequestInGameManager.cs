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
        public Thread(string thread_id, string title, string first_message, string created_at)
        {
            this.thread_id = thread_id;
            this.title = title;
            this.first_message = first_message;
            this.created_at = DateTime.Parse(created_at);
        }
    }
    public class Message
    {
        public string message_id { get; }
        public string content { get; }
        public string sender { get; }
        public DateTime created_at { get; }
        public Message(string message_id, string content, string sender, string created_at)
        {
            this.message_id = message_id;
            this.content = content;
            this.sender = sender;
            this.created_at = DateTime.Parse(created_at);
        }
    }
    private static string Token = string.Empty;
    private static bool ValidatedToken = false;
    private static string FilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Innersloth", "SuperNewRoles", "RequestInGame.token");
    public static void Load()
    {
        Directory.CreateDirectory(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Innersloth", "SuperNewRoles"));
        if (File.Exists(FilePath))
        {
            Token = File.ReadAllText(FilePath);
            ValidatedToken = false;
        }
    }
    public static async Task<string> GetOrCreateToken()
    {
        if (!string.IsNullOrEmpty(Token))
        {
            if (!ValidatedToken)
            {
                using (HttpClient client = await CreateHttpClient(dontValidate: true))
                {
                    var response = await client.GetAsync("https://reports-api.supernewroles.com/validateToken/");
                    if (response.IsSuccessStatusCode)
                        ValidatedToken = true;
                    else
                        Logger.Error($"Failed to validate token: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                }
            }
            if (ValidatedToken)
                return Token;
        }

        using (HttpClient client = new())
        {
            var response = await client.PostAsync("https://reports-api.supernewroles.com/createAccount/", new StringContent(""));
            var content = await response.Content.ReadAsStringAsync();
            var jsonObj = JsonParser.Parse(content) as Dictionary<string, object>;
            Token = (jsonObj != null && jsonObj.TryGetValue("token", out var tokenVal) && tokenVal is string tokenStr) ? tokenStr : string.Empty;
            File.WriteAllText(FilePath, Token);
            ValidatedToken = true;
        }
        return Token;
    }
    public static async Task<List<Thread>> GetThreads(bool unreadOnly = false)
    {
        using (HttpClient client = await CreateHttpClient())
        {
            var response = await client.GetAsync($"https://reports-api.supernewroles.com/getThreads/");
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Failed to get threads: {response.StatusCode}");
                return new List<Thread>();
            }
            var content = await response.Content.ReadAsStringAsync();
            List<Thread> threads = new();
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
                        threads.Add(new Thread(threadId, title, firstMessage, createdAt));
                    }
                }
            }
            return threads;
        }
    }
    public static async Task<List<Message>> GetMessages(string thread_id)
    {
        using (HttpClient client = await CreateHttpClient())
        {
            var response = await client.GetAsync($"https://reports-api.supernewroles.com/getMessages/{thread_id}");
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Failed to get messages: {response.StatusCode}");
                return new List<Message>();
            }
            var content = await response.Content.ReadAsStringAsync();
            List<Message> messages = new();
            var root2 = JsonParser.Parse(content) as Dictionary<string, object>;
            if (root2 != null && root2.TryGetValue("messages", out var msgsValue) && msgsValue is List<object> msgsList)
            {
                foreach (var msgObj in msgsList)
                {
                    if (msgObj is Dictionary<string, object> msgDict)
                    {
                        string messageId = msgDict.TryGetValue("message_id", out var midVal) && midVal is string midStr ? midStr : string.Empty;
                        string msgContent2 = msgDict.TryGetValue("content", out var contVal) && contVal is string contStr ? contStr : string.Empty;
                        string sender = msgDict.TryGetValue("sender", out var senVal) && senVal is string senStr ? senStr : string.Empty;
                        string createdAt2 = msgDict.TryGetValue("created_at", out var ca2Val) && ca2Val is string ca2Str ? ca2Str : string.Empty;
                        messages.Add(new Message(messageId, msgContent2, sender, createdAt2));
                    }
                }
            }
            return messages;
        }
    }
    public static async Task<bool> SendMessage(string thread_id, string text)
    {
        using (HttpClient client = await CreateHttpClient())
        {
            Dictionary<string, string> data = new()
            {
                { "thread_id", thread_id },
                { "content", text }
            };
            var content = new StringContent(JsonConvert.SerializeObject(data.Wrap()), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://reports-api.supernewroles.com/sendMessage/{thread_id}", content);
            if (response.IsSuccessStatusCode)
            {
                Logger.Info($"Message sent: {text}");
                return true;
            }
            else
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                Logger.Error($"Message failed: {response.StatusCode} - {errorDetail} - {text}");
                return false;
            }
        }
    }
    public static async Task<bool> SendReport(string description, string title, string type, Dictionary<string, string> additionalInfo)
    {
        using (HttpClient client = await CreateHttpClient())
        {
            additionalInfo["message"] = description;
            additionalInfo["title"] = title;
            var content = new StringContent(JsonConvert.SerializeObject(additionalInfo.Wrap()), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://reports-api.supernewroles.com/sendRequest/{type}", content);
            if (response.IsSuccessStatusCode)
            {
                Logger.Info($"Report sent: {title} - {description}");
                return true;
            }
            else
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                Logger.Error($"Report failed: {response.StatusCode} - {errorDetail} - {title} - {description}");
                return false;
            }
        }
    }
    private static async Task<HttpClient> CreateHttpClient(bool dontValidate = false)
    {
        var client = new HttpClient();
        string token = dontValidate ? Token : await GetOrCreateToken();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}