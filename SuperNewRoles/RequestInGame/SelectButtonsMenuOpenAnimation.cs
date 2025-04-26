using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SuperNewRoles.RequestInGame
{
    /// <summary>
    /// メニューをスケールアップしながら開くアニメーションクラスです。
    /// </summary>
    public class SelectButtonsMenuOpenAnimation : MonoBehaviour
    {
        // 開く対象のメニューGameObject
        public GameObject menu;
        // 最終的なスケール
        public Vector3 targetScale = Vector3.one;
        // アニメーション時間（秒）
        public float duration = 0.15f;
        private bool isOpen = false;
        public Action onOpen;

        /// <summary>
        /// メニューを開くアニメーションを開始します。
        /// </summary>
        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            onOpen?.Invoke();
            StartCoroutine(OpenCoroutine().WrapToIl2Cpp());
        }

        private IEnumerator OpenCoroutine()
        {
            float timer = 0f;
            Vector3 initialScale = Vector3.zero;
            Vector3 finalScale = targetScale;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                menu.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);
                yield return null;
            }
            menu.transform.localScale = finalScale;
        }
    }
}