using static SuperNewRoles.Patches.CheckGameEndPatch;

namespace SuperNewRoles.Mode.RandomColor
{
    class Main
    {
        public static bool CheckEndGame(ShipStatus __instance, PlayerStatistics statistics)
        {
            return CheckAndEndGameForSabotageWin(__instance)
                || CheckAndEndGameForImpostorWin(__instance, statistics)
                ? false
                : !CheckAndEndGameForCrewmateWin(__instance, statistics)
&& !PlusModeHandler.IsMode(PlusModeId.NotTaskWin) && CheckAndEndGameForTaskWin(__instance) && false;
        }
        public static void CustomEndGame(ShipStatus __instance, GameOverReason reason, bool showAd)
        {
            __instance.enabled = false;
            ShipStatus.RpcEndGame(reason, showAd);
        }
        public static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        public static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                CustomEndGame(__instance, GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive && statistics.TeamJackalAlive == 0)
            {
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };
                CustomEndGame(__instance, endReason, false);
                return true;
            }
            return false;
        }

        public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.CrewAlive > 0 && statistics.TeamImpostorsAlive == 0)
            {
                CustomEndGame(__instance, GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }
        public static void EndGameForSabotage(ShipStatus __instance)
        {
            CustomEndGame(__instance, GameOverReason.ImpostorBySabotage, false);
            return;
        }
    }
}