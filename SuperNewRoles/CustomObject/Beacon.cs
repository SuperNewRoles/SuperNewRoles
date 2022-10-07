using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class Beacon
    {
        public static List<Beacon> AllBeacons = new();
        public static Sprite[] beaconAnimationSprites = new Sprite[3];

        public static Sprite GetBeaconAnimationSprite(int index)
        {
            if (beaconAnimationSprites == null || beaconAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, beaconAnimationSprites.Length - 1);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                CustomAnimation Conjurer_Beacon_Animation = new()
                {
                    Sprites = CustomAnimation.GetSprites("SuperNewRoles.Resources.ConjurerAnimation.Conjurer_Beacon", 60)
                };
                Transform Conjurer_Beacon1 = GameObject.Instantiate(GameObject.Find($"Beacon{Conjurer.Count}").transform);
                Conjurer_Beacon_Animation.Start(30, Conjurer_Beacon1);
            }
            return beaconAnimationSprites[index];
        }


        public static void StartAnimation(int ventId)
        {
            Beacon beacon = AllBeacons.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (beacon == null) return;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) =>
            {
                beacon.BeaconRenderer.sprite = GetBeaconAnimationSprite((int)(p * beaconAnimationSprites.Length));
                beacon.BeaconRenderer.sprite = GetBeaconAnimationSprite(0);
            })));
        }

        private readonly GameObject GameObject;
        public Vent vent;
        private readonly SpriteRenderer BeaconRenderer;

        public Beacon(Vector2 p)
        {
            GameObject = new GameObject($"Beacon{Conjurer.Count}") { layer = 11 };
            GameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
            position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
                                                                            // Create the marker
            GameObject.transform.position = position;
            BeaconRenderer = GameObject.AddComponent<SpriteRenderer>();
            BeaconRenderer.sprite = GetBeaconAnimationSprite(0);
            // Only render the beacon for the conjurer
            var playerIsTrickster = PlayerControl.LocalPlayer;
            GameObject.SetActive(playerIsTrickster);

            AllBeacons.Add(this);
        }


        public static void ClearBeacons()
        {
            int[] num = { -1, -2, -3 };
            foreach (var n in num)
            {
                Logger.Info($"Beacon{Conjurer.Count + n}をClearします", "ClearBeacons");
                GameObject.Find($"Beacon{Conjurer.Count + n}")?.SetActive(false);
                GameObject.Find($"Beacon{Conjurer.Count + n}(Clone)")?.SetActive(false);
            }
        }
    }
}