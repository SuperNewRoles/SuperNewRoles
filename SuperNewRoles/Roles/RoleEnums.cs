namespace SuperNewRoles.Roles;

public enum RoleId : short
{
    None = 0,
    Crewmate,
    Impostor,
    Bait,
    BestFalseCharge,
    Sheriff,
    NiceGuesser,
    EvilGuesser,
    Madmate,
    BlackCat,
    Jackal,
    Sidekick,
}

public enum QuoteMod : byte
{
    Vanilla,
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