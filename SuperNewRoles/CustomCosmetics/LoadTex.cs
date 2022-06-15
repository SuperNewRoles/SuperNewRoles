using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics
{
    class LoadTex
    {
        internal delegate bool LoadImageDelegate(IntPtr tex, IntPtr data, bool markNonReadable);
        internal delegate Il2CppStructArray<byte> EncodeImageDelegate(IntPtr tex);
        internal static LoadImageDelegate LoadImage;
        internal static EncodeImageDelegate EncodeImage;
        public static Sprite loadSprite(string path)
        {
            //画像サイズは150*150
            if (LoadImage == null)
                LoadImage = IL2CPP.ResolveICall<LoadImageDelegate>("UnityEngine.ImageConversion::LoadImage");
            if (LoadImage == null) return null;
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);
                var Array = (Il2CppStructArray<byte>)bytes;
                LoadImage.Invoke(texture.Pointer, Array.Pointer, false);

                Rect rect = new Rect(0f, 0f, texture.width, texture.height);
                return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 115f);
            }
            catch { }
            return null;
        }
    }
}