using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomObject;

public class BloodStain
{
    public static List<BloodStain> BloodStains = new();
    private static readonly Color BloodRed = new(179f / 255f, 0f, 0f);
    private static readonly Color BloodBlack = new(0.2f, 0.2f, 0.2f);
    private static Sprite sprite;
    private Color color;
    private readonly bool forceBlack;

    //「血液表現」が現在一つしかない為、無駄なメモリ確保を防ぐ為、color指定を直接数字で行っています。
    // 今後増やす時は以下のコメントアウトを解除し、BloodStain.colorへの代入をこちらの変数にしてください。
    // public Color BloodRed = new(179f / 255f, 0f, 0f); // 0.1f指定
    // public Color BloodBlack = new(0.2f, 0.2f, 0.2f); // 0.1f指定
    public GameObject BloodStainObject;
    private SpriteRenderer spriteRenderer;
    private PlayerControl owner;
    private byte ownerId;

    public static Sprite getBloodStainSprite()
    {
        if (sprite) return sprite;
        sprite = AssetManager.GetAsset<Sprite>("BloodStain.png");
        return sprite;
    }
    public BloodStain(PlayerControl player, bool isBlack = false, Transform? parent = null, Vector3? pos = null)
    {
        this.owner = player;
        this.ownerId = player.PlayerId;
        this.forceBlack = isBlack;

        Vector3 posdata = pos ?? player.transform.position;
        BloodStainObject = new("BloodStain");
        Vector3 position = new(posdata.x, posdata.y, posdata.z + 1f);
        BloodStainObject.transform.position = position;
        BloodStainObject.transform.localPosition = position;
        BloodStainObject.transform.localScale *= 1.5f;
        BloodStainObject.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        BloodStainObject.transform.SetParent(parent ?? player.transform.parent);

        spriteRenderer = BloodStainObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = getBloodStainSprite();
        ApplyColor();

        BloodStainObject.SetActive(true);

        BloodStains.Add(this);
    }
    public BloodStain(Vector3 pos, bool isBlack = false)
    {
        this.owner = null;
        this.ownerId = 255;
        this.forceBlack = isBlack;

        BloodStainObject = new("BloodStain");
        Vector3 position = new(pos.x, pos.y, pos.z + 1f);
        BloodStainObject.transform.position = position;
        BloodStainObject.transform.localPosition = position;
        BloodStainObject.transform.localScale *= 1.5f;
        BloodStainObject.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        spriteRenderer = BloodStainObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = getBloodStainSprite();
        ApplyColor();

        BloodStains.Add(this);
    }

    private Color GetColor()
    {
        return forceBlack || (ConfigRoles.IsNotUsingBlood != null && ConfigRoles.IsNotUsingBlood.Value)
            ? BloodBlack
            : BloodRed;
    }

    private void ApplyColor()
    {
        color = GetColor();
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public static void RefreshAllColors()
    {
        for (int i = BloodStains.Count - 1; i >= 0; i--)
        {
            BloodStain bloodStain = BloodStains[i];
            if (bloodStain == null || bloodStain.BloodStainObject == null || bloodStain.spriteRenderer == null)
            {
                BloodStains.RemoveAt(i);
                continue;
            }

            bloodStain.ApplyColor();
        }
    }
}
