using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    /// <summary>
    /// 右クリックを検知するためのコンポーネント
    /// </summary>
    public class RightClickDetector : MonoBehaviour
    {
        // 右クリック時に発火するイベント
        public UnityEvent OnRightClick = new();

        // コライダーのキャッシュ
        private BoxCollider2D _collider;

        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            // 右クリックが押された瞬間のみ処理
            if (Input.GetMouseButtonDown(1))
            {
                // マウス位置をワールド座標に変換
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // コライダーとの当たり判定
                if (_collider != null && _collider.OverlapPoint(mousePos))
                {
                    OnRightClick.Invoke();
                }
            }
        }
    }
}