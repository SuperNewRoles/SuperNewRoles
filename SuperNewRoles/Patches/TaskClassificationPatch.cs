using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class TaskClassificationPatch
{
    public static void Prefix(ShipStatus __instance)
    {
        if (!GameSettingOptions.UploadDataTasksAsLongTasks) return;

        // タスクIDの初期化前に移動し、全クライアントの参照順序を揃える。
        var uploads = __instance.ShortTasks.Where(task => task.TaskType == TaskTypes.UploadData).ToArray();
        if (uploads.Length == 0) return;

        __instance.ShortTasks = __instance.ShortTasks.Where(task => task.TaskType != TaskTypes.UploadData).ToArray();
        __instance.LongTasks = __instance.LongTasks.Concat(uploads).ToArray();
        foreach (var task in uploads)
            task.Length = NormalPlayerTask.TaskLength.Long;
    }
}
