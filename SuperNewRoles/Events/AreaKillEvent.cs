using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class AreaKillEventData : IEventData
{
    public PlayerControl killer { get; }
    public List<ExPlayerControl> killedPlayers { get; }
    public CustomDeathType deathType { get; }

    public AreaKillEventData(PlayerControl killer, List<ExPlayerControl> killedPlayers, CustomDeathType deathType)
    {
        this.killer = killer;
        this.killedPlayers = killedPlayers;
        this.deathType = deathType;
    }
}

public class AreaKillEvent : EventTargetBase<AreaKillEvent, AreaKillEventData>
{
    public static void Invoke(PlayerControl killer, List<ExPlayerControl> killedPlayers, CustomDeathType deathType)
    {
        var data = new AreaKillEventData(killer, killedPlayers, deathType);
        Instance.Awake(data);
    }
}