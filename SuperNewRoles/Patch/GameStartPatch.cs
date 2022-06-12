using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using Il2CppSystem;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace SuperNewRoles.Patch
{
    class GameStartPatch
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class LobbyCountDownTimer
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (Input.GetKeyDown(KeyCode.F10) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.countDownTimer = 0;
                }
                if (Input.GetKeyDown(KeyCode.F11) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.ResetStartState();
                }
            }
        }
    }
}
