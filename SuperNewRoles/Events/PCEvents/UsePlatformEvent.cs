using System;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Events.PCEvents;

public class UsePlatformEventData : IEventData
{
    public ExPlayerControl player { get; }
    public MovingPlatformBehaviour platform { get; }

    public UsePlatformEventData(ExPlayerControl player, MovingPlatformBehaviour platform)
    {
        this.player = player;
        this.platform = platform;
    }
}

public class UsePlatformEvent : EventTargetBase<UsePlatformEvent, UsePlatformEventData>
{
    public static void Invoke(ExPlayerControl player, MovingPlatformBehaviour platform)
    {
        var data = new UsePlatformEventData(player, platform);
        Instance.Awake(data);
    }
}

[HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
public static class UsePlatformPatch
{
    public static void Postfix(MovingPlatformBehaviour __instance, PlayerControl target)
    {
        UsePlatformEvent.Invoke(target, __instance);
    }
}