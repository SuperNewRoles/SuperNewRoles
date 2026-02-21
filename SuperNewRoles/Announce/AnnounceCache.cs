using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperNewRoles.Modules;

/// <summary>
/// キャッシュされた個別記事とリストのデータを管理します。
/// 言語と記事IDをキーにしてキャッシュを保存/読み込みます。
/// </summary>
public static class AnnounceCache
{
    private static readonly string CacheDirectory = Path.Combine(SuperNewRolesPlugin.BaseDirectory, "AnnounceCache");
    private static readonly string ListCacheFileName = "list_cache.json";

    // メモリキャッシュ
    private static Dictionary<string, CachedArticle> _articleCache = new();
    private static Dictionary<string, CachedArticlesList> _listCache = new();

    /// <summary>
    /// 個別記事をキャッシュから取得します。
    /// </summary>
    /// <param name="id">記事ID</param>
    /// <param name="lang">言語コード</param>
    /// <returns>キャッシュされた記事、または null</returns>
    public static CachedArticle GetArticle(string id, string lang)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(lang))
            return null;

        string key = GetArticleKey(id, lang);

        // メモリキャッシュをチェック
        if (_articleCache.TryGetValue(key, out var cached))
            return cached;

        // ディスクキャッシュから読み込み
        try
        {
            EnsureCacheDirectory();
            string filePath = GetArticleFilePath(id, lang);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var article = JsonParser.Parse(json) as Dictionary<string, object>;
                if (article != null)
                {
                    var cachedArticle = ParseCachedArticle(article);
                    if (cachedArticle != null)
                    {
                        _articleCache[key] = cachedArticle;
                        return cachedArticle;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to load article cache for {id}/{lang}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 個別記事をキャッシュに保存します。
    /// </summary>
    public static void SaveArticle(string id, string lang, Article article, string etag = null)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(lang) || article == null)
            return;

        string key = GetArticleKey(id, lang);
        var cached = new CachedArticle
        {
            Id = id,
            Lang = lang,
            Article = article,
            ETag = etag ?? string.Empty,
            CachedAt = DateTime.UtcNow.ToString("o")
        };

        // メモリキャッシュに保存
        _articleCache[key] = cached;

        // ディスクに保存
        try
        {
            EnsureCacheDirectory();
            string filePath = GetArticleFilePath(id, lang);
            string json = SerializeCachedArticle(cached);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to save article cache for {id}/{lang}: {ex.Message}");
        }
    }

    /// <summary>
    /// 記事リストをキャッシュから取得します。
    /// </summary>
    public static CachedArticlesList GetArticlesList(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
            return null;

        string key = GetListKey(lang);

        // メモリキャッシュをチェック
        if (_listCache.TryGetValue(key, out var cached))
            return cached;

        // ディスクキャッシュから読み込み
        try
        {
            EnsureCacheDirectory();
            string filePath = GetListFilePath(lang);
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var list = JsonParser.Parse(json) as Dictionary<string, object>;
                if (list != null)
                {
                    var cachedList = ParseCachedArticlesList(list);
                    if (cachedList != null)
                    {
                        _listCache[key] = cachedList;
                        return cachedList;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to load articles list cache for {lang}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 記事リストをキャッシュに保存します。
    /// </summary>
    public static void SaveArticlesList(string lang, ArticlesResponse response, string etag = null)
    {
        if (string.IsNullOrWhiteSpace(lang) || response == null)
            return;

        string key = GetListKey(lang);
        var cached = new CachedArticlesList
        {
            Lang = lang,
            Response = response,
            ETag = etag ?? string.Empty,
            CachedAt = DateTime.UtcNow.ToString("o")
        };

        // メモリキャッシュに保存
        _listCache[key] = cached;

        // ディスクに保存
        try
        {
            EnsureCacheDirectory();
            string filePath = GetListFilePath(lang);
            string json = SerializeCachedArticlesList(cached);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to save articles list cache for {lang}: {ex.Message}");
        }
    }

    /// <summary>
    /// すべてのキャッシュをクリアします。
    /// </summary>
    public static void ClearAll()
    {
        _articleCache.Clear();
        _listCache.Clear();

        try
        {
            if (Directory.Exists(CacheDirectory))
                Directory.Delete(CacheDirectory, true);
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"Failed to clear cache directory: {ex.Message}");
        }
    }

    private static void EnsureCacheDirectory()
    {
        if (!Directory.Exists(CacheDirectory))
            Directory.CreateDirectory(CacheDirectory);
    }

    private static string GetArticleKey(string id, string lang) => $"article_{ModHelpers.HashMD5($"{id ?? string.Empty}\n{lang ?? string.Empty}")}";
    private static string GetListKey(string lang) => $"list_{ModHelpers.HashMD5(lang ?? string.Empty)}";
    private static string GetArticleFilePath(string id, string lang) => Path.Combine(CacheDirectory, $"{GetArticleKey(id, lang)}.json");
    private static string GetListFilePath(string lang) => Path.Combine(CacheDirectory, $"{GetListKey(lang)}_{ListCacheFileName}");

    private static CachedArticle ParseCachedArticle(Dictionary<string, object> dict)
    {
        if (!dict.TryGetValue("id", out var idObj) ||
            !dict.TryGetValue("lang", out var langObj) ||
            !dict.TryGetValue("article", out var articleObj))
            return null;

        string id = idObj?.ToString() ?? string.Empty;
        string lang = langObj?.ToString() ?? string.Empty;
        string etag = dict.TryGetValue("etag", out var etagObj) ? etagObj?.ToString() ?? string.Empty : string.Empty;
        string cachedAt = dict.TryGetValue("cached_at", out var cachedAtObj) ? cachedAtObj?.ToString() ?? string.Empty : string.Empty;

        if (articleObj is not Dictionary<string, object> articleDict)
            return null;

        var article = ParseArticle(articleDict);
        if (article == null)
            return null;

        return new CachedArticle
        {
            Id = id,
            Lang = lang,
            Article = article,
            ETag = etag,
            CachedAt = cachedAt
        };
    }

    private static CachedArticlesList ParseCachedArticlesList(Dictionary<string, object> dict)
    {
        if (!dict.TryGetValue("lang", out var langObj) ||
            !dict.TryGetValue("response", out var responseObj))
            return null;

        string lang = langObj?.ToString() ?? string.Empty;
        string etag = dict.TryGetValue("etag", out var etagObj) ? etagObj?.ToString() ?? string.Empty : string.Empty;
        string cachedAt = dict.TryGetValue("cached_at", out var cachedAtObj) ? cachedAtObj?.ToString() ?? string.Empty : string.Empty;

        if (responseObj is not Dictionary<string, object> responseDict)
            return null;

        var response = ParseArticlesResponse(responseDict);
        if (response == null)
            return null;

        return new CachedArticlesList
        {
            Lang = lang,
            Response = response,
            ETag = etag,
            CachedAt = cachedAt
        };
    }

    private static Article ParseArticle(Dictionary<string, object> dict)
    {
        string id = GetString(dict, "id");
        string title = GetString(dict, "title");
        string url = GetString(dict, "url");
        List<Tag> tags = new();
        if (dict.TryGetValue("tags", out var tagsVal) && tagsVal is List<object> tagList)
        {
            foreach (var tag in tagList)
            {
                if (tag is Dictionary<string, object> tagDict)
                    tags.Add(ParseTag(tagDict));
            }
        }
        string lang = GetString(dict, "lang");
        string requestedLang = GetString(dict, "requested_lang");
        bool isFallback = GetBool(dict, "is_fallback");
        string createdAt = GetString(dict, "created_at");
        string updatedAt = GetString(dict, "updated_at");
        string body = GetString(dict, "body");

        var minimal = new ArticleMinimal(id, title, url, tags, lang, requestedLang, isFallback, createdAt, updatedAt);
        return new Article(minimal, body);
    }

    private static ArticlesResponse ParseArticlesResponse(Dictionary<string, object> dict)
    {
        List<ArticleMinimal> items = new();
        if (dict.TryGetValue("items", out var itemsVal) && itemsVal is List<object> itemList)
        {
            foreach (var item in itemList)
            {
                if (item is Dictionary<string, object> itemDict)
                    items.Add(ParseArticleMinimal(itemDict));
            }
        }

        int page = GetInt(dict, "page");
        int pageSize = GetInt(dict, "page_size");
        int total = GetInt(dict, "total");
        return new ArticlesResponse(items, page, pageSize, total);
    }

    private static ArticleMinimal ParseArticleMinimal(Dictionary<string, object> dict)
    {
        string id = GetString(dict, "id");
        string title = GetString(dict, "title");
        string url = GetString(dict, "url");
        List<Tag> tags = new();
        if (dict.TryGetValue("tags", out var tagsVal) && tagsVal is List<object> tagList)
        {
            foreach (var tag in tagList)
            {
                if (tag is Dictionary<string, object> tagDict)
                    tags.Add(ParseTag(tagDict));
            }
        }
        string lang = GetString(dict, "lang");
        string requestedLang = GetString(dict, "requested_lang");
        bool isFallback = GetBool(dict, "is_fallback");
        string createdAt = GetString(dict, "created_at");
        string updatedAt = GetString(dict, "updated_at");
        return new ArticleMinimal(id, title, url, tags, lang, requestedLang, isFallback, createdAt, updatedAt);
    }

    private static Tag ParseTag(Dictionary<string, object> dict)
    {
        string id = GetString(dict, "id");
        string name = GetString(dict, "name");
        string lang = GetString(dict, "lang");
        string color = GetString(dict, "color");
        return new Tag(id, name, lang, color);
    }

    private static string SerializeCachedArticle(CachedArticle cached)
    {
        var dict = new Dictionary<string, object>
        {
            ["id"] = cached.Id,
            ["lang"] = cached.Lang,
            ["etag"] = cached.ETag,
            ["cached_at"] = cached.CachedAt,
            ["article"] = SerializeArticle(cached.Article)
        };
        return JsonParser.Serialize(dict);
    }

    private static string SerializeCachedArticlesList(CachedArticlesList cached)
    {
        var dict = new Dictionary<string, object>
        {
            ["lang"] = cached.Lang,
            ["etag"] = cached.ETag,
            ["cached_at"] = cached.CachedAt,
            ["response"] = SerializeArticlesResponse(cached.Response)
        };
        return JsonParser.Serialize(dict);
    }

    private static Dictionary<string, object> SerializeArticle(Article article)
    {
        return new Dictionary<string, object>
        {
            ["id"] = article.Id,
            ["title"] = article.Title,
            ["url"] = article.Url,
            ["tags"] = article.Tags.Select(t => SerializeTag(t)).ToList(),
            ["lang"] = article.Lang,
            ["requested_lang"] = article.RequestedLang,
            ["is_fallback"] = article.IsFallback,
            ["created_at"] = article.CreatedAt,
            ["updated_at"] = article.UpdatedAt,
            ["body"] = article.Body
        };
    }

    private static Dictionary<string, object> SerializeArticlesResponse(ArticlesResponse response)
    {
        return new Dictionary<string, object>
        {
            ["items"] = response.Items.Select(i => SerializeArticleMinimal(i)).ToList(),
            ["page"] = response.Page,
            ["page_size"] = response.PageSize,
            ["total"] = response.Total
        };
    }

    private static Dictionary<string, object> SerializeArticleMinimal(ArticleMinimal article)
    {
        return new Dictionary<string, object>
        {
            ["id"] = article.Id,
            ["title"] = article.Title,
            ["url"] = article.Url,
            ["tags"] = article.Tags.Select(t => SerializeTag(t)).ToList(),
            ["lang"] = article.Lang,
            ["requested_lang"] = article.RequestedLang,
            ["is_fallback"] = article.IsFallback,
            ["created_at"] = article.CreatedAt,
            ["updated_at"] = article.UpdatedAt
        };
    }

    private static Dictionary<string, object> SerializeTag(Tag tag)
    {
        return new Dictionary<string, object>
        {
            ["id"] = tag.Id,
            ["name"] = tag.Name,
            ["lang"] = tag.Lang,
            ["color"] = tag.Color
        };
    }

    private static string GetString(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
            return val.ToString();
        return string.Empty;
    }

    private static int GetInt(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
        {
            if (val is long longVal)
                return (int)longVal;
            if (val is int intVal)
                return intVal;
            if (int.TryParse(val.ToString(), out int parsed))
                return parsed;
        }
        return 0;
    }

    private static bool GetBool(Dictionary<string, object> dict, string key)
    {
        if (dict.TryGetValue(key, out var val) && val != null)
        {
            if (val is bool boolVal)
                return boolVal;
            if (bool.TryParse(val.ToString(), out bool parsed))
                return parsed;
        }
        return false;
    }
}

/// <summary>
/// キャッシュされた個別記事
/// </summary>
public sealed class CachedArticle
{
    public string Id { get; set; }
    public string Lang { get; set; }
    public Article Article { get; set; }
    public string ETag { get; set; }
    public string CachedAt { get; set; }
}

/// <summary>
/// キャッシュされた記事リスト
/// </summary>
public sealed class CachedArticlesList
{
    public string Lang { get; set; }
    public ArticlesResponse Response { get; set; }
    public string ETag { get; set; }
    public string CachedAt { get; set; }
}
