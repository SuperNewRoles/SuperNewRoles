using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class Lantern : MonoBehaviour
{
    public static Sprite InactiveSprite => AssetManager.GetAsset<Sprite>("PhosphorusLanternInactive.png");
    public static Sprite ActiveSprite => AssetManager.GetAsset<Sprite>("PhosphorusLanternActive.png");
    public static Sprite LightMask => AssetManager.GetAsset<Sprite>("PhosphorusLightMask.png");
    public static List<Lantern> AllLanterns = new();

    public ExPlayerControl Owner { get; private set; }
    public bool IsActivating { get; private set; } = false;
    private SpriteRenderer myRend;
    private SpriteRenderer light;

    public void Init(ExPlayerControl owner)
    {
        Owner = owner;
        transform.position = owner.transform.position;
        transform.localScale = Vector3.one * 0.25f;
        gameObject.layer = 9; // レイヤーをShipに

        myRend = gameObject.GetComponent<SpriteRenderer>();
        if (myRend == null)
            myRend = gameObject.AddComponent<SpriteRenderer>();
        myRend.color = new(1f, 1f, 1f, Owner.AmOwner ? 0.5f : 0f);
        myRend.sprite = InactiveSprite;

        light = CreateCustomLight(gameObject.transform.position, Phosphorus.PhosphorusLightRange, false);
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
    }

    public void LightingOff()
    {
        if (!IsActivating)
            return;

        myRend.sprite = InactiveSprite;
        light.enabled = false;
    }

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

    public static IEnumerable<Lantern> GetLanternsByOwner(ExPlayerControl player)
        => AllLanterns.Where(x => x.Owner == player && x != null);

    public static void ResetLanterns()
    {
        AllLanterns.Clear();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class ResetLanternsPatch
    {
        public static void Postfix()
        {
            ResetLanterns();
        }
    }
}