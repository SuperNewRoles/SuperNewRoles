using System;
using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Events;

public class EndGameEventData : IEventData
{
    public GameOverReason reason { get; }
    public List<ExPlayerControl> winners { get; }
    public EndGameEventData(GameOverReason reason, List<ExPlayerControl> winners)
    {
        this.reason = reason;
        this.winners = winners;
    }
}

public class EndGameEvent : EventTargetBase<EndGameEvent, EndGameEventData>
{
    public static void Invoke(GameOverReason reason, List<ExPlayerControl> winners)
    {
        var data = new EndGameEventData(reason, winners);
        Instance.Awake(data);
    }
}
