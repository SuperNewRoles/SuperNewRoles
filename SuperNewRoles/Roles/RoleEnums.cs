using System;
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
    MadRaccoon,
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
    Datahacker,
    Santa,
    Arsonist,
    BlackSanta,
    Penguin,
    Pusher,
    EvilHacker,
    JumpDancer,
    SuicideWisher,
    WellBehaver,
    NekoKabocha,
    NiceNekomata,
    EvilNekomata,
    Matryoshka,
    Amnesiac,
    Conjurer,
    Doppelganger,
    FalseCharges,
    Pteranodon,
    Hitman,
    LoversBreaker,
    NiceButtoner,
    EvilButtoner,
    Mayor,
    Worshiper,
    Necromancer,
    Teleporter,
    Pursuer,
    Bakery,
    God,
    Owl,
    SchrodingersCat,
    Truelover,
    Cupid,
    Crab,
    VentTrapper,
    SpeedBooster,
    EvilSpeedBooster,
    SatsumaAndImo,
    NiceTeleporter,
    Taskmaster,
    ToiletFan,
    Rocket,
    Vampire,
    VampireDependent,
    SidekickWaveCannon,
    Spelunker,
    BlackHatHacker,
    PartTimer,
    NiceMechanic,
    EvilMechanic,
    BodyBuilder,
    HamburgerShop,
    NiceHawk,
    EvilHawk,
    MadHawk,
    WiseMan,
    Bullet,
}

public enum GhostRoleId : short
{
    None = 0,
    Cantera,
    Revenant,
    Mirage,
}

[Flags]
public enum ModifierRoleId : short
{
    None = 0,
    ModifierGuesser = 1 << 0,
    ModifierMadmate = 1 << 1,
    RulerModifier = 1 << 2,
    Lovers = 1 << 3,
    JumboModifier = 1 << 4,
    ModifierSpelunker = 1 << 5,
    ModifierHawk = 1 << 6,
}

public enum QuoteMod : byte
{
    Vanilla,
    SuperNewRoles,
    NebulaOnTheShip,
    TownOfHost,
    TheOtherRoles,
    TheOtherRolesGM,
    TheOtherRolesGMH,
    AuLibHalt,
    ExtremeRoles,
    exr
}

public enum TeamTag : byte
{
    Crewmate,
    Impostor,
    Neutral,
    Lover,
    Jackal,
    Madmate,
    Agi,
    None
}

public enum WinnerTeamType : byte
{
    None,
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
    Information,
    Killer,
    CustomObject,
    GhostRole,
    Support,
    ImpostorTeam,
}