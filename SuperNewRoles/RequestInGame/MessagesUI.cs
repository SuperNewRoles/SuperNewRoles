using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
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
        PassiveButton sendButton = chatUI.transform.Find("SendButton").gameObject.AddComponent<PassiveButton>();
        sendButton.Colliders = new Collider2D[] { sendButton.GetComponent<Collider2D>() };
        sendButton.OnClick = new();
        float lastY = 5.5f;
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
                            GenerateMessage(textBox.text, scroller, true, "", isContinuity, false, ref lastY);
                            lastMessageSender = currentToken; // 送信者を更新
                            textBox.SetText("");
                            UpdateScrollerMax(scroller);
                        }).WrapToIl2Cpp());
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
                            lastY -= 1.5f;
                            GenerateMessage(thread.first_message, scroller, true, "", false, false, ref lastY);
                            lastMessageSender = token; // 最初のメッセージの送信者を設定
                            int index = 1;
                            foreach (var message in messages)
                            {
                                bool isMe = message.sender == token;
                                bool showAuthorForce = !isMe && lastTarget && lastSender != message.sender;
                                GenerateMessage(message.content, scroller, isMe, message.sender.Replace("github:", ""), lastSender == message.sender, showAuthorForce, ref lastY);
                                lastSender = message.sender;
                                lastMessageSender = message.sender; // 最後のメッセージの送信者を更新
                                lastTarget = !isMe;
                                Logger.Info($"Message: {message.content}");
                                index++;
                            }
                            UpdateScrollerMax(scroller);
                        }, 0f, "MessagesUI");
                    }
                }).WrapToIl2Cpp());
            }
        }).WrapToIl2Cpp());
        return chatUI;
    }
    private static void UpdateScrollerMax(Scroller scroller)
    {
        int childCount = scroller.Inner.childCount;

        // 各メッセージの行数を考慮した実質的なメッセージ数を計算
        float effectiveMessageCount = 0f;
        float totalHeight = 0f;

        for (int i = 0; i < childCount; i++)
        {
            Transform messageTransform = scroller.Inner.GetChild(i);
            TextMeshPro textComponent = messageTransform.Find("Text")?.GetComponent<TextMeshPro>();
            if (textComponent != null)
            {
                int lineCount = textComponent.text.Split('\n').Length;
                // 基本の高さ + 追加行による高さ
                float messageHeight = 1.4f + (lineCount - 1) * 0.8f;
                totalHeight += messageHeight;
                // 行数に応じて実質的なメッセージ数を増加
                effectiveMessageCount += 1f + (lineCount - 1) * 0.57f; // 0.8f / 1.4f ≈ 0.57f
            }
            else
            {
                // テキストコンポーネントが見つからない場合はデフォルト値を使用
                totalHeight += 1.4f;
                effectiveMessageCount += 1f;
            }
        }

        if (effectiveMessageCount <= 5f)
        {
            scroller.ContentYBounds.max = 0;
        }
        else
        {
            // 表示可能な範囲を超えた分を可動域として設定
            float visibleHeight = 5 * 1.4f; // 5メッセージ分の高さ
            scroller.ContentYBounds.max = totalHeight > visibleHeight ? totalHeight - visibleHeight + 1.25f : 0;
        }
        scroller.Inner.transform.localPosition = new(0, scroller.ContentYBounds.max, 0);
    }
    private static void GenerateMessage(string message, Scroller scroller, bool isMe, string author, bool isContinuity, bool showAuthorForce, ref float lastY)
    {
        // 20文字で強制改行する
        message = ModHelpers.WrapText(message, 20);
        GameObject messageObject = AssetManager.Instantiate("ChatMessage", scroller.Inner);

        int lineCount = message.Split('\n').Length;

        // メッセージ間のスペース（調整）
        lastY -= 0.15f; // 0.3f から 0.15f に縮小

        // 送信者が変わる場合の追加マージン（継続性がない場合）
        if (!isContinuity)
        {
            lastY -= 0.2f; // 送信者変更時の追加スペース
        }

        // 作者表示の強制がある場合の追加スペース
        if (showAuthorForce)
        {
            lastY -= 0.3f; // 0.5f から 0.3f に縮小
        }

        // 複数行メッセージの場合、上部に少し余裕を持たせる
        if (lineCount > 1)
        {
            lastY -= 0.1f * (lineCount - 1); // 複数行の場合の上部マージン
        }

        // メッセージオブジェクトの位置設定
        messageObject.transform.localPosition = new Vector3(isMe ? 2.8f : -2f, lastY, -2f);
        messageObject.transform.localScale = Vector3.one * 0.9f;

        if (!isMe)
        {
            messageObject.transform.localScale = new(-1, 1, 1);
            messageObject.transform.Find("Text").transform.localScale = new(-1, 1, 1);
            messageObject.transform.Find("Author").transform.localScale = new(-1, 1, 1);
            messageObject.transform.Find("Author").transform.localPosition = new(-0.06f, 1.5f + 0.072f * (lineCount - 1), 0);
        }

        var textBG = messageObject.transform.Find("ChatWindow/chatTextBG");
        textBG.localScale = new Vector3(1, 1 + 0.7f * (lineCount - 1), 1);
        textBG.localPosition = new Vector3(0, -0.74f * (lineCount - 1), 0);

        TextMeshPro textMeshPro = messageObject.transform.Find("Text").GetComponent<TextMeshPro>();
        TextMeshPro authorText = messageObject.transform.Find("Author").GetComponent<TextMeshPro>();
        textMeshPro.text = message;
        authorText.text = isMe || (isContinuity && !showAuthorForce) ? "" : author;
        textMeshPro.transform.localPosition -= new Vector3(0, 0.33f * (lineCount - 1), 0);

        // 次のメッセージのためにlastYを更新
        // 基本の高さ + 複数行による追加高さ + 下部マージン
        float messageHeight = 1.0f + (lineCount - 1) * 0.6f; // 基本高さと追加行高さを調整
        lastY -= messageHeight;

        // 複数行メッセージの場合、下部に追加マージン
        if (lineCount > 1)
        {
            lastY -= 0.1f * (lineCount - 1);
        }
    }
}
