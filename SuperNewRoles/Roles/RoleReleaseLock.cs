using System;
using System.Collections.Generic;

namespace SuperNewRoles.Roles;

public static class RoleReleaseLock
{
    // リークしないでくれたら嬉しい
    // Please don't leak it.


    private static readonly HashSet<RoleId> ReleaseAtJan4Roles = new()
    {
        RoleId.Banshee,
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
        RoleId.Psychometrist,
        RoleId.DyingMessenger,
    };

    private static Il2CppSystem.DateTime ReleaseAtJan4Utc = new(2026, 1, 4, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);
    private static Il2CppSystem.DateTime ReleaseAtJan5Utc = new(2026, 1, 5, 8, 0, 0, Il2CppSystem.DateTimeKind.Utc);

    private static bool IsReleaseLockEnabled = ThisAssembly.Git.Branch == BranchConfig.MasterBranch;

    public static int GetReleaseStateToken()
    {
        if (!IsReleaseLockEnabled)
            return 3;
        int token = 0;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan4Utc)
            token |= 4;
        if (AmongUsDateTime.UtcNow >= ReleaseAtJan5Utc)
            token |= 2;
        return token;
    }

    public static bool IsLocked(RoleId roleId)
    {
        if (!IsReleaseLockEnabled)
            return false;
        if (ReleaseAtJan4Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan4Utc;
        if (ReleaseAtJan5Roles.Contains(roleId))
            return AmongUsDateTime.UtcNow < ReleaseAtJan5Utc;
        return false;
    }

    /*
        private static void Debug_UpdateTime(int hour, int minute)
        {
            ReleaseAtJan1Utc = new(2025, 12, 31, hour, minute, 0, Il2CppSystem.DateTimeKind.Utc);
        }*/
}
