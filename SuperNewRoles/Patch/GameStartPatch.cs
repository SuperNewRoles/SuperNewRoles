using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Patch
{
    class GameStartPatch
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
        class MakePublicPatch
        {
            public static bool Prefix()
            {
                bool NameIncludeMod = SaveManager.PlayerName.ToLower().Contains("mod");
                bool NameIncludeSNR = SaveManager.PlayerName.ToUpper().Contains("SNR");
                bool NameIncludeSHR = SaveManager.PlayerName.ToUpper().Contains("SHR");
                if (NameIncludeMod && !NameIncludeSNR && !NameIncludeSHR)
                {
                    SuperNewRolesPlugin.Logger.LogWarning("\"mod\"が名前に含まれている状態では公開部屋にすることはできません。");
                    return false;
                }
                else if (ModeHandler.isMode(ModeId.SuperHostRoles, false) && NameIncludeSNR && !NameIncludeSHR || ModeHandler.isMode(ModeId.SuperHostRoles, false) && NameIncludeMod && !NameIncludeSHR)
                {
                    SuperNewRolesPlugin.Logger.LogWarning("SHRモードで\"SNR\"が名前に含まれている状態では公開部屋にすることはできません。");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class LobbyCountDownTimer
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (Input.GetKeyDown(KeyCode.F8) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.countDownTimer = 0;
                }
                if (Input.GetKeyDown(KeyCode.F7) && GameStartManager._instance && AmongUsClient.Instance.AmHost)
                {
                    GameStartManager.Instance.ResetStartState();
                }
            }
        }
    }
}
