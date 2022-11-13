using HarmonyLib;

namespace SuperNewRoles.Patches
{
    class DebugMode
    {
        [HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
        public static class MapConsoleUsePatch
        {
            public static void Prefix(MapConsole __instance)
            {
                if (ConfigRoles.DebugMode.Value)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(x):" + __instance.transform.position.x);
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(y):" + __instance.transform.position.y);
                    SuperNewRolesPlugin.Logger.LogInfo("[DebugMode]Admin Coordinate(Z):" + __instance.transform.position.z);
                }
            }
        }
        public static bool IsDebugMode() => ConfigRoles.DebugMode.Value && CustomOptionHolder.IsDebugMode.GetBool();

        public static class MurderPlayerPatch
        {
            /// <summary>
            /// MurderPlayerが発動した時に通知します。
            /// </summary>
            public static void Announce()
            {
                if (!(IsDebugMode() && CustomOptionHolder.IsMurderPlayerAnnounce.GetBool())) return;

                new CustomMessage("MurderPlayerが発生しました", 5f);
                Logger.Info("MurderPlayerが発生しました", "DebugMode");
            }
        }
    }
}