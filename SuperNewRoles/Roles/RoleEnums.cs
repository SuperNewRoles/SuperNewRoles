using SuperNewRoles.CustomOptions;

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
    JackalFriends,
    Balancer,
    Samurai,
    SelfBomber,
    WaveCannon,
    WaveCannonJackal,
    Tuna,
    Teruteru,
    Opportunist,
    Chief,
    Workperson,
    SerialKiller,
    PavlovsOwner,
    PavlovsDog,
    SideKiller,
    MadKiller,
    EvilGambler,
    HomeSecurityGuard,
    Seer,
    Celebrity,
    Vulture,
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
    Crewmate = RoleOptionMenuType.Crewmate,
    Impostor = RoleOptionMenuType.Impostor,
    Neutral = RoleOptionMenuType.Neutral,
}

public enum RoleTag : byte
{
    SpecialKiller,
    PowerPlayResistance,
}