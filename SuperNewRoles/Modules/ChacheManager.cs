using System.Collections.Generic;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

class ChacheManager
{
    public static Dictionary<int, RoleId> MyRoleChache;
    public static Dictionary<int, RoleId> MyGhostRoleChache;
    public static Dictionary<int, PlayerControl> HauntedWolfChache;
    public static Dictionary<int, PlayerControl> LoversChache;
    public static Dictionary<int, PlayerControl> FakeLoversChache;
    public static Dictionary<int, PlayerControl> QuarreledChache;
    public static void Load()
    {
        MyRoleChache = new();
        MyGhostRoleChache = new();
        HauntedWolfChache = new();
        LoversChache = new();
        FakeLoversChache = new();
        QuarreledChache = new();
    }
    public static void ResetChache()
    {
        ResetQuarreledChache();
        ResetHauntedWolfChache();
        ResetLoversChache();
        ResetMyRoleChache();
        ResetMyGhostRoleChache();
    }
    public static void ResetQuarreledChache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            QuarreledChache[p.PlayerId] = p.IsQuarreled(false) ? p.GetOneSideQuarreled(false) : null;
        }
    }
    public static void ResetHauntedWolfChache()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers)
        {
            if (player.IsHauntedWolf(false)) HauntedWolfChache[player.PlayerId] = player;
        }
    }
    public static void ResetLoversChache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            LoversChache[p.PlayerId] = p.IsLovers(false) ? p.GetOneSideLovers(false) : null;
            FakeLoversChache[p.PlayerId] = p.IsFakeLovers(false) ? p.GetOneSideFakeLovers(false) : null;
        }
    }
    public static void ResetMyRoleChache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            MyRoleChache[p.PlayerId] = p.GetRole(false);
        }
    }
    public static void ResetMyGhostRoleChache()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            MyGhostRoleChache[p.PlayerId] = p.GetGhostRole(false);
        }
    }
}