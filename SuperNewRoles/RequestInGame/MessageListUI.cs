using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class MessageListUI
{
    public static void ShowMessageListUI(Transform parent)
    {
        GameObject messageListUI = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("Scroller"), parent);
        messageListUI.transform.localPosition = new(0f, 0f, -10f);
        bool active = true;
        LoadingUI.ShowLoadingUI(parent, () => ModTranslation.GetString("RequestInGameLoading"), () => active);
        AmongUsClient.Instance.StartCoroutine(RequestInGameManager.GetThreads(threads =>
        {
            if (threads == null)
            {
                Logger.Error($"Failed to get threads");
                active = false;
            }
            else
            {
                active = false;
                new LateTask(() =>
                {
                    int index = 0;
                    foreach (var thread in threads)
                    {
                        GameObject messageTitleButton = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("MessageTitleButton"), messageListUI.transform);
                        messageTitleButton.transform.localPosition = new(0f, 3f - 1.5f * index, -10f);
                        PassiveButton passiveButton = messageTitleButton.AddComponent<PassiveButton>();
                        passiveButton.Colliders = new[] { messageTitleButton.GetComponent<Collider2D>() };
                        passiveButton.OnClick = new();
                        passiveButton.OnClick.AddListener((UnityAction)(() =>
                        {
                            GameObject messagesUI = MessagesUI.ShowMessagesUI(messageListUI.transform.parent, thread);
                            SelectButtonsMenu.SetReturnButtonActive(parent.gameObject, () =>
                            {
                                messageListUI.SetActive(true);
                                GameObject.Destroy(messagesUI);
                                SelectButtonsMenu.SetReturnButtonNonActive(parent.gameObject);
                            });
                            messageListUI.SetActive(false);
                        }));
                        passiveButton.OnMouseOut = new();
                        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
                        {
                            messageTitleButton.transform.Find("Selected").gameObject.SetActive(false);
                        }));
                        passiveButton.OnMouseOver = new();
                        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
                        {
                            messageTitleButton.transform.Find("Selected").gameObject.SetActive(true);
                        }));
                        messageTitleButton.transform.Find("Text").GetComponent<TextMeshPro>().text = thread.title;
                        index++;
                    }
                }, 0f, "MessageListUI");
            }
        }, unreadOnly: true).WrapToIl2Cpp());
    }
}