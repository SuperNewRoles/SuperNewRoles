// https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Utilities/MapUtilities.cs

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppSystem;

namespace SuperNewRoles;

public static class MapUtilities
{
    public static ShipStatus CachedShipStatus = ShipStatus.Instance;

    public static void MapDestroyed()
    {
        CachedShipStatus = ShipStatus.Instance;
        _systems.Clear();
    }

    private static readonly Dictionary<SystemTypes, Object> _systems = new();
    public static Dictionary<SystemTypes, Object> Systems
    {
        get
        {
            if (_systems.Count == 0) GetSystems();
            return _systems;
        }
    }

    private static void GetSystems()
    {
        if (!CachedShipStatus) return;

        var systems = CachedShipStatus.Systems;
        if (systems.Count <= 0) return;

        foreach (var systemTypes in SystemTypeHelpers.AllTypes)
        {
            if (!systems.ContainsKey(systemTypes)) continue;
            _systems[systemTypes] = systems[systemTypes].TryCast<Object>();
        }
    }

    public static void AddVent(Vent vent)
    {
        var allVents = CachedShipStatus.AllVents.ToList();
        allVents.Add(vent);
        CachedShipStatus.AllVents = allVents.ToArray();
    }
    public static void RemoveVent(Vent vent)
    {
        var allVents = CachedShipStatus.AllVents.ToList();
        allVents.Remove(vent);
        CachedShipStatus.AllVents = allVents.ToArray();
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatus_Awake_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix(ShipStatus __instance)
    {
        MapUtilities.CachedShipStatus = __instance;
    }
}
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
public static class ShipStatus_OnDestroy_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void Postfix()
    {
        MapUtilities.CachedShipStatus = null;
        MapUtilities.MapDestroyed();
    }
}