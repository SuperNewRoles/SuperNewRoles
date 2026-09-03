using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class OfficialSnrServer
{
    private const float FallbackProbeIntervalSeconds = 1f;
    // IsIdentityEnabled() is queried from MainMenuManager.LateUpdate.  Do not
    // enumerate the IL2CPP server array (or create a LINQ array/enumerator) on
    // every frame.  Region-selection hooks invalidate the cache immediately;
    // the slow fallback probe also catches builds where a region is installed
    // by assigning CurrentRegion directly without invoking either hook.
    private static bool _cachedResult;
    private static bool _hasCache;
    private static float _nextProbeAt;

    /// <summary>
    /// ServerManager.SetRegion is the authoritative transition for the game's
    /// selected region.  Invalidate there instead of polling CurrentRegion from
    /// every MainMenu.LateUpdate call; the IL2CPP property access itself wraps a
    /// native object and allocates even when the region has not changed.
    /// </summary>
    public static void Invalidate()
    {
        _hasCache = false;
        _nextProbeAt = 0f;
    }

    public static bool IsCurrent()
    {
        float now = Time.unscaledTime;
        if (_hasCache && now < _nextProbeAt)
            return _cachedResult;

        var serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        var region = serverManager?.CurrentRegion;
        if (serverManager == null || region == null)
        {
            _cachedResult = false;
            _hasCache = true;
            _nextProbeAt = now + FallbackProbeIntervalSeconds;
            return false;
        }

        if (!ModHelpers.IsCustomServer())
        {
            _cachedResult = false;
            _hasCache = true;
            _nextProbeAt = now + FallbackProbeIntervalSeconds;
            return false;
        }

        var servers = region?.Servers;
        bool result = false;
        if (servers != null)
        {
            for (int i = 0; i < servers.Length; i++)
            {
                var server = servers[i];
                string ip = server?.Ip;
                if (string.IsNullOrEmpty(ip))
                    continue;
                if (ip.Contains("supernewroles.com")
                    || ip.Contains("cs.supernewroles")
                    || ip.Contains("cs-useast"))
                {
                    result = true;
                    break;
                }
            }
        }

        _cachedResult = result;
        _hasCache = true;
        _nextProbeAt = now + FallbackProbeIntervalSeconds;
        return result;
    }

    /// <summary>
    /// 公式 CS にいるときだけ身元・規約・通報を使う。
    /// 3.2.0.2b 以前を落とさない判定は Impostor が RPC 253 のバージョンで行う。
    /// </summary>
    public static bool IsIdentityEnabled() => IsCurrent();
}

/// <summary>
/// Keep OfficialSnrServer's result valid when the player changes the selected
/// Among Us region.  The dynamic target lookup tolerates game builds that add
/// a SetRegion overload without making the per-frame path use reflection.
/// </summary>
[HarmonyPatch]
internal static class OfficialSnrServerRegionPatch
{
    private static readonly string[] InvalidatingMethodNames = { "SetRegion", "ReselectServer" };

    public static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (MethodBase method in typeof(ServerManager).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            foreach (string name in InvalidatingMethodNames)
            {
                if (string.Equals(method.Name, name, StringComparison.Ordinal))
                {
                    yield return method;
                    break;
                }
            }
        }
    }

    public static void Postfix()
    {
        OfficialSnrServer.Invalidate();
    }
}
