using System;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using SuperNewRoles.Safety.Api;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class MessagesUI
{
    public static GameObject ShowMessagesUI(Transform parent, RequestInGameManager.Thread thread)
    {
        GameObject chatUI = AssetManager.Instantiate("ChatUI", parent);
        chatUI.transform.localPosition = new Vector3(0f, -0.23f, -2f);
        chatUI.transform.localScale = Vector3.one * 0.9f;
        TextBoxTMP textBox = chatUI.transform.Find("InputBox").GetComponent<TextBoxTMP>();
        ReportUIMenu.ConfigureTextBox(textBox);
        Scroller scroller = chatUI.transform.Find("Scroller").GetComponent<Scroller>();
        ConfigureMessageScroller(scroller);
        PassiveButton sendButton = chatUI.transform.Find("SendButton").gameObject.AddComponent<PassiveButton>();
        sendButton.Colliders = new Collider2D[] { sendButton.GetComponent<Collider2D>() };
        sendButton.OnClick = new();
        float nextTopY = ChatMessageLayout.FirstMessageTopY;
        float contentBottomY = ChatMessageLayout.FirstMessageTopY;
        string lastMessageSender = ""; // 最後のメッセージの送信者を追跡
        if (thread.isSafetyNotice)
        {
            chatUI.transform.Find("InputBox")?.gameObject.SetActive(false);
            chatUI.transform.Find("SendButton")?.gameObject.SetActive(false);
            chatUI.transform.Find("chatBG")?.gameObject.SetActive(false);
            bool activeSafety = true;
            LoadingUI.ShowLoadingUI(chatUI.transform, () => ModTranslation.GetString("RequestInGameLoading"), () => activeSafety);
            activeSafety = false;
            new LateTask(() =>
            {
                string author = ModTranslation.GetString("SafetyReportActionedAuthor");
                string body = ModTranslation.GetString("SafetyReportActionedBody");
                lastY -= ThreadHeaderSpacing;
                bool first = true;
                foreach (DateTime at in thread.safetyEvents)
                {
                    string text = body + "\n" + at.ToLocalTime().ToString("g");
                    GenerateMessage(text, scroller, false, author, !first, first, true, ref lastY);
                    first = false;
                }
                UpdateScrollerMax(scroller, lastY);
            }, 0f, "SafetyNoticeUI");
            AmongUsClient.Instance.StartCoroutine(PlayerSafetyApiClient.AckNotices(_ =>
            {
                CreateButtons.ModManagerLateUpdatePatch.ForceNotificationCheck = true;
            }).WrapToIl2Cpp());
            return chatUI;
        }
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
                            GenerateMessage(textBox.text, scroller, true, "", isContinuity, false, true, ref nextTopY, ref contentBottomY);
                            lastMessageSender = currentToken; // 送信者を更新
                            textBox.SetText("");
                            UpdateScrollerMax(scroller, contentBottomY);
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
                            GenerateMessage(thread.first_message, scroller, true, "", false, false, true, ref nextTopY, ref contentBottomY);
                            lastMessageSender = token; // 最初のメッセージの送信者を設定
                            int index = 1;
                            foreach (var messageBase in messages)
                            {
                                switch (messageBase)
                                {
                                    case RequestInGameManager.Message message:
                                        bool isMe = message.sender == token;
                                        bool showAuthorForce = !isMe && lastTarget && lastSender != message.sender;
                                        GenerateMessage(message.content, scroller, isMe, message.sender.Replace("github:", ""), lastSender == message.sender, showAuthorForce, true, ref nextTopY, ref contentBottomY);
                                        lastSender = message.sender;
                                        lastMessageSender = message.sender; // 最後のメッセージの送信者を更新
                                        lastTarget = !isMe;
                                        Logger.Debug($"Message: {message.content}");
                                        index++;
                                        break;
                                    case RequestInGameManager.StatusUpdate statusUpdate:
                                        GenerateMessage(
                                            $"<color={statusUpdate.status.color}> {statusUpdate.status.mark} </color> " + ModTranslation.GetString("RequestInGame.UpdateStatusTo", $"\"{statusUpdate.status.status}\""),
                                            scroller,
                                            false,
                                            ModTranslation.GetString("RequestInGame.UpdateStatus"),
                                            lastSender == RequestInGameManager.StatusUpdater,
                                            lastSender != RequestInGameManager.StatusUpdater,
                                            false,
                                            ref nextTopY,
                                            ref contentBottomY,
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
                            UpdateScrollerMax(scroller, contentBottomY);
                        }, 0f, "MessagesUI");
                    }
                }).WrapToIl2Cpp());
            }
        }, createIfMissing: false).WrapToIl2Cpp());
        return chatUI;
    }
    private static void ConfigureMessageScroller(Scroller scroller)
    {
        Transform hitboxTransform = scroller.transform.Find("Hitbox");
        if (hitboxTransform == null)
            return;
        BoxCollider2D hitbox = hitboxTransform.GetComponent<BoxCollider2D>();
        if (hitbox == null)
            return;

        float currentTop = hitbox.offset.y + hitbox.size.y * 0.5f;
        float bottom = ChatMessageLayout.MessageAreaBottomY;
        float height = currentTop - bottom;
        if (height <= 0.1f)
            return;
        hitbox.size = new Vector2(hitbox.size.x, height);
        hitbox.offset = new Vector2(hitbox.offset.x, (currentTop + bottom) * 0.5f);
    }

    private static void UpdateScrollerMax(Scroller scroller, float contentBottomY)
    {
        scroller.ContentYBounds.max = ChatMessageLayout.ComputeScrollerMax(
            ChatMessageLayout.FirstMessageTopY,
            contentBottomY,
            ChatMessageLayout.MessageAreaBottomY);
        scroller.Inner.transform.localPosition = new(0, scroller.ContentYBounds.max, 0);
    }

    private static void GenerateMessage(
        string message,
        Scroller scroller,
        bool isMe,
        string author,
        bool isContinuity,
        bool showAuthorForce,
        bool showChatTail,
        ref float nextTopY,
        ref float contentBottomY,
        bool enableWordWrapping = true,
        bool renderMarkdown = true)
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
        ChatMessageRichText.Result rendered = renderMarkdown
            ? ChatMessageRichText.ConvertDetailed(message)
            : ChatMessageRichText.Result.Plain(message);
        string renderedMessage = rendered.Text;

        Vector2 textAnchored = ResolveAnchoredPosition(textRect, textTransform);
        Vector2 authorAnchored = ResolveAnchoredPosition(authorRect, authorTransform);
        var prefab = new ChatMessageLayout.PrefabMetrics(
            textRect.sizeDelta.x, textRect.sizeDelta.y, textAnchored.x, textAnchored.y,
            authorRect.sizeDelta.x, authorRect.sizeDelta.y, authorAnchored.x, authorAnchored.y,
            textBackgroundRenderer.size.x, textBackgroundRenderer.size.y, textBG.localPosition.x, textBG.localPosition.y,
            chatTail.localPosition.x, chatTail.localPosition.y);

        textMeshPro.richText = true;
        textMeshPro.enableAutoSizing = false;
        textMeshPro.fontSize = textMeshPro.fontSizeMin > 0f ? textMeshPro.fontSizeMin : 6f;
        textMeshPro.overflowMode = TextOverflowModes.Overflow;
        textRect.sizeDelta = new Vector2(prefab.TextWidth, prefab.TextHeight);

        textMeshPro.enableWordWrapping = false;
        textMeshPro.text = renderedMessage;
        textMeshPro.ForceMeshUpdate();
        float preferredTextWidth = textMeshPro.preferredWidth;

        textMeshPro.enableWordWrapping = enableWordWrapping;
        float targetTextWidth = Mathf.Clamp(
            preferredTextWidth + ChatMessageLayout.WidthMeasurementPadding,
            prefab.TextWidth * ChatMessageLayout.MinimumTextWidthRatio,
            prefab.TextWidth);
        textRect.sizeDelta = new Vector2(targetTextWidth, prefab.TextHeight);
        textMeshPro.ForceMeshUpdate();
        float preferredTextHeight = textMeshPro.preferredHeight;

        authorText.text = isMe || (isContinuity && !showAuthorForce) ? "" : author;
        bool hasVisibleAuthor = !string.IsNullOrEmpty(authorText.text);
        float preferredAuthorWidth = 0f;
        if (hasVisibleAuthor)
        {
            authorText.ForceMeshUpdate();
            preferredAuthorWidth = authorText.preferredWidth;
        }

        ChatMessageLayout.Result layout = ChatMessageLayout.Calculate(
            prefab,
            preferredTextWidth,
            preferredTextHeight,
            preferredAuthorWidth,
            hasVisibleAuthor);

        float yScale = isMe ? ChatMessageLayout.MyMessageScale : 1f;
        ChatMessageLayout.PlaceMessage(
            nextTopY,
            layout.VisualTop,
            layout.VisualBottom,
            yScale,
            ChatMessageLayout.NextMessageSpacing(isContinuity, showAuthorForce),
            out float originY,
            out nextTopY,
            out contentBottomY);

        messageObject.transform.localPosition = new Vector3(
            isMe ? ChatMessageLayout.MyMessageX : ChatMessageLayout.OtherMessageX,
            originY,
            ChatMessageLayout.MessageZ);
        messageObject.transform.localScale = isMe
            ? Vector3.one * ChatMessageLayout.MyMessageScale
            : Vector3.one;

        if (!isMe)
        {
            messageObject.transform.localScale = new(-1, 1, 1);
            textTransform.localScale = new(-1, 1, 1);
            authorTransform.localScale = new(-1, 1, 1);
        }

        if (!showChatTail)
            chatTail.gameObject.SetActive(false);

        textRect.sizeDelta = new Vector2(layout.TextWidth, layout.TextHeight);
        textRect.anchoredPosition = new Vector2(layout.TextX, layout.TextY);
        authorRect.sizeDelta = new Vector2(layout.AuthorWidth, prefab.AuthorHeight);
        authorRect.anchoredPosition = new Vector2(layout.AuthorX, layout.AuthorY);
        textBackgroundRenderer.size = new Vector2(layout.BubbleWidth, layout.BubbleHeight);
        textBG.localPosition = new Vector3(layout.BubbleX, layout.BubbleY, textBG.localPosition.z);
        chatTail.localPosition = new Vector3(layout.TailX, layout.TailY, chatTail.localPosition.z);
        textMeshPro.ForceMeshUpdate();
        if (rendered.Urls.Length > 0)
            textTransform.gameObject.AddComponent<ChatMessageLinkHandler>().Init(textMeshPro, rendered.Urls);
    }

    private static Vector2 ResolveAnchoredPosition(RectTransform rect, Transform transform)
    {
        Vector2 anchored = rect.anchoredPosition;
        Vector3 local = transform.localPosition;
        float x = Mathf.Approximately(anchored.x, 0f) && !Mathf.Approximately(local.x, 0f) ? local.x : anchored.x;
        float y = Mathf.Approximately(anchored.y, 0f) && !Mathf.Approximately(local.y, 0f) ? local.y : anchored.y;
        return new Vector2(x, y);
    }
}
