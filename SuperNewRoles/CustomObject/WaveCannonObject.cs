using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomObject
{
    public class WaveCannonObject
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class FixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer)
                    AllFixedUpdate();
            }
        }
        public static List<WaveCannonObject> Objects = new();

        public GameObject gameObject;
        public Transform transform => gameObject.transform;

        private List<Sprite> sprites;
        private float UpdateTime;
        private float DefaultUpdateTime => 1f / freamrate;
        private int freamrate;
        private int index;
        private bool IsLoop;
        private bool Playing;
        private SpriteRenderer render;

        public WaveCannonObject(Vector3 pos)
        {
            gameObject = new("WaveCannonObject");
            render = gameObject.AddComponent<SpriteRenderer>();
            index = 0;
            sprites = new();
            for (int i = 1; i <= 5; i++)
            {
                sprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.WaveCannon.Charge_000{i}.png", 115f));
            }
            render.sprite = sprites[0];
            IsLoop = true;
            freamrate = 20;
            Playing = true;
            Objects.Add(this);
            transform.position = pos;
        }
        public static void AllFixedUpdate()
        {
            if (Objects.Count <= 0) return;
            foreach (WaveCannonObject obj in Objects.ToArray())
            {
                obj.FixedUpdate();
            }
        }
        public void FixedUpdate()
        {
            if (render == null) {Objects.Remove(this); return; }
            if (Playing)
            {
                UpdateTime -= Time.fixedDeltaTime;
                if (UpdateTime <= 0)
                {
                    index++;
                    if (index >= sprites.Count)
                    {
                        index = 0;
                        if (!IsLoop)
                        {
                            Playing = false;
                            UpdateTime = DefaultUpdateTime;
                            return;
                        }
                    }
                    render.sprite = sprites[index];
                }
            }
        }
    }
}
