using System;
using System.Collections.Generic;

namespace SuperNewRoles.Roles;

public static class RoleReleaseLock
{
    private static readonly HashSet<RoleId> LockedRoles = new()
    {
        RoleId.Slugger,
        RoleId.Psychometrist,
        RoleId.DyingMessenger,
        RoleId.Kunoichi,
        RoleId.Squid,
        RoleId.Sauner,
    };

    private static readonly DateTimeOffset ReleaseAtJst = new(2026, 1, 1, 17, 0, 0, TimeSpan.FromHours(9));
    private static readonly DateTimeOffset ReleaseAtUtc = ReleaseAtJst.ToUniversalTime();

    public static bool IsReleaseLocked => DateTimeOffset.UtcNow < ReleaseAtUtc;

    public static bool IsLocked(RoleId roleId)
    {
        return LockedRoles.Contains(roleId) && IsReleaseLocked;
    }
}
