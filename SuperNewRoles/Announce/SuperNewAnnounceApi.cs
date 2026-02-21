using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;

public static class SuperNewAnnounceApi
{
    public static string BaseUrl { get; set; } = SNRURLs.SuperNewAnnounceApi;
    private const int DefaultTimeoutSeconds = 15;

    public static IEnumerator GetHealthz(Action<ApiResult<HealthResponse>> callback, string etag = null)
    {
        if (!EnsureBaseUrl(callback))
            yield break;

        string url = BuildUrl("healthz", null);
        yield return SendGetRequest(url, etag, ParseHealthResponse, callback);
    }

    public static IEnumerator GetArticle(string id, string lang, Action<ApiResult<Article>> callback, bool? fallback = null, string etag = null)
    {
        if (!EnsureBaseUrl(callback))
            yield break;

        if (string.IsNullOrWhiteSpace(id))
        {
            callback?.Invoke(ApiResult<Article>.FromTransportError("Article id is required."));
            yield break;
        }
        if (string.IsNullOrWhiteSpace(lang))
        {
            callback?.Invoke(ApiResult<Article>.FromTransportError("lang is required."));
            yield break;
        }

        var query = new Dictionary<string, string>
        {
            { "lang", lang }
        };
        if (fallback.HasValue)
            query["fallback"] = BoolToString(fallback.Value);

        string url = BuildUrl($"articles/{Uri.EscapeDataString(id)}", query);
        yield return SendGetRequest(url, etag, ParseArticle, callback);
    }

    public static IEnumerator ListArticles(string lang, Action<ApiResult<ArticlesResponse>> callback, int? page = null, int? pageSize = null, bool? fallback = null, string etag = null)
    {
        if (!EnsureBaseUrl(callback))
            yield break;

        if (string.IsNullOrWhiteSpace(lang))
        {
            callback?.Invoke(ApiResult<ArticlesResponse>.FromTransportError("lang is required."));
            yield break;
        }

        var query = new Dictionary<string, string>
        {
            { "lang", lang }
        };
        if (page.HasValue && page.Value > 0)
            query["page"] = page.Value.ToString();
        if (pageSize.HasValue && pageSize.Value > 0)
            query["page_size"] = pageSize.Value.ToString();
        if (fallback.HasValue)
            query["fallback"] = BoolToString(fallback.Value);

        string url = BuildUrl("articles", query);
        yield return SendGetRequest(url, etag, ParseArticlesResponse, callback);
    }

    public static IEnumerator SearchArticles(string text, string lang, Action<ApiResult<ArticlesResponse>> callback, int? page = null, int? pageSize = null, bool? fallback = null, string etag = null)
    {
        if (!EnsureBaseUrl(callback))
            yield break;

        if (string.IsNullOrWhiteSpace(text))
        {
            callback?.Invoke(ApiResult<ArticlesResponse>.FromTransportError("text is required."));
            yield break;
        }
        if (string.IsNullOrWhiteSpace(lang))
        {
            callback?.Invoke(ApiResult<ArticlesResponse>.FromTransportError("lang is required."));
            yield break;
        }

        var query = new Dictionary<string, string>
        {
            { "text", text },
            { "lang", lang }
        };
        if (page.HasValue && page.Value > 0)
            query["page"] = page.Value.ToString();
        if (pageSize.HasValue && pageSize.Value > 0)
            query["page_size"] = pageSize.Value.ToString();
        if (fallback.HasValue)
            query["fallback"] = BoolToString(fallback.Value);

        string url = BuildUrl("articles/search", query);
        yield return SendGetRequest(url, etag, ParseArticlesResponse, callback);
    }

    public static IEnumerator ListTags(string lang, Action<ApiResult<List<Tag>>> callback, string etag = null)
    {
        if (!EnsureBaseUrl(callback))
            yield break;

        if (string.IsNullOrWhiteSpace(lang))
        {
            callback?.Invoke(ApiResult<List<Tag>>.FromTransportError("lang is required."));
            yield break;
        }

        var query = new Dictionary<string, string>
        {
            { "lang", lang }
        };
        string url = BuildUrl("tags", query);
        yield return SendGetRequest(url, etag, ParseTagList, callback);
    }

    private static string BoolToString(bool value) => value ? "true" : "false";

