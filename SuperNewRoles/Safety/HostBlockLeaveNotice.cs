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
    private static readonly Dictionary<string, long> SuppressNames = new(StringComparer.Ordinal);
    private static readonly Queue<(int clientId, string name)> _pendingShows = new();
    private static bool _postingOwn;
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
            PruneExpiredSuppressUnlocked();
            string match = null;
            foreach (var row in SuppressNames)
            {
                string name = row.Key;
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
        {
            PruneExpiredSuppressUnlocked();
            SuppressNames[playerName] = Environment.TickCount64;
        }
    }

    public static void Show(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return;
        RememberName(playerName);
        var hud = DestroyableSingleton<HudManager>.Instance;
        if (hud == null || hud.Notifier == null)
        {
            Logger.Warning("HostBlockLeaveNotice could not show: HudManager.Notifier is unavailable");
            return;
        }
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
        lock (_pendingShows)
            _pendingShows.Enqueue((clientId, playerName));
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
        List<(int clientId, string name)> batch;
        lock (_pendingShows)
        {
            batch = new List<(int clientId, string name)>(_pendingShows.Count);
            while (_pendingShows.Count > 0)
                batch.Add(_pendingShows.Dequeue());
        }
        for (int i = 0; i < batch.Count; i++)
        {
            int clientId = batch[i].clientId;
            string name = batch[i].name;
            if (string.IsNullOrEmpty(name))
                name = ConductBanLeaveNotice.FindPlayerName(clientId);
            Show(name);
        }
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

    private static void PruneExpiredSuppressUnlocked()
    {
        long now = Environment.TickCount64;
        List<string> expired = null;
        foreach (var row in SuppressNames)
        {
            if (now - row.Value <= ExpireMs)
                continue;
            expired ??= new List<string>();
            expired.Add(row.Key);
        }
        if (expired == null)
            return;
        foreach (string name in expired)
            SuppressNames.Remove(name);
    }
}
