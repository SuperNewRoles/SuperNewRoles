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

            // Process headings first as they affect entire lines.
            // H1: # text
            result = Regex.Replace(result, @"^# (.*?)$", "<size=150%>$1</size>", RegexOptions.Multiline);
            // H2: ## text
            result = Regex.Replace(result, @"^## (.*?)$", "<size=125%>$1</size>", RegexOptions.Multiline);
            // H3: ### text
            result = Regex.Replace(result, @"^### (.*?)$", "<size=110%>$1</size>", RegexOptions.Multiline);

            // Blockquotes: > text
            // result = Regex.Replace(result, @"^> (.*?)$", "<indent>$1</indent>", RegexOptions.Multiline);

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

            // Bold (__)
            result = Regex.Replace(result, @"__([^_]+?)__", "<b>$1</b>");

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

            // Underline: Unity supports <u>text</u>. If a markdown like ++text++ is desired:
            // result = Regex.Replace(result, @"\+\+([^\+]+?)\+\+", "<u>$1</u>");

            // Subscript: Unity supports <sub>text</sub>. If a markdown like ~text~ is desired:
            // (Be careful with ~ for strikethrough (~~) if using single ~ for subscript)
            // result = Regex.Replace(result, @"~([^~]+?)~", "<sub>$1</sub>");

            // Superscript: Unity supports <sup>text</sup>. If a markdown like ^text^ is desired:
            // result = Regex.Replace(result, @"\^([^\^]+?)\^", "<sup>$1</sup>");

            return result;
        }
    }
}
