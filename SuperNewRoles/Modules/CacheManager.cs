using System.Collections.Generic;

namespace SuperNewRoles.Modules;

class CacheManager
{
    public static Dictionary<int, RoleId> MyRoleCache;
    public static Dictionary<int, RoleId> MyGhostRoleCache;
    public static Dictionary<int, PlayerControl> HauntedWolfCache;
    public static Dictionary<int, PlayerControl> LoversCache;
    public static Dictionary<int, PlayerControl> FakeLoversCache;
    public static Dictionary<int, PlayerControl> QuarreledCache;
    public static Dictionary<byte, PlayerVoteArea> PlayerVoteAreaCache;
    private static int lastMeetingHud;

    public static void Load()
    {
        MyRoleCache = new();
        MyGhostRoleCache = new();
        HauntedWolfCache = new();
        LoversCache = new();
        FakeLoversCache = new();
        QuarreledCache = new();
        PlayerVoteAreaCache = new();
    }
    public static void ResetCache()
    {
        ResetQuarreledCache();
        ResetHauntedWolfCache();
        ResetLoversCache();
        ResetMyRoleCache();
        ResetMyGhostRoleCache();
    }
    private static void ResetCacheMeeting()
    {
        ResetPlayerVoteAreaCache();
    }
    public static void ResetQuarreledCache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            QuarreledCache[p.PlayerId] = p.IsQuarreled(false) ? p.GetOneSideQuarreled(false) : null;
        }
    }
    public static void ResetHauntedWolfCache()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            if (player.IsHauntedWolf(false)) HauntedWolfCache[player.PlayerId] = player;
        }
    }
    public static void ResetLoversCache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            LoversCache[p.PlayerId] = p.IsLovers(false) ? p.GetOneSideLovers(false) : null;
            FakeLoversCache[p.PlayerId] = p.IsFakeLovers(false) ? p.GetOneSideFakeLovers(false) : null;
        }
    }
    public static void ResetMyRoleCache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            MyRoleCache[p.PlayerId] = p.GetRole(false);
        }
    }
    public static void ResetMyGhostRoleCache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            MyGhostRoleCache[p.PlayerId] = p.GetGhostRole(false);
        }
    }
    private static void ResetPlayerVoteAreaCache()
    {
        if (!MeetingHud.Instance)
        {
            PlayerVoteAreaCache = null;
            return;
        }
        PlayerVoteAreaCache = new();
        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
        {
            PlayerVoteAreaCache[player.TargetPlayerId] = player;
        }
        if (PlayerVoteAreaCache.Count == 0)
            PlayerVoteAreaCache = null;
        lastMeetingHud = MeetingHud.Instance.GetInstanceID();
    }
    public static PlayerVoteArea GetVoteAreaById(byte playerId)
    {
        if (!MeetingHud.Instance)
            return null;
        if (lastMeetingHud != MeetingHud.Instance.GetInstanceID() || PlayerVoteAreaCache == null)
            ResetPlayerVoteAreaCache();
        if (!PlayerVoteAreaCache.TryGetValue(playerId, out PlayerVoteArea voteArea))
            return null;
        return voteArea;
    }
}