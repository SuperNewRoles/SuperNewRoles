using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules;

/// <summary>遅延実行タスク管理クラス</summary>
public class LateTask
{
    private static readonly object _lock = new();
    private static readonly List<LateTask> Tasks = new();
    private static readonly List<LateTask> AddTasks = new();
    private static readonly List<LateTask> RemoveTasks = new();
    private static readonly List<LateTask> CompletedTasks = new();

    public string Name { get; }
    public float Timer { get; private set; }
    public Action Action { get; }
    private readonly bool logEnabled;

    /// <summary>遅延タスクコンストラクタ</summary>
    /// <param name="action">実行アクション</param>
    /// <param name="delayTime">遅延時間（秒）</param>
    /// <param name="taskName">タスク名（デバッグ用）</param>
    public LateTask(Action action, float delayTime, string taskName = "No Name Task", bool log = true)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Timer = delayTime >= 0 ? delayTime : 0;
        Name = taskName;
        logEnabled = log;

        lock (_lock)
        {
            AddTasks.Add(this);
        }

        if (logEnabled)
            Logger.Info($"New LateTask \"{Name}\" created (Delay: {delayTime}s)", "LateTask");
    }
    public void Cancel()
    {
        lock (_lock)
        {
            RemoveTasks.Add(this);
        }
    }
    public void UpdateDelay(float delayTime)
    {
        lock (_lock)
        {
            Timer = delayTime;
        }
    }

    private bool Execute(float deltaTime)
    {
        try
        {
            Timer -= deltaTime;
            if (Timer > 0) return false;

            Action.Invoke();
            if (logEnabled)
                Logger.Info($"LateTask \"{Name}\" completed", "LateTask");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in LateTask \"{Name}\": {ex.Message}\n{ex.StackTrace}", "LateTask");
            return true; // エラー発生時もタスクを削除
        }
    }

    public static void Update(float deltaTime)
    {
        CompletedTasks.Clear();
        foreach (var task in Tasks)
        {
            if (task.Execute(deltaTime))
                CompletedTasks.Add(task);
        }

        RemoveFromTasks(CompletedTasks);
        CompletedTasks.Clear();

        lock (_lock)
        {
            if (AddTasks.Count > 0)
            {
                Tasks.AddRange(AddTasks);
                AddTasks.Clear();
            }
            if (RemoveTasks.Count > 0)
            {
                RemoveFromTasks(RemoveTasks);
                RemoveTasks.Clear();
            }
        }
    }

    private static void RemoveFromTasks(List<LateTask> removeTasks)
    {
        if (removeTasks.Count <= 0)
            return;

        for (int i = Tasks.Count - 1; i >= 0; i--)
        {
            if (removeTasks.Contains(Tasks[i]))
                Tasks.RemoveAt(i);
        }
    }
}

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
internal class LateUpdatePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        try
        {
            LateTask.Update(Time.deltaTime);
        }
        catch (Exception ex)
        {
            Logger.Error($"LateTask Update Error: {ex.Message}", "LateUpdate");
        }
    }
}
