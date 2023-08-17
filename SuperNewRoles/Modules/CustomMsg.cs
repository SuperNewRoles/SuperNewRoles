using System;
using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Modules;

public class CustomMessage
{
    public readonly TMPro.TMP_Text text;
    private static readonly List<CustomMessage> customMessages = new();

    /// <summary>
    /// タスクフェイズ中画面下部にメッセージを表示する。
    /// </summary>
    /// <param name="message"></param>
    /// <param name="duration">メッセージを表示する時間</param>
    /// <param name="useCustomColor">メッセージの色を変更するか</param>
    /// <param name="firstColor">最初に表示するMessageの色</param>
    /// <param name="secondColor"></param>
    /// <returns></returns>
    public CustomMessage(string message, float duration, bool useCustomColor = false, Color firstColor = new(), Color secondColor = default)
    {
        RoomTracker roomTracker = FastDestroyableSingleton<HudManager>.Instance?.roomTracker;
        if (roomTracker != null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);

            gameObject.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
            UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
            text = gameObject.GetComponent<TMPro.TMP_Text>();
            text.text = message;

            // Use local position to place it in the player's view instead of the world location
            gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
            customMessages.Add(this);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                if (text == null)
                {
                    customMessages.Remove(this);
                    return;
                }
                bool even = ((int)(p * duration / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                if (useCustomColor)
                {
                    firstColor = firstColor == default ? Color.yellow : firstColor;
                    secondColor = secondColor == default ? firstColor : secondColor;

                    text.text = even
                        ? ModHelpers.Cs(firstColor, message)
                        : ModHelpers.Cs(secondColor, message);

                    if (text != null)
                        text.color = even ? firstColor : secondColor;
                }
                else
                {
                    string prefix = even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                    text.text = prefix + message + "</color>";
                    if (text != null)
                        text.color = even ? Color.yellow : Color.red;
                }
                if (p == 1f && text != null && text.gameObject != null)
                {
                    UnityEngine.Object.Destroy(text.gameObject);
                    customMessages.Remove(this);
                }
            })));
        }
    }
}