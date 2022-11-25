using System;
using System.Collections.Generic;
using System.Linq;
using Agartha;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class HamburgerShop
    {
        [HarmonyPatch(typeof(Console), nameof(Console.Use))]
        public static class ConsolsUsePatch
        {
            static Minigame tempminigame;
            public static void Prefix(Console __instance)
            {
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.HamburgerShop)
                    || !CustomOptionHolder.HamburgerShopChangeTaskPrefab.GetBool()) return;
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
                    tempminigame = task.MinigamePrefab;
                    ShipStatus ship = PlayerControl.GameOptions.MapId == (int)MapNames.Airship ? ShipStatus.Instance : MapLoader.Airship;
                    task.MinigamePrefab = ship.NormalTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger).MinigamePrefab;
                }
            }
            public static void Postfix(Console __instance)
            {
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.HamburgerShop)
                    || !CustomOptionHolder.HamburgerShopChangeTaskPrefab.GetBool()) return;
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
                    task.MinigamePrefab = tempminigame;
                    tempminigame = null;
                }
            }
        }
        public static List<byte> GenerateTasks(int count)
        {
            var tasks = new List<byte>();

            var task = MapUtilities.CachedShipStatus.NormalTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger);

            for (int i = 0; i < count; i++)
            {
                tasks.Add((byte)task.Index);
            }

            return tasks.ToArray().ToList();
        }
    }
}