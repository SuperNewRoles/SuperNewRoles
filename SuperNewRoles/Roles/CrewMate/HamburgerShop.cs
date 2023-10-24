using System.Collections.Generic;
using System.Linq;
using Agartha;
using HarmonyLib;

namespace SuperNewRoles.Roles.CrewMate;

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
                if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles) return;
                ShipStatus ship = GameManager.Instance.LogicOptions.currentGameOptions.MapId == (int)MapNames.Airship ? ShipStatus.Instance : MapLoader.Airship;
                task.MinigamePrefab = ship.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger).MinigamePrefab;
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

        var task = MapUtilities.CachedShipStatus.ShortTasks.FirstOrDefault(x => x.TaskType == TaskTypes.MakeBurger);

        for (int i = 0; i < count; i++)
        {
            tasks.Add((byte)task.Index);
        }

        return tasks.ToArray().ToList();
    }
}