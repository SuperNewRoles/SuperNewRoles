namespace SuperNewRoles.Roles;

public enum RoleId : short
{
    None = 0,
    Bait,
    BestFalseCharge,
}

public enum QuoteMod : byte
{
    SuperNewRoles,
    NebulaOnTheShip,
    TownOfHost,
    TheOtherRoles,
}

public enum TeamTag : byte
{
    Crewmate,
    Impostor,
    Neutral,
    Lover,
    Arsonist,
    Jackal,
    Madmate,
    Agi,
    None
}

public enum WinnerTeamType : byte
{
    Crewmate,
    Impostor,
    Neutral,
}

public enum AssignedTeamType : byte
{
    Crewmate,
    Impostor,
    Neutral,
}

public enum RoleTag : byte
{
    SpecialKiller,
    PowerPlayResistance,
}