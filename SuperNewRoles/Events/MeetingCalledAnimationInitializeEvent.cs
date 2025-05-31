using HarmonyLib;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Events;

public class MeetingCalledAnimationInitializeEventData : IEventData
{
    public MeetingCalledAnimation animation { get; }
    public NetworkedPlayerInfo.PlayerOutfit outfit { get; }
    public MeetingCalledAnimationInitializeEventData(MeetingCalledAnimation animation, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        this.animation = animation;
        this.outfit = outfit;
    }
}
public class MeetingCalledAnimationInitializeEvent : EventTargetBase<MeetingCalledAnimationInitializeEvent, MeetingCalledAnimationInitializeEventData>
{
    public static void Invoke(MeetingCalledAnimation animation, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        Instance.Awake(new MeetingCalledAnimationInitializeEventData(animation, outfit));
    }
}

[HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
public static class MeetingCalledAnimationInitializePatch
{
    public static void Postfix(MeetingCalledAnimation __instance, NetworkedPlayerInfo.PlayerOutfit outfit)
    {
        MeetingCalledAnimationInitializeEvent.Invoke(__instance, outfit);
    }
}
