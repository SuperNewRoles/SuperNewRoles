using System.Threading;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.RequestInGame;

public class MessagesUI
{
    public static void ShowMessagesUI(Transform parent, RequestInGameManager.Thread thread)
    {
        GameObject chatUI = AssetManager.Instantiate("ChatUI", parent);
        chatUI.transform.localPosition = new Vector3(0f, -0.23f, -2f);
        chatUI.transform.localScale = Vector3.one * 0.9f;
        bool active = true;
        LoadingUI.ShowLoadingUI(chatUI.transform, () => "Loading...", () => active);
        RequestInGameManager.GetMessages(thread.thread_id).ContinueWith(task =>
        {
            active = false;
            if (task.IsFaulted)
            {
                Logger.Error($"Failed to get messages: {task.Exception}");
            }
            else
            {
                var messages = task.Result;
                foreach (var message in messages)
                {
                    Logger.Info($"Message: {message.content}");
                }
            }
        });
    }
}
