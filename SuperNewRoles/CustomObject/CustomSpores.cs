using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomObject;

public static class CustomSpores
{
    public static Dictionary<int, Mushroom> mushRooms { get; private set; } = new();

    public static void ClearAndReloads()
    {
        mushRooms = new();
    }

    public static void AddMushroom(Vector2 position, Action<Mushroom> callback, int id = -1)
    {
        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        Mushroom mushroom = null;
        if (fungleShipStatus != null)
        {
            mushroom = AddMushroomAsync(fungleShipStatus, position, id);
            callback(mushroom);
        }
        else
            MapLoader.LoadMap(MapNames.Fungle, (map) =>
            {
                mushroom = AddMushroomAsync(map.TryCast<FungleShipStatus>(), position, id);
                callback(mushroom);
            });
    }

    public static Mushroom AddMushroomAsync(FungleShipStatus ship, Vector2 position, int id = -1)
    {
        Dictionary<int, Mushroom> mushrooms = mushRooms;
        Vector3 position3 = position;
        position3.z = 1;
        Il2CppSystem.Collections.Generic.Dictionary<int, Mushroom> mushroomsShip = null;

        if (ModHelpers.IsMap(MapNames.Fungle))
        {
            mushroomsShip = ship.sporeMushrooms;
            mushrooms = null;
        }
        else
        {
            mushroomsShip = null;
        }

        if (id == -1)
        {
            if (mushrooms != null)
                id = mushrooms.Count <= 0 ? 0 : mushrooms.Max(x => x.Key) + 1;
            else
            {
                int maxId = 0;
                foreach (var kvp in mushroomsShip)
                {
                    if (kvp.Key > maxId) maxId = kvp.Key;
                }
                id = maxId + 1;
            }
        }

        // FungleマップのMushroomプレハブを取得
        Mushroom templateMushroom = ship.GetComponentInChildren<Mushroom>();

        if (templateMushroom == null)
        {
            Logger.Error("CustomSpores: Mushroom template not found!");
            return null;
        }

        Mushroom newmushRoom = UnityEngine.Object.Instantiate(templateMushroom, ShipStatus.Instance.transform);
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

        Logger.Error($"Failed to check spore mushroom {id} - no mushroom exists");
        return null;
    }

    public static void TriggerSporesFromMushroom(int id)
    {
        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus != null)
        {
            var mushroom = fungleShipStatus.GetMushroomFromId(id);
            if (mushroom != null)
            {
                mushroom.TriggerSpores();
                return;
            }
        }
        if (mushRooms.TryGetValue(id, out var value))
            value.TriggerSpores();
        else
            Logger.Error($"Failed to trigger spore mushroom {id} - no mushroom exists");
    }
}