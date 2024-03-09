using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomObject;

public class Garbage
{
    public static List<Garbage> AllGarbage;
    public static GameObject AllGarbageObject;
    public static GameObject InstantiateGarbage;
    public static readonly float Distance = 0.75f;
    public static int Count;
    public static Sprite[] GarbageSprites = new Sprite[3]
    {
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Garbage_1.png", 300f),
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Garbage_2.png", 300f),
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Garbage_3.png", 300f),
    };
    public static void ClearAndReload()
    {
        AllGarbage = new();
        Count = 0;
    }

    public GameObject GarbageObject;
    public SpriteRenderer GarbageRenderer;
    public CircleCollider2D CircleCollider;
    public ButtonBehavior Button;
    public Garbage(Vector2 pos)
    {
        GarbageObject = Object.Instantiate(InstantiateGarbage, AllGarbageObject.transform);
        GarbageObject.transform.position = new(pos.x, pos.y, (pos.y / 1000f) + 0.0005f);
        GarbageObject.name = $"Garbage {Count}";
        GarbageObject.SetActive(true);

        GarbageRenderer = GarbageObject.GetComponent<SpriteRenderer>();
        GarbageRenderer.sprite = GarbageSprites.GetRandom();

        CircleCollider = GarbageObject.GetComponent<CircleCollider2D>();

        Button = GarbageObject.GetComponent<ButtonBehavior>();
        Button.OnClick.AddListener((Action)(() =>
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.WellBehaver) || PlayerControl.LocalPlayer.IsDead()) return;
            if (Vector2.Distance(GarbageObject.transform.position, PlayerControl.LocalPlayer.GetTruePosition()) <= Distance)
            {
                Logger.Info($"{GarbageObject.name}を削除", "Garbage");
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.DestroyGarbage);
                writer.Write(GarbageObject.name);
                writer.EndRPC();
                Clear();
            }
        }));

        AllGarbage.Add(this);
        Count++;
    }

    public void Clear()
    {
        AllGarbage.Remove(this);
        Object.Destroy(GarbageObject);
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix()
        {
            AllGarbageObject = new("AllGarbageObject");
            AllGarbageObject.transform.position = new(0f, 0f, 0f);

            InstantiateGarbage = new("Instantiate Garbage") { layer = 11 };
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