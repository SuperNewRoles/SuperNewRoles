using System;
using System.Collections.Generic;

namespace SuperNewRoles.Roles;

public static class RoleReleaseLock
{
    // リークしないでくれたら嬉しい
    // Please don't leak it.


    private static readonly HashSet<RoleId> ReleaseAtJan2Roles = new()
    {
        RoleId.TriggerHappy,
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
        RoleId.Psychometrist,
        RoleId.DyingMessenger,
    };

    private static Il2CppSystem.DateTime ReleaseAtJan2Utc = new(2026, 1, 2, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);
    private static Il2CppSystem.DateTime ReleaseAtJan5Utc = new(2026, 1, 5, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);
    private static Il2CppSystem.DateTime ReleaseAtJan3CursedUtc = new(2026, 1, 3, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);

    private static bool IsReleaseLockEnabled = ThisAssembly.Git.Branch == BranchConfig.MasterBranch;

    public static int GetReleaseStateToken()
    {
        if (!IsReleaseLockEnabled)
            return 3;
        int token = 0;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan2Utc)
            token |= 1;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan5Utc)
            token |= 2;
        return token;
    }

    public static bool IsLocked(RoleId roleId)
    {
        if (!IsReleaseLockEnabled)
            return false;
        if (ReleaseAtJan2Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan2Utc;
        if (ReleaseAtJan5Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan5Utc;
        return false;
    }

    public static bool IsCursedModeLocked()
    {
        if (!IsReleaseLockEnabled)
            return false;
        return AmongUsDateTime.UtcNow < ReleaseAtJan3CursedUtc;
    }
    /*
        private static void Debug_UpdateTime(int hour, int minute)
        {
            ReleaseAtJan1Utc = new(2025, 12, 31, hour, minute, 0, Il2CppSystem.DateTimeKind.Utc);
        }*/
}
