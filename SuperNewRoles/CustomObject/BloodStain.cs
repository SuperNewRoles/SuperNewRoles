using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class BloodStain
{
    public static List<BloodStain> BloodStains = new();
    private static Sprite sprite;
    private Color color;

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
        sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.BloodStain.png", 600f);
        return sprite;
    }
    public BloodStain(PlayerControl player, Vector3? pos = null)
    {
        this.owner = player;
        this.ownerId = player.PlayerId;
        this.color = ConfigRoles.IsNotUsingBlood.Value ? new Color(0.2f, 0.2f, 0.2f) : new Color(179f / 255f, 0f, 0f); // 直接数値で血の色代入中 [? BloodBlack : BloodRed;]

        Vector3 posdata = pos != null ? (Vector3)pos : player.transform.position;
        BloodStainObject = new("BloodStain");
        Vector3 position = new(posdata.x, posdata.y, posdata.z + 1f);
        BloodStainObject.transform.position = position;
        BloodStainObject.transform.localPosition = position;
        BloodStainObject.transform.localScale *= 1.5f;
        BloodStainObject.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        BloodStainObject.transform.SetParent(player.transform.parent);

        spriteRenderer = BloodStainObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = getBloodStainSprite();
        spriteRenderer.color = color;

        BloodStainObject.SetActive(false);

        BloodStains.Add(this);
    }
    public BloodStain(Vector3 pos)
    {
        this.owner = null;
        this.ownerId = 255;
        this.color = ConfigRoles.IsNotUsingBlood.Value ? new Color(0.2f, 0.2f, 0.2f) : new Color(179f / 255f, 0f, 0f);

        BloodStainObject = new("BloodStain");
        Vector3 position = new(pos.x, pos.y, pos.z + 1f);
        BloodStainObject.transform.position = position;
        BloodStainObject.transform.localPosition = position;
        BloodStainObject.transform.localScale *= 1.5f;
        BloodStainObject.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        spriteRenderer = BloodStainObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = getBloodStainSprite();
        spriteRenderer.color = color;

        BloodStains.Add(this);
    }
}