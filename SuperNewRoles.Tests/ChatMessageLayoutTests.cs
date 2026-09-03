using FluentAssertions;
using SuperNewRoles.RequestInGame;
using Xunit;

namespace SuperNewRoles.Tests;

// ゲーム内報告スレッドの吹き出し配置。
// chatTextBG は底辺ピボットなので、複数行は上端固定で下に伸ばし、入力欄と重ならないこと。
public class ChatMessageLayoutTests
{
    private static ChatMessageLayout.PrefabMetrics Prefab() => new(
        textWidth: 14.5642f, textHeight: 1.1371f, textX: 0f, textY: 0.5794f,
        authorWidth: 14.6773f, authorHeight: 0.6991f, authorX: -0.0225f, authorY: 1.4999f,
        bubbleWidth: 14.7294f, bubbleHeight: 1.1601f, bubbleX: 0f, bubbleY: 0f,
        tailX: 7.4185f, tailY: 0.5929f);

    [Fact]
    public void Calculate_KeepsBubbleTopFixed_WhenTextGrows()
    {
        var prefab = Prefab();
        float originalTop = prefab.BubbleTop;

        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth: 6f,
            preferredTextHeight: 3.5f,
            preferredAuthorWidth: 2f,
            hasVisibleAuthor: true);

        (layout.BubbleY + layout.BubbleHeight).Should().BeApproximately(originalTop, 0.0001f);
        layout.BubbleY.Should().BeLessThan(prefab.BubbleY);
        layout.BubbleHeight.Should().BeGreaterThan(prefab.BubbleHeight);
    }

    [Fact]
    public void Calculate_KeepsTailSideWhenWidthShrinks()
    {
        var prefab = Prefab();
        float originalRight = prefab.BubbleRight;

        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth: 4f,
            preferredTextHeight: prefab.TextHeight,
            preferredAuthorWidth: 2f,
            hasVisibleAuthor: false);

        (layout.BubbleX + layout.BubbleWidth * 0.5f).Should().BeApproximately(originalRight, 0.001f);
        layout.BubbleWidth.Should().BeLessThan(prefab.BubbleWidth);
        layout.TailX.Should().BeApproximately(prefab.TailX, 0.0001f);
        layout.TailY.Should().BeApproximately(prefab.TailY, 0.0001f);
    }

    [Fact]
    public void Calculate_HiddenAuthorUsesBubbleTopAsVisualTop()
    {
        var prefab = Prefab();
        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth: 5f,
            preferredTextHeight: prefab.TextHeight,
            preferredAuthorWidth: 10f,
            hasVisibleAuthor: false);

        layout.VisualTop.Should().BeApproximately(prefab.BubbleTop, 0.0001f);
        layout.VisualBottom.Should().BeApproximately(layout.BubbleY, 0.0001f);
    }

    [Fact]
    public void Calculate_VisibleAuthorKeepsNameAboveBubble()
    {
        var prefab = Prefab();
        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth: 5f,
            preferredTextHeight: 2.5f,
            preferredAuthorWidth: 3f,
            hasVisibleAuthor: true);

        layout.VisualTop.Should().BeApproximately(prefab.AuthorTop, 0.0001f);
        layout.AuthorY.Should().BeApproximately(prefab.AuthorY, 0.0001f);
        layout.AuthorY.Should().BeGreaterThan(layout.BubbleY + layout.BubbleHeight - 0.05f);
    }

    [Fact]
    public void Calculate_DoesNotShrinkBelowPrefabTextHeight()
    {
        var prefab = Prefab();
        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth: 3f,
            preferredTextHeight: 0.2f,
            preferredAuthorWidth: 1f,
            hasVisibleAuthor: false);

        layout.TextHeight.Should().BeApproximately(prefab.TextHeight, 0.0001f);
        layout.BubbleHeight.Should().BeApproximately(prefab.BubbleHeight, 0.0001f);
    }

    [Fact]
    public void PlaceMessage_PutsVisualTopOnTheCursor()
    {
        ChatMessageLayout.PlaceMessage(
            nextTopY: 4.2f,
            visualTop: 1.16f,
            visualBottom: -2.0f,
            yScale: 1f,
            spacingAfter: 0.2f,
            out float originY,
            out float newNextTopY,
            out float contentBottomY);

        (originY + 1.16f).Should().BeApproximately(4.2f, 0.0001f);
        contentBottomY.Should().BeApproximately(originY - 2.0f, 0.0001f);
        newNextTopY.Should().BeApproximately(contentBottomY - 0.2f, 0.0001f);
    }

    [Fact]
    public void PlaceMessage_AppliesYScaleToOccupiedHeight()
    {
        ChatMessageLayout.PlaceMessage(
            nextTopY: 4.2f,
            visualTop: 1.16f,
            visualBottom: -2.0f,
            yScale: 0.9f,
            spacingAfter: 0.2f,
            out float originY,
            out _,
            out float contentBottomY);

        (originY + 1.16f * 0.9f).Should().BeApproximately(4.2f, 0.0001f);
        (4.2f - contentBottomY).Should().BeApproximately((1.16f + 2.0f) * 0.9f, 0.0001f);
    }

    [Fact]
    public void TwoMessages_DoNotOverlap()
    {
        var prefab = Prefab();
        ChatMessageLayout.Result first = ChatMessageLayout.Calculate(prefab, 6f, 2.4f, 0f, false);
        ChatMessageLayout.Result second = ChatMessageLayout.Calculate(prefab, 8f, 3.5f, 3f, true);

        float nextTopY = ChatMessageLayout.FirstMessageTopY;
        ChatMessageLayout.PlaceMessage(
            nextTopY, first.VisualTop, first.VisualBottom, 0.9f,
            ChatMessageLayout.NextMessageSpacing(false, false),
            out float firstOrigin, out nextTopY, out float firstBottom);

        ChatMessageLayout.PlaceMessage(
            nextTopY, second.VisualTop, second.VisualBottom, 1f,
            ChatMessageLayout.NextMessageSpacing(false, false),
            out float secondOrigin, out _, out float secondBottom);

        float firstVisualBottom = firstOrigin + first.VisualBottom * 0.9f;
        float secondVisualTop = secondOrigin + second.VisualTop;
        secondVisualTop.Should().BeLessThanOrEqualTo(firstVisualBottom);
        firstBottom.Should().BeGreaterThan(secondBottom);
    }

    [Fact]
    public void ComputeScrollerMax_IsZeroWhenContentFits()
    {
        float max = ChatMessageLayout.ComputeScrollerMax(
            ChatMessageLayout.FirstMessageTopY,
            contentBottomY: 1.0f,
            ChatMessageLayout.MessageAreaBottomY);

        max.Should().Be(0f);
    }

    [Fact]
    public void ComputeScrollerMax_AlignsLastMessageToInput()
    {
        float contentBottom = -6.5f;
        float max = ChatMessageLayout.ComputeScrollerMax(
            ChatMessageLayout.FirstMessageTopY,
            contentBottom,
            ChatMessageLayout.MessageAreaBottomY);

        (contentBottom + max).Should().BeApproximately(ChatMessageLayout.MessageAreaBottomY, 0.0001f);
        max.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void NextMessageSpacing_GrowsWhenSenderChanges()
    {
        float sameSender = ChatMessageLayout.NextMessageSpacing(true, false);
        float differentSender = ChatMessageLayout.NextMessageSpacing(false, false);
        float forcedAuthor = ChatMessageLayout.NextMessageSpacing(false, true);

        differentSender.Should().BeGreaterThan(sameSender);
        forcedAuthor.Should().BeGreaterThan(differentSender);
    }
}