    private static bool EnsureBaseUrl<T>(Action<ApiResult<T>> callback)
    {
        if (!string.IsNullOrWhiteSpace(BaseUrl))
            return true;

        callback?.Invoke(ApiResult<T>.FromTransportError("BaseUrl is not set."));
        return false;
    }

    private static string BuildUrl(string path, Dictionary<string, string> query)
    {
        string baseUrl = BaseUrl.TrimEnd('/');
        string normalizedPath = path.TrimStart('/');
        string url = $"{baseUrl}/{normalizedPath}";

        if (query == null || query.Count == 0)
            return url;

        var parts = new List<string>(query.Count);
        foreach (var kvp in query)
        {
            if (string.IsNullOrEmpty(kvp.Value))
                continue;
            string key = Uri.EscapeDataString(kvp.Key);
            string value = Uri.EscapeDataString(kvp.Value);
            parts.Add($"{key}={value}");
        }
        if (parts.Count == 0)
            return url;

        return url + "?" + string.Join("&", parts);
    }

    private static IEnumerator SendGetRequest<T>(string url, string etag, Func<object, T> parseFunc, Action<ApiResult<T>> callback)
    {
        var request = UnityWebRequest.Get(url);
        request.timeout = DefaultTimeoutSeconds;
        try
        {
            if (!string.IsNullOrEmpty(etag))
                request.SetRequestHeader("If-None-Match", etag);

            yield return request.SendWebRequest();

            bool isNotModified = request.responseCode == 304;
            string cacheControl = request.GetResponseHeader("Cache-Control");
            string responseEtag = request.GetResponseHeader("ETag");
            string rawText = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;

            if (isNotModified)
            {
                callback?.Invoke(new ApiResult<T>(false, true, request.responseCode, cacheControl, responseEtag, default, null, rawText, request.error));
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                ApiError apiError = TryParseError(rawText);
                callback?.Invoke(new ApiResult<T>(false, false, request.responseCode, cacheControl, responseEtag, default, apiError, rawText, request.error));
                yield break;
            }

            if (!TryParseJson(rawText, out object root, out string parseError))
            {
                callback?.Invoke(new ApiResult<T>(false, false, request.responseCode, cacheControl, responseEtag, default, null, rawText, parseError));
                yield break;
            }

            T data = default;
            try
            {
                data = parseFunc != null ? parseFunc(root) : default;
            }
            catch (Exception ex)
            {
                callback?.Invoke(new ApiResult<T>(false, false, request.responseCode, cacheControl, responseEtag, default, null, rawText, ex.Message));
                yield break;
            }

            callback?.Invoke(new ApiResult<T>(true, false, request.responseCode, cacheControl, responseEtag, data, null, rawText, null));
        }
        finally
        {
            request.Dispose();
        }
    }

