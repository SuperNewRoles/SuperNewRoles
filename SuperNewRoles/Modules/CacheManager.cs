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
    public static void Load()
    {
        MyRoleCache = new();
        MyGhostRoleCache = new();
        HauntedWolfCache = new();
        LoversCache = new();
        FakeLoversCache = new();
        QuarreledCache = new();
    }
    public static void ResetCache()
    {
        ResetQuarreledCache();
        ResetHauntedWolfCache();
        ResetLoversCache();
        ResetMyRoleCache();
        ResetMyGhostRoleCache();
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
}