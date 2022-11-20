using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches
{
    class GameStartPatch
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
        class MakePublicPatch
        {
            /*public static bool Prefix(GameStartManager __instance)
            {
                bool NameIncludeMod = LegacySaveManager.PlayerName.ToLower().Contains("mod");
                bool NameIncludeSNR = LegacySaveManager.PlayerName.ToUpper().Contains("SNR");
                bool NameIncludeSHR = LegacySaveManager.PlayerName.ToUpper().Contains("SHR");
                if (AmongUsClient.Instance.AmHost)
                {
                    if (NameIncludeMod && !NameIncludeSNR && !NameIncludeSHR)
                    {
                        SuperNewRolesPlugin.Logger.LogWarning("\"mod\"が名前に含まれている状態では公開部屋にすることはできません。");
                        __instance.MakePublicButton.color = Palette.DisabledClear;
                        __instance.privatePublicText.color = Palette.DisabledClear;
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format("Modが名前に含まれている状態では公開部屋にすることはできません。"));
                        return false;
                    }
                    else if ((ModeHandler.IsMode(ModeId.SuperHostRoles, false) && NameIncludeSNR && !NameIncludeSHR) || (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && NameIncludeMod && !NameIncludeSHR))
                    {
                        SuperNewRolesPlugin.Logger.LogWarning("SHRモードで\"SNR\"が名前に含まれている状態では公開部屋にすることはできません。");
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format("SHRモードでSNRが名前に含まれている状態では公開部屋にすることはできません。"));
                        return false;
                    }
                    else if (!NameIncludeSNR && !NameIncludeSHR)
                    {
                        SuperNewRolesPlugin.Logger.LogWarning("Mod関連のワードが名前にないので公開部屋にできません");
                        __instance.MakePublicButton.color = Palette.DisabledClear;
                        __instance.privatePublicText.color = Palette.DisabledClear;
                        PlayerControl.LocalPlayer.RpcSendChat(string.Format("Mod関連のワードが名前に入っていないと公開部屋にすることはできません。特殊モードをご利用の場合は名前に「SHR」を入れてください。"));
                        return false;
                    }
                }
                return true;
            }*/
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class LobbyCountDownTimer
        {
            public static void Postfix()
            {
                if (Input.GetKeyDown(KeyCode.F8) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
                }
                if (CustomOptionHolder.DebugModeFastStart != null && CustomOptionHolder.DebugModeFastStart.GetBool() && CustomOptionHolder.IsDebugMode.GetBool())//デバッグモードでデバッグ即開始が有効
                {//カウントダウン中
                    if (GameStartManager.InstanceExists && FastDestroyableSingleton<GameStartManager>.Instance.startState == GameStartManager.StartingStates.Countdown)
                    {//カウント0
                        FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
                    }
                }
                if (Input.GetKeyDown(KeyCode.F7) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    FastDestroyableSingleton<GameStartManager>.Instance.ResetStartState();
                }
            }
        }
    }
}