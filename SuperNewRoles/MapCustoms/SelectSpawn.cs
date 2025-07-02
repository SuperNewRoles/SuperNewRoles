using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

public static class SelectSpawn
{
    public class MapSpawnData
    {
        public Func<SpawnTypeOptions, bool> IsSpawnTypeSelect { get; }
        public (StringNames, Vector3, string, Func<AudioClip>)[] Locations { get; }
        public string AssetPrefix { get; }
        public string MapName { get; }

        public MapSpawnData(Func<SpawnTypeOptions, bool> isSpawnTypeSelect, (StringNames, Vector3, string, Func<AudioClip>)[] locations, string assetPrefix, string mapName)
        {
            IsSpawnTypeSelect = isSpawnTypeSelect;
            Locations = locations;
            AssetPrefix = assetPrefix;
            MapName = mapName;
        }
    }

    private static readonly MapSpawnData PolusMapData = new(
        isSpawnTypeSelect: PolusHandler.IsPolusSpawnType,
        locations: new (StringNames, Vector3, string, Func<AudioClip>)[]
        {
            ((StringNames)51000, new(25.7343f, -12.8777f), "BackRock", null),
            (StringNames.MonitorOxygen, new(3.3584f, -21.68f), "Oxygen", null),
            (StringNames.Electrical, new(5.3372f, -9.7048f), "Electrical", null),
            (StringNames.Admin, new(23.9309f, -22.5169f), "Admin", null),
            (StringNames.Office, new(19.5145f, -17.4998f), "Office", null),
            (StringNames.Weapons, new(12.0384f, -23.34f), "Weapons", null),
            (StringNames.Comms, new(10.6821f, -16.0105f), "Comms", null),
            (StringNames.Storage, new(20.5637f, -11.9088f), "Storage", null),
            (StringNames.Dropship, new(16.6458f, -3.2058f), "Dropship", null),
            (StringNames.Laboratory, new(34.3056f, -7.8901f), "Laboratory", null),
            (StringNames.Specimens, new(34.3056f, -7.8901f), "Specimens", null)
        },
        assetPrefix: "PolusSelectSpawn",
        mapName: "Polus"
    );

    private static readonly MapSpawnData FungleMapData = new(
        isSpawnTypeSelect: FungleHandler.IsFungleSpawnType,
        locations: new (StringNames, Vector3, string, Func<AudioClip>)[]
        {
            ((StringNames)50999, new Vector3(-9.81f, 0.6f), "Campfire", null), // StringName for Campfire
            (StringNames.Dropship, new Vector3(-8f, 10.5f), "Dropship", null),
            (StringNames.Cafeteria, new Vector3(-16.16f, 7.25f), "Cafeteria", null),
            (StringNames.Kitchen, new Vector3(-15.5f, -7.5f), "Kitchen", null),
            (StringNames.Greenhouse, new Vector3(9.25f, -12f), "Hotroom", null),
            (StringNames.UpperEngine, new Vector3(14.75f, 0f), "UpperEngine", null),
            (StringNames.Comms, new Vector3(21.65f, 13.75f), "Comms", null)
        },
        assetPrefix: "FungleSelectSpawn",
        mapName: "TheFungle"
    );

    // Fungle specific StringPatch
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
    private class StringPatch // This patch might need adjustment if it causes issues outside Fungle map context
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            if ((int)name == 50999)
            {
                __result = ModTranslation.GetString("Campfire");
                return false;
            }
            else if ((int)name == 51000)
            {
                __result = ModTranslation.GetString("BackRock");
                return false;
            }
            return true;
        }
    }

    static SpawnInMinigame.SpawnLocation CreateSpawnLocation((StringNames, Vector3, string, Func<AudioClip>) obj, string assetPrefix)
    {
        return new()
        {
            Name = obj.Item1,
            Location = obj.Item2,
            Image = AssetManager.GetAsset<Sprite>($"{assetPrefix}.{obj.Item3}.png"),
            Rollover = null,
            RolloverSfx = obj.Item4?.Invoke()
        };
    }

    public static IEnumerator SelectSpawnCoroutine(MapSpawnData mapData)
    {
        SpawnInMinigame spawnInMinigame = null;
        yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
        bool loaded = false;
        yield return MapLoader.LoadMapAsync(MapNames.Airship, (ship) =>
        {
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.black;
            loaded = true;
            spawnInMinigame = GameObject.Instantiate<SpawnInMinigame>(ship.TryCast<AirshipStatus>().SpawnInGame);
            spawnInMinigame.transform.SetParent(Camera.main.transform, false);
            spawnInMinigame.transform.localPosition = new Vector3(0f, 0f, -600f);
            List<SpawnInMinigame.SpawnLocation> locations = new(mapData.Locations.Length);
            foreach (var loc in mapData.Locations)
            {
                locations.Add(CreateSpawnLocation(loc, mapData.AssetPrefix));
            }
            spawnInMinigame.Locations = new(locations.ToArray());
            spawnInMinigame.Begin(null);

            foreach (PassiveButton button in spawnInMinigame.LocationButtons)
            {
                button.transform.localPosition = new(button.transform.localPosition.x, 0.5f, 0);
                button.GetComponentInChildren<TextMeshPro>().transform.localPosition = new(0f, -1.09f, 0f);
                BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
                collider.size = new(1.7f, 1.5f);
                collider.offset = new(0f, 0.03f);
            }
        });
        new LateTask(() =>
        {
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.color = Color.black;
        }, 0.02f);
        while (!loaded) yield return null;
        yield return spawnInMinigame.WaitForFinish();
        yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
    public static class ShipStatus_PrespawnStep_Patch
    {
        public static bool Prefix(ref Il2CppSystem.Collections.IEnumerator __result)
        {
            MapSpawnData currentMapData = GetMapData();

            if (currentMapData != null && currentMapData.IsSpawnTypeSelect(SpawnTypeOptions.Select)) // Double check IsSpawnTypeSelect for safety
            {
                __result = SelectSpawnCoroutine(currentMapData).WrapToIl2Cpp();
                return false; // Skip original method
            }

            return true; // Proceed with original method if not Polus or Fungle with select spawn
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    public static class ExileController_WrapUp_Patch
    {
        public static void Postfix(ExileController __instance)
        {
            Logger.Info("ExileController_WrapUp_Patch");
            MapSpawnData currentMapData = GetMapData();

            if (currentMapData != null && currentMapData.IsSpawnTypeSelect(SpawnTypeOptions.Select)) // Double check IsSpawnTypeSelect for safety
            {
                DestroyableSingleton<HudManager>.Instance.StopAllCoroutines();
                AmongUsClient.Instance.StartCoroutine(SelectSpawnCoroutine(currentMapData).WrapToIl2Cpp());
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class AmongUsClient_CoStartGame_Patch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            Logger.Info("AmongUsClient_CoStartGame_Patch");
            MapSpawnData currentMapData = GetMapData();
            if (currentMapData != null && currentMapData.IsSpawnTypeSelect(SpawnTypeOptions.Select))
            {
                // 先に読み込んでおく
                MapLoader.LoadMap(MapNames.Airship, (ship) => { });
            }
        }
    }

    private static MapSpawnData GetMapData()
    {
        if (PolusHandler.IsPolusSpawnType(SpawnTypeOptions.Select))
            return PolusMapData;
        else if (FungleHandler.IsFungleSpawnType(SpawnTypeOptions.Select))
            return FungleMapData;
        return null;
    }
}