
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
class OptimizeMapPatch
{
    public static void Postfix()
    {
        AddWireTasks();
        VentInfo.ShipStatusAwake();
    }
    public static void AddWireTasks()
    {
        // Airship配線タスク追加
        if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AddWireTask.GetBool())
        {
            ActivateWiring("task_wiresHallway2", 2);
            ActivateWiring("task_electricalside2", 3).Room = SystemTypes.Armory;
            ActivateWiring("task_wireShower", 4);
            ActivateWiring("taks_wiresLounge", 5);
            ActivateWiring("panel_wireHallwayL", 6);
            ActivateWiring("task_wiresStorage", 7);
            ActivateWiring("task_electricalSide", 8).Room = SystemTypes.VaultRoom;
            ActivateWiring("task_wiresMeeting", 9);
        }
    }
    protected static Console ActivateWiring(string consoleName, int consoleId)
    {
        Console console = ActivateConsole(consoleName);

        if (console == null)
        {
            SuperNewRolesPlugin.Logger.LogError($"consoleName \"{consoleName}\" is null");
            return null;
        }

        if (!console.TaskTypes.Contains(TaskTypes.FixWiring))
        {
            var list = console.TaskTypes.ToList();
            list.Add(TaskTypes.FixWiring);
            console.TaskTypes = list.ToArray();
        }
        console.ConsoleId = consoleId;
        return console;
    }
    protected static Console ActivateConsole(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            SuperNewRolesPlugin.Logger.LogError($"Object \"{objectName}\" was not found!");
            return null;
        }
        obj.layer = LayerMask.NameToLayer("ShortObjects");
        Console console = obj.GetComponent<Console>();
        PassiveButton button = obj.GetComponent<PassiveButton>();
        CircleCollider2D collider = obj.GetComponent<CircleCollider2D>();
        if (!console)
        {
            console = obj.AddComponent<Console>();
            console.checkWalls = true;
            console.usableDistance = 0.7f;
            console.TaskTypes = new TaskTypes[0];
            console.ValidTasks = new Il2CppReferenceArray<TaskSet>(0);
            var list = ShipStatus.Instance.AllConsoles.ToList();
            list.Add(console);
            ShipStatus.Instance.AllConsoles = new Il2CppReferenceArray<Console>(list.ToArray());
        }
        if (console.Image == null)
        {
            console.Image = obj.GetComponent<SpriteRenderer>();
            console.Image.material = new Material(ShipStatus.Instance.AllConsoles[0].Image.material);
        }
        if (!button)
        {
            button = obj.AddComponent<PassiveButton>();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver = new UnityEngine.Events.UnityEvent();
            button._CachedZ_k__BackingField = 0.1f;
            button.CachedZ = 0.1f;
        }
        if (!collider)
        {
            collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
        }
        return console;
    }
}