using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.MapOptions;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Patch
{
    class TaskCount
    {
        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.PickRandomConsoles))]
        class NormalPlayerTaskPickRandomConsolesPatch
        {
            static void Postfix(NormalPlayerTask __instance, TaskTypes taskType, byte[] consoleIds)
            {
                if (taskType != TaskTypes.FixWiring || !MapOption.WireTaskIsRandom) return;
                List<Console> orgList = ShipStatus.Instance.AllConsoles.Where((global::Console t) => t.TaskTypes.Contains(taskType)).ToList<global::Console>();
                List<Console> list = new(orgList);

                __instance.MaxStep = MapOption.WireTaskNum;
                __instance.Data = new byte[MapOption.WireTaskNum];
                for (int i = 0; i < __instance.Data.Length; i++)
                {
                    if (list.Count == 0)
                        list = new List<Console>(orgList);
                    int index = ModHelpers.GetRandomIndex(list);
                    __instance.Data[i] = (byte)list[index].ConsoleId;
                    list.RemoveAt(index);
                }
            }
        }
        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
        public static class NormalPlayerTaskPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (__instance.IsComplete && __instance.Arrow?.isActiveAndEnabled == true)
                    __instance.Arrow?.gameObject?.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
        public static class AirshipUploadTaskPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (__instance.IsComplete)
                    __instance.Arrows?.DoIf(x => x != null && x.isActiveAndEnabled, x => x.gameObject?.SetActive(false));
            }
        }
        public static Tuple<int, int> TaskDateNoClearCheck(GameData.PlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;

            for (int j = 0; j < playerInfo.Tasks.Count; j++)
            {
                TotalTasks++;
                if (playerInfo.Tasks[j].Complete)
                {
                    CompletedTasks++;
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }
        public static Tuple<int, int> TaskDate(GameData.PlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress
                )
            {
                for (int j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        CompletedTasks++;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            public static void Postfix(GameData __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                switch (ModeHandler.GetMode())
                {
                    case ModeId.SuperHostRoles:
                    case ModeId.Default:
                        CountDefaultTask(__instance);
                        return;
                    case ModeId.Zombie:
                        Mode.Zombie.Main.CountTaskZombie(__instance);
                        return;
                    case ModeId.Detective:
                        Mode.Detective.Task.TaskCountDetective(__instance);
                        return;
                    case ModeId.HideAndSeek:
                        Mode.HideAndSeek.Task.TaskCountHideAndSeek(__instance);
                        return;
                }
            }
        }
        static void CountDefaultTask(GameData __instance)
        {
            __instance.TotalTasks = 0;
            __instance.CompletedTasks = 0;
            for (int i = 0; i < __instance.AllPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                if (!RoleHelpers.IsClearTask(playerInfo.Object) && playerInfo.Object.IsPlayer())
                {
                    var (playerCompleted, playerTotal) = TaskDate(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
            }
        }
    }
}