using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules; // Assuming MapCustomHandler and Handlers (PolusHandler, FungleHandler) are here
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

public static class RandomSpawn
{
    public class MapRandomSpawnData
    {
        public Vector2[] SpawnPositions { get; }
        public System.Func<SpawnTypeOptions, bool> IsRandomSpawnTypeEnabled { get; }
        public MapCustomHandler.MapCustomId MapId { get; }

        public MapRandomSpawnData(Vector2[] spawnPositions, System.Func<SpawnTypeOptions, bool> isRandomSpawnTypeEnabled, MapCustomHandler.MapCustomId mapId)
        {
            SpawnPositions = spawnPositions;
            IsRandomSpawnTypeEnabled = isRandomSpawnTypeEnabled;
            MapId = mapId;
        }
    }

    private static readonly MapRandomSpawnData PolusRandomData = new(
        spawnPositions: new Vector2[]
        {
            new(25.7343f, -12.8777f), //BackRock
            new(3.3584f, -21.68f),    //Oxygen
            new(5.3372f, -9.7048f),   //Electrical
            new(23.9309f, -22.5169f), //Admin
            new(19.5145f, -17.4998f), //Office
            new(12.0384f, -23.34f),   //Weapons
            new(10.6821f, -16.0105f), //Comms
            new(20.5637f, -11.9088f), //Storage
            new(16.6458f, -3.2058f),  //Dropship
            new(34.3056f, -7.8901f),  //Laboratory
            new(34.3056f, -7.8901f)   //Specimens (Same as Laboratory)
        },
        isRandomSpawnTypeEnabled: PolusHandler.IsPolusSpawnType,
        mapId: MapCustomHandler.MapCustomId.Polus
    );

    private static readonly MapRandomSpawnData FungleRandomData = new(
        spawnPositions: new Vector2[]
        {
            new(-9.81f, 0.6f),    // Campfire area
            new(-8f, 10.5f),      // Dropship
            new(-16.16f, 7.25f),  // Cafeteria
            new(-15.5f, -7.5f),   // Kitchen
            new(9.25f, -12f),     // Greenhouse (Hotroom)
            new(14.75f, 0f),      // Upper Engine (Mid-slope)
            new(21.65f, 13.75f)   // Comms
        },
        isRandomSpawnTypeEnabled: FungleHandler.IsFungleSpawnType,
        mapId: MapCustomHandler.MapCustomId.TheFungle
    );

    public static IEnumerator SpawnPlayerAtRandomLocationCoroutine(PlayerControl player, Vector2[] spawnPositions)
    {
        yield return Effects.Wait(0.1f); // Reduced wait time slightly, adjust if necessary
        Vector2 position = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length)];
        player.NetTransform.RpcSnapTo(position);
    }

    private static void TriggerRandomSpawnForMap(MapRandomSpawnData mapData)
    {
        if (!MapCustomHandler.IsMapCustom(mapData.MapId, false) || !mapData.IsRandomSpawnTypeEnabled(SpawnTypeOptions.Random))
        {
            return;
        }

        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer != null)
        {
            HudManager.Instance.StartCoroutine(SpawnPlayerAtRandomLocationCoroutine(localPlayer, mapData.SpawnPositions).WrapToIl2Cpp());
        }

        if (AmongUsClient.Instance.AmHost)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player != null && player != localPlayer)
                {
                    HudManager.Instance.StartCoroutine(SpawnPlayerAtRandomLocationCoroutine(player, mapData.SpawnPositions).WrapToIl2Cpp());
                }
            }
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    private static class ShipStatus_Patch
    {
        [HarmonyPatch(nameof(ShipStatus.SpawnPlayer))]
        [HarmonyPostfix]
        public static void SpawnPlayer_Postfix()
        {
            TriggerRandomSpawnForMap(PolusRandomData);
            TriggerRandomSpawnForMap(FungleRandomData);
        }
    }

    [HarmonyPatch(typeof(MeetingHud))]
    private static class MeetingHud_Patch
    {
        [HarmonyPatch(nameof(MeetingHud.Close))]
        [HarmonyPostfix]
        public static void Close_Postfix()
        {
            TriggerRandomSpawnForMap(PolusRandomData);
            TriggerRandomSpawnForMap(FungleRandomData);
        }
    }
}