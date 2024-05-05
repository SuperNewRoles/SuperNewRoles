using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class MapInsideManager
{
    private static byte currentMapColliderMapId = 255;
    private static Collider2D currentMapCollider;
    private static List<Collider2D> currentMapColliderInsides;

    public static bool CheckInside(Collider2D collider)
    {
        if (currentMapCollider == null)
            return false;
        if (!currentMapCollider.IsTouching(collider))
            return false;
        foreach (var currentMapColliderInside in currentMapColliderInsides)
        {
            if (currentMapColliderInside.IsTouching(collider))
                return false;
        }
        return true;
    }
    public static void ClearAndReloads()
    {
        byte currentMapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        if (currentMapCollider != null && currentMapColliderInsides != null && currentMapColliderMapId == currentMapId)
            return;
        Logger.Info("Loading map collider:" + ((MapNames)currentMapId).ToString() + "InsideCollider.prefab", "LoadMapCollider");
        GameObject ColliderPrefab = AssetManager.GetAsset<GameObject>(((MapNames)currentMapId).ToString() + "InsideCollider.prefab", AssetManager.AssetBundleType.Insidecollider);
        if (ColliderPrefab == null)
        {
            Logger.Error("Failed to load map collider", "LoadMapCollider");
            return;
        }
        currentMapCollider = GameObject.Instantiate(ColliderPrefab).GetComponent<Collider2D>();
        currentMapColliderInsides = currentMapCollider.transform.FindChild("InsideCollider").GetComponents<Collider2D>().ToList();
        currentMapColliderMapId = currentMapId;
    }
}