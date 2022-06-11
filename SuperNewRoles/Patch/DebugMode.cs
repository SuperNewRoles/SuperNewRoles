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
                if (Input.GetKeyDown(KeyCode.G))
                {
                    PlayerControl bot = BotManager.Spawn(PlayerControl.LocalPlayer.nameText.text);

                    new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                    new LateTask(() => { foreach (var pc in CachedPlayer.AllPlayers) pc.PlayerControl.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                    new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
                }
                /*
                if (Input.GetKeyDown(KeyCode.I))
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p == PlayerControl.LocalPlayer) continue;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.MyPhysics.NetId, (byte)RpcCalls.EnterVent, SendOption.None, p.getClientId());
                        writer.WritePacked(MapUtilities.CachedShipStatus.AllVents[0].Id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        SuperNewRolesPlugin.Logger.LogInfo(MapUtilities.CachedShipStatus.AllVents[0].transform);
                    }
                }
                    
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("CHANGE!!!");
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            RoleManager.Instance.SetRole(p, RoleTypes.Engineer);
                            AmongUsClient.Instance.Spawn(GameData.Instance, -2, SpawnFlags.IsClientCharacter);
                            AmongUsClient.Instance.Spawn(p, p.OwnerId, SpawnFlags.IsClientCharacter);
                        }
                    }
                    */

                    if (Input.GetKeyDown(KeyCode.F10))
                {
                    BotManager.Spawn($"bot{(byte)GameData.Instance.GetAvailableId()}");                
                }
                if (Input.GetKeyDown(KeyCode.F11))
                {
                    BotManager.AllBotDespawn();
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
                    IsDebugModeBool = true;
                }
            }
            return IsDebugModeBool;
        }
    }
}
