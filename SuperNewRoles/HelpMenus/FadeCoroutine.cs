using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

namespace SuperNewRoles.HelpMenus;

/// <summary>
/// フェードイン/アウト処理を行うコルーチンクラス
/// </summary>
public class FadeCoroutine : MonoBehaviour
{
    private bool isFadeIn = false; // フェードイン中かどうか
    private bool isFadeOut = false; // フェードアウト中かどうか
    private GameObject parent; // フェード対象のGameObject
    public Action onFadeOut = () => { };
    // 画面を表示中か
    public bool isActive { get; private set; }
    private float duration; // フェードにかける時間
    private float elapsed; // 経過時間
    private bool destroyOnFinish = false;
    private SpriteRenderer[] _renderers;
    private TextMeshPro[] _textRenderers;

    /// <summary>
    /// フェードインを開始する
    /// </summary>
    /// <param name="renderers">フェード対象のSpriteRenderer配列</param>
    /// <param name="duration">フェードにかける時間</param>///
    public void StartFadeIn(GameObject parent, float duration)
    {
        gameObject.SetActive(true);
        isFadeIn = true;
        isFadeOut = false;
        this.parent = parent;
        this.duration = duration;
        elapsed = 0f;
        _renderers = parent.GetComponentsInChildren<SpriteRenderer>();
        _textRenderers = parent.GetComponentsInChildren<TextMeshPro>();
        foreach (var renderer in _renderers)
        {
            _defaultAlpha.TryAdd(renderer.GetInstanceID(), renderer.color.a);
        }
        foreach (var textRenderer in _textRenderers)
        {
            _defaultAlpha.TryAdd(textRenderer.GetInstanceID(), textRenderer.color.a);
        }
        isActive = true;
        ApplyAlphaToRenderers(0f);
    }

    /// <summary>
    /// フェードアウトを開始する
    /// </summary>
    /// <param name="renderers">フェード対象のSpriteRenderer配列</param>
    /// <param name="duration">フェードにかける時間</param>
    public void StartFadeOut(GameObject parent, float duration, bool destroyOnFinish = false)
    {
        gameObject.SetActive(true);
        isFadeOut = true;
        isFadeIn = false;
        this.parent = parent;
        this.duration = duration;
        this.destroyOnFinish = destroyOnFinish;
        elapsed = 0f;
        _renderers = parent.GetComponentsInChildren<SpriteRenderer>();
        _textRenderers = parent.GetComponentsInChildren<TextMeshPro>();
        foreach (var renderer in _renderers)
        {
            if (!_defaultAlpha.ContainsKey(renderer.GetInstanceID()))
            {
                _defaultAlpha.TryAdd(renderer.GetInstanceID(), renderer.color.a);
            }
        }
        foreach (var textRenderer in _textRenderers)
        {
            if (!_defaultAlpha.ContainsKey(textRenderer.GetInstanceID()))
            {
                _defaultAlpha.TryAdd(textRenderer.GetInstanceID(), textRenderer.color.a);
            }
        }

        // 現在のアルファ値を取得して、そこから開始
        float currentAlpha = _renderers.Length > 0 ? _renderers[0].color.a :
                              (_textRenderers.Length > 0 ? _textRenderers[0].color.a : 1f);
        ApplyAlphaToRenderers(currentAlpha);

        // 画面を閉じているのでactiveではない
        isActive = false;
        this.onFadeOut?.Invoke();
    }

    /// <summary>
    /// フェードを反転する
    /// </summary>
    /// <returns>フェードインになったか</returns>
    public bool ReverseFade()
    {
        if (parent == null) return false;
        if (isFadeIn)
        {
            StartFadeOut(parent, duration);
        }
        else
        {
            StartFadeIn(parent, duration);
        }
        return isFadeIn;
    }

    /// <summary>
    /// 現在設定されているフェード時間を取得する
    /// </summary>
    /// <returns>フェード時間</returns>
    public float GetDuration()
    {
        return duration;
    }

    /// <summary>
    /// 毎フレームの更新処理
    /// </summary>
    public void Update()
    {
        if (!isFadeIn && !isFadeOut) return;

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / duration);
        float alpha = isFadeIn ? progress : 1f - progress;
        ApplyAlphaToRenderers(alpha);
        bool active = alpha > 0f;
        if (gameObject.activeSelf != active)
            gameObject.SetActive(active);
        if (destroyOnFinish && progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 計算されたアルファ値をSpriteRendererに適用する
    /// </summary>
    /// <param name="alpha">適用するアルファ値</param>
    private void ApplyAlphaToRenderers(float alpha)
    {
        foreach (var renderer in _renderers)
        {
            if (renderer == null) continue;
            float calculatedAlpha = CalculateAlpha(renderer);
            renderer.color = new Color(
                renderer.color.r,
                renderer.color.g,
                renderer.color.b,
                calculatedAlpha);
        }
        foreach (var textRenderer in _textRenderers)
        {
            if (textRenderer == null) continue;
            float calculatedAlpha = CalculateAlpha(textRenderer);
            textRenderer.color = new Color(
                textRenderer.color.r,
                textRenderer.color.g,
                textRenderer.color.b,
                calculatedAlpha
            );
        }
    }

    /// <summary>
    /// 計算されたアルファ値を計算する
    /// </summary>
    /// <param name="renderer">計算対象のRenderer</param>
    /// <returns>計算されたアルファ値</returns>
    private float CalculateAlpha(Renderer renderer)
    {
        if (renderer == null || !_defaultAlpha.TryGetValue(renderer.GetInstanceID(), out float defaultAlpha))
        {
            defaultAlpha = 1f;
        }

        float progress = Mathf.Clamp01(elapsed / duration);
        return isFadeIn ?
            Mathf.Lerp(0f, defaultAlpha, progress) :
            Mathf.Lerp(defaultAlpha, 0f, progress);
    }
    /// <summary>
    /// 計算されたアルファ値を計算する
    /// </summary>
    /// <param name="renderer">計算対象のRenderer</param>
    /// <returns>計算されたアルファ値</returns>
    private float CalculateAlpha(TextMeshPro textRenderer)
    {
        if (textRenderer == null || !_defaultAlpha.TryGetValue(textRenderer.GetInstanceID(), out float defaultAlpha))
        {
            defaultAlpha = 1f;
        }

        float progress = Mathf.Clamp01(elapsed / duration);
        return isFadeIn ?
            Mathf.Lerp(0f, defaultAlpha, progress) :
            Mathf.Lerp(defaultAlpha, 0f, progress);
    }

    private Dictionary<int, float> _defaultAlpha = new();

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    public void OnDestroy()
    {
        float targetAlpha = isFadeIn ? 1f : isFadeOut ? 0f : -1f;
        if (targetAlpha < 0) return;

        foreach (var renderer in _renderers)
        {
            if (renderer == null) continue;
            renderer.material.color = new Color(
                renderer.material.color.r,
                renderer.material.color.g,
                renderer.material.color.b,
                targetAlpha
            );
        }

        foreach (var textRenderer in _textRenderers)
        {
            if (textRenderer == null) continue;
            textRenderer.color = new Color(
                textRenderer.color.r,
                textRenderer.color.g,
                textRenderer.color.b,
                targetAlpha
            );
        }
    }
}
public class DefaultAlpha : MonoBehaviour
{
    private float alpha;
    public void SetAlpha(float alpha)
    {
        this.alpha = alpha;
    }
    public float GetAlpha()
    {
        return alpha;
    }
}