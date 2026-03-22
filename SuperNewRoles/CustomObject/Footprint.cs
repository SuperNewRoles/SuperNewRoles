using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public sealed class Footprint
{
    private static Sprite _sprite;
    private static Texture2D _generatedTexture;

    public static List<Footprint> All { get; } = new();

    public GameObject FootprintObject { get; }
    private readonly SpriteRenderer _spriteRenderer;

    public Footprint(float durationSeconds, bool anonymousFootprints, Vector2 position, Color? color = null)
    {
        Color footprintColor = anonymousFootprints || color == null
            ? Palette.PlayerColors[6]
            : color.Value;

        FootprintObject = new GameObject("Footprint");
        FootprintObject.transform.position = new Vector3(position.x, position.y, 1f);
        FootprintObject.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 360f));

        _spriteRenderer = FootprintObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = GetSprite();
        _spriteRenderer.color = footprintColor;
        FootprintObject.SetActive(true);

        All.Add(this);

        if (durationSeconds > 0f && FastDestroyableSingleton<HudManager>.Instance != null)
        {
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(durationSeconds, new Action<float>(p =>
            {
                if (_spriteRenderer != null)
                {
                    var c = footprintColor;
                    _spriteRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1f - p));
                }

                if (p >= 1f)
                {
                    Destroy();
                }
            })));
        }
    }

    public void Destroy()
    {
        if (FootprintObject != null)
        {
            UnityEngine.Object.Destroy(FootprintObject);
        }
        All.Remove(this);
    }

    public static void ClearAll()
    {
        for (int i = All.Count - 1; i >= 0; i--)
        {
            All[i].Destroy();
        }
        All.Clear();
    }

    private static Sprite GetSprite()
    {
        if (_sprite != null) return _sprite;
        _sprite = AssetManager.GetAsset<Sprite>("Footprint.png");
        if (_sprite != null) return _sprite;

        const int size = 64;
        _generatedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        float radius = (size - 4) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float a = Mathf.Clamp01(1f - (dist / radius));
                a = a * a;
                _generatedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        _generatedTexture.Apply(false, false);

        _sprite = Sprite.Create(_generatedTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return _sprite;
    }
}

