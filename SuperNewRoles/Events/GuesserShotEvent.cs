using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class GuesserShotEventData : IEventData
{
    public PlayerControl killer { get; }
    public PlayerControl target { get; }
    public bool isMisFire { get; }

    public GuesserShotEventData(PlayerControl killer, PlayerControl target, bool isMisFire)
    {
        this.killer = killer;
        this.target = target;
        this.isMisFire = isMisFire;
    }
}

public class GuesserShotEvent : EventTargetBase<GuesserShotEvent, GuesserShotEventData>
{
    public static void Invoke(PlayerControl killer, PlayerControl target, bool isMisFire)
    {
        var data = new GuesserShotEventData(killer, target, isMisFire);
        Instance.Awake(data);
    }
}