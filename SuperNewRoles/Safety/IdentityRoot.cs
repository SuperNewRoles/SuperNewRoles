namespace SuperNewRoles.Safety;

/// <summary>
/// 0xD1 identity root protocol. Keep values in sync with Impostor StarlightSafety <c>IdentityRoot</c>.
/// </summary>
public static class IdentityRoot
{
    public const byte Flag = 0xD1;
}

public enum IdentityRootSubtype : byte
{
    Prove = 1,
    HostKick = 2,
    Notify = 3,
    Hello = 4,
    Challenge = 5,
    ParticipantIds = 6,
    IdentityAccepted = 7,
    Warn = 8,
    ConductBanLeave = 9,
    HostBlockLeave = 10,
}
