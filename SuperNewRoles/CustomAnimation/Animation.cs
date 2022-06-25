using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.CustomAnimation
{
    public class Animation
    {
        public Sprite[] Sprites;
        public Transform Object;
        public int index;
        public SpriteRenderer render;
        public float Updatetime;
        public float UpdateDefaultTime;
        public static List<Animation> Animations = new List<Animation>();
        public void Start(float freamlate,Transform obj)
        {
            UpdateDefaultTime = 1f/freamlate;
            if (UpdateDefaultTime == 0)
            {
                UpdateDefaultTime = 1;
            }
            Updatetime = UpdateDefaultTime;
            Object = obj;
            render = obj.GetComponent<SpriteRenderer>();
            index = 0;
            Animations.Add(this);
        }
        public static void Update()
        {
            var deltatime = Time.deltaTime;
            foreach (Animation anim in Animations)
            {
                anim.AnimationUpdate(deltatime);
            }
        }
        public void AnimationUpdate(float Deltatime)
        {
            //SuperNewRolesPlugin.Logger.LogInfo("アップデート:"+Deltatime);
            Updatetime -= Deltatime;
            if (Updatetime <= 0)
            {
                render.sprite = Sprites[index];
                index++;
                if (Sprites.Length <= index)
                {
                    index = 0;
                }
                Updatetime = UpdateDefaultTime;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager),nameof(HudManager.Update))]
    class AnimationUpdatePatch
    {
        static void Postfix(HudManager __instance)
        {
            Animation.Update();
        }
    }
}
