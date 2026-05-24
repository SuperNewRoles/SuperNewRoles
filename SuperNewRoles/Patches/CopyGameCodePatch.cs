using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

public static class CopyGameCodePatch
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            if (ConfigRoles.AutoCopyGameCode == null || !ConfigRoles.AutoCopyGameCode.Value)
                return;

            if (AmongUsClient.Instance == null)
                return;

            GUIUtility.systemCopyBuffer = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        }
    }
}
