using System.Collections.Generic;
using HarmonyLib;
using Hazel;

using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.ShareCosmetics
{
    class ShareNamePlate
    {
        public static Dictionary<int, Sprite> NamePlateSprites;
    }
    class SharePatch
    {
        public static Dictionary<int, string> PlayerUrl;
        public static Dictionary<int, string> PlayerDatas;
        public static Dictionary<int, CosmeticsObject> PlayerObjects;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            public static int Proce;
            public static void Postfix(GameStartManager __instance)
            {
                Proce++;
                if (Proce >= 10)
                {
                    Proce = 0;
                }
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix()
            {
                ShareNamePlate.NamePlateSprites = new Dictionary<int, Sprite>();
                PlayerUrl = new Dictionary<int, string>();
                PlayerDatas = new Dictionary<int, string>();
                PlayerObjects = new Dictionary<int, CosmeticsObject>();
            }
        }
    }
}