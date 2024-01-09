using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Modules;
public static class CustomSpores
{
    public static Dictionary<int, Mushroom> mushRooms { get; private set; }
    public static void ClearAndReloads()
    {
        mushRooms = new();
    }
    public static Mushroom AddMushroom(Vector2 position, int id = -1)
    {
        Dictionary<int, Mushroom> mushrooms = mushRooms;
        Vector3 position3 = position;
        position3.z = 1;
        Il2CppSystem.Collections.Generic.Dictionary<int, Mushroom> mushroomsShip = null;
        if (ModHelpers.IsMap(MapNames.Fungle))
        {
            mushroomsShip = ShipStatus.Instance.TryCast<FungleShipStatus>().sporeMushrooms;
            mushrooms = null;
        }
        if (id == -1)
            if (mushrooms != null)
                id = mushrooms.Count <= 0 ? 0 : mushrooms.Max(x => x.Key) + 1;
            else
                id = mushroomsShip.entries.Max(x => x.hashCode) + 1;
        Mushroom newmushRoom = GameObject.Instantiate(Agartha.MapLoader.FungleObject.GetComponentInChildren<Mushroom>(), ShipStatus.Instance.transform);
        newmushRoom.transform.position = position3;
        newmushRoom.id = id;
        newmushRoom.origPosition = position;
        if (mushrooms != null)
            mushrooms.Add(id, newmushRoom);
        else
            mushroomsShip.Add(id, newmushRoom);
        return newmushRoom;
    }
    public static Mushroom GetMushroomFromId(int id)
    {
        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus != null)
            return fungleShipStatus.GetMushroomFromId(id);
        if (mushRooms.TryGetValue(id, out var value))
            return value;
        ShipStatus.Instance.logger.Error($"Failed to check spore mushroom {id} - no mushroom exists");
        return null;
    }
    public static void TriggerSporesFromMushroom(int id)
    {
        if (mushRooms.TryGetValue(id, out var value))
            value.TriggerSpores();
        else
            ShipStatus.Instance.logger.Error($"Failed to trigger spore mushroom {id} - no mushroom exists");
    }
}
