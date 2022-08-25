using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles
{
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
                action();
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
            //Logger.info("New LateTask \"" + name + "\" is created");
        }
        public static void Update(float deltaTime)
        {
            var TasksToRemove = new List<LateTask>();
            Tasks.ForEach((task) =>
            {
                //SuperNewRolesPlugin.Logger.LogInfo("LateTask \"" + task.name + "\" Start");
                if (task.Run(deltaTime))
                {
                    //SuperNewRolesPlugin.Logger.LogInfo("LateTask \"" + task.name + "\" is finished");
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
            LateTask.Update(Time.deltaTime);
        }
    }
}