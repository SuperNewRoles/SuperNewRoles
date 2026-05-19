using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SuperNewRoles.RequestInGame;

public record RequestInGameDraft(string Title, string Description, string Map, string Role, string Timing)
{
    public static RequestInGameDraft Empty => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public bool IsEmpty()
    {
        return
        string.IsNullOrEmpty(Title) &&
        string.IsNullOrEmpty(Description) &&
        string.IsNullOrEmpty(Map) &&
        string.IsNullOrEmpty(Role) &&
        string.IsNullOrEmpty(Timing);
    }
}

public static class RequestInGameDraftStore
{
    private const string SaveFileName = "RequestInGameDrafts.json";
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };
    private static string testSaveFilePath;

    public static RequestInGameDraft Load(RequestInGameType requestInGameType)
    {
        Dictionary<string, RequestInGameDraft> drafts = LoadAll();
        return drafts.TryGetValue(GetDraftKey(requestInGameType), out RequestInGameDraft draft)
            ? Normalize(draft)
            : RequestInGameDraft.Empty;
    }

    public static void Save(RequestInGameType requestInGameType, RequestInGameDraft draft)
    {
        draft = Normalize(draft);
        if (draft.IsEmpty())
        {
            Clear(requestInGameType);
            return;
        }

        Dictionary<string, RequestInGameDraft> drafts = LoadAll();
        drafts[GetDraftKey(requestInGameType)] = draft;
        SaveAll(drafts);
    }

    public static void Clear(RequestInGameType requestInGameType)
    {
        Dictionary<string, RequestInGameDraft> drafts = LoadAll();
        if (!drafts.Remove(GetDraftKey(requestInGameType)))
            return;

        SaveAll(drafts);
    }

    public static void SetTestSaveFilePath(string saveFilePath)
    {
        testSaveFilePath = saveFilePath;
    }

    public static void ClearTestSaveFilePath()
    {
        testSaveFilePath = null;
    }

    private static string SaveFilePath =>
        testSaveFilePath ?? Path.Combine(SuperNewRolesPlugin.BaseDirectory, "SaveData", SaveFileName);

    private static string GetDraftKey(RequestInGameType requestInGameType)
    {
        return requestInGameType.ToString();
    }

    private static Dictionary<string, RequestInGameDraft> LoadAll()
    {
        string saveFilePath = SaveFilePath;
        if (!File.Exists(saveFilePath))
            return new Dictionary<string, RequestInGameDraft>();

        try
        {
            string json = File.ReadAllText(saveFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, RequestInGameDraft>>(json)
                ?? new Dictionary<string, RequestInGameDraft>();
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
        {
            return new Dictionary<string, RequestInGameDraft>();
        }
    }

    private static void SaveAll(Dictionary<string, RequestInGameDraft> drafts)
    {
        string saveFilePath = SaveFilePath;
        string directory = Path.GetDirectoryName(saveFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (drafts.Count == 0)
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
            return;
        }

        string json = JsonSerializer.Serialize(drafts, JsonSerializerOptions);
        File.WriteAllText(saveFilePath, json);
    }

    private static RequestInGameDraft Normalize(RequestInGameDraft draft)
    {
        if (draft == null)
            return RequestInGameDraft.Empty;

        return new RequestInGameDraft(
            draft.Title ?? string.Empty,
            draft.Description ?? string.Empty,
            draft.Map ?? string.Empty,
            draft.Role ?? string.Empty,
            draft.Timing ?? string.Empty);
    }
}
