using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    class CopyGameCode
    {
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (ConfigRoles.AutoCopyGameCode.Value)
                {
                    string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                    GUIUtility.systemCopyBuffer = code;
                }
            }
        }
    }
}