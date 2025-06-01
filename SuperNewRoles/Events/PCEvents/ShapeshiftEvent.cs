using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events.PCEvents;

public class ShapeshiftEventData : IEventData
{
    public PlayerControl shapeshifter { get; }
    public PlayerControl target { get; }
    public bool animate { get; }
    public ShapeshiftEventData(PlayerControl shapeshifter, PlayerControl target, bool animate)
    {
        this.shapeshifter = shapeshifter;
        this.target = target;
        this.animate = animate;
    }
}


public class ShapeshiftEvent : EventTargetBase<ShapeshiftEvent, ShapeshiftEventData>
{
    public static bool Invoke(PlayerControl shapeshifter, PlayerControl target, bool animate)
    {
        var data = new ShapeshiftEventData(shapeshifter, target, animate);
        Instance.Awake(data);
        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class ShapeshiftEventPatch
{
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        ShapeshiftEvent.Invoke(__instance, targetPlayer, animate);
    }
}
