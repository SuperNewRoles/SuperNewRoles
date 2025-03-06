using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles.Modules;

public static class MapLoader
{
    private static ShipStatus skeld;
    public static ShipStatus Skeld => skeld;

    private static ShipStatus airship;
    public static ShipStatus Airship => airship;

    private static ShipStatus polus;
    public static ShipStatus Polus => polus;

    private static ShipStatus fungle;
    public static ShipStatus Fungle => fungle;

    public static GameObject SkeldObject => Skeld.gameObject;

    public static GameObject AirshipObject => Airship.gameObject;

    public static GameObject PolusObject => polus.gameObject;

    public static GameObject FungleObject => fungle.gameObject;

    public static IEnumerator LoadMaps()
    {
        while (AmongUsClient.Instance == null) { yield return null; }
        var prefabs = AmongUsClient.Instance.ShipPrefabs;
        foreach (MapNames map in new MapNames[] { MapNames.Skeld, MapNames.Polus, MapNames.Airship, MapNames.Fungle })
        {
            AssetReference ship = prefabs[(int)map];
            int retryCount = 0;
            while (retryCount < 10)
            {
                if (ship.Asset != null) break;
                AsyncOperationHandle op = ship.LoadAssetAsync<GameObject>();
                if (!op.IsValid())
                {
                    SuperNewRoles.Logger.Warning($"Could not import [{ship.AssetGUID}] due to invalid Async Operation. Trying again in 5 seconds... (Retry {retryCount + 1}/10)");
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
                switch (prefab.name)
                {
                    case "SkeldShip":
                        skeld = status;
                        break;
                    case "PolusShip":
                        polus = status;
                        break;
                    case "Airship":
                        airship = status;
                        break;
                    case "FungleShip":
                        fungle = status;
                        break;
                }

                SuperNewRoles.Logger.Info($"...{prefab.name} Loaded");
                yield return null;
            }
            else SuperNewRoles.Logger.Warning($"Could not import [{ship.AssetGUID}]. Ignoring...");
        }
        SuperNewRoles.Logger.Info("Loading End Maps");
        yield break;
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class AmongUsClientAwakePatch
    {
        static bool Loaded = false;

        public static void Prefix(AmongUsClient __instance)
        {
            if (Loaded) return;
            Loaded = true;
            __instance.StartCoroutine(LoadMaps().WrapToIl2Cpp());
        }
    }
}