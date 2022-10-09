using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules
{
    public class CustomAnimation
    {
        public static Sprite[] GetSprites(string path, int Count)
        {
            List<Sprite> Sprites = new();
            for (int i = 1; i < Count + 1; i++)
            {
                string countdata = "000" + i.ToString();
                if (i >= 10)
                {
                    countdata = i >= 100 ? "0" + i.ToString() : "00" + i.ToString();
                }
                SuperNewRolesPlugin.Logger.LogInfo("パス:" + path + "_" + countdata + ".png");
                Sprites.Add(ModHelpers.LoadSpriteFromResources(path + "_" + countdata + ".png", 110f));
            }
            return Sprites.ToArray();
        }

        public Sprite[] Sprites;
        public Transform Object;
        public int index;
        public SpriteRenderer render;
        public float Updatetime;
        public float UpdateDefaultTime;
        public static List<CustomAnimation> Animations = new();
        public void Start(float freamlate, Transform obj)
        {
            this.UpdateDefaultTime = 1f / freamlate;
            if (this.UpdateDefaultTime == 0)
            {
                this.UpdateDefaultTime = 1;
            }
            this.Updatetime = this.UpdateDefaultTime;
            this.Object = obj;
            this.render = obj.GetComponent<SpriteRenderer>();
            this.index = 0;
            Animations.Add(this);
        }
        public static void Update()
        {
            float deltatime = Time.deltaTime;
            foreach (CustomAnimation anim in Animations.ToArray())
            {
                anim.AnimationUpdate(deltatime);
            }
        }
        public void AnimationUpdate(float Deltatime)
        {
            if (this.render == null) { Animations.Remove(this); return; }
            this.Updatetime -= Deltatime;
            if (this.Updatetime <= 0)
            {
                if (this.render == null)
                {
                    Animations.Remove(this);
                    return;
                }
                this.render.sprite = this.Sprites[this.index];
                this.index++;

                if (this.Sprites.Length <= this.index)
                    this.index = 0;

                this.Updatetime = this.UpdateDefaultTime;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class AnimationUpdatePatch
        {
            static void Postfix()
                => Update();
        }
    }
}