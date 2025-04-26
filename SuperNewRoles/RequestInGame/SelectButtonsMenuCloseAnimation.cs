using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SuperNewRoles.RequestInGame
{
    /// <summary>
    /// メニューをスケールダウンしながら閉じるアニメーションクラスです。
    /// </summary>
    public class SelectButtonsMenuCloseAnimation : MonoBehaviour
    {
        // 閉じる対象のメニューGameObject
        public GameObject menu;
        // アニメーション時間（秒）
        public float duration = 0.15f;
        private bool isClose = false;
        public Action onClose;
        /// <summary>
        /// メニューを閉じるアニメーションを開始します。
        /// </summary>
        public void Close()
        {
            if (isClose) return;
            isClose = true;
            onClose?.Invoke();
            StartCoroutine(CloseCoroutine().WrapToIl2Cpp());
        }

        private IEnumerator CloseCoroutine()
        {
            float timer = 0f;
            Vector3 initialScale = menu.transform.localScale;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                menu.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
                yield return null;
            }
            // アニメーション後にメニューを破棄
            Destroy(menu);
        }
    }
}