    private static bool TryParseJson(string text, out object root, out string error)
    {
        root = null;
        error = null;
        if (string.IsNullOrEmpty(text))
        {
            error = "Empty response body.";
            return false;
        }

        try
        {
            root = JsonParser.Parse(text);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static ApiError TryParseError(string text)
    {
        if (!TryParseJson(text, out object root, out _))
            return null;

        if (root is Dictionary<string, object> dict &&
            dict.TryGetValue("error", out var errVal) &&
            errVal is Dictionary<string, object> errDict)
        {
            string code = GetString(errDict, "code");
            string message = GetString(errDict, "message");
            Dictionary<string, object> details = null;
            if (errDict.TryGetValue("details", out var detailsVal) && detailsVal is Dictionary<string, object> detailsDict)
                details = detailsDict;
            return new ApiError(code, message, details);
        }

        return null;
    }

    private static HealthResponse ParseHealthResponse(object root)
    {
        if (root is Dictionary<string, object> dict && dict.TryGetValue("ok", out var okVal))
        {
            bool ok = okVal is bool okBool ? okBool : string.Equals(okVal?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
            return new HealthResponse(ok);
        }

        return new HealthResponse(false);
    }

    private static Article ParseArticle(object root)
    {
        if (root is Dictionary<string, object> dict)
        {
            var minimal = ParseArticleMinimal(dict);
            string body = GetString(dict, "body");
            return new Article(minimal, body);
        }
        return null;
    }

    private static ArticlesResponse ParseArticlesResponse(object root)
    {
        if (root is not Dictionary<string, object> dict)
            return null;

        List<ArticleMinimal> items = new();
        if (dict.TryGetValue("items", out var itemsVal) && itemsVal is List<object> itemList)
        {
            foreach (var item in itemList)
            {
                if (item is Dictionary<string, object> itemDict)
                {
                    items.Add(ParseArticleMinimal(itemDict));
                }
            }
        }

        int page = GetInt(dict, "page");
        int pageSize = GetInt(dict, "page_size");
        int total = GetInt(dict, "total");
        return new ArticlesResponse(items, page, pageSize, total);
    }

    private static List<Tag> ParseTagList(object root)
    {
        if (root is not List<object> list)
            return new List<Tag>();

        List<Tag> tags = new();
        foreach (var item in list)
        {
            if (item is Dictionary<string, object> tagDict)
                tags.Add(ParseTag(tagDict));
        }
        return tags;
    }

    private static Tag ParseTag(Dictionary<string, object> tagDict)
    {
        string id = GetString(tagDict, "id");
        string name = GetString(tagDict, "name");
        string lang = GetString(tagDict, "lang");
        string color = GetString(tagDict, "color");
        return new Tag(id, name, lang, color);
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

public sealed class ApiResult<T>
{
    public bool IsSuccess { get; }
    public bool IsNotModified { get; }
    public long StatusCode { get; }
    public string CacheControl { get; }
    public string ETag { get; }
    public T Data { get; }
    public ApiError Error { get; }
    public string RawText { get; }
    public string TransportError { get; }

    public ApiResult(bool isSuccess, bool isNotModified, long statusCode, string cacheControl, string eTag, T data, ApiError error, string rawText, string transportError)
    {
        IsSuccess = isSuccess;
        IsNotModified = isNotModified;
        StatusCode = statusCode;
        CacheControl = cacheControl ?? string.Empty;
        ETag = eTag ?? string.Empty;
        Data = data;
        Error = error;
        RawText = rawText ?? string.Empty;
        TransportError = transportError ?? string.Empty;
    }

    public static ApiResult<T> FromTransportError(string message)
    {
        return new ApiResult<T>(false, false, 0, string.Empty, string.Empty, default, null, string.Empty, message);
    }
}

public sealed class ApiError
{
    public string Code { get; }
    public string Message { get; }
    public Dictionary<string, object> Details { get; }

    public ApiError(string code, string message, Dictionary<string, object> details)
    {
        Code = code ?? string.Empty;
        Message = message ?? string.Empty;
        Details = details;
    }
}

public sealed class HealthResponse
{
    public bool Ok { get; }

    public HealthResponse(bool ok)
    {
        Ok = ok;
    }
}

public sealed class Tag
{
    public string Id { get; }
    public string Name { get; }
    public string Lang { get; }
    public string Color { get; }

    public Tag(string id, string name, string lang, string color)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Lang = lang ?? string.Empty;
        Color = color ?? string.Empty;
    }
}

public class ArticleMinimal
{
    public string Id { get; }
    public string Title { get; }
    public string Url { get; }
    public List<Tag> Tags { get; }
    public string Lang { get; }
    public string RequestedLang { get; }
    public bool IsFallback { get; }
    public string CreatedAt { get; }
    public string UpdatedAt { get; }

    public ArticleMinimal(string id, string title, string url, List<Tag> tags, string lang, string requestedLang, bool isFallback, string createdAt, string updatedAt)
    {
        Id = id ?? string.Empty;
        Title = title ?? string.Empty;
        Url = url ?? string.Empty;
        Tags = tags ?? new List<Tag>();
        Lang = lang ?? string.Empty;
        RequestedLang = requestedLang ?? string.Empty;
        IsFallback = isFallback;
        CreatedAt = createdAt ?? string.Empty;
        UpdatedAt = updatedAt ?? string.Empty;
    }
}

public sealed class Article : ArticleMinimal
{
    public string Body { get; }

    public Article(ArticleMinimal minimal, string body)
        : base(minimal?.Id, minimal?.Title, minimal?.Url, minimal?.Tags, minimal?.Lang, minimal?.RequestedLang, minimal?.IsFallback ?? false, minimal?.CreatedAt, minimal?.UpdatedAt)
    {
        Body = body ?? string.Empty;
    }
}

public sealed class ArticlesResponse
{
    public List<ArticleMinimal> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int Total { get; }

    public ArticlesResponse(List<ArticleMinimal> items, int page, int pageSize, int total)
    {
        Items = items ?? new List<ArticleMinimal>();
        Page = page;
        PageSize = pageSize;
        Total = total;
    }
}
