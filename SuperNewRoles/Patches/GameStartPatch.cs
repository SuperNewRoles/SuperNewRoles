using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches;

class GameStartPatch
{
    /*[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
    class MakePublicPatch
    {
        public static bool Prefix(GameStartManager __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;

            var HostVersion = ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers[AmongUsClient.Instance.HostId].version;
            var error = (Mode.ModeHandler.IsMode(Mode.ModeId.Default) || Mode.ModeHandler.IsMode(Mode.ModeId.Werewolf))
                ? string.Format(ModTranslation.GetString("PublicRoomErrorClientMode"), HostVersion)
                : string.Format(ModTranslation.GetString("PublicRoomErrorHostMode"), HostVersion);

            Logger.Error(error, "MakePublicPatch");
            __instance.MakePublicButton.color = Palette.DisabledClear;
            __instance.privatePublicText.color = Palette.DisabledClear;
            PlayerControl.LocalPlayer.RpcSendChat(error);

            return false;
        }
    }*/
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class LobbyCountDownTimer
    {
        public static void Postfix()
        {
            if (!GameStartManager._instance || !AmongUsClient.Instance.AmHost) return; // 以下ホストのみで動作

            if (Input.GetKeyDown(KeyCode.F7))
            {
                FastDestroyableSingleton<GameStartManager>.Instance.ResetStartState();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
            }

            // 以下デバッグモード限定の機能
            if (!ModHelpers.IsDebugMode()) return;

            if (CustomOptionHolder.DebugModeFastStart.GetBool()) // デバッグモードでデバッグ即開始が有効
            {
                if (GameStartManager.InstanceExists && FastDestroyableSingleton<GameStartManager>.Instance.startState == GameStartManager.StartingStates.Countdown) // カウントダウン中
                {
                    FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0; //カウント0
                }
            }
        }
    }
}