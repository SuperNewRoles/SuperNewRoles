using Hazel;

namespace SuperNewRoles.Modules
{
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
        MeetingSheriffKill,
        MeetingSheriffMisFire,
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
        SheriffHauntedWolfKill,
        MeetingSheriffHauntedWolfKill,
        RemoteSheriffHauntedWolfKill,
        RemoteSheriffKill,
        RemoteSheriffMisFire,
        SerialKillerSelfDeath,
        VampireKill,
        OverKillerOverKill,
        SuicideWisherSelfDeath,
        MadmakerMisSet,
        Revenge,//猫カボチャの道連れ
    }
}