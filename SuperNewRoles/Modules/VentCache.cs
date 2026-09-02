using System.Collections.Generic;
using HarmonyLib;

namespace SuperNewRoles.Modules;

public class VentCache
{
    public static Vent[] Vents;
    public static void Init()
    {
        Vents = new Vent[255];
        foreach (var vent in ShipStatus.Instance.AllVents)
        {
            Vents[vent.Id] = vent;
        }
    }
    public static Vent VentById(int id)
    {
        if (Vents == null)
        {
            Logger.Error($"VentCache: Vents is not initialized: {id}");
            return null;
        }
        if (id < 0 || id >= Vents.Length)
        {
            Logger.Error($"VentCache: VentId is out of range: {id}");
            return null;
        }
        return Vents[id];
    }
    public static void Add(Vent vent)
    {
        if (vent == null)
            return;
        if (Vents == null)
            Init();
        if (vent.Id < 0 || vent.Id >= Vents.Length)
        {
            Logger.Error($"VentCache: VentId is out of range: {vent.Id}");
            return;
        }
        Vents[vent.Id] = vent;
    }
    public static void Remove(Vent vent)
    {
        if (vent == null || Vents == null)
            return;
        if (vent.Id < 0 || vent.Id >= Vents.Length)
            return;
        if (Vents[vent.Id] == vent)
            Vents[vent.Id] = null;
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatus_RpcSetVentStatus
    {
        public static void Postfix(ShipStatus __instance)
        {
            Init();
        }
    }
}
