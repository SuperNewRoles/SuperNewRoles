using System;
using System.Collections.Generic;

namespace SuperNewRoles.Roles;

public static class RoleReleaseLock
{
    // リークしないでくれたら嬉しい
    // Please don't leak it.

    private static readonly HashSet<RoleId> ReleaseAtJan1Roles = new()
    {
        RoleId.Slugger,
        RoleId.Psychometrist,
        RoleId.DyingMessenger,
        RoleId.Kunoichi,
        RoleId.Squid,
        RoleId.Sauner,
    };

    private static readonly HashSet<RoleId> ReleaseAtJan5Roles = new()
    {
        RoleId.Moira,
        RoleId.TheThreeLittlePigs,
        RoleId.TheFirstLittlePig,
        RoleId.TheSecondLittlePig,
        RoleId.TheThirdLittlePig,
        RoleId.Frankenstein,
        RoleId.RemoteController,
        RoleId.Mafia,
        RoleId.PoliceSurgeon,
    };

    // 1月1日17時(JST)
    private static Il2CppSystem.DateTime ReleaseAtJan1Utc = new(2026, 1, 1, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);
    // 1月5日17時(JST)
    private static Il2CppSystem.DateTime ReleaseAtJan5Utc = new(2026, 1, 5, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);
    // 1月2日17時(JST)
    private static Il2CppSystem.DateTime ReleaseAtJan2CursedUtc = new(2026, 1, 2, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);

    private static bool IsReleaseLockEnabled = ThisAssembly.Git.Branch == SuperNewRoles.BranchConfig.MasterBranch;

    public static int GetReleaseStateToken()
    {
        if (!IsReleaseLockEnabled)
            return 3;
        int token = 0;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan1Utc)
            token |= 1;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan5Utc)
            token |= 2;
        return token;
    }

    public static bool IsLocked(RoleId roleId)
    {
        if (!IsReleaseLockEnabled)
            return false;
        if (ReleaseAtJan1Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan1Utc;
        if (ReleaseAtJan5Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan5Utc;
        return false;
    }

    public static bool IsCursedModeLocked()
    {
        if (!IsReleaseLockEnabled)
            return false;
        return AmongUsDateTime.UtcNow < ReleaseAtJan2CursedUtc;
    }
    /*
        private static void Debug_UpdateTime(int hour, int minute)
        {
            ReleaseAtJan1Utc = new(2025, 12, 31, hour, minute, 0, Il2CppSystem.DateTimeKind.Utc);
        }*/
}
