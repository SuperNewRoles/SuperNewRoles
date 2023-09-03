/*using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class CustomAnimation_old
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
        UpdateDefaultTime = 1f / freamlate;
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
    public void AnimationUpdate(float Deltatime)
    {
        if (render == null) { Animations.Remove(this); return; }
        Updatetime -= Deltatime;
        if (Updatetime <= 0)
        {
            if (render == null)
            {
                Animations.Remove(this);
                return;
            }
            render.sprite = Sprites[index];
            index++;

            if (Sprites.Length <= index)
                index = 0;

            Updatetime = UpdateDefaultTime;
        }
    }

}*/