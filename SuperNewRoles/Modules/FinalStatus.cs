using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Modules;

public enum FinalStatus
{
    None,
    Alive,
    Kill,
    Disconnect,
    Revange,
    Exiled,
    GuesserKill,
    GuesserMisFire,
    Sabotage,
    WaveCannon,
    FalseCharge,
    Suicide,
    Samurai,
    BombBySelfBomb,
    SelfBomb,
    Tuna,
    Push,
    Ignite,
    FalseCharges,
    SheriffSelfDeath,
    SheriffKill,
    LoversSuicide,
    LaunchByRocket,
    VampireKill,
    VampireWithDead,
    SpelunkerSetRoleDeath,
    SpelunkerVentDeath,
    SpelunkerCommsElecDeath,
    SpelunkerOpenDoor,
    NunDeath,
    LadderDeath,
}

public static class FinalStatusListener
{
    public static void LoadListener()
    {
        WrapUpEvent.Instance.AddListener(x => SetFinalStatus(x.exiled, FinalStatus.Exiled));
        MurderEvent.Instance.AddListener(x => SetFinalStatus(x.target, FinalStatus.Kill));
        DisconnectEvent.Instance.AddListener(x => SetFinalStatus(x.disconnectedPlayer, FinalStatus.Disconnect));
    }
    private static void SetFinalStatus(ExPlayerControl exPlayer, FinalStatus finalStatus)
    {
        if (exPlayer == null) return;
        exPlayer.FinalStatus = finalStatus;
    }
}
public static class FinalStatusManager
{
    [CustomRPC]
    public static void RpcSetFinalStatus(ExPlayerControl exPlayer, FinalStatus finalStatus)
    {
        SetFinalStatus(exPlayer, finalStatus);
    }
    public static void SetFinalStatus(ExPlayerControl exPlayer, FinalStatus finalStatus)
    {
        exPlayer.FinalStatus = finalStatus;
    }
}