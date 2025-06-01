using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.HelpMenus;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
class ControllerManagerUpdatePatch
{
    static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
    static int resolutionIndex = 0;
    static AudioSource source;

    public static KeyCode[] HaisonKeyCodes = [KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift];
    static KeyCode[] ForceEndMeetingKeyCodes = [KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift];
    static KeyCode[] LogKeyCodes = [KeyCode.S, KeyCode.LeftShift, KeyCode.RightShift];


    public static void Postfix()
    {
        //解像度変更
        if (Input.GetKeyDown(KeyCode.F9))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }

        // その時点までのlogを切り出す
        if (ModHelpers.GetManyKeyDown(LogKeyCodes))
        {
            // TODO: 後で実装
            string via = "KeyCmdVia";
            throw new NotImplementedException("LogKey not supported");
            // LoggerPlus.SaveLog(via, via);
        }
        if (!AmongUsClient.Instance.AmHost) return;
        //　ゲーム中
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
        {
            // 廃村
            if (ModHelpers.GetManyKeyDown(HaisonKeyCodes))
            {
                Logger.Info("===================== 廃村 ======================", "End Game");
                EndGamer.RpcHaison();
                ShipStatus.Instance.enabled = false;
            }
            // 会議を強制終了
            if (MeetingHud.Instance != null && ModHelpers.GetManyKeyDown(ForceEndMeetingKeyCodes))
            {
                MeetingHud.Instance.RpcClose();
            }
        }
        else if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined && GameStartManager.Instance != null)
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                FastDestroyableSingleton<GameStartManager>.Instance.ResetStartState();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
            }
        }
    }
}
