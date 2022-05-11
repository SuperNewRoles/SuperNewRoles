using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnhollowerBaseLib;

namespace SuperNewRoles.MapOptions
{
    class RandomMap
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public static bool Prefix(RandomMap __instance)
        {
            bool continueStart = true;
            if (MapOption.RandomMapOption.getBool())
            {
                var rand = new System.Random();
                System.Collections.Generic.List<byte> RandomMaps = new System.Collections.Generic.List<byte>();
                if (MapOption.ValidationSkeld == true) RandomMaps.Add(0);
                if (MapOption.ValidationMira == true) RandomMaps.Add(1);
                if (MapOption.ValidationPolus == true) RandomMaps.Add(2);
                if (MapOption.ValidationAirship == true) RandomMaps.Add(4);
                var MapsId = RandomMaps[rand.Next(RandomMaps.Count)];
                PlayerControl.GameOptions.MapId = MapsId;
            }
            return continueStart;
        }
    }
}
