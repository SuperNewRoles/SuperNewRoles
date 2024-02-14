using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Safecracker
{
    private const int OptionId = 301800;// 設定のId
    public static CustomRoleOption SafecrackerOption;
    public static CustomOption SafecrackerPlayerCount;
    public static CustomOption SafecrackerKillGuardTask;
    public static CustomOption SafecrackerMaxKillGuardCount;
    public static CustomOption SafecrackerExiledGuardTask;
    public static CustomOption SafecrackerMaxExiledGuardCount;
    public static CustomOption SafecrackerUseVentTask;
    public static CustomOption SafecrackerUseSaboTask;
    public static CustomOption SafecrackerIsImpostorLightTask;
    public static CustomOption SafecrackerCheckImpostorTask;
    public static CustomOption SafecrackerIsSettingNumberOfUniqueTasks;
    public static CustomOption SafecrackerCommonTask;
    public static CustomOption SafecrackerShortTask;
    public static CustomOption SafecrackerLongTask;
    public static CustomOption SafecrackerChangeTaskPrefab;

    public static string[] CustomRates
    {
        get
        {
            List<string> customRates = new() { "NotAbility" };
            customRates.AddRange(CustomOptionHolder.rates);
            return customRates.ToArray();
        }
    }
    public static void SetupCustomOptions()
    {
        SafecrackerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Safecracker);
        SafecrackerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], SafecrackerOption);
        SafecrackerKillGuardTask = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "SafecrackerKillGuardTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerMaxKillGuardCount = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "SafecrackerMaxKillGuardCountSetting", 1f, 1f, 15f, 1f, SafecrackerKillGuardTask);
        SafecrackerExiledGuardTask = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "SafecrackerExiledGuardTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerMaxExiledGuardCount = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "SafecrackerMaxExiledGuardCountSetting", 1f, 1f, 15f, 1f, SafecrackerExiledGuardTask);
        SafecrackerUseVentTask = CustomOption.Create(OptionId + 6, false, CustomOptionType.Neutral, "SafecrackerUseVentTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerUseSaboTask = CustomOption.Create(OptionId + 7, false, CustomOptionType.Neutral, "SafecrackerUseSaboTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerIsImpostorLightTask = CustomOption.Create(OptionId + 8, false, CustomOptionType.Neutral, "SafecrackerIsImpostorLightTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerCheckImpostorTask = CustomOption.Create(OptionId + 9, false, CustomOptionType.Neutral, "SafecrackerCheckImpostorTaskSetting", CustomRates, SafecrackerOption);
        SafecrackerIsSettingNumberOfUniqueTasks = CustomOption.Create(OptionId + 14, false, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, SafecrackerOption);
        SafecrackerCommonTask = CustomOption.Create(OptionId + 10, false, CustomOptionType.Neutral, "GameCommonTasks", 1f, 0f, 300f, 1f, SafecrackerIsSettingNumberOfUniqueTasks);
        SafecrackerShortTask = CustomOption.Create(OptionId + 11, false, CustomOptionType.Neutral, "GameShortTasks", 1f, 0f, 300f, 1f, SafecrackerIsSettingNumberOfUniqueTasks);
        SafecrackerLongTask = CustomOption.Create(OptionId + 12, false, CustomOptionType.Neutral, "GameLongTasks", 1f, 0f, 300f, 1f, SafecrackerIsSettingNumberOfUniqueTasks);
        SafecrackerChangeTaskPrefab = CustomOption.Create(OptionId + 13, false, CustomOptionType.Neutral, "SafecrackerChangeTaskPrefabSetting", false, SafecrackerOption);
    }

    public static List<PlayerControl> SafecrackerPlayer;
    public static Color32 color = new(248, 217, 88, byte.MaxValue);
    public static int AllTask;
    public static PlayerControl TriggerPlayer;
    public static Dictionary<byte, int> KillGuardCount;
    public static Dictionary<byte, int> ExiledGuardCount;
    public static void ClearAndReload()
    {
        SafecrackerPlayer = new();
        AllTask = SelectTask.GetTotalTasks(RoleId.Safecracker);
        TriggerPlayer = null;
        KillGuardCount = new();
        ExiledGuardCount = new();
        CheckedTask = new();
    }

    public enum CheckTasks
    {
        KillGuard,
        ExiledGuard,
        UseVent,
        UseSabo,
        ImpostorLight,
        CheckImpostor
    }
    /// <summary>
    /// Key : プレイヤーId, Value : (キルガードが出来る, 追放ガードが出来る, ベントを使える, サボが使える, インポスタービジョン, インポスターを視認できる)
    /// </summary>
    public static Dictionary<byte, (bool, bool, bool, bool, bool, bool)> CheckedTask;
    public static bool CheckTask(PlayerControl target, CheckTasks type)
    {
        if (target == null) return false;
        var taskData = TaskCount.TaskDate(target.Data).Item1;
        switch (type)
        {
            case CheckTasks.KillGuard:
                if (SafecrackerKillGuardTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item1) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerKillGuardTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (true, CheckedTask[target.PlayerId].Item2, CheckedTask[target.PlayerId].Item3, CheckedTask[target.PlayerId].Item4, CheckedTask[target.PlayerId].Item5, CheckedTask[target.PlayerId].Item6);
                else CheckedTask.Add(target.PlayerId, (true, false, false, false, false, false));
                return true;
            case CheckTasks.ExiledGuard:
                if (SafecrackerExiledGuardTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item2) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerExiledGuardTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (CheckedTask[target.PlayerId].Item1, true, CheckedTask[target.PlayerId].Item3, CheckedTask[target.PlayerId].Item4, CheckedTask[target.PlayerId].Item5, CheckedTask[target.PlayerId].Item6);
                else CheckedTask.Add(target.PlayerId, (false, true, false, false, false, false));
                return true;
            case CheckTasks.UseVent:
                if (SafecrackerUseVentTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item3) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerUseVentTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (CheckedTask[target.PlayerId].Item1, CheckedTask[target.PlayerId].Item2, true, CheckedTask[target.PlayerId].Item4, CheckedTask[target.PlayerId].Item5, CheckedTask[target.PlayerId].Item6);
                else CheckedTask.Add(target.PlayerId, (false, false, true, false, false, false));
                return true;
            case CheckTasks.UseSabo:
                if (SafecrackerUseSaboTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item4) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerUseSaboTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (CheckedTask[target.PlayerId].Item1, CheckedTask[target.PlayerId].Item2, CheckedTask[target.PlayerId].Item3, true, CheckedTask[target.PlayerId].Item5, CheckedTask[target.PlayerId].Item6);
                else CheckedTask.Add(target.PlayerId, (false, false, false, true, false, false));
                return true;
            case CheckTasks.ImpostorLight:
                if (SafecrackerIsImpostorLightTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item5) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerIsImpostorLightTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (CheckedTask[target.PlayerId].Item1, CheckedTask[target.PlayerId].Item2, CheckedTask[target.PlayerId].Item3, CheckedTask[target.PlayerId].Item4, true, CheckedTask[target.PlayerId].Item6);
                else CheckedTask.Add(target.PlayerId, (false, false, false, false, true, false));
                return true;
            case CheckTasks.CheckImpostor:
                if (SafecrackerCheckImpostorTask.GetSelection() == 0) return false;
                if (CheckedTask.ContainsKey(target.PlayerId) && CheckedTask[target.PlayerId].Item6) return true;
                if ((int)(AllTask * (int.Parse(SafecrackerCheckImpostorTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                if (CheckedTask.ContainsKey(target.PlayerId)) CheckedTask[target.PlayerId] = (CheckedTask[target.PlayerId].Item1, CheckedTask[target.PlayerId].Item2, CheckedTask[target.PlayerId].Item3, CheckedTask[target.PlayerId].Item4, CheckedTask[target.PlayerId].Item5, true);
                else CheckedTask.Add(target.PlayerId, (false, false, false, false, false, true));
                return true;
        }
        return false;
    }

    [HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static class ConsolsUsePatch
    {
        static Minigame tempminigame;
        public static void Prefix(Console __instance)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Safecracker)
                && (SafecrackerChangeTaskPrefab.GetBool() || GameManager.Instance.LogicOptions.currentGameOptions.MapId != (int)MapNames.Airship))
            {
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
                    Logger.Info($"タスクタイプ : {task.TaskType}, タスクID : {(int)task.TaskType}", "Task Data");
                    tempminigame = task.MinigamePrefab;
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles or TaskTypes.MushroomMixupSabotage) return;
                    ShipStatus ship = GameManager.Instance.LogicOptions.currentGameOptions.MapId == (int)MapNames.Airship ? ShipStatus.Instance : Agartha.MapLoader.Airship;
                    task.MinigamePrefab = ship.LongTasks.FirstOrDefault(x => x.TaskType == TaskTypes.UnlockSafe).MinigamePrefab;
                }
            }
        }
        public static void Postfix(Console __instance)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Safecracker)
                && (SafecrackerChangeTaskPrefab.GetBool() || GameManager.Instance.LogicOptions.currentGameOptions.MapId != (int)MapNames.Airship))
            {
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out var _);
                if (canUse)
                {
                    PlayerTask task = __instance.FindTask(CachedPlayer.LocalPlayer);
                    task.MinigamePrefab = tempminigame;
                    tempminigame = null;
                }
            }
        }
    }
    public static List<byte> GenerateTasks(int count)
    {
        var tasks = new List<byte>();

        var task = MapUtilities.CachedShipStatus.LongTasks.FirstOrDefault(x => x.TaskType == TaskTypes.UnlockSafe);

        for (int i = 0; i < count; i++)
        {
            tasks.Add((byte)task.Index);
        }

        return tasks.ToArray().ToList();
    }
}