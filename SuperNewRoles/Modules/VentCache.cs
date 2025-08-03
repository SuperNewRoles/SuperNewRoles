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
        if (id < 0 || id >= Vents.Length)
        {
            Logger.Error($"VentCache: VentId is out of range: {id}");
            return null;
        }
        return Vents[id];
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
