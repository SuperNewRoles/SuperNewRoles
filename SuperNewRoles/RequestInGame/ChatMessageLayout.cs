using System;

namespace SuperNewRoles.RequestInGame;

/// <summary>
/// ChatMessage プレハブの実寸から吹き出し配置を計算する。
/// chatTextBG のスプライトピボットは底辺中央 (0.5, 0) なので、高さは上端を固定して下方向に伸ばす。
/// </summary>
internal static class ChatMessageLayout
{
    public const float FirstMessageTopY = 4.2f;
    public const float MessageAreaBottomY = -3.0f;
    public const float InterMessageSpacing = 0.2f;
    public const float SenderChangeSpacing = 0.25f;
    public const float ForcedAuthorSpacing = 0.15f;
    public const float WidthMeasurementPadding = 0.45f;
    public const float MinimumTextWidthRatio = 0.18f;
    public const float MyMessageX = 2.8f;
    public const float OtherMessageX = -2f;
    public const float MyMessageScale = 0.9f;
    public const float MessageZ = -2f;

    internal readonly struct PrefabMetrics
    {
        public readonly float TextWidth;
        public readonly float TextHeight;
        public readonly float TextX;
        public readonly float TextY;
        public readonly float AuthorWidth;
        public readonly float AuthorHeight;
        public readonly float AuthorX;
        public readonly float AuthorY;
        public readonly float BubbleWidth;
        public readonly float BubbleHeight;
        public readonly float BubbleX;
        public readonly float BubbleY;
        public readonly float TailX;
        public readonly float TailY;

        public PrefabMetrics(
            float textWidth, float textHeight, float textX, float textY,
            float authorWidth, float authorHeight, float authorX, float authorY,
            float bubbleWidth, float bubbleHeight, float bubbleX, float bubbleY,
            float tailX, float tailY)
        {
            TextWidth = textWidth;
            TextHeight = textHeight;
            TextX = textX;
            TextY = textY;
            AuthorWidth = authorWidth;
            AuthorHeight = authorHeight;
            AuthorX = authorX;
            AuthorY = authorY;
            BubbleWidth = bubbleWidth;
            BubbleHeight = bubbleHeight;
            BubbleX = bubbleX;
            BubbleY = bubbleY;
            TailX = tailX;
            TailY = tailY;
        }

        public float BubbleTop => BubbleY + BubbleHeight;
        public float BubbleRight => BubbleX + BubbleWidth * 0.5f;
        public float TextTop => TextY + TextHeight * 0.5f;
        public float AuthorTop => AuthorY + AuthorHeight * 0.5f;
    }

    internal readonly struct Result
    {
        public readonly float TextWidth;
        public readonly float TextHeight;
        public readonly float TextX;
        public readonly float TextY;
        public readonly float AuthorWidth;
        public readonly float AuthorX;
        public readonly float AuthorY;
        public readonly float BubbleWidth;
        public readonly float BubbleHeight;
        public readonly float BubbleX;
        public readonly float BubbleY;
        public readonly float TailX;
        public readonly float TailY;
        public readonly float VisualTop;
        public readonly float VisualBottom;

        public Result(
            float textWidth, float textHeight, float textX, float textY,
            float authorWidth, float authorX, float authorY,
            float bubbleWidth, float bubbleHeight, float bubbleX, float bubbleY,
            float tailX, float tailY,
            float visualTop, float visualBottom)
        {
            TextWidth = textWidth;
            TextHeight = textHeight;
            TextX = textX;
            TextY = textY;
            AuthorWidth = authorWidth;
            AuthorX = authorX;
            AuthorY = authorY;
            BubbleWidth = bubbleWidth;
            BubbleHeight = bubbleHeight;
            BubbleX = bubbleX;
            BubbleY = bubbleY;
            TailX = tailX;
            TailY = tailY;
            VisualTop = visualTop;
            VisualBottom = visualBottom;
        }
    }

    public static Result Calculate(
        in PrefabMetrics prefab,
        float preferredTextWidth,
        float preferredTextHeight,
        float preferredAuthorWidth,
        bool hasVisibleAuthor)
    {
        float minTextWidth = prefab.TextWidth * MinimumTextWidthRatio;
        float textWidth = Clamp(Sanitize(preferredTextWidth) + WidthMeasurementPadding, minTextWidth, prefab.TextWidth);
        float textHeight = Math.Max(prefab.TextHeight, Sanitize(preferredTextHeight));

        float authorWidth = textWidth;
        if (hasVisibleAuthor)
        {
            authorWidth = Clamp(
                Math.Max(textWidth, Sanitize(preferredAuthorWidth) + WidthMeasurementPadding),
                minTextWidth,
                prefab.AuthorWidth);
        }

        float textAreaWidth = hasVisibleAuthor ? Math.Max(textWidth, authorWidth) : textWidth;
        textAreaWidth = Math.Min(textAreaWidth, prefab.TextWidth);

        float bubbleRight = prefab.BubbleRight;
        float textRight = prefab.TextX + prefab.TextWidth * 0.5f;
        float textLeft = prefab.TextX - prefab.TextWidth * 0.5f;
        float bubbleLeft = prefab.BubbleX - prefab.BubbleWidth * 0.5f;
        float leftPadding = textLeft - bubbleLeft;
        float rightPadding = bubbleRight - textRight;

        float bubbleWidth = textAreaWidth + leftPadding + rightPadding;
        float bubbleX = bubbleRight - bubbleWidth * 0.5f;

        float extraHeight = textHeight - prefab.TextHeight;
        float bubbleHeight = prefab.BubbleHeight + extraHeight;
        float bubbleTop = prefab.BubbleTop;
        float bubbleY = bubbleTop - bubbleHeight;

        float textTop = prefab.TextTop;
        float textY = textTop - textHeight * 0.5f;
        float textX = bubbleRight - rightPadding - textAreaWidth * 0.5f;
        float authorX = bubbleRight - rightPadding - authorWidth * 0.5f;

        float visualTop = hasVisibleAuthor ? prefab.AuthorTop : bubbleTop;
        float visualBottom = bubbleY;

        return new Result(
            textAreaWidth, textHeight, textX, textY,
            authorWidth, authorX, prefab.AuthorY,
            bubbleWidth, bubbleHeight, bubbleX, bubbleY,
            prefab.TailX, prefab.TailY,
            visualTop, visualBottom);
    }

    public static float NextMessageSpacing(bool isContinuity, bool showAuthorForce)
    {
        float spacing = InterMessageSpacing;
        if (!isContinuity)
            spacing += SenderChangeSpacing;
        if (showAuthorForce)
            spacing += ForcedAuthorSpacing;
        return spacing;
    }

    public static void PlaceMessage(
        float nextTopY,
        float visualTop,
        float visualBottom,
        float yScale,
        float spacingAfter,
        out float originY,
        out float newNextTopY,
        out float contentBottomY)
    {
        float scale = yScale <= 0f ? 1f : yScale;
        originY = nextTopY - visualTop * scale;
        contentBottomY = originY + visualBottom * scale;
        newNextTopY = contentBottomY - spacingAfter;
    }

    public static float ComputeScrollerMax(float contentTopY, float contentBottomY, float areaBottomY)
    {
        float contentHeight = Math.Max(0f, contentTopY - contentBottomY);
        float visibleHeight = Math.Max(0.01f, contentTopY - areaBottomY);
        return Math.Max(0f, contentHeight - visibleHeight);
    }

    private static float Sanitize(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            return 0f;
        return value;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (min > max)
            (min, max) = (max, min);
        return Math.Min(max, Math.Max(min, value));
    }
}
