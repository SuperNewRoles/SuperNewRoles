using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class ControllerManagerUpdatePatch
    {
        static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
        static int resolutionIndex = 0;
        public static void Postfix()
        {
            //解像度変更
            if (Input.GetKeyDown(KeyCode.F9))
            {
                resolutionIndex++;
                if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
                ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
            }
            // 以下ホストのみ
            if (!AmongUsClient.Instance.AmHost) return;

            //　ゲーム中
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                // 廃村
                if (ModHelpers.GetManyKeyDown(new[] { KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift }))
                {
                    RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                    RPCProcedure.SetHaison();
                    Logger.Info("===================== Haison =====================", "End Game");
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        EndGameCheck.CustomEndGame(ShipStatus.Instance, GameOverReason.ImpostorDisconnect, false);
                    }
                    else
                    {
                        ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                        MapUtilities.CachedShipStatus.enabled = false;
                    }
                }
            }

            // 会議を強制終了
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift }) && RoleClass.IsMeeting)
            {
                if (MeetingHud.Instance != null)
                    MeetingHud.Instance.RpcClose();
            }

            // 以下フリープレイのみ
            if (AmongUsClient.Instance.GameMode != GameModes.FreePlay) return;
            // エアーシップのトイレのドアを開ける
            if (Input.GetKeyDown(KeyCode.T))
            {
                RPCHelper.RpcOpenToilet();
            }
        }
    }
}
