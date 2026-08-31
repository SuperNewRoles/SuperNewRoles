using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace SuperNewRoles.Safety;

public static class HostBlockLeaveNotice
{
    public const string TranslationKey = "SafetyHostBlockLeave";
    private const int ExpireMs = 15000;
    private static readonly Dictionary<int, long> Pending = new();
    private static readonly HashSet<string> SuppressNames = new(StringComparer.Ordinal);
    private static bool _postingOwn;
    private static int _pendingClientId = -1;
    private static string _pendingShowName;
    private static bool _deferredShowStarted;

    public static void Mark(int clientId)
    {
        if (clientId < 0)
            return;
        lock (Pending)
        {
            PruneExpiredUnlocked();
            Pending[clientId] = Environment.TickCount64;
        }
    }

    public static bool TryConsume(int clientId)
    {
        lock (Pending)
        {
            PruneExpiredUnlocked();
            return Pending.Remove(clientId);
        }
    }

    public static string Format(string playerName)
    {
        return string.Format(ModTranslation.GetString(TranslationKey), playerName ?? string.Empty);
    }

    public static bool ShouldSuppressVanilla(string item)
    {
        if (_postingOwn || string.IsNullOrEmpty(item))
            return false;
        lock (SuppressNames)
        {
            string match = null;
            foreach (string name in SuppressNames)
            {
                if (string.IsNullOrEmpty(name) || item.IndexOf(name, StringComparison.Ordinal) < 0)
                    continue;
                match = name;
                break;
            }
            if (match == null)
                return false;
            SuppressNames.Remove(match);
            return true;
        }
    }

    public static void RememberName(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return;
        lock (SuppressNames)
            SuppressNames.Add(playerName);
    }

    public static void Show(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return;
        RememberName(playerName);
        var hud = DestroyableSingleton<HudManager>.Instance;
        if (hud == null || hud.Notifier == null)
            return;
        _postingOwn = true;
        try
        {
            hud.Notifier.AddDisconnectMessage(Format(playerName));
        }
        finally
        {
            _postingOwn = false;
        }
    }

    public static void Receive(int clientId, string playerName)
    {
        Mark(clientId);
        _pendingClientId = clientId;
        _pendingShowName = playerName;
        RememberNameForSuppress(clientId, playerName);
        ScheduleDeferredShow();
    }

    private static void RememberNameForSuppress(int clientId, string playerName)
    {
        if (!string.IsNullOrEmpty(playerName))
        {
            RememberName(playerName);
            return;
        }
        string resolved = ConductBanLeaveNotice.FindPlayerName(clientId);
        if (!string.IsNullOrEmpty(resolved))
            RememberName(resolved);
    }

    private static void ScheduleDeferredShow()
    {
        MonoBehaviour host = SafetyPopupUi.EnsureHost();
        if (host == null || _deferredShowStarted)
            return;
        _deferredShowStarted = true;
        host.StartCoroutine(CoShowNextFrame().WrapToIl2Cpp());
    }

    private static IEnumerator CoShowNextFrame()
    {
        yield return null;
        _deferredShowStarted = false;
        int clientId = _pendingClientId;
        string name = _pendingShowName;
        _pendingClientId = -1;
        _pendingShowName = null;
        if (string.IsNullOrEmpty(name))
            name = ConductBanLeaveNotice.FindPlayerName(clientId);
        Show(name);
    }

    private static void PruneExpiredUnlocked()
    {
        long now = Environment.TickCount64;
        List<int> expired = null;
        foreach (var row in Pending)
        {
            if (now - row.Value <= ExpireMs)
                continue;
            expired ??= new List<int>();
            expired.Add(row.Key);
        }
        if (expired == null)
            return;
        foreach (int id in expired)
            Pending.Remove(id);
    }
}
