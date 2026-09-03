using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SuperNewRoles.RequestInGame;

/// <summary>
/// ゲーム内報告スレッド本文向けの markdown / URL 変換。
/// TMP は link ID にコロンを含むとタグを壊すため、URL 本体は別配列に持つ。
/// </summary>
internal static class ChatMessageRichText
{
    private const string HoverColorTag = "<color=#00FF00>";
    private const string HoverColorCloseTag = "</color>";

    private static readonly Regex MarkdownLinkRegex = new(
        @"\[([^\[\]]+?)\]\(([^)\s]+)\)",
        RegexOptions.Compiled);

    private static readonly Regex AutoUrlRegex = new(
        @"(?:https?://|www\.)[^\s<>""]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ListItemRegex = new(
        @"^(\s*)- ",
        RegexOptions.Compiled | RegexOptions.Multiline);

    internal readonly struct Result
    {
        public readonly string Text;
        public readonly string[] Urls;

        public Result(string text, string[] urls)
        {
            Text = text;
            Urls = urls ?? Array.Empty<string>();
        }

        public static Result Plain(string text)
        {
            return new Result(text ?? string.Empty, Array.Empty<string>());
        }
    }

    public static string Convert(string text)
    {
        return ConvertDetailed(text).Text;
    }

    public static Result ConvertDetailed(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Result.Plain(text);

        var urls = new List<string>();
        string result = ConvertMarkdownLinks(text, urls);
        result = AutoLinkifyUrls(result, urls);
        result = ModHelpers.ConvertSimpleMarkdownToRichText(result);
        result = ListItemRegex.Replace(result, "$1・");
        return new Result(result, urls.ToArray());
    }

    public static bool TryNormalizeHttpUrl(string raw, out string url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string trimmed = raw.Trim();
        if (trimmed.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            trimmed = "https://" + trimmed;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        if (string.IsNullOrEmpty(uri.Host))
            return false;

        url = trimmed;
        return true;
    }

    public static string WithHoveredLinkColor(string text, int linkIndex)
    {
        if (string.IsNullOrEmpty(text) || linkIndex < 0)
            return text;

        string open = LinkOpenTag(linkIndex);
        int contentStart = text.IndexOf(open, StringComparison.Ordinal);
        if (contentStart < 0)
            return text;

        contentStart += open.Length;
        int close = text.IndexOf("</link>", contentStart, StringComparison.Ordinal);
        if (close < 0)
            return text;

        return text[..contentStart] + HoverColorTag + text[contentStart..close] + HoverColorCloseTag + text[close..];
    }

    internal static string LinkOpenTag(int index)
    {
        return $"<link={index}>";
    }

    private static string ConvertMarkdownLinks(string text, List<string> urls)
    {
        return MarkdownLinkRegex.Replace(text, match =>
        {
            string label = match.Groups[1].Value;
            if (!TryNormalizeHttpUrl(match.Groups[2].Value, out string url))
                return match.Value;
            return WrapLink(urls, url, label);
        });
    }

    private static string AutoLinkifyUrls(string text, List<string> urls)
    {
        MatchCollection matches = AutoUrlRegex.Matches(text);
        if (matches.Count == 0)
            return text;

        var builder = new StringBuilder(text.Length + 32);
        int lastIndex = 0;
        foreach (Match match in matches)
        {
            if (IsInsideExistingLink(text, match.Index))
                continue;

            SplitTrailingPunctuation(match.Value, out string urlText, out string trailing);
            if (!TryNormalizeHttpUrl(urlText, out string url))
                continue;

            builder.Append(text, lastIndex, match.Index - lastIndex);
            builder.Append(WrapLink(urls, url, urlText));
            builder.Append(trailing);
            lastIndex = match.Index + match.Length;
        }

        builder.Append(text, lastIndex, text.Length - lastIndex);
        return builder.ToString();
    }

    private static string WrapLink(List<string> urls, string url, string label)
    {
        int index = urls.Count;
        urls.Add(url);
        return $"{LinkOpenTag(index)}{label}</link>";
    }

    private static bool IsInsideExistingLink(string text, int index)
    {
        int open = text.LastIndexOf("<link=", index, StringComparison.OrdinalIgnoreCase);
        int close = text.LastIndexOf("</link>", index, StringComparison.OrdinalIgnoreCase);
        return open > close;
    }

    private static void SplitTrailingPunctuation(string value, out string urlText, out string trailing)
    {
        int end = value.Length;
        while (end > 0 && IsTrailingUrlPunctuation(value[end - 1]))
            end--;

        urlText = value[..end];
        trailing = value[end..];
    }

    private static bool IsTrailingUrlPunctuation(char c)
    {
        return c is '.' or ',' or ';' or ':' or '!' or '?' or ')' or ']' or '}' or '>'
            or '"' or '\'' or '。' or '、' or '」' or '』' or '）';
    }
}
