
using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles.Crewmate.Phosphorus;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class Lantern : MonoBehaviour
{
    public static readonly Sprite InactiveSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LanternInactive.png", 115f);
    public static readonly Sprite ActiveSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LanternActive.png", 115f);
    public static List<Lantern> AllLanterns = new();

    public PlayerControl Owner { get; private set; }
    public bool IsActivating { get; private set; } = false;
    private SpriteRenderer myRend;
    private SpriteRenderer light;

    public void Init(PlayerControl owner)
    {
        Owner = owner;
        transform.position = owner.GetTruePosition();
        transform.localScale = Vector3.one * 0.25f;
        gameObject.layer = 9; //レイヤーをShipに

        myRend = gameObject.GetOrAddComponent<SpriteRenderer>();
        myRend.color = new(1f, 1f, 1f, Owner.AmOwner ? 0.5f : 0f);
        myRend.sprite = InactiveSprite;

        light = CreateCustomLight(gameObject.transform.position, Phosphorus.LightRange.GetFloat(), false);
        light.gameObject.transform.parent = transform;

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
        light.enabled = true;

        new LateTask(() =>
        {
            LightingOff();
        }, Phosphorus.Optioninfo.DurationTime);
    }
    public void LightingOff()
    {
        if (!IsActivating)
            return;

        myRend.sprite = InactiveSprite;
        light.enabled = false;
    }

    public static readonly Sprite LightMask = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Phosphorus.LightMask.png", 115f);
    public static SpriteRenderer CreateCustomLight(Vector2 pos, float range, bool enabled = true, Sprite maskSprite = null)
    {
        var trueRange = ShipStatus.Instance.MaxLightRadius * range * 6;
        var light = new GameObject("Light");
        light.transform.position = (Vector3)pos + new Vector3(0f, 0f, -50f);
        light.transform.localScale = new(trueRange, trueRange, 1f);
        light.layer = LayerMask.NameToLayer("Shadow");

        var lightRenderer = light.AddComponent<SpriteRenderer>();
        lightRenderer.sprite = maskSprite == null ? LightMask : maskSprite;
        lightRenderer.material.shader = PlayerControl.LocalPlayer.LightPrefab.LightCutawayMaterial.shader;
        lightRenderer.enabled = enabled;
        return lightRenderer;
    }

    public static IEnumerable<Lantern> GetLanternsByOwner(PlayerControl player)
        => AllLanterns.Where(x => x.Owner == player && x != null);
}