using System;
using SuperNewRoles.Modules;
using UnityEngine;
using System.Collections.Generic;

namespace SuperNewRoles.CustomObject;

public record CustomPlayerAnimationSimpleOption(
    Sprite[] Sprites,
    bool PlayerFlipX,
    bool IsLoop,
    int frameRate,
    bool Adaptive,
    bool DestroyOnMeeting = true,
    Vector3? localPosition = null,
    Vector3? localScale = null,
    AudioClip Sound = null
)
{
    public Vector3 LocalPosition => localPosition ?? Vector3.zero;
    public Vector3 LocalScale => localScale ?? Vector3.one;
}
public class CustomPlayerAnimationSimple : MonoBehaviour
{
    private PlayerControl player;
    private CustomPlayerAnimationSimpleOption option;
    private SpriteRenderer spriteRenderer;
    private float time;
    private int index;
    private void Init(PlayerControl player, CustomPlayerAnimationSimpleOption option, SpriteRenderer spriteRenderer)
    {
        this.player = player;
        this.option = option;
        this.spriteRenderer = spriteRenderer;
        if (option.Adaptive)
        {
            spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
            PlayerMaterial.SetColors(player.Data.DefaultOutfit.ColorId, spriteRenderer);
        }
        spriteRenderer.transform.localPosition = option.LocalPosition;
        spriteRenderer.transform.localScale = option.LocalScale;
    }
    private void Update()
    {
        if ((option.DestroyOnMeeting && MeetingHud.Instance != null) ||
            player == null ||
            player.Data == null ||
            player.Data.IsDead ||
            !player.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }
        if (option.PlayerFlipX)
        {
            spriteRenderer.transform.localScale = new Vector3(player.cosmetics.FlipX ? option.LocalScale.x : -option.LocalScale.x, option.LocalScale.y, option.LocalScale.z);
            spriteRenderer.transform.localPosition = new Vector3(player.cosmetics.FlipX ? option.LocalPosition.x : -option.LocalPosition.x, option.LocalPosition.y, option.LocalPosition.z);
        }
        else
        {
            spriteRenderer.transform.localScale = option.LocalScale;
            spriteRenderer.transform.localPosition = option.LocalPosition;
        }
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
        player = null;
        spriteRenderer = null;
    }

    public static CustomPlayerAnimationSimple Spawn(PlayerControl player, CustomPlayerAnimationSimpleOption option)
    {
        if (option.Sprites.Length == 0)
            throw new ArgumentException("Sprites must not be empty");
        GameObject gameObject = new("CustomPlayerAnimationSimple");
        CustomPlayerAnimationSimple customPlayerAnimationSimple = gameObject.AddComponent<CustomPlayerAnimationSimple>();
        customPlayerAnimationSimple.Init(player, option, gameObject.AddComponent<SpriteRenderer>());
        gameObject.transform.SetParent(player.transform);
        return customPlayerAnimationSimple;
    }
    public static Sprite[] GetSprites(string name, int min, int max, int zeroPadding = 0)
    {
        if (min > max)
            throw new ArgumentException("min must be less than max");
        Sprite[] sprites = new Sprite[max - min + 1];
        for (int i = min; i <= max; i++)
        {
            // zeroPaddingが指定されている場合、数字を (zeroPadding + 1) 桁になるようにゼロ埋めします
            string formattedIndex = (zeroPadding > 0) ? i.ToString("D" + (zeroPadding + 1)) : i.ToString();
            Logger.Info($"loading {string.Format(name, formattedIndex)}");
            sprites[i - min] = AssetManager.GetAsset<Sprite>(string.Format(name, formattedIndex));
        }

        return sprites;
    }
}