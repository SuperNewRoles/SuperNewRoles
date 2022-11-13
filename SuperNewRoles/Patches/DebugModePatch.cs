using System;
using System.Collections.Generic;
using System.Linq;
using Agartha;
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
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.G, KeyCode.LeftControl }))
                {
                    PlayerControl bot = BotManager.Spawn(CachedPlayer.LocalPlayer.PlayerControl.NameText().text);

                    bot.NetTransform.SnapTo(CachedPlayer.LocalPlayer.PlayerControl.transform.position);
                    //new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                    //new LateTask(() => { foreach (PlayerControl pc in CachedPlayer.AllPlayers) pc.PlayerControl.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                    //new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
                }

                //ここにデバッグ用のものを書いてね
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.I, KeyCode.LeftControl }))
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers){p.RpcMurderPlayer(p);}
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.K, KeyCode.LeftControl }))
                {
                    PVCreator.Start();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.L, KeyCode.LeftControl }))
                {
                    PVCreator.End();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.M, KeyCode.LeftControl }))
                {
                    PVCreator.Start2();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.N, KeyCode.LeftControl }))
                {
                    ModHelpers.PlayerById(1).RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl);//ModHelpers.PlayerById(2));
                }

                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.F10, KeyCode.LeftControl }))
                {
                    BotManager.Spawn($"bot{(byte)GameData.Instance.GetAvailableId()}");
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.F11, KeyCode.LeftControl }))
                {
                    BotManager.AllBotDespawn();
                }
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.F1, KeyCode.LeftControl }))
                {
                    SuperNewRolesPlugin.Logger.LogInfo("new Vector2(" + (CachedPlayer.LocalPlayer.PlayerControl.transform.position.x - 12.63f) + "f, " + (CachedPlayer.LocalPlayer.PlayerControl.transform.position.y + 3.46f) + "f), ");
                }
            }

            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }
        public static bool IsDebugMode() => ConfigRoles.DebugMode.Value && CustomOptionHolder.IsDebugMode.GetBool();

        public static class MurderPlayerPatch
        {
            /// <summary>
            /// MurderPlayerが発動した時に通知します。
            /// </summary>
            public static void Announce()
            {
                if (!(IsDebugMode() && CustomOptionHolder.IsMurderPlayerAnnounce.GetBool())) return;

                new CustomMessage("MurderPlayerが発生しました", 5f);
                Logger.Info("MurderPlayerが発生しました", "DebugMode");
            }
        }
    }
}