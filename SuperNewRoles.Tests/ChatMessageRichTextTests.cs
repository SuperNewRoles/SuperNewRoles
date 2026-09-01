using FluentAssertions;
using SuperNewRoles.RequestInGame;
using Xunit;

namespace SuperNewRoles.Tests;

// ゲーム内報告スレッド本文の markdown / URL 変換。
public class ChatMessageRichTextTests
{
    [Fact]
    public void Convert_ReplacesMarkdownListDashWithNakaguro()
    {
        string converted = ChatMessageRichText.Convert("- 項目1\n- 項目2");

        converted.Should().Be("・項目1\n・項目2");
    }

    [Fact]
    public void Convert_KeepsIndentWhenReplacingListDash()
    {
        string converted = ChatMessageRichText.Convert("  - 子項目");

        converted.Should().Be("  ・子項目");
    }

    [Fact]
    public void Convert_DoesNotReplaceDashInTheMiddleOfALine()
    {
        string converted = ChatMessageRichText.Convert("A - B");

        converted.Should().Be("A - B");
    }

    [Fact]
    public void Convert_TurnsMarkdownLinkIntoTmpLink()
    {
        string converted = ChatMessageRichText.Convert("詳しくは [ここ](https://example.com/path) へ");

        converted.Should().Be("詳しくは <link=0>ここ</link> へ");
    }

    [Fact]
    public void Convert_AutoLinkifiesBareHttpsUrl()
    {
        string converted = ChatMessageRichText.Convert("see https://example.com/foo now");

        converted.Should().Be("see <link=0>https://example.com/foo</link> now");
    }

    [Fact]
    public void Convert_AutoLinkifiesWwwUrlWithHttpsHref()
    {
        string converted = ChatMessageRichText.Convert("www.example.com");

        converted.Should().Be("<link=0>www.example.com</link>");
    }

    [Fact]
    public void Convert_DoesNotDoubleWrapMarkdownLinkUrl()
    {
        ChatMessageRichText.Result converted = ChatMessageRichText.ConvertDetailed("[https://example.com](https://example.com)");

        converted.Text.Should().Be("<link=0>https://example.com</link>");
        converted.Urls.Should().Equal("https://example.com");
    }

    [Fact]
    public void Convert_LeavesUnsafeMarkdownLinkUnchanged()
    {
        string converted = ChatMessageRichText.Convert("[bad](javascript:alert(1))");

        converted.Should().Be("[bad](javascript:alert(1))");
    }

    [Fact]
    public void Convert_KeepsTrailingPunctuationOutsideAutoLink()
    {
        string converted = ChatMessageRichText.Convert("https://example.com.");

        converted.Should().Be("<link=0>https://example.com</link>.");
    }

    [Fact]
    public void Convert_AppliesBoldAndListTogether()
    {
        string converted = ChatMessageRichText.Convert("- **重要**");

        converted.Should().Be("・<b>重要</b>");
    }

    [Fact]
    public void ConvertDetailed_StoresUrlBesideNumericLinkId()
    {
        ChatMessageRichText.Result converted = ChatMessageRichText.ConvertDetailed("[ここ](https://example.com/path)");

        converted.Text.Should().Be("<link=0>ここ</link>");
        converted.Urls.Should().Equal("https://example.com/path");
    }

    [Fact]
    public void WithHoveredLinkColor_WrapsTargetLinkContent()
    {
        string hovered = ChatMessageRichText.WithHoveredLinkColor(
            "前 <link=0>ここ</link> 後 <link=1>次</link>",
            1);

        hovered.Should().Be("前 <link=0>ここ</link> 後 <link=1><color=#00FF00>次</color></link>");
    }

    [Fact]
    public void WithHoveredLinkColor_LeavesTextWhenIndexIsMissing()
    {
        const string source = "<link=0>ここ</link>";

        ChatMessageRichText.WithHoveredLinkColor(source, -1).Should().Be(source);
        ChatMessageRichText.WithHoveredLinkColor(source, 3).Should().Be(source);
    }

    [Theory]
    [InlineData("https://example.com", "https://example.com")]
    [InlineData("http://example.com/a", "http://example.com/a")]
    [InlineData("www.example.com", "https://www.example.com")]
    public void TryNormalizeHttpUrl_AcceptsHttpLikeUrls(string raw, string expected)
    {
        ChatMessageRichText.TryNormalizeHttpUrl(raw, out string url).Should().BeTrue();
        url.Should().Be(expected);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///tmp/x")]
    [InlineData("not a url")]
    [InlineData("")]
    public void TryNormalizeHttpUrl_RejectsUnsafeOrInvalidValues(string raw)
    {
        ChatMessageRichText.TryNormalizeHttpUrl(raw, out string url).Should().BeFalse();
        url.Should().BeNull();
    }
}
