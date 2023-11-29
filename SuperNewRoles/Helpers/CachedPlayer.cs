using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles;

public class CachedPlayer
{
    public static readonly Dictionary<IntPtr, CachedPlayer> PlayerPtrs = new();
    public static readonly List<CachedPlayer> AllPlayers = new();
    public static CachedPlayer LocalPlayer;

    public Transform transform;
    public PlayerControl PlayerControl;
    public PlayerPhysics PlayerPhysics;
    public CustomNetworkTransform NetTransform;
    public GameData.PlayerInfo Data;
    public byte PlayerId;
    public uint NetId;

    public static implicit operator bool(CachedPlayer player)
    {
        return player != null && player.PlayerControl;
    }

    public static implicit operator PlayerControl(CachedPlayer player) => player.PlayerControl;
    public static implicit operator PlayerPhysics(CachedPlayer player) => player.PlayerPhysics;
}

[HarmonyPatch]
public static class CachedPlayerPatches
{
    [HarmonyPatch]
    private class CacheLocalPlayerPatch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            var type = typeof(PlayerControl).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.Name.Contains("Start"));
            return AccessTools.Method(type, nameof(IEnumerator.MoveNext));
        }

        [HarmonyPostfix]
        public static void SetLocalPlayer()
        {
            var localPlayer = PlayerControl.LocalPlayer;
            if (!localPlayer)
            {
                CachedPlayer.LocalPlayer = null;
                return;
            }

            var cached = CachedPlayer.AllPlayers.FirstOrDefault(p => p.PlayerControl.Pointer == localPlayer.Pointer);
            if (cached != null)
            {
                CachedPlayer.LocalPlayer = cached;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
    [HarmonyPostfix]
    public static void CachePlayerPatch(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) return;
        var player = new CachedPlayer
        {
            transform = __instance.transform,
            PlayerControl = __instance,
            PlayerPhysics = __instance.MyPhysics,
            NetTransform = __instance.NetTransform
        };
        CachedPlayer.AllPlayers.Add(player);
        CachedPlayer.PlayerPtrs[__instance.Pointer] = player;

        foreach (var cachedPlayer in CachedPlayer.AllPlayers.ToArray())
        {
            if (!cachedPlayer.PlayerControl || !cachedPlayer.PlayerPhysics || !cachedPlayer.NetTransform || !cachedPlayer.transform)
            {
                SuperNewRolesPlugin.Logger.LogError($"CachedPlayer {cachedPlayer.PlayerControl?.name} has null fields");
                CachedPlayer.AllPlayers.Remove(cachedPlayer);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.OnDestroy))]
    [HarmonyPostfix]
    public static void RemoveCachedPlayerPatch(PlayerControl __instance)
    {
        if (__instance.notRealPlayer) return;
        CachedPlayer.AllPlayers.RemoveAll(p => p.PlayerControl.Pointer == __instance.Pointer);
        CachedPlayer.PlayerPtrs.Remove(__instance.Pointer);
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.Deserialize))]
    [HarmonyPostfix]
    public static void AddCachedDataOnDeserialize()
    {
        foreach (CachedPlayer cachedPlayer in CachedPlayer.AllPlayers)
        {
            cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
            cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
            cachedPlayer.NetId = cachedPlayer.PlayerControl.NetId;
        }
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.AddPlayer))]
    [HarmonyPostfix]
    public static void AddCachedDataOnAddPlayer()
    {
        foreach (CachedPlayer cachedPlayer in CachedPlayer.AllPlayers)
        {
            cachedPlayer.Data = cachedPlayer.PlayerControl.Data;
            cachedPlayer.PlayerId = cachedPlayer.PlayerControl.PlayerId;
            cachedPlayer.NetId = cachedPlayer.PlayerControl.NetId;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Deserialize))]
    [HarmonyPostfix]
    public static void SetCachedPlayerId(PlayerControl __instance)
    {
        CachedPlayer.PlayerPtrs[__instance.Pointer].PlayerId = __instance.PlayerId;
        CachedPlayer.PlayerPtrs[__instance.Pointer].NetId = __instance.NetId;
    }
}