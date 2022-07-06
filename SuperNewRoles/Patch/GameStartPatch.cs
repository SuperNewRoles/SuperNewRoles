using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Patch
{
    class GameStartPatch
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
        class MakePublicPatch
        {
            public static bool Prefix()
            {
                bool ContainsMod = SaveManager.PlayerName.ToLower().Contains("mod");
                bool ContainsSNR = SaveManager.PlayerName.ToUpper().Contains("SNR") || SaveManager.PlayerName.ToUpper().Contains("SHR");
                if (ContainsMod && !ContainsSNR)
                {
                    SuperNewRolesPlugin.Logger.LogWarning("\"mod\"が名前に含まれている状態では公開部屋にすることはできません。");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class LobbyCountDownTimer
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (Input.GetKeyDown(KeyCode.F8) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.countDownTimer = 0;
                }
                if (Input.GetKeyDown(KeyCode.F7) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.ResetStartState();
                }
            }
        }
    }
}
