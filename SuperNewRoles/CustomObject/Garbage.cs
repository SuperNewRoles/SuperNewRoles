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
        GarbageObject = new($"Garbage {Count}") { layer = 11 };
        GarbageObject.transform.SetParent(AllGarbageObject.transform);
        GarbageObject.transform.position = new(pos.x, pos.y, pos.y / 1000f + 0.01f);

        GarbageRenderer = GarbageObject.AddComponent<SpriteRenderer>();
        GarbageRenderer.sprite = GarbageSprites.GetRandom();

        CircleCollider = GarbageObject.AddComponent<CircleCollider2D>();
        CircleCollider.radius = 0.25f;
        CircleCollider.isTrigger = true;

        Button = GarbageObject.AddComponent<ButtonBehavior>();
        Button.colliders = new Collider2D[1] { CircleCollider };
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
        }
    }
}
