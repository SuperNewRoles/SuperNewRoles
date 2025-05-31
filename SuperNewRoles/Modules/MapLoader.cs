using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles.Modules;

public static class MapLoader
{
    private static ShipStatus airship;
    private static ShipStatus fungle;

    private static HashSet<AssetReference> loadedMaps = new();
    private static Dictionary<MapNames, List<Action<ShipStatus>>> loadingMaps = new();

    public static void LoadMap(MapNames map, Action<ShipStatus> onLoaded)
    {
        switch (map)
        {
            case MapNames.Airship when airship != null:
                onLoaded(airship);
                break;
            case MapNames.Fungle when fungle != null:
                onLoaded(fungle);
                break;
            default:
                AmongUsClient.Instance.StartCoroutine(LoadMapCoroutine(map, onLoaded).WrapToIl2Cpp());
                break;
        }
    }
    public static IEnumerator LoadMapAsync(MapNames map, Action<ShipStatus> onLoaded)
    {
        switch (map)
        {
            case MapNames.Airship when airship != null:
                onLoaded(airship);
                break;
            case MapNames.Fungle when fungle != null:
                onLoaded(fungle);
                break;
            default:
                yield return LoadMapCoroutine(map, onLoaded).WrapToIl2Cpp();
                break;
        }
    }
    private static IEnumerator LoadMapCoroutine(MapNames map, Action<ShipStatus> onLoaded)
    {
        while (AmongUsClient.Instance == null) { yield return null; }
        Stopwatch sw = new();
        sw.Start();
        if (loadingMaps.ContainsKey(map) && loadingMaps[map] != null && loadingMaps[map].Count > 0)
        {
            loadingMaps[map].Add(onLoaded);
            while (loadingMaps.ContainsKey(map) && loadingMaps[map] != null && loadingMaps[map].Count > 0)
            {
                yield return null;
            }
            yield break;
        }
        var prefabs = AmongUsClient.Instance.ShipPrefabs;
        if (prefabs.Count <= (int)map)
        {
            Logger.Error($"Out of range: {map}");
            yield break;
        }
        AssetReference ship = prefabs[(int)map];
        int retryCount = 0;
        while (retryCount < 10)
        {
            if (ship.Asset != null) break;
            AsyncOperationHandle op = ship.LoadAssetAsync<GameObject>();
            if (!op.IsValid())
            {
                Logger.Warning($"Could not import [{ship.AssetGUID}] due to invalid Async Operation. Trying again in 5 seconds... (Retry {retryCount + 1}/10)");
                yield return new WaitForSeconds(5);
                retryCount++;
                continue;
            }
            yield return op;
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                SuperNewRoles.Logger.Warning($"Could not import [{ship.AssetGUID}] due to failed Async Operation. (Retry {retryCount + 1}/10)");
                retryCount++;
            }
            else
            {
                break;
            }
        }
        if (ship.Asset == null)
        {
            SuperNewRoles.Logger.Error($"Failed to load asset [{ship.AssetGUID}] after 10 retries. Ignoring...");
        }
        if (ship.Asset != null)
        {
            GameObject prefab = ship.Asset.Cast<GameObject>();
            ShipStatus status = prefab.GetComponent<ShipStatus>();
            switch (map)
            {
                case MapNames.Airship:
                    airship = status;
                    break;
                case MapNames.Fungle:
                    fungle = status;
                    break;
            }
            loadedMaps.Add(ship);
            SuperNewRoles.Logger.Info($"...{prefab.name} Loaded");
            if (loadingMaps.ContainsKey(map))
            {
                foreach (var action in loadingMaps[map])
                {
                    action(status);
                }
                loadingMaps[map].Clear();
            }
            yield return null;
        }
        else SuperNewRoles.Logger.Warning($"Could not import [{ship.AssetGUID}]. Ignoring...");
        sw.Stop();
        SuperNewRoles.Logger.Info($"Loading {map} End Maps: {sw.ElapsedMilliseconds}ms");
        yield break;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatus_OnDestroy_Patch
    {
        public static void Prefix()
        {
            UnloadMaps();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatus_Awake_Patch
    {
        public static void Prefix()
        {
        }
    }
    private static void UnloadMaps()
    {
        foreach (var ship in loadedMaps)
        {
            ship.ReleaseAsset();
        }
        loadingMaps.Clear();
        if (airship is not null)
            airship = null;
        if (fungle is not null)
            fungle = null;
        loadedMaps.Clear();
        Resources.UnloadUnusedAssets();
    }
}