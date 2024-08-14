using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Modules;

//TownOfHostより！！
public class LateTask
{
    public string name;
    public float timer;
    public Action action;
    public static List<LateTask> Tasks = new();
    public static List<LateTask> AddTasks = new();
    public bool Run(float deltaTime)
    {
        timer -= deltaTime;
        if (timer <= 0)
        {
            try {
                action();
            }
            catch (Exception e) {
                Logger.Error("Error in LateTask", "LateTask");
                Logger.Error(e.ToString(), "LateTask");
                Logger.Error(e.StackTrace, "LateTask");
            }
            return true;
        }
        return false;
    }
    public LateTask(Action action, float time, string name = "No Name Task")
    {
        this.action = action;
        timer = time;
        this.name = name;
        AddTasks.Add(this);
        Logger.Info("New LateTask \"" + name + "\" is created", "LateTask");
    }
    public static void Update(float deltaTime)
    {
        var TasksToRemove = new List<LateTask>();
        Tasks.ForEach((task) =>
        {
            //Logger.Info("LateTask \"" + task.name + "\" Start","LateTask");
            if (task.Run(deltaTime))
            {
                Logger.Info("LateTask \"" + task.name + "\" is finished", "LateTask");
                TasksToRemove.Add(task);
            }
        });
        TasksToRemove.ForEach(task => Tasks.Remove(task));
        foreach (LateTask task in AddTasks)
        {
            Tasks.Add(task);
        }
        AddTasks = new List<LateTask>();
    }
}
[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
class LateUpdate
{
    public static void Postfix()
    {
        // LoversBreaker.LateUpdate();
        LateTask.Update(Time.deltaTime);
    }
}