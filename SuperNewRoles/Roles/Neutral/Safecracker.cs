using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Patches;
using SuperNewRoles.ReplayManager;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class Safecracker : RoleBase<Safecracker>
{
    public static Color32 color = new(248, 217, 88, byte.MaxValue);

    public Safecracker()
    {
        RoleId = roleId = RoleId.Safecracker;
        //以下いるもののみ変更
        OptionId = 1147;
        OptionType = CustomOptionType.Neutral;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate() { }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() {  }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public static CustomOption SafecrackerKillGuardTask;
    public static CustomOption SafecrackerMaxKillGuardCount;
    public static CustomOption SafecrackerExiledGuardTask;
    public static CustomOption SafecrackerMaxExiledGuardCount;
    public static CustomOption SafecrackerUseVentTask;
    public static CustomOption SafecrackerUseSaboTask;
    public static CustomOption SafecrackerIsImpostorLightTask;
    public static CustomOption SafecrackerCheckImpostorTask;
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
    public override void SetupMyOptions()
    {
        SafecrackerKillGuardTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerKillGuardTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerMaxKillGuardCount = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerMaxKillGuardCountSetting", 1f, 1f, 15f, 1f, SafecrackerKillGuardTask); OptionId++;
        SafecrackerExiledGuardTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerExiledGuardTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerMaxExiledGuardCount = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerMaxExiledGuardCountSetting", 1f, 1f, 15f, 1f, SafecrackerExiledGuardTask); OptionId++;
        SafecrackerUseVentTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerUseVentTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerUseSaboTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerUseSaboTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerIsImpostorLightTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerIsImpostorLightTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerCheckImpostorTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerCheckImpostorTaskSetting", CustomRates, RoleOption); OptionId++;
        SafecrackerCommonTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "GameCommonTasks", 1f, 0f, 300f, 1f, RoleOption); OptionId++;
        SafecrackerShortTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "GameShortTasks", 1f, 0f, 300f, 1f, RoleOption); OptionId++;
        SafecrackerLongTask = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "GameLongTasks", 1f, 0f, 300f, 1f, RoleOption); OptionId++;
        SafecrackerChangeTaskPrefab = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "SafecrackerChangeTaskPrefabSetting", false, RoleOption); OptionId++;
    }

    public static int AllTaskNum
    {
        get
        {
            int num = SafecrackerCommonTask.GetInt() + SafecrackerShortTask.GetInt() + SafecrackerLongTask.GetInt();
            return num != 0 ? num : GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks) +
                                    GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks) +
                                    GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
        }
    }

    public static Dictionary<byte, Ability> CheckedTask;
    public static Dictionary<byte, int> KillGuardCount;
    public static Dictionary<byte, int> ExiledGuardCount;
    public static void Clear()
    {
        players = new();
        CheckedTask = new();
        KillGuardCount = new();
        ExiledGuardCount = new();
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

    public static bool CheckTask(PlayerControl target, CheckTasks type)
    {
        if (!target || !target.IsRole(RoleId.Safecracker)) return false;
        byte id = target.PlayerId;
        if (!CheckedTask.ContainsKey(id)) CheckedTask.Add(id, new());
        var taskData = TaskCount.TaskDate(target.Data).Item1;
        switch (type)
        {
            case CheckTasks.KillGuard:
                if (SafecrackerKillGuardTask.GetSelection() == 0) return false;
                if (CheckedTask[id].KillGuard) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerKillGuardTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].KillGuard = true;
                return true;
            case CheckTasks.ExiledGuard:
                if (SafecrackerExiledGuardTask.GetSelection() == 0) return false;
                if (CheckedTask[id].ExiledGuard) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerExiledGuardTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].ExiledGuard = true;
                return true;
            case CheckTasks.UseVent:
                if (SafecrackerUseVentTask.GetSelection() == 0) return false;
                if (CheckedTask[id].UseVent) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerUseVentTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].UseVent = true;
                return true;
            case CheckTasks.UseSabo:
                if (SafecrackerUseSaboTask.GetSelection() == 0) return false;
                if (CheckedTask[id].UseSabo) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerUseSaboTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].UseSabo = true;
                return true;
            case CheckTasks.ImpostorLight:
                if (SafecrackerIsImpostorLightTask.GetSelection() == 0) return false;
                if (CheckedTask[id].ImpostorLight) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerIsImpostorLightTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].ImpostorLight = true;
                return true;
            case CheckTasks.CheckImpostor:
                if (SafecrackerCheckImpostorTask.GetSelection() == 0) return false;
                if (CheckedTask[id].CheckImpostor) return true;
                if ((int)(AllTaskNum * (int.Parse(SafecrackerCheckImpostorTask.GetString().Replace("%", "")) / 100f)) > taskData) return false;
                CheckedTask[id].CheckImpostor = true;
                return true;
        }
        return false;
    }

    public class Ability
    {
        public bool KillGuard;
        public bool ExiledGuard;
        public bool UseVent;
        public bool UseSabo;
        public bool ImpostorLight;
        public bool CheckImpostor;
        public Ability()
        {
            KillGuard = false;
            ExiledGuard = false;
            UseVent = false;
            UseSabo = false;
            ImpostorLight = false;
            CheckImpostor = false;
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
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles) return;
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
    /*
    public static int AllTask;
    public static PlayerControl TriggerPlayer;
    public static Dictionary<byte, int> KillGuardCount;
    public static Dictionary<byte, int> ExiledGuardCount;
    public static void ClearAndReload()
    {
        SafecrackerPlayer = new();
        int num = SafecrackerCommonTask.GetInt() + SafecrackerShortTask.GetInt() + SafecrackerLongTask.GetInt();
        AllTask = num != 0 ? num : GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks) +
                                   GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks) +
                                   GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
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
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles) return;
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
    //*/
}