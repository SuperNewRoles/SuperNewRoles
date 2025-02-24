using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Modules;

public enum FinalStatus
{
    None,
    Alive,
    Kill,
    Disconnect,
    Exiled,
    GuesserKill,
    GuesserMisFire,
    Sabotage,
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