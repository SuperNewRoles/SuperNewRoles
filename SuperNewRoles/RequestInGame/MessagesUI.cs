using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class MessagesUI
{
    private const float InitialMessageAnchorY = 5.5f;
    private const float ThreadHeaderSpacing = 1.5f;
    private const float VisibleContentHeight = 7f;
    private const float ScrollBottomPadding = 1.25f;

    private const float InterMessageSpacing = 0.15f;
    private const float SenderChangeSpacing = 0.2f;
    private const float ForcedAuthorSpacing = 0.3f;
    private const float MultiLineTopSpacingPerLine = 0.1f;
    private const float BaseMessageHeight = 1f;
    private const float AdditionalLineHeight = 0.6f;
    private const float MultiLineBottomSpacingPerLine = 0.1f;

    private const float BackgroundHeightPerExtraLine = 0.7f;
    private const float BackgroundOffsetPerExtraLine = 0.74f;
    private const float TextOffsetPerExtraLine = 0.33f;
    private const float AuthorOffsetPerExtraLine = 0.072f;
    private const float WidthMeasurementPadding = 0.45f;
    private const float MinimumTextWidthRatio = 0.18f;

    public static GameObject ShowMessagesUI(Transform parent, RequestInGameManager.Thread thread)
    {
        GameObject chatUI = AssetManager.Instantiate("ChatUI", parent);
        chatUI.transform.localPosition = new Vector3(0f, -0.23f, -2f);
        chatUI.transform.localScale = Vector3.one * 0.9f;
        TextBoxTMP textBox = chatUI.transform.Find("InputBox").GetComponent<TextBoxTMP>();
        ReportUIMenu.ConfigureTextBox(textBox);
        Scroller scroller = chatUI.transform.Find("Scroller").GetComponent<Scroller>();
        PassiveButton sendButton = chatUI.transform.Find("SendButton").gameObject.AddComponent<PassiveButton>();
        sendButton.Colliders = new Collider2D[] { sendButton.GetComponent<Collider2D>() };
        sendButton.OnClick = new();
        float lastY = InitialMessageAnchorY;
        string lastMessageSender = ""; // 最後のメッセージの送信者を追跡
        sendButton.OnClick.AddListener((UnityAction)(() =>
        {
            bool activeSendLoading = true;
            LoadingUI.ShowLoadingUI(chatUI.transform, () => ModTranslation.GetString("RequestInGameSending"), () => activeSendLoading);
            if (textBox.text.Length < 3)
            {
                activeSendLoading = false;
                chatUI.transform.Find("ErrorText").GetComponent<TextMeshPro>().text = ModTranslation.GetString("RequestInGame_MessageTooShort");
                return;
            }
            AmongUsClient.Instance.StartCoroutine(RequestInGameManager.SendMessage(thread.thread_id, textBox.text, success =>
            {
                if (!success)
                {
                    Logger.Error($"Failed to send message");
                }
                else
                {
                    activeSendLoading = false;
                    new LateTask(() =>
                    {
                        // 現在のユーザーのトークンを取得して継続性を判断
                        AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetOrCreateToken(currentToken =>
                        {
                            bool isContinuity = lastMessageSender == currentToken;
                            GenerateMessage(textBox.text, scroller, true, "", isContinuity, false, true, ref lastY);
                            lastMessageSender = currentToken; // 送信者を更新
                            textBox.SetText("");
                            UpdateScrollerMax(scroller, lastY);
                        }, createIfMissing: false).WrapToIl2Cpp());
                    }, 0f, "MessagesUI");
                }
            }).WrapToIl2Cpp());
        }));
        sendButton.OnMouseOut = new();
        sendButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            sendButton.transform.Find("Selected").gameObject.SetActive(false);
        }));
        sendButton.OnMouseOver = new();
        sendButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            sendButton.transform.Find("Selected").gameObject.SetActive(true);
        }));
        bool active = true;
        LoadingUI.ShowLoadingUI(chatUI.transform, () => ModTranslation.GetString("RequestInGameLoading"), () => active);
        AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetOrCreateToken(token =>
        {
            if (token == null)
            {
                Logger.Error($"Failed to get token");
            }
            else
            {
                AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetMessages(thread.thread_id, messages =>
                {
                    active = false;
                    if (messages == null)
                    {
                        Logger.Error($"Failed to get messages");
                    }
                    else
                    {
                        new LateTask(() =>
                        {
                            string lastSender = "";
                            bool lastTarget = false;
                            lastY -= ThreadHeaderSpacing;
                            GenerateMessage(thread.first_message, scroller, true, "", false, false, true, ref lastY);
                            lastMessageSender = token; // 最初のメッセージの送信者を設定
                            int index = 1;
                            foreach (var messageBase in messages)
                            {
                                switch (messageBase)
                                {
                                    case RequestInGameManager.Message message:
                                        bool isMe = message.sender == token;
                                        bool showAuthorForce = !isMe && lastTarget && lastSender != message.sender;
                                        GenerateMessage(message.content, scroller, isMe, message.sender.Replace("github:", ""), lastSender == message.sender, showAuthorForce, true, ref lastY);
                                        lastSender = message.sender;
                                        lastMessageSender = message.sender; // 最後のメッセージの送信者を更新
                                        lastTarget = !isMe;
                                        Logger.Debug($"Message: {message.content}");
                                        index++;
                                        break;
                                    case RequestInGameManager.StatusUpdate statusUpdate:
                                        GenerateMessage(
                                            $"<color={statusUpdate.status.color}> {statusUpdate.status.mark} </color>: " + ModTranslation.GetString("RequestInGame.UpdateStatusTo", $"\"{statusUpdate.status.status}\""),
                                            scroller,
                                            false,
                                            ModTranslation.GetString("RequestInGame.UpdateStatus"),
                                            lastSender == RequestInGameManager.StatusUpdater,
                                            lastSender != RequestInGameManager.StatusUpdater,
                                            false,
                                            ref lastY,
                                            enableWordWrapping: false,
                                            renderMarkdown: false);
                                        lastSender = RequestInGameManager.StatusUpdater;
                                        lastMessageSender = RequestInGameManager.StatusUpdater; // 最後のメッセージの送信者を更新
                                        lastTarget = true;
                                        Logger.Debug($"Status: {statusUpdate.status.status}");
                                        Logger.Debug($"Color: {statusUpdate.status.color}");
                                        Logger.Debug($"Mark: {statusUpdate.status.mark}");
                                        index++;
                                        break;
                                }
                            }
                            UpdateScrollerMax(scroller, lastY);
                        }, 0f, "MessagesUI");
                    }
                }).WrapToIl2Cpp());
            }
        }, createIfMissing: false).WrapToIl2Cpp());
        return chatUI;
    }
    private static void UpdateScrollerMax(Scroller scroller, float lastY)
    {
        float totalContentHeight = Mathf.Max(0f, InitialMessageAnchorY - lastY);
        scroller.ContentYBounds.max = Mathf.Max(0f, totalContentHeight - VisibleContentHeight + ScrollBottomPadding);
        scroller.Inner.transform.localPosition = new(0, scroller.ContentYBounds.max, 0);
    }
    private static void GenerateMessage(string message, Scroller scroller, bool isMe, string author, bool isContinuity, bool showAuthorForce, bool showChatTail, ref float lastY, bool enableWordWrapping = true, bool renderMarkdown = true)
    {
        GameObject messageObject = AssetManager.Instantiate("ChatMessage", scroller.Inner);
        Transform textTransform = messageObject.transform.Find("Text");
        Transform authorTransform = messageObject.transform.Find("Author");
        Transform textBG = messageObject.transform.Find("ChatWindow/chatTextBG");
        Transform chatTail = messageObject.transform.Find("ChatWindow/chatTail");

        TextMeshPro textMeshPro = textTransform.GetComponent<TextMeshPro>();
        TextMeshPro authorText = authorTransform.GetComponent<TextMeshPro>();
        RectTransform textRect = textTransform.GetComponent<RectTransform>();
        RectTransform authorRect = authorTransform.GetComponent<RectTransform>();
        SpriteRenderer textBackgroundRenderer = textBG.GetComponent<SpriteRenderer>();
        string renderedMessage = renderMarkdown ? ModHelpers.ConvertSimpleMarkdownToRichText(message) : message;

        Vector3 baseTextLocalPosition = textTransform.localPosition;
        Vector3 baseAuthorLocalPosition = authorTransform.localPosition;
        Vector3 baseTextBGLocalPosition = textBG.localPosition;
        Vector3 baseTailLocalPosition = chatTail.localPosition;
        Vector2 baseTextSize = textRect.sizeDelta;
        Vector2 baseAuthorSize = authorRect.sizeDelta;
        Vector2 baseBackgroundSize = textBackgroundRenderer.size;

        float baseBubbleRight = baseTextBGLocalPosition.x + baseBackgroundSize.x * 0.5f;
        float baseBubbleLeft = baseTextBGLocalPosition.x - baseBackgroundSize.x * 0.5f;
        float baseTextRight = baseTextLocalPosition.x + baseTextSize.x * 0.5f;
        float baseTextLeft = baseTextLocalPosition.x - baseTextSize.x * 0.5f;
        float leftPadding = baseTextLeft - baseBubbleLeft;
        float rightPadding = baseBubbleRight - baseTextRight;
        float minimumTextWidth = baseTextSize.x * MinimumTextWidthRatio;

        textMeshPro.richText = true;
        textMeshPro.enableWordWrapping = enableWordWrapping;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textRect.sizeDelta = baseTextSize;
        textMeshPro.text = renderedMessage;
        textMeshPro.ForceMeshUpdate();

        float targetTextWidth = Mathf.Clamp(
            (textMeshPro.textBounds.size.x > 0f ? textMeshPro.textBounds.size.x : textMeshPro.preferredWidth) + WidthMeasurementPadding,
            minimumTextWidth,
            baseTextSize.x);
        textRect.sizeDelta = new Vector2(targetTextWidth, baseTextSize.y);
        textMeshPro.ForceMeshUpdate();

        int lineCount = Mathf.Max(1, textMeshPro.textInfo.lineCount);
        int extraLineCount = lineCount - 1;

        // メッセージ間のスペース（調整）
        lastY -= InterMessageSpacing;

        // 送信者が変わる場合の追加マージン（継続性がない場合）
        if (!isContinuity)
        {
            lastY -= SenderChangeSpacing;
        }

        // 作者表示の強制がある場合の追加スペース
        if (showAuthorForce)
        {
            lastY -= ForcedAuthorSpacing;
        }

        // 複数行メッセージの場合、上部に少し余裕を持たせる
        if (extraLineCount > 0)
        {
            lastY -= MultiLineTopSpacingPerLine * extraLineCount;
        }

        // メッセージオブジェクトの位置設定
        messageObject.transform.localPosition = new Vector3(isMe ? 2.8f : -2f, lastY, -2f);
        messageObject.transform.localScale = isMe ? Vector3.one * 0.9f : Vector3.one;

        if (!isMe)
        {
            messageObject.transform.localScale = new(-1, 1, 1);
            textTransform.localScale = new(-1, 1, 1);
            authorTransform.localScale = new(-1, 1, 1);
        }

        if (!showChatTail)
        {
            chatTail.gameObject.SetActive(false);
        }

        authorText.text = isMe || (isContinuity && !showAuthorForce) ? "" : author;
        authorRect.sizeDelta = baseAuthorSize;
        authorText.ForceMeshUpdate();
        bool hasVisibleAuthor = !string.IsNullOrEmpty(authorText.text);

        float targetAuthorWidth = !hasVisibleAuthor
            ? baseAuthorSize.x
            : Mathf.Clamp(
                Mathf.Max(targetTextWidth, authorText.preferredWidth + WidthMeasurementPadding),
                minimumTextWidth,
                baseAuthorSize.x);

        float textAreaWidth = hasVisibleAuthor ? Mathf.Max(targetTextWidth, targetAuthorWidth) : targetTextWidth;

        textRect.sizeDelta = new Vector2(textAreaWidth, baseTextSize.y);

        float bubbleWidth = textAreaWidth + leftPadding + rightPadding;
        float bubbleCenterX = baseBubbleRight - bubbleWidth * 0.5f;
        float textCenterX = baseBubbleRight - rightPadding - textAreaWidth * 0.5f;
        float authorCenterX = baseBubbleRight - rightPadding - targetAuthorWidth * 0.5f;

        textBackgroundRenderer.size = new Vector2(bubbleWidth, baseBackgroundSize.y + BackgroundHeightPerExtraLine * extraLineCount);
        textBG.localPosition = new Vector3(bubbleCenterX, baseTextBGLocalPosition.y - BackgroundOffsetPerExtraLine * extraLineCount, baseTextBGLocalPosition.z);
        chatTail.localPosition = baseTailLocalPosition;
        textTransform.localPosition = new Vector3(textCenterX, baseTextLocalPosition.y - TextOffsetPerExtraLine * extraLineCount, baseTextLocalPosition.z);
        authorRect.sizeDelta = new Vector2(targetAuthorWidth, baseAuthorSize.y);
        authorTransform.localPosition = new Vector3(authorCenterX, baseAuthorLocalPosition.y + AuthorOffsetPerExtraLine * extraLineCount, baseAuthorLocalPosition.z);

        // 次のメッセージのためにlastYを更新
        // 基本の高さ + 複数行による追加高さ + 下部マージン
        float messageHeight = BaseMessageHeight + extraLineCount * AdditionalLineHeight;
        lastY -= messageHeight;

        // 複数行メッセージの場合、下部に追加マージン
        if (extraLineCount > 0)
        {
            lastY -= MultiLineBottomSpacingPerLine * extraLineCount;
        }
    }
}
