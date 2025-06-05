using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class SaboStartEventData : IEventData
{
    public SystemTypes saboType { get; }
    public SaboStartEventData(SystemTypes saboType)
    {
        this.saboType = saboType;
    }
}

public class SaboStartEvent : EventTargetBase<SaboStartEvent, SaboStartEventData>
{
    public static void Invoke(SystemTypes saboType)
    {
        var data = new SaboStartEventData(saboType);
        Instance.Awake(data);
    }
}

public class SaboEndEventData : IEventData
{
    public SystemTypes saboType { get; }
    public SaboEndEventData(SystemTypes saboType)
    {
        this.saboType = saboType;
    }
}

public class SaboEndEvent : EventTargetBase<SaboEndEvent, SaboEndEventData>
{
    public static void Invoke(SystemTypes saboType)
    {
        var data = new SaboEndEventData(saboType);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(byte)])]
public static class ShipStatusUpdateSystemPatch
{
    public static void Postfix(ShipStatus __instance, SystemTypes systemType)
    {
        ShipStatusUpdateSystemMessageReaderPatch.UpdateSabo(__instance, systemType);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader)])]
public static class ShipStatusUpdateSystemMessageReaderPatch
{
    public static HashSet<SystemTypes> activeSaboTypes { get; } = new();
    public static void Postfix(ShipStatus __instance, SystemTypes systemType)
    {
        UpdateSabo(__instance, systemType);
    }
    public static void UpdateSabo(ShipStatus __instance, SystemTypes systemType)
    {
        if (!__instance.Systems.TryGetValue(systemType, out var value)) return;
        value.TryCastOut(out IActivatable activatable);
        if (activatable == null) return;
        if (activatable.IsActive && !activeSaboTypes.Contains(systemType))
        {
            SaboStartEvent.Invoke(systemType);
            activeSaboTypes.Add(systemType);
        }
        else if (!activatable.IsActive && activeSaboTypes.Contains(systemType))
        {
            SaboEndEvent.Invoke(systemType);
            activeSaboTypes.Remove(systemType);
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClientCoStartGamePatch
{
    public static void Postfix()
    {
        ShipStatusUpdateSystemMessageReaderPatch.activeSaboTypes.Clear();
    }
}