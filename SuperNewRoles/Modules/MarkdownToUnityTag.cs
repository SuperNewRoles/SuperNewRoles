using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SuperNewRoles.Modules
{
    public static class MarkdownToUnityTag
    {
        public static string Convert(string markdownText)
        {
            if (string.IsNullOrEmpty(markdownText))
            {
                return string.Empty;
            }

            string result = markdownText;

            // Process Unix timestamp tags: <t:UNIX(:format)?>
            // <t-dynamic:...> はコロン直後が数字ではないためこの式では一致しない
            result = Regex.Replace(result, @"<t:(\d+)(?::([fFdDR]))?>", match =>
            {
                if (long.TryParse(match.Groups[1].Value, out long unixSeconds))
                {
                    string format = match.Groups[2].Success ? match.Groups[2].Value : "F";
                    if (format == "R")
                    {
                        // リアルタイム更新のために、不可視のタグを埋め込んでおく
                        string current = FormatUnixTimestamp(unixSeconds, "R");
                        // <alpha=#00>だけだと、テキスト領域が確保されてしまいレイアウトが崩れるため、<size=0>を併用して無効化する
                        // また、TextMeshProのタグは状態が継続するため、属性をリセットする必要がある
                        return $"<size=0><alpha=#00><t-dynamic:{unixSeconds}:R></alpha></size><size=100%><alpha=#FF>{current}<size=0><alpha=#00><t-end:R></alpha></size><size=100%><alpha=#FF>";
                    }
                    return FormatUnixTimestamp(unixSeconds, format);
                }
                return match.Value;
            });

            // Process headings first as they affect entire lines.
            // H1: # text
            result = Regex.Replace(result, @"^# (.*?)$", "<size=150%>$1</size>", RegexOptions.Multiline);
            // H2: ## text
            result = Regex.Replace(result, @"^## (.*?)$", "<size=125%>$1</size>", RegexOptions.Multiline);
            // H3: ### text
            result = Regex.Replace(result, @"^### (.*?)$", "<size=110%>$1</size>", RegexOptions.Multiline);

            // Horizontal Rule: --- (3 or more dashes)
            result = Regex.Replace(result, @"^---+$", "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━", RegexOptions.Multiline);

            // Blockquotes: > text
            // result = Regex.Replace(result, @"^> (.*?)$", "<indent>$1</indent>", RegexOptions.Multiline);

            // Nested List Items (2-level indent)
            result = Regex.Replace(result, @"^  - (.*?)$", "    ◦ $1", RegexOptions.Multiline);
            // List Items: - text
            result = Regex.Replace(result, @"^- (.*?)$", "• $1", RegexOptions.Multiline);

            // Links [text](url)
            // Processed first to avoid interference with other markdown characters within the link text or URL.
            result = Regex.Replace(result, @"\[([^\[\]]*?)\]\((.*?)\)", "<link=\"$2\">$1</link>");

            // Bold and Italic (***text***)
            result = Regex.Replace(result, @"\*\*\*([^\*]+?)\*\*\*", "<b><i>$1</i></b>");

            // Bold (**)
            result = Regex.Replace(result, @"\*\*([^\*]+?)\*\*", "<b>$1</b>");

            // Italic (*) - ensure it's not part of a bold marker
            result = Regex.Replace(result, @"\*([^\*]+?)\*", "<i>$1</i>");

            // Bold and Italic (___text___)
            result = Regex.Replace(result, @"___([^_]+?)___", "<b><i>$1</i></b>");

            // Underline (__text__) - Changed from Bold to Underline
            result = Regex.Replace(result, @"__([^_]+?)__", "<u>$1</u>");

            // Italic (_)
            // This version matches underscores not adjacent to other underscores.
            result = Regex.Replace(result, @"_([^_]+?)_", "<i>$1</i>");

            // Strikethrough (~~text~~)
            result = Regex.Replace(result, @"~~([^~]+?)~~", "<s>$1</s>");

            // Inline Code (`text`)
            // Ensure "monospace" font is available in TextMeshPro or use <mspace>
            result = Regex.Replace(result, @"\`([^\`]+?)\`", "<font=\"monospace\">$1</font>");
            // Alternative with mspace:
            // result = Regex.Replace(result, @"\`([^\`]+?)\`", "<mspace=0.6em>$1</mspace>");

            // --- Unity Specific Tags or Custom Markdown (Examples, can be extended) ---

            // Subscript: Unity supports <sub>text</sub>. If a markdown like ~text~ is desired:
            // (Be careful with ~ for strikethrough (~~) if using single ~ for subscript)
            // result = Regex.Replace(result, @"~([^~]+?)~", "<sub>$1</sub>");

            // Superscript: Unity supports <sup>text</sup>. If a markdown like ^text^ is desired:
            // result = Regex.Replace(result, @"\^([^\^]+?)\^", "<sup>$1</sup>");

            return result;
        }

        public static string FormatUnixTimestamp(long unixSeconds, string format)
        {
            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime();

                bool isJa = false;
                try
                {
                    isJa = FastDestroyableSingleton<TranslationController>.Instance?.currentLanguage?.languageID == SupportedLangs.Japanese;
                }
                catch { }

                string tzSuffix = isJa ? "" : $" (UTC{dateTime.ToString("zzz")})";
                string prefix = isJa ? "日本時間 " : "";

                switch (format)
                {
                    case "f": // Short Date/Time (例: 日本時間 2026年1月18日 17:00)
                        return isJa ? prefix + dateTime.ToString("yyyy年M月d日 H:mm") : dateTime.ToString("yyyy/MM/dd HH:mm") + tzSuffix;
                    case "F": // Long Date/Time (例: 日本時間 2026年1月18日 日曜日 17:00:00)
                        return isJa ? prefix + dateTime.ToString("yyyy年M月d日 dddd H:mm:ss") : dateTime.ToString("dddd, MMMM d, yyyy HH:mm:ss") + tzSuffix;
                    case "d": // Short Date (例: 日本時間 2026/01/18)
                        return isJa ? prefix + dateTime.ToString("yyyy/MM/dd") : dateTime.ToString("yyyy/MM/dd") + tzSuffix;
                    case "D": // Short Date (例: 日本時間 2026/01/18)
                        return isJa ? prefix + dateTime.ToString("yyyy年MM月dd日") : dateTime.ToString("yyyy/MM/dd") + tzSuffix;
                    case "R": // Relative Time (例: 3分前、5時間後)
                        return GetRelativeTime(dateTime, isJa);
                    default:
                        return isJa ? prefix + dateTime.ToString("yyyy年M月d日 H:mm:ss") : dateTime.ToString("yyyy/MM/dd HH:mm:ss") + tzSuffix;
                }
            }
            catch
            {
                return $"<t:{unixSeconds}:{format}>";
            }
        }

        public static string GetRelativeTime(DateTimeOffset dateTime, bool isJa)
        {
            var now = DateTimeOffset.Now;
            var diff = now - dateTime;
            bool isPast = diff.TotalSeconds >= 0;
            var absDiff = isPast ? diff : -diff;

            if (isJa)
            {
                if (absDiff.TotalSeconds < 60) return isPast ? "たった今" : $"{(int)absDiff.TotalSeconds}秒後";
                if (absDiff.TotalMinutes < 60) return isPast ? $"{(int)absDiff.TotalMinutes}分前" : $"{(int)absDiff.TotalMinutes}分後";
                if (absDiff.TotalHours < 24) return isPast ? $"{(int)absDiff.TotalHours}時間前" : $"{(int)absDiff.TotalHours}時間後";
                if (absDiff.TotalDays < 30) return isPast ? $"{(int)absDiff.TotalDays}日前" : $"{(int)absDiff.TotalDays}日後";
                if (absDiff.TotalDays < 365) return isPast ? $"{(int)(absDiff.TotalDays / 30)}ヶ月前" : $"{(int)(absDiff.TotalDays / 30)}ヶ月後";
                return isPast ? $"{(int)(absDiff.TotalDays / 365)}年前" : $"{(int)(absDiff.TotalDays / 365)}年後";
            }
            else
            {
                if (absDiff.TotalSeconds < 60) return isPast ? "just now" : $"in {(int)absDiff.TotalSeconds}s";
                if (absDiff.TotalMinutes < 60) return isPast ? $"{(int)absDiff.TotalMinutes}m ago" : $"in {(int)absDiff.TotalMinutes}m";
                if (absDiff.TotalHours < 24) return isPast ? $"{(int)absDiff.TotalHours}h ago" : $"in {(int)absDiff.TotalHours}h";
                if (absDiff.TotalDays < 30) return isPast ? $"{(int)absDiff.TotalDays}d ago" : $"in {(int)absDiff.TotalDays}d";
                if (absDiff.TotalDays < 365) return isPast ? $"{(int)(absDiff.TotalDays / 30)}mo ago" : $"in {(int)(absDiff.TotalDays / 30)}mo";
                return isPast ? $"{(int)(absDiff.TotalDays / 365)}y ago" : $"in {(int)(absDiff.TotalDays / 365)}y";
            }
        }
    }
}
