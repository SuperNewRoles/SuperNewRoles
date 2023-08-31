using System.Collections.Generic;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomObject;

public class Beacon
{
    public static List<Beacon> AllBeacons = new();
    public static Sprite[] beaconAnimationSprites = new Sprite[3];

    public static Sprite GetBeaconAnimationSprite(int index)
    {
        if (beaconAnimationSprites == null || beaconAnimationSprites.Length == 0) return null;
        index = Mathf.Clamp(index, 0, beaconAnimationSprites.Length - 1);
        CustomAnimation Beacon = new()
        {
            Sprites = CustomAnimation.GetSprites("SuperNewRoles.Resources.ConjurerAnimation.Conjurer_Beacon", 60)
        };
        Beacon.Start(30, GameObject.Find($"Beacon{Conjurer.Count}").transform);
        return beaconAnimationSprites[index];
    }

    private readonly GameObject GameObject;

    public Beacon(Vector2 p)
    {
        GameObject = new GameObject($"Beacon{Conjurer.Count}") { layer = 11 };
        Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
        position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
                                                                        // Create the marker
        GameObject.transform.position = position;
        SpriteRenderer sprite = GameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = GetBeaconAnimationSprite(0);
        // Only render the beacon for the conjurer
        var playerIsTrickster = PlayerControl.LocalPlayer;
        GameObject.SetActive(playerIsTrickster);

        AllBeacons.Add(this);
    }


    public static void ClearBeacons()
    {
        //int[] num = { -1, -2, -3 };
        foreach (var beacon in AllBeacons)
        {
            Logger.Info($"{beacon.GameObject.name}をClearします", "ClearBeacons");
            Object.Destroy(beacon.GameObject);
        }
        AllBeacons.Clear();
    }
}