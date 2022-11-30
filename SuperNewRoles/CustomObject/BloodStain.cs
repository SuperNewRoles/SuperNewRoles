using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class BloodStain
{
    public static List<BloodStain> BloodStains = new();
    private static Sprite sprite;
    private Color color;
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
        this.color = Color.red;

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
}
