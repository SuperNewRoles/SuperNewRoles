using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using SuperNewRoles.CustomOption;
using InnerNet;
using System.Collections;
using System.Collections.Generic;

namespace SuperNewRoles.Patch
{
    class DebugMode
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class MapConsoleUsePatch
        {
            public static void Prefix(MapConsole __instance)
            {
                if (ConfigRoles.DebugMode.Value)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("アドミンの場所(x):" + __instance.transform.position.x);
                    SuperNewRolesPlugin.Logger.LogInfo("アドミンの場所(y):" + __instance.transform.position.y);
                    SuperNewRolesPlugin.Logger.LogInfo("アドミンの場所(Z):" + __instance.transform.position.z);
                }
            }
        }
        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class DebugManager
        {
            private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
            private static List<PlayerControl> bots = new List<PlayerControl>();
            public class LateTask
            {
                public string name;
                public float timer;
                public Action action;
                public static List<LateTask> Tasks = new List<LateTask>();
                public bool run(float deltaTime)
                {
                    timer -= deltaTime;
                    if (timer <= 0)
                    {
                        action();
                        return true;
                    }
                    return false;
                }
                public LateTask(Action action, float time, string name = "No Name Task")
                {
                    this.action = action;
                    this.timer = time;
                    this.name = name;
                    Tasks.Add(this);
                }
                public static void Update(float deltaTime)
                {
                    var TasksToRemove = new List<LateTask>();
                    Tasks.ForEach((task) => {
                        if (task.run(deltaTime))
                        {
                            TasksToRemove.Add(task);
                        }
                    });
                    TasksToRemove.ForEach(task => Tasks.Remove(task));
                }
            }
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (!ConfigRoles.DebugMode.Value) return;

                // Spawn dummys
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
                {
                    var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                    var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                    bots.Add(playerControl);
                    GameData.Instance.AddPlayer(playerControl);
                    AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                    int hat = random.Next(HatManager.Instance.allHats.Count);
                    int pet = random.Next(HatManager.Instance.allPets.Count);
                    int skin = random.Next(HatManager.Instance.allSkins.Count);
                    int visor = random.Next(HatManager.Instance.allVisors.Count);
                    int color = random.Next(Palette.PlayerColors.Length);
                    int nameplate = random.Next(HatManager.Instance.allNamePlates.Count);

                    playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                    playerControl.GetComponent<DummyBehaviour>().enabled = true;
                    playerControl.NetTransform.enabled = false;
                    playerControl.SetName(RandomString(10));
                    playerControl.SetColor(color);
                    playerControl.SetHat(HatManager.Instance.AllHats[hat].ProductId, color);
                    playerControl.SetPet(HatManager.Instance.AllPets[pet].ProductId, color);
                    playerControl.SetVisor(HatManager.Instance.AllVisors[visor].ProductId);
                    playerControl.SetSkin(HatManager.Instance.allSkins[skin].ProductId,0);
                    playerControl.SetNamePlate(HatManager.Instance.AllNamePlates[nameplate].ProductId);
                    GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
                }
                /*
                if (Input.GetKeyDown(KeyCode.I))
                {
                    MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    PlayerControl.LocalPlayer.RpcStartMeeting(PlayerControl.LocalPlayer.Data);
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                }
                */
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
                    IsDebugModeBool = true;
                }
            }
            return IsDebugModeBool;
        }
    }
}
