using System;
using System.Linq;
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
        try
        {
            Logger.Info("CoStartGame");

            // プレイヤー接続状態を確認
            if (PlayerControl.LocalPlayer == null || PlayerControl.AllPlayerControls == null)
            {
                Logger.Warning("Player control not initialized in CoStartGame");
                return;
            }

            // 全プレイヤーの接続状態を確認
            var disconnectedPlayers = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => p == null || p.Data == null || p.Data.Disconnected)
                .ToArray();

            if (disconnectedPlayers.Length > 0)
            {
                Logger.Info($"Found {disconnectedPlayers.Length} disconnected players during game start");
            }

            ExPlayerControl.SetUpExPlayers();
            EventListenerManager.ResetAllListener();
            SuperTrophyManager.CoStartGame();
            Garbage.ClearAndReload();
            CustomKillAnimationManager.ClearCurrentCustomKillAnimation();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in CoStartGame: {ex.Message}\n{ex.StackTrace}");
            // エラーが発生してもゲームを続行できるようにする
        }
    }
}