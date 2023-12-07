using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.MapOption;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Patches;

class TaskCount
{
    public static PlayerData<bool> IsClearTaskPlayer;
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.PickRandomConsoles), new Type[] { typeof(TaskTypes), typeof(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>) })]
    class NormalPlayerTaskPickRandomConsolesPatch
    {
        static void Postfix(NormalPlayerTask __instance, TaskTypes taskType)
        {
            if (taskType != TaskTypes.FixWiring || !ModeHandler.IsMode(ModeId.Default) || !MapOption.MapOption.WireTaskIsRandom) return;
            List<Console> orgList = MapUtilities.CachedShipStatus.AllConsoles.Where((global::Console t) => t.TaskTypes.Contains(taskType)).ToList<global::Console>();
            List<Console> list = new(orgList);

            __instance.MaxStep = MapOption.MapOption.WireTaskNum;
            __instance.Data = new byte[MapOption.MapOption.WireTaskNum];
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
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.Tasker) && __instance.TaskStep > 0 && !__instance.IsComplete)
                __instance.Arrow.gameObject.SetActive(true);
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
            (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !playerInfo.IsDead) &&
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
                case ModeId.Werewolf:
                    CountDefaultTask(__instance);
                    ReleaseHountAbility(__instance);
                    break;
                case ModeId.CopsRobbers:
                    CountDefaultTask(__instance);
                    break;
                case ModeId.Zombie:
                    Mode.Zombie.Main.CountTaskZombie(__instance);
                    break;
                case ModeId.Detective:
                    Mode.Detective.Task.TaskCountDetective(__instance);
                    break;
            }
            if (__instance.TotalTasks <= 0)
                __instance.TotalTasks = 1;
        }
    }
    static void CountDefaultTask(GameData __instance)
    {
        __instance.TotalTasks = 0;
        __instance.CompletedTasks = 0;
        for (int i = 0; i < __instance.AllPlayers.Count; i++)
        {
            GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
            if (!RoleHelpers.IsClearTask(playerInfo.Object) && !playerInfo.Object.IsBot())
            {
                var (playerCompleted, playerTotal) = TaskDate(playerInfo);
                __instance.TotalTasks += playerTotal;
                __instance.CompletedTasks += playerCompleted;
            }
        }
    }

    /// <summary>
    /// 全タスク完了後に, 憑依能力を開放する
    /// (タスク完了の判定処理 及び, クルーメイトゴーストへの変更処理)
    /// </summary>
    /// <param name="__instance"></param>
    static void ReleaseHountAbility(GameData __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return; // 生存していたら早期return
        if (!Mode.PlusMode.PlusGameOptions.IsReleasingHauntAfterCompleteTasks) return; // 設定が無効の場合早期return

        for (int i = 0; i < __instance.AllPlayers.Count; i++)
        {
            GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
            PlayerControl player = playerInfo.Object;

            if (player == null || player.IsBot()) continue;
            if (player.IsAlive()) continue;

            RoleTypes roleType = playerInfo.Role.Role;
            // クルーメイト以外は以降の処理を破棄 (ImpostorとShapeshifterはここまで届かない為, 省略)
            if (roleType is RoleTypes.ImpostorGhost or RoleTypes.CrewmateGhost or RoleTypes.GuardianAngel) continue;

            bool isRelease = Roles.HandleGhostRole.AssignRole.GetReleaseHauntAbility(player);
            if (isRelease) player.RpcSetRole(RoleTypes.CrewmateGhost); // タスクが完了していたらクルーメイトゴーストに変更し, 憑依能力を開放する。
        }
    }
}