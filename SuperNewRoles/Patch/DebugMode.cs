using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    class DebugMode
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerPatch
        {
            public static void Prefix(GameStartManager __instance) {

                if (ConfigRoles.DebugMode.Value)
                {
                }
            }
        }
    }
}
