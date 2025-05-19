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
        sendButton.OnClick.AddListener((UnityAction)(() =>
        {
            bool activeSendLoading = true;
            LoadingUI.ShowLoadingUI(chatUI.transform, () => "Sending...", () => activeSendLoading);
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
                        GenerateMessage(textBox.text, scroller, true, "", false, false, ref lastY);
                        textBox.SetText("");
                        UpdateScrollerMax(scroller);
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
        LoadingUI.ShowLoadingUI(chatUI.transform, () => "Loading...", () => active);
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
                            GenerateMessage(thread.first_message, scroller, true, "", false, false, ref lastY);
                            int index = 1;
                            foreach (var message in messages)
                            {
                                bool isMe = message.sender == token;
                                bool showAuthorForce = !isMe && lastTarget && lastSender != message.sender;
                                GenerateMessage(message.content, scroller, isMe, message.sender.Replace("github:", ""), lastSender == message.sender, showAuthorForce, ref lastY);
                                lastSender = message.sender;
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
        int count = scroller.Inner.childCount;
        scroller.ContentYBounds.max = count <= 5 ? 0 : (count - 6) * 1.4f + 1.25f;
        scroller.Inner.transform.localPosition = new(0, scroller.ContentYBounds.max, 0);
    }
    private static void GenerateMessage(string message, Scroller scroller, bool isMe, string author, bool isContinuity, bool showAuthorForce, ref float lastY)
    {
        GameObject messageObject = AssetManager.Instantiate("ChatMessage", scroller.Inner);
        lastY -= 1.4f;
        if (showAuthorForce)
        {
            lastY -= 0.5f;
        }
        messageObject.transform.localPosition = new Vector3(isMe ? 2.8f : -2f, lastY, -2f);
        messageObject.transform.localScale = Vector3.one * 0.9f;
        if (!isMe)
        {
            messageObject.transform.localScale = new(-1, 1, 1);
            messageObject.transform.Find("Text").transform.localScale = new(-1, 1, 1);
            messageObject.transform.Find("Author").transform.localScale = new(-1, 1, 1);
        }
        TextMeshPro textMeshPro = messageObject.transform.Find("Text").GetComponent<TextMeshPro>();
        TextMeshPro authorText = messageObject.transform.Find("Author").GetComponent<TextMeshPro>();
        textMeshPro.text = message;
        authorText.text = isMe || (isContinuity && !showAuthorForce) ? "" : author;
    }
}
