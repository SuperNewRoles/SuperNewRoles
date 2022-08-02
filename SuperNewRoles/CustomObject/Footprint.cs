using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class Footprint
    {
        public static List<Footprint> footprints = new();
        private static Sprite sprite;
        private Color color;
        public GameObject footprint;
        private SpriteRenderer spriteRenderer;
        private PlayerControl owner;
        private bool anonymousFootprints;

        public static Sprite getFootprintSprite()
        {
            if (sprite) return sprite;
            sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Footprint.png", 600f);
            return sprite;
        }

        public Footprint(float footprintDuration, bool anonymousFootprints, PlayerControl player, Vector3? pos = null)
        {
            this.owner = player;
            this.anonymousFootprints = anonymousFootprints;
            if (anonymousFootprints)
                this.color = Palette.PlayerColors[6];
            else
                this.color = Palette.PlayerColors[(int)player.Data.DefaultOutfit.ColorId];

            Vector3 posdata = pos != null ? (Vector3)pos : player.transform.position;
            footprint = new GameObject("Footprint");
            Vector3 position = new(posdata.x, posdata.y, posdata.z + 1f);
            footprint.transform.position = position;
            footprint.transform.localPosition = position;

            footprint.transform.Rotate(0f, 0f, UnityEngine.Random.Range(0f, 360f));


            spriteRenderer = footprint.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = getFootprintSprite();
            spriteRenderer.color = color;

            footprint.SetActive(true);

            if (footprintDuration > 0)
            {
                footprints.Add(this);

                HudManager.Instance.StartCoroutine(Effects.Lerp(footprintDuration, new Action<float>((p) =>
                {
                    Color c = color;

                    if (spriteRenderer) spriteRenderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1 - p));

                    if (p == 1f && footprint != null)
                    {
                        UnityEngine.Object.Destroy(footprint);
                        footprints.Remove(this);
                    }
                })));
            }
        }
    }
}
