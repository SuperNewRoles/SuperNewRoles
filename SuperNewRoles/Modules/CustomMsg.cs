using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class CustomMessage
{
    public readonly TMPro.TMP_Text text;
    private static readonly List<CustomMessage> customMessages = new();

    /// <summary>
    /// タスクフェイズ中、画面下部にメッセージを表示するクラス
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    /// <param name="duration">表示時間</param>
    /// <param name="useCustomColor">メッセージの色をカスタムするかどうか</param>
    /// <param name="firstColor">初回に使用する色（オプション）</param>
    /// <param name="secondColor">次回に使用する色（オプション）</param>
    public CustomMessage(string message, float duration, bool useCustomColor = false, Color firstColor = default, Color secondColor = default)
    {
        var hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if (hudManager?.roomTracker == null) return;

        RoomTracker roomTracker = hudManager.roomTracker;
        GameObject messageObject = UnityEngine.Object.Instantiate(roomTracker.gameObject);
        messageObject.transform.SetParent(hudManager.transform);
        UnityEngine.Object.DestroyImmediate(messageObject.GetComponent<RoomTracker>());

        text = messageObject.GetComponent<TMPro.TMP_Text>();
        text.text = message;

        // プレイヤーのビュー内に配置するため、ローカル座標を設定
        messageObject.transform.localPosition = new Vector3(0f, -1.8f, messageObject.transform.localPosition.z);
        customMessages.Add(this);

        // 有効な色を決定（パラメータがデフォルトの場合、既定の色を用いる）
        Color effectiveFirstColor = (firstColor == default) ? Color.yellow : firstColor;
        Color effectiveSecondColor = (secondColor == default) ? effectiveFirstColor : secondColor;

        hudManager.StartCoroutine(Effects.Lerp(duration, new Action<float>(UpdateEffect)));

        // ローカル関数により、表示中のテキストの色と内容を更新
        void UpdateEffect(float progress)
        {
            if (text == null)
            {
                customMessages.Remove(this);
                return;
            }

            // 0.25秒ごとに色を交互に切り替える
            bool isEvenInterval = ((int)(progress * duration / 0.25f)) % 2 == 0;

            if (useCustomColor)
            {
                text.text = isEvenInterval
                    ? ModHelpers.Cs(effectiveFirstColor, message)
                    : ModHelpers.Cs(effectiveSecondColor, message);
                text.color = isEvenInterval ? effectiveFirstColor : effectiveSecondColor;
            }
            else
            {
                string prefix = isEvenInterval ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                text.text = prefix + message + "</color>";
                text.color = isEvenInterval ? Color.yellow : Color.red;
            }

            if (progress >= 1f && text != null && text.gameObject != null)
            {
                UnityEngine.Object.Destroy(text.gameObject);
                customMessages.Remove(this);
            }
        }
    }
}