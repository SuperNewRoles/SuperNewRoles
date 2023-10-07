using System.Collections.Generic;
using System.Timers;
using AmongUs.GameOptions;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public class TheThreeLittlePigs
{
    private const int OptionId = 301700;
    private const CustomOptionType type = CustomOptionType.Neutral;
    public static CustomRoleOption TheThreeLittlePigsOption;
    public static CustomOption TheThreeLittlePigsTeamCount;
    public static CustomOption TheThreeLittlePigsIsSettingNumberOfUniqueTasks;
    public static CustomOption TheThreeLittlePigsCommonTask;
    public static CustomOption TheThreeLittlePigsShortTask;
    public static CustomOption TheThreeLittlePigsLongTask;
    public static CustomOption TheFirstLittlePigClearTask;
    public static CustomOption TheFirstLittlePigIsCustomTimer;
    public static CustomOption TheFirstLittlePigCustomTimer;
    public static CustomOption TheSecondLittlePigClearTask;
    public static CustomOption TheSecondLittlePigMaxGuardCount;
    public static CustomOption TheThirdLittlePigClearTask;
    public static CustomOption TheThirdLittlePigMaxCounterCount;
    public static void SetupCustomOptions()
    {
        (TheThreeLittlePigsOption = new(OptionId, false, type, "TheThreeLittlePigsName", color, 1)).RoleId = RoleId.TheFirstLittlePig;
        TheThreeLittlePigsTeamCount = CustomOption.Create(OptionId + 1, false, type, "QuarreledTeamCountSetting", 1f, 1f, 4f, 1f, TheThreeLittlePigsOption);
        TheThreeLittlePigsIsSettingNumberOfUniqueTasks = CustomOption.Create(OptionId + 2, false, type, "IsSettingNumberOfUniqueTasks", false, TheThreeLittlePigsOption);
        var TheThreeLittlePigsoption = SelectTask.TaskSetting(OptionId + 3, OptionId + 4, OptionId + 5, TheThreeLittlePigsIsSettingNumberOfUniqueTasks, type);
        TheThreeLittlePigsCommonTask = TheThreeLittlePigsoption.Item1;
        TheThreeLittlePigsShortTask = TheThreeLittlePigsoption.Item2;
        TheThreeLittlePigsLongTask = TheThreeLittlePigsoption.Item3;
        TheFirstLittlePigClearTask = CustomOption.Create(OptionId + 6, false, type, "TheFirstLittlePigClearTaskSetting", ModHelpers.CustomRates(3), TheThreeLittlePigsOption);
        TheFirstLittlePigIsCustomTimer = CustomOption.Create(OptionId + 7, false, type, "TheFirstLittlePigIsCustomTimerSetting", false, TheThreeLittlePigsOption);
        TheFirstLittlePigCustomTimer = CustomOption.Create(OptionId + 8, false, type, "TheFirstLittlePigCustomTimerSetting", 30f, 5f, 60f, 2.5f, TheFirstLittlePigIsCustomTimer);
        TheSecondLittlePigClearTask = CustomOption.Create(OptionId + 9, false, type, "TheSecondLittlePigClearTaskSetting", ModHelpers.CustomRates(6), TheThreeLittlePigsOption);
        TheSecondLittlePigMaxGuardCount = CustomOption.Create(OptionId + 10, false, type, "TheSecondLittlePigManGuardMaxCountSetting", 1f, 1f, 15f, 1f, TheThreeLittlePigsOption);
        TheThirdLittlePigClearTask = CustomOption.Create(OptionId + 11, false, type, "TheThirdLittlePigClearTaskSetting", ModHelpers.CustomRates(10), TheThreeLittlePigsOption);
        TheThirdLittlePigMaxCounterCount = CustomOption.Create(OptionId + 12, false, type, "TheThirdLittlePigCounterKillSetting", 1f, 1f, 15f, 1f, TheThreeLittlePigsOption);
    }

    public static Color32 color = new(255, 99, 123, byte.MaxValue);
    public static List<List<PlayerControl>> TheThreeLittlePigsPlayer;
    public static TheFirstLittlePigClass TheFirstLittlePig;
    public static TheSecondLittlePigClass TheSecondLittlePig;
    public static TheThirdLittlePigClass TheThirdLittlePig;
    public static int AllTask
    {
        get
        {
            int num = TheThreeLittlePigsCommonTask.GetInt() + TheThreeLittlePigsShortTask.GetInt() + TheThreeLittlePigsLongTask.GetInt();
            if (!TheThreeLittlePigsIsSettingNumberOfUniqueTasks.GetBool() || num == 0)
                num = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks) +
                      GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks) +
                      GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
            return num;
        }
    }
    public static void ClearAndReload()
    {
        TheThreeLittlePigsPlayer = new();
        Logger.Info($"タスク総数 : {AllTask}", "TheThreeLittlePigs");
        TheFirstLittlePig = new();
        TheSecondLittlePig = new();
        TheThirdLittlePig = new();
    }

    //タスクを完了しているかの判定
    public static bool TaskCheck(PlayerControl player)
    {
        int clearTask = 0;
        switch (player.GetRole())
        {
            case RoleId.TheFirstLittlePig:
                clearTask = TheFirstLittlePig.ClearTask;
                break;
            case RoleId.TheSecondLittlePig:
                clearTask = TheSecondLittlePig.ClearTask;
                break;
            case RoleId.TheThirdLittlePig:
                clearTask = TheThirdLittlePig.ClearTask;
                break;
            default:
                return false;
        }
        return clearTask <= TaskCount.TaskDate(player.Data).Item1;
    }

    public static void FixedUpdate()
    {
        foreach (List<PlayerControl> plist in TheThreeLittlePigsPlayer)
        {
            List<PlayerControl> removes = new();
            foreach (PlayerControl player in plist)
                if (!IsTheThreeLittlePigs(player)) removes.Add(player);
            if (removes.Count > 0)
            {
                foreach (PlayerControl id in removes)
                    plist.Remove(id);
            }
        }
    }

    public class TheFirstLittlePigClass
    {
        public List<PlayerControl> Player;
        public int ClearTask;
        public float FlashTime;
        public Timer Timer;
        public void WrapUp()
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.TheFirstLittlePig)) return;
            TimerSet();
        }
        public void TimerSet()
        {
            Timer = new(FlashTime);
            Timer.Elapsed += (source, e) =>
            {
                if (PlayerControl.LocalPlayer.IsAlive() && TaskCheck(PlayerControl.LocalPlayer))
                {
                    Seer.ShowFlash(new Color32(245, 95, 71, byte.MaxValue), 2.5f);
                    Logger.Info($"{FlashTime / 1000}s経過して、条件が達成されていた為発光させました", "TheFirstLittlePig");
                }
                else
                {
                    Logger.Info($"{FlashTime / 1000}s経過しましたが、条件が達成されませんでした。条件(生きているか : {PlayerControl.LocalPlayer.IsAlive()}, タスクを完了しているか : {TaskCheck(PlayerControl.LocalPlayer)})", "TheFirstLittlePig");
                }
            };
            Timer.AutoReset = PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.IsAlive() : true;
            Timer.Enabled = PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.IsAlive() : true;
            if (PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.IsAlive() : true) return;
            Logger.Info($"{FlashTime / 1000}sにタイマーセット", "TheFirstLittlePig");
        }
        public void TimerStop(bool isEndGame = false)
        {
            if (Timer == null) return;
            Timer.Stop();
            if (isEndGame) Timer.Dispose();
            Logger.Info($"タイマーを止めました", "TheFirstLittlePig");
        }
        public TheFirstLittlePigClass()
        {
            Player = new();
            ClearTask = (int)(AllTask * (int.Parse(TheFirstLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            Logger.Info($"1番目の仔豚のタスク割合 : {ClearTask}, 割合 : {TheFirstLittlePigClearTask.GetString()}", "TheThreeLittlePigs");
            FlashTime = (TheFirstLittlePigIsCustomTimer.GetBool() ? TheFirstLittlePigCustomTimer.GetFloat() :
                        RoleClass.DefaultKillCoolDown >= 5 ? RoleClass.DefaultKillCoolDown : 5) * 1000;
        }
    }
    public class TheSecondLittlePigClass
    {
        public List<PlayerControl> Player;
        public int ClearTask;
        public Dictionary<byte, int> GuardCount;
        public TheSecondLittlePigClass()
        {
            Player = new();
            ClearTask = (int)(AllTask * (int.Parse(TheSecondLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            Logger.Info($"2番目の仔豚のタスク割合 : {ClearTask}, 割合 : {TheSecondLittlePigClearTask.GetString()}", "TheThreeLittlePigs");
            GuardCount = new();
        }
    }
    public class TheThirdLittlePigClass
    {
        public List<PlayerControl> Player;
        public int ClearTask;
        public Dictionary<byte, int> CounterCount;
        public TheThirdLittlePigClass()
        {
            Player = new();
            ClearTask = (int)(AllTask * (int.Parse(TheThirdLittlePigClearTask.GetString().Replace("%", "")) / 100f));
            Logger.Info($"3番目の仔豚のタスク割合 : {ClearTask}, 割合 : {TheThirdLittlePigClearTask.GetString()}", "TheThreeLittlePigs");
            CounterCount = new();
        }
    }

    public static bool IsTheThreeLittlePigs(PlayerControl player) =>
        player.IsRole(RoleId.TheFirstLittlePig, RoleId.TheSecondLittlePig, RoleId.TheThirdLittlePig);
    public static bool IsTheThreeLittlePigs(List<PlayerControl> players)
    {
        foreach (PlayerControl player in players)
            if (IsTheThreeLittlePigs(player)) return true;
        return false;
    }
}