using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class DoorConsoleUseEventData : IEventData
{
    public ExPlayerControl player { get; }
    public DoorConsole doorConsole { get; }

    public DoorConsoleUseEventData(ExPlayerControl player, DoorConsole doorConsole)
    {
        this.player = player;
        this.doorConsole = doorConsole;
    }
}

public class DoorConsoleUseEvent : EventTargetBase<DoorConsoleUseEvent, DoorConsoleUseEventData>
{
    public static void Invoke(ExPlayerControl player, DoorConsole doorConsole)
    {
        var data = new DoorConsoleUseEventData(player, doorConsole);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.Use))]
public static class DoorConsoleUsePatch
{
    public static void Postfix(DoorConsole __instance)
    {
        __instance.CanUse(ExPlayerControl.LocalPlayer.Data, out var canUse, out var _);
        if (canUse)
        {
            DoorConsoleUseEvent.Invoke(ExPlayerControl.LocalPlayer, __instance);
        }
    }
}