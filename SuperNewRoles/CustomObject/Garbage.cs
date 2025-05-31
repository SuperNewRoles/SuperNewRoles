using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.CrewMate;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomObject;

public class Garbage
{
    public static List<Garbage> AllGarbage = new();
    public static GameObject AllGarbageObject;
    public static GameObject InstantiateGarbage;
    public static readonly float Distance = 0.75f;
    public static Sprite[] GarbageSprites;
    public ExPlayerControl MadeBy;

    public static void LoadSprites()
    {
        GarbageSprites = new Sprite[3]
        {
            AssetManager.GetAsset<Sprite>("Garbage_1.png"),
            AssetManager.GetAsset<Sprite>("Garbage_2.png"),
            AssetManager.GetAsset<Sprite>("Garbage_3.png"),
        };
    }

    public static void ClearAndReload()
    {
        AllGarbage = new();
    }

    public GameObject GarbageObject;
    public SpriteRenderer GarbageRenderer;
    public CircleCollider2D CircleCollider;
    public ButtonBehavior Button;
    public int Index { get; }
    public bool AllPlayerCanSeeGarbage { get; }
    public Garbage(Vector2 pos, ExPlayerControl madeBy, int index, bool allPlayerCanSeeGarbage)
    {
        AllPlayerCanSeeGarbage = allPlayerCanSeeGarbage;
        GarbageObject = Object.Instantiate(InstantiateGarbage, AllGarbageObject.transform);
        GarbageObject.transform.position = new(pos.x, pos.y, (pos.y / 1000f) + 0.0005f);
        GarbageObject.transform.localScale *= 1.5f;
        GarbageObject.name = $"Garbage {madeBy.PlayerId} {index}";
        Index = index;
        MadeBy = madeBy;
        GarbageObject.SetActive(AllPlayerCanSeeGarbage || ExPlayerControl.LocalPlayer.Role == RoleId.WellBehaver);

        GarbageRenderer = GarbageObject.GetComponent<SpriteRenderer>();
        GarbageRenderer.sprite = GarbageSprites[UnityEngine.Random.Range(0, GarbageSprites.Length)];

        CircleCollider = GarbageObject.GetComponent<CircleCollider2D>();

        Button = GarbageObject.GetComponent<ButtonBehavior>();
        Button.OnClick.AddListener((Action)(() =>
        {
            ExPlayerControl player = PlayerControl.LocalPlayer;
            if (player.Role != RoleId.WellBehaver || player.Data.IsDead) return;
            if (Vector2.Distance(GarbageObject.transform.position, player.GetTruePosition()) <= Distance)
            {
                Logger.Info($"{GarbageObject.name}を削除", "Garbage");
                WellBehaverAbility.RpcDestroyGarbage(ExPlayerControl.LocalPlayer, madeBy, index);
                Clear();
            }
        }));

        AllGarbage.Add(this);
    }

    public void Clear()
    {
        AllGarbage.Remove(this);
        Object.Destroy(GarbageObject);
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public static class ShipStatusPatch
    {
        public static void Postfix()
        {
            LoadSprites();

            AllGarbageObject = new GameObject("AllGarbageObject");
            AllGarbageObject.transform.position = new(0f, 0f, 0f);

            InstantiateGarbage = new GameObject("Instantiate Garbage") { layer = 11 };
            InstantiateGarbage.transform.SetParent(AllGarbageObject.transform);
            InstantiateGarbage.SetActive(false);

            InstantiateGarbage.AddComponent<SpriteRenderer>();

            CircleCollider2D collider = InstantiateGarbage.AddComponent<CircleCollider2D>();
            collider.radius = 0.25f;
            collider.isTrigger = true;

            InstantiateGarbage.AddComponent<ButtonBehavior>().colliders = new Collider2D[1] { collider };
        }
    }
}