using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
            using var document = System.Text.Json.JsonDocument.Parse(content);
            Token = document.RootElement.GetProperty("token").GetString() ?? string.Empty;
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
            Logger.Info(content);
            using var document = JsonDocument.Parse(content);
            var threads = document.RootElement.EnumerateArray().Select(thread => new Thread
            (
                title: thread.GetProperty("title").GetString() ?? string.Empty,
                thread_id: thread.GetProperty("thread_id").GetString() ?? string.Empty,
                first_message: thread.GetProperty("message").GetString() ?? string.Empty,
                created_at: thread.GetProperty("created_at").GetString() ?? string.Empty
            )).ToList();
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
            Logger.Info(content);
            using var document = JsonDocument.Parse(content);
            var messages = document.RootElement.EnumerateArray()
                .Select(msg => new Message(
                    message_id: msg.GetProperty("message_id").GetString() ?? string.Empty,
                    content: msg.GetProperty("content").GetString() ?? string.Empty,
                    sender: msg.TryGetProperty("sender", out var senderEl) && senderEl.ValueKind != JsonValueKind.Null
                        ? senderEl.GetString() ?? string.Empty
                        : string.Empty,
                    created_at: msg.GetProperty("created_at").GetString() ?? string.Empty
                )).ToList();
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
            var content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json");
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
            var content = new StringContent(JsonSerializer.Serialize(additionalInfo), System.Text.Encoding.UTF8, "application/json");
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