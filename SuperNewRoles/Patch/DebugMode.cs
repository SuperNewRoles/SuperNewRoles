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
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.Patch
{
    class DebugMode
    {
        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class DebugManager
        {
            private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
            private static List<PlayerControl> bots = new List<PlayerControl>();

            public static void Postfix(KeyboardJoystick __instance)
            {
                if (!ConfigRoles.DebugMode.Value) return;

                // Spawn dummys
                if (Input.GetKeyDown(KeyCode.F))
                {
                    var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                    var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                    bots.Add(playerControl);
                    GameData.Instance.AddPlayer(playerControl);
                    AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                    int hat = random.Next(HatManager.Instance.AllHats.Count);
                    int pet = random.Next(HatManager.Instance.AllPets.Count);
                    int skin = random.Next(HatManager.Instance.AllSkins.Count);
                    int visor = random.Next(HatManager.Instance.AllVisors.Count);
                    int color = random.Next(Palette.PlayerColors.Length);
                    int nameplate = random.Next(HatManager.Instance.AllNamePlates.Count);

                    playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                    playerControl.GetComponent<DummyBehaviour>().enabled = true;
                    playerControl.NetTransform.enabled = false;
                    playerControl.SetName(RandomString(10));
                    playerControl.SetColor(color);
                    playerControl.SetHat(HatManager.Instance.AllHats[hat].ProductId, color);
                    playerControl.SetPet(HatManager.Instance.AllPets[pet].ProductId, color);
                    playerControl.SetVisor(HatManager.Instance.AllVisors[visor].ProductId);
                    playerControl.SetSkin(HatManager.Instance.AllSkins[skin].ProductId);
                    playerControl.SetNamePlate(HatManager.Instance.AllNamePlates[nameplate].ProductId);
                    GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
                }
            }

            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }
        public static bool IsDebugMode() {
            var IsDebugModeBool = false;
            if (ConfigRoles.DebugMode.Value) {
                if (CustomOptions.IsDebugMode.getBool()) {
                    IsDebugModeBool = false;
                }
            }
            return IsDebugModeBool;
        }
    }
}
