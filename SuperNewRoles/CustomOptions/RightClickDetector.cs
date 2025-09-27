using UnityEngine;
using UnityEngine.Events;
using SuperNewRoles;

namespace SuperNewRoles.CustomOptions;

/// <summary>
/// 右クリックを検知するためのコンポーネント
/// </summary>
public class RightClickDetector : MonoBehaviour
{
    // 右クリック時に発火するイベント
    public UnityEvent OnRightClick = new();

    // コライダーのキャッシュ
    private BoxCollider2D _collider;

    // Android向けダブルタップ検出用
    private const float DoubleClickThreshold = 0.7f;
    private float _lastTapTime = -1f;
    private bool _firstTapInside = false;

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

        // Android では 0.5 秒以内のダブルタップを右クリック相当として扱う
        if (ModHelpers.IsAndroid())
        {
            bool tapBegan = false;
            Vector2 tapPos = default;

            // タッチ入力の Began を優先
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        tapBegan = true;
                        tapPos = Camera.main.ScreenToWorldPoint(touch.position);
                        break;
                    }
                }
            }
            // フォールバックとして左クリックイベントも監視（Unityのタッチ→マウスブリッジ対策）
            if (!tapBegan && Input.GetMouseButtonDown(0))
            {
                tapBegan = true;
                tapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (tapBegan)
            {
                float now = Time.time;
                bool inside = _collider != null && _collider.OverlapPoint(tapPos);

                if (_lastTapTime >= 0f && (now - _lastTapTime) <= DoubleClickThreshold && _firstTapInside && inside)
                {
                    OnRightClick.Invoke();
                    _lastTapTime = -1f;
                    _firstTapInside = false;
                }
                else
                {
                    _lastTapTime = now;
                    _firstTapInside = inside;
                }
            }
        }
    }
}