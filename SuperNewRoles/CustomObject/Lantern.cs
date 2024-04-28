
using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Crewmate.Phosphorus;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class Lantern : MonoBehaviour
{
    public static readonly Sprite InactiveSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LanternInactive.png", 115f);
    public static readonly Sprite ActiveSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LanternActive.png", 115f);
    public static readonly Sprite LightMask = null;
    public static List<Lantern> AllLanterns = new();

    public PlayerControl Owner { get; private set; }
    public bool IsActivating { get; private set; } = false;
    private SpriteRenderer myRend;

    public void Init(PlayerControl owner)
    {
        Owner = owner;
        transform.position = owner.GetTruePosition();
        transform.localScale = Vector3.one * 0.25f;
        myRend = gameObject.GetOrAddComponent<SpriteRenderer>();
        myRend.sprite = InactiveSprite;

        //オーナーだったらAlphaを0.5f,でなければ0f
        myRend.color = new(1f, 1f, 1f, 0.5f * Convert.ToInt32(Owner.AmOwner));

        AllLanterns.Add(this);
    }

    void OnDestroy()
        => AllLanterns.Remove(this);

    public void Activate()
    {
        myRend.color = new(1f, 1f, 1f, 1f);
        IsActivating = true;
    }
    public void LightingOn()
    {
        if (!IsActivating)
            return;

        myRend.sprite = ActiveSprite;
        CreateCustomLight(Phosphorus.LightRange.GetFloat());
    }
    public void LightingOff()
    {
        if (!IsActivating)
            return;

        myRend.sprite = InactiveSprite;
    }

    public static void CreateCustomLight(float range)
    {

    }
}