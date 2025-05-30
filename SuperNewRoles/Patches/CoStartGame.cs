using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
class AmongUsClientStartPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        Logger.Info("CoStartGame");
        ExPlayerControl.SetUpExPlayers();
        EventListenerManager.ResetAllListener();
        SuperTrophyManager.CoStartGame();
        Garbage.ClearAndReload();
        CustomKillAnimationManager.ClearCurrentCustomKillAnimation();
    }
}