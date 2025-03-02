using System;

namespace SuperNewRoles.Modules;

public class DelayTask : LateTask
{
    public DelayTask(Action action, float delayTime, string taskName = "No Name Task") : base(action, delayTime, taskName)
    {
    }
    public static void UpdateOrAdd(Action action, float delayTime, ref DelayTask task, string taskName = "No Name Task")
    {
        if (task == null)
            task = new DelayTask(action, delayTime, taskName);
        else
            task.UpdateDelay(delayTime);
    }
    public static void CancelIfExist(ref DelayTask task)
    {
        if (task != null)
            task.Cancel();
        task = null;
    }
}