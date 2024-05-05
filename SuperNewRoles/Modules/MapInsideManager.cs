using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class MapInsideManager
{
    private static byte currentMapColliderMapId = 255;
    private static Collider2D currentMapCollider;

    public static bool CheckInside(Collider2D collider)
    {
        if (currentMapCollider == null)
            return false;
        return currentMapCollider.IsTouching(collider);
    }
    public static void ClearAndReloads()
    {
        byte currentMapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
        if (currentMapCollider != null && currentMapColliderMapId == currentMapId)
            return;
        Logger.Info("Loading map collider:" + ((MapNames)currentMapId).ToString() + "InsideCollider.prefab", "LoadMapCollider");
        GameObject ColliderPrefab = AssetManager.GetAsset<GameObject>(((MapNames)currentMapId).ToString() + "InsideCollider.prefab", AssetManager.AssetBundleType.Insidecollider);
        if (ColliderPrefab == null)
        {
            Logger.Error("Failed to load map collider", "LoadMapCollider");
            return;
        }
        currentMapCollider = GameObject.Instantiate(ColliderPrefab).GetComponent<Collider2D>();
        currentMapColliderMapId = currentMapId;
    }
}