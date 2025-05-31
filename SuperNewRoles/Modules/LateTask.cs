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

    public string Name { get; }
    public float Timer { get; private set; }
    public Action Action { get; }

    /// <summary>遅延タスクコンストラクタ</summary>
    /// <param name="action">実行アクション</param>
    /// <param name="delayTime">遅延時間（秒）</param>
    /// <param name="taskName">タスク名（デバッグ用）</param>
    public LateTask(Action action, float delayTime, string taskName = "No Name Task")
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Timer = delayTime >= 0 ? delayTime : 0;
        Name = taskName;

        lock (_lock)
        {
            AddTasks.Add(this);
        }

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

    private bool Execute()
    {
        try
        {
            Timer -= Time.deltaTime;
            if (Timer > 0) return false;

            Action.Invoke();
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
        var completedTasks = new List<LateTask>(Tasks.Count);
        foreach (var task in Tasks)
        {
            if (task.Execute())
                completedTasks.Add(task);
        }

        if (completedTasks.Count > 0)
        {
            Tasks.RemoveAll(t => completedTasks.Contains(t));
        }

        lock (_lock)
        {
            if (AddTasks.Count > 0)
            {
                Tasks.AddRange(AddTasks);
                AddTasks.Clear();
            }
            if (RemoveTasks.Count > 0)
            {
                Tasks.RemoveAll(t => RemoveTasks.Contains(t));
                RemoveTasks.Clear();
            }
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
