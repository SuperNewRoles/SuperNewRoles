using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Emote
{
    public enum EmoteType
    {
        Simple,
        Animation
    }
    public class EmoteData
    {
        public static List<EmoteData> Datas = new();
        public static int MaxId = 0;
        public string EmoteName;
        public EmoteType Type;
        public bool HideSkin;
        public bool HideHat;
        public bool HideVisor;
        public bool HidePlayer;
        public Sprite[] Sprites;
        public int FrameRate = 5;
        public bool IsLoop = true;
        public float UpdateTime => 1f / (float)FrameRate;
        public int Id;
        public EmoteData(EmoteType Type)
        {
            Id = MaxId;
            MaxId++;
            if (Type == EmoteType.Simple)
            {
                HideSkin = false;
                HideHat = false;
                HideVisor = false;
            } else if (Type == EmoteType.Animation)
            {
                HideSkin = true;
                HideHat = true;
                HideVisor = false;
            }
            Datas.Add(this);
        }
        public static void Start()
        {
            new EmoteData(EmoteType.Simple)
            {
                EmoteName = "バツ",
                Sprites = new Sprite[] { ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Emotes.batu.png", 115f) }
            };
        }
        public static Sprite[] GetEmoteSprites(string Path, int MaxCount)
        {
            List<Sprite> sprites = new();
            for (int i = 1; i <= MaxCount; i++)
            {
                string NumText = "";
                if (i < 10)
                    NumText = "000" + i.ToString();
                else if (i < 100)
                    NumText = "00" + i.ToString();
                else if (i < 100)
                    NumText = "0" + i.ToString();
                else
                    NumText = i.ToString();
                sprites.Add(ModHelpers.LoadSpriteFromResources(Path + "_" + NumText + ".png", 115f));
            }
            return sprites.ToArray();
        }
    }
}
