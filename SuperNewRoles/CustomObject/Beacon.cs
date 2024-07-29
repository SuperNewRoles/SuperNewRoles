using System.Collections.Generic;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.CustomObject;

public class Beacon
{
    public static List<Beacon> AllBeacons = new();
    public static Sprite[] beaconAnimationSprites = new Sprite[3];

    public Sprite GetBeaconAnimationSprite(int index)
    {
        if (beaconAnimationSprites == null || beaconAnimationSprites.Length == 0) return null;
        index = Mathf.Clamp(index, 0, beaconAnimationSprites.Length - 1);
        GameObject BeaconObject = GameObject.Find($"Beacon{Parent.Count}");
        CustomAnimation Beacon = BeaconObject.AddComponent<CustomAnimation>();
        Beacon.Init(new CustomAnimationOptions(CustomAnimation.GetSprites("SuperNewRoles.Resources.ConjurerAnimation.Conjurer_Beacon", 60)
            , 30, loop: true));
        return beaconAnimationSprites[index];
    }

    private readonly GameObject GameObject;
    public PlayerControl Source { get; }
    public Conjurer Parent { get; }

    public Beacon(PlayerControl source, Vector2 p)
    {
        Source = source;
        Parent = Source.GetRoleBase<Conjurer>();
        GameObject = new GameObject($"Beacon{Parent.Count}") { layer = 11 };
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
            if (beacon.GameObject == null)
                continue;
            Logger.Info($"{beacon.GameObject.name}をClearします", "ClearBeacons");
            Object.Destroy(beacon.GameObject);
        }
        AllBeacons.Clear();
    }
}