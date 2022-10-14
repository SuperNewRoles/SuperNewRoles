using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Patches
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
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(x):" + __instance.transform.position.x);
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(y):" + __instance.transform.position.y);
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(Z):" + __instance.transform.position.z);
                }
            }
        }
        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class DebugManager
        {
            private static readonly System.Random random = new((int)DateTime.Now.Ticks);
            private static readonly List<PlayerControl> bots = new();

            public static void Postfix(KeyboardJoystick __instance)
            {
                if (!ConfigRoles.DebugMode.Value) return;

                // Spawn dummys
                if (Input.GetKeyDown(KeyCode.G))
                {
                    PlayerControl bot = BotManager.Spawn(PlayerControl.LocalPlayer.NameText().text);

                    bot.NetTransform.SnapTo(PlayerControl.LocalPlayer.transform.position);
                    //new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                    //new LateTask(() => { foreach (var pc in CachedPlayer.AllPlayers) pc.PlayerControl.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                    //new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
                }

                //ここにデバッグ用のものを書いてね
                if (Input.GetKeyDown(KeyCode.I))
                {
                    foreach (PlayerControl p in Roles.RoleClass.Jackal.JackalPlayer)
                        p.ShowReactorFlash();
                }
                if (Input.GetKeyDown(KeyCode.K))
                {
                    PVCreator.Start();
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    PVCreator.End();
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    PVCreator.Start2();
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    ModHelpers.PlayerById(1).RpcMurderPlayer(PlayerControl.LocalPlayer);//ModHelpers.PlayerById(2));
                }

                if (Input.GetKeyDown(KeyCode.F10))
                {
                    BotManager.Spawn($"bot{(byte)GameData.Instance.GetAvailableId()}");
                }
                if (Input.GetKeyDown(KeyCode.F11))
                {
                    BotManager.AllBotDespawn();
                }
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    SuperNewRolesPlugin.Logger.LogInfo("new Vector2(" + (PlayerControl.LocalPlayer.transform.position.x - 12.63f) + "f, " + (PlayerControl.LocalPlayer.transform.position.y + 3.46f) + "f), ");
                }
            }

            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }
        public static bool IsDebugMode()
        {
            var IsDebugModeBool = false;
            if (ConfigRoles.DebugMode.Value)
            {
                if (CustomOptions.IsDebugMode.GetBool())
                {
                    IsDebugModeBool = true;
                }
            }
            return IsDebugModeBool;
        }
    }
}