using Hazel;

namespace SuperNewRoles.Modules;

public static class FinalStatusClass
{
    public static void RpcSetFinalStatus(this PlayerControl player, FinalStatus status)
    {
        MessageWriter writer = Helpers.RPCHelper.StartRPC(CustomRPC.SetFinalStatus);
        writer.Write(player.PlayerId);
        writer.Write((byte)status);
        Helpers.RPCHelper.EndRPC(writer);
        RPCProcedure.SetFinalStatus(player.PlayerId, status);
    }
}
public enum FinalStatus
{
    Alive,
    Kill,
    Exiled,
    SheriffKill,
    SheriffMisFire,
    HauntedSheriffKill,
    HauntedSheriffMisFire,
    SheriffHauntedWolfKill,
    SheriffInvolvedOutburst,
    Ignite,
    Disconnected,
    Dead,
    Sabotage,
    NekomataExiled,

    SluggerHarisen,
    LoversBomb,
    KunaiKill,
    SamuraiKill,
    ChiefMisSet,
    FalseChargesFalseCharge,
    SelfBomberBomb,
    BySelfBomberBomb,
    LadderDeath,
    NunDeath,
    SpelunkerCommsElecDeath,
    SpelunkerSetRoleDeath,
    SpelunkerVentDeath,
    SpelunkerOpenDoor,
    TunaSelfDeath,
    BestFalseChargesFalseCharge,
    SerialKillerSelfDeath,
    VampireKill,
    DependentsExiled,
    OverKillerOverKill,
    SuicideWisherSelfDeath,
    MadmakerMisSet,
    Revenge,//猫カボチャの道連れ
    HitmanKill,
    HitmanDead,
    WorshiperSelfDeath,
    TheThirdLittlePigCounterKill,
    GuesserKill,
    GuesserMisFire,
    LoversBreakerKill,
    MadJesterExiled,
}