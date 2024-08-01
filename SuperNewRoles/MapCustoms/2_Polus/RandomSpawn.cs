using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;

public static class PolusRandomSpawn
{
    public static readonly Vector2[] SpawnPositions = new Vector2[]
    {
        new(34.8f, -6.5f),   // 研究室
        new(36.52f, -19.9f), // 標本室
        new(19.5f, -17.4f),  // ミーティング
        new(32.6f, -15.7f),  // 溶岩上
        new(20.65f, -12f),   // ストレージ
        new(9.75f, -12.2f),  // エレキ
        new(2.3f, -24.1f),   // 酸素
        new(12.1f, -16.5f),  // コミュ
    };

    public static IEnumerator Spawn(PlayerControl player)
    {
        yield return Effects.Wait(3f);
        int randam = new System.Random().Next(SpawnPositions.Length + 1);
        if (randam == 0) player.NetTransform.RpcSnapTo(ShipStatus.Instance.InitialSpawnCenter);
        else player.NetTransform.RpcSnapTo(SpawnPositions[randam - 1]);
        yield break;
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.SpawnPlayer)), HarmonyPostfix]
        public static void SpawnPlayerPostfix()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus, false) && MapCustom.PolusRandomSpawn.GetBool())
            {
                HudManager.Instance.StartCoroutine(Spawn(PlayerControl.LocalPlayer).WrapToIl2Cpp());
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (player.IsMod()) continue;
                        HudManager.Instance.StartCoroutine(Spawn(player).WrapToIl2Cpp());
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud))]
    public static class MeetingHudClosePatch
    {
        [HarmonyPatch(nameof(MeetingHud.Close)), HarmonyPostfix]
        public static void ClosePostfix()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus, false) && MapCustom.PolusRandomSpawn.GetBool())
            {
                HudManager.Instance.StartCoroutine(Spawn(PlayerControl.LocalPlayer).WrapToIl2Cpp());
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
                {
                    foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (player.IsMod()) continue;
                        HudManager.Instance.StartCoroutine(Spawn(player).WrapToIl2Cpp());
                    }
                }
            }
        }
    }
}