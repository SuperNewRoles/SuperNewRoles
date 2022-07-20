using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.CustomAnimation
{
    public static class LoadSprites
    {
        public static Sprite[] GetSprites(string path,int Count)
        {
            List<Sprite> Sprites = new List<Sprite>();
            for (int i = 1; i < Count+1; i++)
            {
                string countdata = "000"+i.ToString();
                if (i >= 10)
                {
                    if (i >= 100)
                    {
                        countdata = "0" + i.ToString();
                    }
                    else
                    {
                        countdata = "00" + i.ToString();
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("パス:"+path+"_"+countdata+".png");
                Sprites.Add(ModHelpers.LoadSpriteFromResources(path+"_"+countdata+".png",110f));
            }
            return Sprites.ToArray();
        }
        public static Sprite[] GetSpritesAgartha(string path, int Count)
        {
            List<Sprite> Sprites = new List<Sprite>();
            for (int i = 1; i < Count + 1; i++)
            {
                string countdata = "000" + i.ToString();
                if (i >= 10)
                {
                    if (i >= 100)
                    {
                        countdata = "0" + i.ToString();
                    }
                    else
                    {
                        countdata = "00" + i.ToString();
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("パス:" + path + "_" + countdata + ".png");
                Sprites.Add(ModHelpers.LoadSpriteFromResources(path + "_" + countdata + ".png", 110f));
            }
            return Sprites.ToArray();
        }
    }
}
