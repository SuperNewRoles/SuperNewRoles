using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Modules;
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

public static class SaboStateTracker
{
    private const float CountdownStopped = 10000f;
    private static readonly SystemTypes[] TrackedSaboTypes = new SystemTypes[]
    {
        SystemTypes.Electrical,
        SystemTypes.LifeSupp,
        SystemTypes.Reactor,
        SystemTypes.Laboratory,
        SystemTypes.Comms,
        SystemTypes.HeliSabotage,
        SystemTypes.MushroomMixupSabotage,
    };

    public static HashSet<SystemTypes> activeSaboTypes { get; } = new();

    public static void UpdateAll(ShipStatus shipStatus)
    {
        if (shipStatus?.Systems == null)
        {
            activeSaboTypes.Clear();
            return;
        }

        // O2 や Mushroom Mixup は UpdateSystem ではなく Deteriorate 側で解除されるため、都度実状態を差分検知する。
        var currentActiveSaboTypes = new HashSet<SystemTypes>();
        foreach (var systemType in TrackedSaboTypes)
        {
            if (IsSabotageActive(shipStatus, systemType))
            {
                currentActiveSaboTypes.Add(systemType);
            }
        }

        foreach (var systemType in currentActiveSaboTypes)
        {
            if (activeSaboTypes.Contains(systemType))
            {
                continue;
            }

            Logger.Info("Sabotage Start: " + systemType);
            SaboStartEvent.Invoke(systemType);
            activeSaboTypes.Add(systemType);
        }

        var endedSaboTypes = new List<SystemTypes>();
        foreach (var systemType in activeSaboTypes)
        {
            if (!currentActiveSaboTypes.Contains(systemType))
            {
                endedSaboTypes.Add(systemType);
            }
        }

        foreach (var systemType in endedSaboTypes)
        {
            Logger.Info("Sabotage End: " + systemType);
            SaboEndEvent.Invoke(systemType);
            activeSaboTypes.Remove(systemType);
        }
    }

    private static bool IsSabotageActive(ShipStatus shipStatus, SystemTypes systemType)
    {
        if (!shipStatus.Systems.TryGetValue(systemType, out var system))
        {
            return false;
        }

        // HeliSabotageSystem は修理後も Countdown が残るため、実状態を持つ IsActive を優先する。
        var activatable = system.TryCast<IActivatable>();
        if (activatable != null)
        {
            return activatable.IsActive;
        }

        var lifeSupp = system.TryCast<LifeSuppSystemType>();
        if (lifeSupp != null)
        {
            return lifeSupp.Countdown < CountdownStopped;
        }

        var criticalSabotage = system.TryCast<ICriticalSabotage>();
        if (criticalSabotage != null)
        {
            return criticalSabotage.Countdown < CountdownStopped;
        }

        var switchSystem = system.TryCast<SwitchSystem>();
        if (switchSystem != null)
        {
            return switchSystem.ExpectedSwitches != switchSystem.ActualSwitches;
        }

        var mushroomMixup = system.TryCast<MushroomMixupSabotageSystem>();
        if (mushroomMixup != null)
        {
            return mushroomMixup.CurrentSecondsUntilHeal > 0f;
        }

        return false;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(byte)])]
public static class ShipStatusUpdateSystemPatch
{
    public static void Postfix(ShipStatus __instance)
    {
        SaboStateTracker.UpdateAll(__instance);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.UpdateSystem), [typeof(SystemTypes), typeof(PlayerControl), typeof(MessageReader)])]
public static class ShipStatusUpdateSystemMessageReaderPatch
{
    public static void Postfix(ShipStatus __instance)
    {
        SaboStateTracker.UpdateAll(__instance);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
public static class ShipStatusFixedUpdatePatch
{
    public static void Postfix(ShipStatus __instance)
    {
        SaboStateTracker.UpdateAll(__instance);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class AmongUsClientCoStartGamePatch
{
    public static void Postfix()
    {
        SaboStateTracker.activeSaboTypes.Clear();
    }
}
