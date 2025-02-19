using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules;
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class CheckGameEndPatch
{
    private static float Timer;
    private const float EndGameTimerMax = 0.15f;
    private static bool TimerUpdate()
    {
        Timer -= Time.fixedDeltaTime;
        if (0 < Timer && Timer < 0.5f)
            return false;
        Timer = EndGameTimerMax;
        return true;
    }
    public static bool Prefix()
    {
        // 更新回数を減らして負荷軽減
        if (!TimerUpdate()) return false;

        if (!GameData.Instance) return false;
        if (CustomOptionManager.DebugMode && CustomOptionManager.DebugModeNoGameEnd) return false;
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        ShipStatus __instance = ShipStatus.Instance;
        PlayerStatistics statistics = new();

        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForTaskWin(__instance)) return false;
        return false;
    }
    public static void CustomEndGame(GameOverReason reason, bool showAd)
    {
        GameManager.Instance.RpcEndGame(reason, showAd);
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
        ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.HeliSabotage) ? __instance.Systems[SystemTypes.HeliSabotage] : null;
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
        }
        if (systemType2 == null)
        {
            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
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
            __instance.enabled = false;
            CustomEndGame(GameOverReason.HumansByTask, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive)
        {
            __instance.enabled = false;
            var endReason = GameData.LastDeathReason switch
            {
                DeathReason.Exile => GameOverReason.ImpostorByVote,
                DeathReason.Kill => GameOverReason.ImpostorByKill,
                _ => GameOverReason.ImpostorByVote,
            };

            CustomEndGame(endReason, false);
            return true;
        }
        return false;
    }

    public static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamImpostorsAlive == 0)
        {
            __instance.enabled = false;
            CustomEndGame(GameOverReason.HumansByVote, false);
            return true;
        }
        return false;
    }
    public static void EndGameForSabotage(ShipStatus __instance)
    {
        __instance.enabled = false;
        CustomEndGame(GameOverReason.ImpostorBySabotage, false);
        return;
    }
}
public class PlayerStatistics
{
    public int TeamImpostorsAlive { get; set; }
    public int CrewAlive { get; set; }
    public int TotalAlive { get; set; }

    public PlayerStatistics()
    {
        GetPlayerCounts();
    }
    private void GetPlayerCounts()
    {
        int numImpostorsAlive = 0;
        int numCrewAlive = 0;
        int numTotalAlive = 0;

        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.IsDead())
                continue;
            if (player.IsImpostor())
            {
                numImpostorsAlive++;
            }
            else if (player.IsCrewmate())
            {
                numCrewAlive++;
            }
            else if (player.IsNeutral())
            {
            }
        }

        TeamImpostorsAlive = numImpostorsAlive;
        TotalAlive = numTotalAlive;
        CrewAlive = numCrewAlive;
    }
}