using System;
using SuperNewRoles.Modules;
using UnityEngine;
using System.Collections.Generic;

namespace SuperNewRoles.CustomObject;

public record CustomAnimationObjectOption(
    Sprite[] Sprites,
    bool IsLoop,
    int frameRate,
    bool DestroyOnMeeting = true
);

public class CustomAnimationObject : MonoBehaviour
{
    private CustomAnimationObjectOption option;
    private SpriteRenderer spriteRenderer;
    private float time;
    private int index;
    public void Init(CustomAnimationObjectOption option, SpriteRenderer spriteRenderer)
    {
        this.option = option;
        this.spriteRenderer = spriteRenderer;
    }
    private void Update()
    {
        if (option.Sprites.Length == 0)
            return;
        if (option.frameRate > 0)
            time += Time.deltaTime;
        if (time >= 1f / option.frameRate)
        {
            time = 0;
            index++;
            if (index >= option.Sprites.Length)
            {
                index = 0;
                if (!option.IsLoop)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            spriteRenderer.sprite = option.Sprites[index];
        }
    }

    private void OnDestroy()
    {
        // リソースの解放
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteRenderer.sprite = null;
        }

        // 必要に応じて参照をクリア
        spriteRenderer = null;
    }
}