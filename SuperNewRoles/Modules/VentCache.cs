using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules.Compatibility;

namespace SuperNewRoles.Modules;

public class VentCache
{
    public static Vent[] Vents = [];
    private static Dictionary<int, Vent> _byId = new();

    public static void Init()
    {
        _byId = new Dictionary<int, Vent>();
        int maxId = -1;
        if (ShipStatus.Instance?.AllVents != null)
        {
            foreach (Vent vent in ShipStatus.Instance.AllVents)
            {
                if (vent == null)
                    continue;
                _byId[vent.Id] = vent;
                if (vent.Id > maxId)
                    maxId = vent.Id;
            }
        }

        int size = maxId < 254 ? 255 : maxId + 1;
        Vents = new Vent[size];
        foreach (KeyValuePair<int, Vent> kvp in _byId)
        {
            if (kvp.Key >= 0 && kvp.Key < Vents.Length)
                Vents[kvp.Key] = kvp.Value;
        }
    }

    public static Vent VentById(int id)
    {
        if (_byId != null && _byId.TryGetValue(id, out Vent vent))
            return vent;
        if (id < 0 || Vents == null || id >= Vents.Length)
        {
            Logger.Error($"VentCache: VentId is out of range: {id}");
            return null;
        }
        return Vents[id];
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatus_RpcSetVentStatus
    {
        [HarmonyPostfix]
        [HarmonyAfter(LevelImposterSupport.PluginGuid)]
        public static void Postfix(ShipStatus __instance)
        {
            Init();
        }
    }
}
