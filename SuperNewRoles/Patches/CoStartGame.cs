using HarmonyLib;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.SuperTrophies;
using SuperNewRoles.MapCustoms;

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
        
        // The Fungle マップ初期化フラグをリセット
        FungleAdditionalAdmin.Reset();
        FungleAdditionalElectrical.Reset();
        ZiplineUpdown.Reset();
    }
}