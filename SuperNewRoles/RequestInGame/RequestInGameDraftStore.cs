using System;
using System.Collections.Generic;
using System.IO;
using SuperNewRoles.Modules;

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
            if (JsonParser.Parse(json) is not Dictionary<string, object> parsed)
                return new Dictionary<string, RequestInGameDraft>();

            Dictionary<string, RequestInGameDraft> drafts = new();
            foreach (var pair in parsed)
            {
                if (pair.Value is Dictionary<string, object> draftDict)
                    drafts[pair.Key] = ParseDraft(draftDict);
            }
            return drafts;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonParseException)
        {
            return new Dictionary<string, RequestInGameDraft>();
        }
    }

    private static void SaveAll(Dictionary<string, RequestInGameDraft> drafts)
    {
        try
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

            string json = JsonParser.Serialize(SerializeDrafts(drafts));
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception)
        {
            // Draft persistence is best-effort; report UI should keep working if saving fails.
        }
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

    private static Dictionary<string, object> SerializeDrafts(Dictionary<string, RequestInGameDraft> drafts)
    {
        Dictionary<string, object> serializedDrafts = new();
        foreach (var pair in drafts)
            serializedDrafts[pair.Key] = SerializeDraft(pair.Value);
        return serializedDrafts;
    }

    private static Dictionary<string, object> SerializeDraft(RequestInGameDraft draft)
    {
        draft = Normalize(draft);
        return new Dictionary<string, object>
        {
            ["Title"] = draft.Title,
            ["Description"] = draft.Description,
            ["Map"] = draft.Map,
            ["Role"] = draft.Role,
            ["Timing"] = draft.Timing
        };
    }

    private static RequestInGameDraft ParseDraft(Dictionary<string, object> draft)
    {
        return Normalize(new RequestInGameDraft(
            GetString(draft, "Title"),
            GetString(draft, "Description"),
            GetString(draft, "Map"),
            GetString(draft, "Role"),
            GetString(draft, "Timing")));
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
        return dict.TryGetValue(key, out object value) && value is string stringValue
            ? stringValue
            : string.Empty;
    }
}
