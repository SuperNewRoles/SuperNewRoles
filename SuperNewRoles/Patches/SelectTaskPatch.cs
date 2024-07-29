using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;
using IEnumerator = System.Collections.IEnumerator;

namespace SuperNewRoles.Patches;

public static class SelectTask
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    public static class ShipStatusBegin
    {
        public static bool Prefix(ShipStatus __instance)
        {
            __instance.numScans = 0;
            __instance.AssignTaskIndexes();

            List<NormalPlayerTask> _common = __instance.CommonTasks.ToList();
            _common.ForEach(t => t.Length = NormalPlayerTask.TaskLength.Common);
            _common.Shuffle(0);
            Il2CppSystem.Collections.Generic.List<NormalPlayerTask> CommonTasks = _common.ToIl2CppList();

            List<NormalPlayerTask> _long = __instance.LongTasks.ToList();
            _long.ForEach(t => t.Length = NormalPlayerTask.TaskLength.Long);
            _long.Shuffle(0);
            Il2CppSystem.Collections.Generic.List<NormalPlayerTask> LongTasks = _long.ToIl2CppList();

            List<NormalPlayerTask> _short = __instance.ShortTasks.ToList();
            _short.ForEach(t => t.Length = NormalPlayerTask.TaskLength.Short);
            _short.Shuffle(0);
            Il2CppSystem.Collections.Generic.List<NormalPlayerTask> ShortTasks = _short.ToIl2CppList();

            IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
            int numShort = currentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
            int numCommon = currentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
            int numLong = currentGameOptions.GetInt(Int32OptionNames.NumLongTasks);

            Il2CppSystem.Collections.Generic.HashSet<TaskTypes> types = new();
            Il2CppSystem.Collections.Generic.List<byte> list = new();
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;

            __instance.AddTasksFromList(ref num1, numCommon, list, types, CommonTasks);
            for (int i = 0; i < numCommon; i++)
            {
                if (list.Count == 0)
                {
                    Debug.LogWarning("Not enough common tasks");
                    break;
                }
                list.Add((byte)CommonTasks.GetRandom().Index);
            }
            if (numCommon + numLong + numShort == 0) numShort = 1;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsBot())
                {
                    player.Data.RpcSetTasks(new(0));
                    continue;
                }
                if (GetHaveTaskManageAbility(player.GetRole()) && ModeHandler.IsMode(ModeId.Default, ModeId.SuperHostRoles, ModeId.CopsRobbers) && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
                {
                    ModdedSetTasks(player,
                        ModHelpers.GenerateTasks(
                            player, player.GetTaskCount()
                        ).ToArray()
                    );
                }
                else
                {
                    types.Clear();
                    list.RemoveRange(numCommon, list.Count - numCommon);
                    __instance.AddTasksFromList(ref num2, numLong, list, types, LongTasks);
                    __instance.AddTasksFromList(ref num3, numShort, list, types, ShortTasks);
                    if (player && !player.GetComponent<DummyBehaviour>().enabled)
                        ModdedSetTasks(player, list.ToArray());
                }
            }
            PlayerControl.LocalPlayer.cosmetics.SetAsLocalPlayer();
            return false;
        }
    }
    private static void ModdedSetTasks(PlayerControl player, byte[] list)
    {
        if (RoleSelectHandler.SetTasksBuffer != null &&
            ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            RoleSelectHandler.SetTasksBuffer.Add(player.PlayerId, list);
            return;
        }
        player.Data.RpcSetTasks(list);
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CoSetTasks))]
    public static class PlayerControlCoSetTasks
    {
        public static bool Prefix(PlayerControl __instance, Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo> tasks, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            __result = CoSetTasks(__instance, tasks).WrapToIl2Cpp();
            return false;
        }

        public static IEnumerator CoSetTasks(PlayerControl __instance, Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo> tasks)
        {
            while (!ShipStatus.Instance) yield return null;
            if (__instance.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.TaskStuff.SetActive(true);
                StatsManager.Instance.IncrementStat(StringNames.StatsGamesStarted);
                if (!DestroyableSingleton<TutorialManager>.InstanceExists) DestroyableSingleton<AchievementManager>.Instance.OnMatchStart(__instance.Data.Role.Role);
            }
            ModHelpers.DestroyList(__instance.myTasks);
            __instance.Data.Role.SpawnTaskHeader(__instance);
            for (int i = 0; i < tasks.Count; i++)
            {
                NetworkedPlayerInfo.TaskInfo taskInfo = tasks[i];
                NormalPlayerTask taskById = ShipStatus.Instance.GetTaskById(taskInfo.TypeId);
                NormalPlayerTask normalPlayerTask = Object.Instantiate(taskById, __instance.transform);
                normalPlayerTask.Id = taskInfo.Id;
                normalPlayerTask.Index = taskById.Index;
                normalPlayerTask.Owner = __instance;
                normalPlayerTask.Initialize();
                __instance.logger.Info(string.Format("Assigned task {0} to {1}", normalPlayerTask.name, __instance.PlayerId), null);
                __instance.myTasks.Add(normalPlayerTask);
            }
            PlayerControlHelper.RefreshRoleDescription(__instance);
            yield break;
        }
    }

    public static (int, int, int) GetTaskCount(this PlayerControl p)
    {
        RoleId roleId = p.GetRole();

        // 特殊なタスク数設定の場合個別で判断(RoleIdで判断できない役職)
        if (p.IsLovers() && !p.IsImpostor())
        {
            int commont = LoversCommonTask.GetInt();
            int shortt = LoversShortTask.GetInt();
            int longt = LoversLongTask.GetInt();
            if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
        }

        if (GetHaveTaskManageAbility(roleId)) return GetRoleTaskData(roleId);
        else return (SyncSetting.DefaultOption.GetInt(Int32OptionNames.NumCommonTasks), SyncSetting.DefaultOption.GetInt(Int32OptionNames.NumShortTasks), SyncSetting.DefaultOption.GetInt(Int32OptionNames.NumLongTasks));
    }
    public static (CustomOption, CustomOption, CustomOption) TaskSetting(int commonid, int shortid, int longid, CustomOption Child = null, CustomOptionType type = CustomOptionType.Generic, bool IsSHROn = false)
    {
        CustomOption CommonOption = CustomOption.Create(commonid, IsSHROn, type, "GameCommonTasks", 1, 0, 12, 1, Child);
        CustomOption ShortOption = CustomOption.Create(shortid, IsSHROn, type, "GameShortTasks", 1, 0, 69, 1, Child);
        CustomOption LongOption = CustomOption.Create(longid, IsSHROn, type, "GameLongTasks", 1, 0, 45, 1, Child);
        return (CommonOption, ShortOption, LongOption);
    }

    /// <summary>
    /// 設定されたタスク数の合計を取得する
    /// </summary>
    /// <param name="roleId">取得したい役職のRoleId</param>
    /// <returns>int = 設定されている, commonタスク数, shortタスク数, longタスク数の合計</returns>
    internal static int GetTotalTasks(RoleId roleId)
    {
        var (roleCommon, roleLong, roleShort) = GetRoleTaskData(roleId);
        int roleTotalTasks = 0;

        roleTotalTasks = roleCommon + roleLong + roleShort;
        return roleTotalTasks;
    }

    /// <summary>
    /// タスクで管理する能力を有すか判定する。
    /// </summary>
    /// /// <param name="id">判定したい役職のRoleId</param>
    /// <returns> true : 有する, false : 有さない</returns>
    internal static bool GetHaveTaskManageAbility(RoleId id)
    {
        ITaskHolder holder = RoleBaseManager.GetInterfaces<ITaskHolder>().FirstOrDefault(x => (x as RoleBase).Role == id);
        if (holder != null && holder.HaveMyNumTask(out var _)) return true;

        // RoleIdと タスクで管理する能力を有すか
        // RoleIdが重複するとタスクが配布されず, 非導入者の画面でもTaskInfoが開けなくなる。
        Dictionary<RoleId, bool> taskTriggerAbilityData = new()
        {
            { RoleId.Madmate, MadmateIsCheckImpostor.GetBool() },
            { RoleId.MadMayor, MadMayorIsCheckImpostor.GetBool() },
            { RoleId.MadStuntMan, MadStuntManIsCheckImpostor.GetBool() },
            { RoleId.MadHawk, MadHawkIsCheckImpostor.GetBool() },
            { RoleId.MadSeer, MadSeerIsCheckImpostor.GetBool() },
            { RoleId.MadCleaner, MadCleanerIsCheckImpostor.GetBool() },
            { RoleId.BlackCat, BlackCatIsCheckImpostor.GetBool() },
            { RoleId.JackalFriends, JackalFriendsIsCheckJackal.GetBool() },
            { RoleId.SeerFriends, SeerFriendsIsCheckJackal.GetBool() },
            { RoleId.MayorFriends, MayorFriendsIsCheckJackal.GetBool() },
            { RoleId.Jester, JesterIsWinCleartask.GetBool() },
            { RoleId.MadJester, IsMadJesterTaskClearWin.GetBool() || MadJesterIsCheckImpostor.GetBool() },
            { RoleId.God, GodIsEndTaskWin.GetBool() },
            { RoleId.Worshiper, Worshiper.CustomOptionData.IsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles)},
            { RoleId.Workperson, true },
            { RoleId.TaskManager, true },
            { RoleId.SuicidalIdeation, true },
            { RoleId.Tasker, true },
            { RoleId.HamburgerShop, true },
            { RoleId.Safecracker, true },
            { RoleId.TheFirstLittlePig, true },
            { RoleId.TheSecondLittlePig, true },
            { RoleId.TheThirdLittlePig, true },
            { RoleId.OrientalShaman, OrientalShaman.OrientalShamanWinTask.GetBool() },
            { RoleId.MadRaccoon, MadRaccoon.CustomOptionData.IsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles) },
            { RoleId.BlackSanta, BlackSanta.CanCheckImpostorOption.GetBool() },
        };

        if (taskTriggerAbilityData.ContainsKey(id)) return taskTriggerAbilityData[id];
        else return false;
    }

    /// <summary>
    /// 役職に設定されているタスク数を取得する。
    /// </summary>
    /// <param name="id">取得したい役職のRoleId</param>
    /// <returns>
    /// (int,int,int) : (commonタスク数, shortタスク数, longタスク数)
    /// (固有のタスク設定数の合計が0の場合, バニラのタスク数を返す)
    /// </returns>
    private static (int, int, int) GetRoleTaskData(RoleId id)
    {
        int vanillaCommon = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
        int vanillaShort = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
        int vanillaLong = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);

        Dictionary<RoleId, (int, int, int)> taskData = new()
        {
            { RoleId.Madmate, MadmateIsSettingNumberOfUniqueTasks.GetBool() ? (MadmateCommonTask.GetInt(), MadmateShortTask.GetInt(), MadmateLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadMayor, MadMayorIsSettingNumberOfUniqueTasks.GetBool() ? (MadMayorCommonTask.GetInt(), MadMayorShortTask.GetInt(), MadMayorLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadStuntMan, MadStuntManIsSettingNumberOfUniqueTasks.GetBool() ? (MadStuntManCommonTask.GetInt(), MadStuntManShortTask.GetInt(), MadStuntManLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadHawk, MadHawkIsSettingNumberOfUniqueTasks.GetBool() ? (MadHawkCommonTask.GetInt(), MadHawkShortTask.GetInt(), MadHawkLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadSeer, MadSeerIsSettingNumberOfUniqueTasks.GetBool() ? (MadSeerCommonTask.GetInt(), MadSeerShortTask.GetInt(), MadSeerLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadCleaner, MadCleanerIsSettingNumberOfUniqueTasks.GetBool() ? (MadCleanerCommonTask.GetInt(), MadCleanerShortTask.GetInt(), MadCleanerLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.BlackCat, BlackCatIsSettingNumberOfUniqueTasks.GetBool() ? (BlackCatCommonTask.GetInt(), BlackCatShortTask.GetInt(), BlackCatLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.JackalFriends, JackalFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (JackalFriendsCommonTask.GetInt(), JackalFriendsShortTask.GetInt(), JackalFriendsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.SeerFriends, SeerFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (SeerFriendsCommonTask.GetInt(), SeerFriendsShortTask.GetInt(), SeerFriendsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MayorFriends, MayorFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (MayorFriendsCommonTask.GetInt(), MayorFriendsShortTask.GetInt(), MayorFriendsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.Jester, JesterIsSettingNumberOfUniqueTasks.GetBool() ? (JesterCommonTask.GetInt(), JesterShortTask.GetInt(), JesterLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadJester, MadJesterIsSettingNumberOfUniqueTasks.GetBool() ? (MadJesterCommonTask.GetInt(), MadJesterShortTask.GetInt(), MadJesterLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.God, GodIsSettingNumberOfUniqueTasks.GetBool() ? (GodCommonTask.GetInt(), GodShortTask.GetInt(), GodLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.Worshiper, Worshiper.CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles) ? (Worshiper.CustomOptionData.CommonTask.GetInt(), Worshiper.CustomOptionData.ShortTask.GetInt(), Worshiper.CustomOptionData.LongTask.GetInt())  : (0, 0, 0) },
            { RoleId.Workperson, WorkpersonIsSettingNumberOfUniqueTasks.GetBool() ? (WorkpersonCommonTask.GetInt(), WorkpersonShortTask.GetInt(), WorkpersonLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.TaskManager, (TaskManagerCommonTask.GetInt(), TaskManagerShortTask.GetInt(), TaskManagerLongTask.GetInt())},
            { RoleId.SuicidalIdeation, SuicidalIdeationIsSettingNumberOfUniqueTasks.GetBool() ? (SuicidalIdeationCommonTask.GetInt(), SuicidalIdeationShortTask.GetInt(), SuicidalIdeationLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.Tasker, TaskerIsSettingNumberOfUniqueTasks.GetBool() ? (TaskerCommonTask.GetInt(), TaskerShortTask.GetInt(), TaskerLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.HamburgerShop, HamburgerShopIsSettingNumberOfUniqueTasks.GetBool() ? (HamburgerShopCommonTask.GetInt(), HamburgerShopShortTask.GetInt(), HamburgerShopLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.Safecracker, Safecracker.SafecrackerIsSettingNumberOfUniqueTasks.GetBool() ? (Safecracker.SafecrackerCommonTask.GetInt(), Safecracker.SafecrackerShortTask.GetInt(), Safecracker.SafecrackerLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.TheFirstLittlePig, TheThreeLittlePigs.TheThreeLittlePigsIsSettingNumberOfUniqueTasks.GetBool() ? (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.TheSecondLittlePig, TheThreeLittlePigs.TheThreeLittlePigsIsSettingNumberOfUniqueTasks.GetBool() ? (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.TheThirdLittlePig, TheThreeLittlePigs.TheThreeLittlePigsIsSettingNumberOfUniqueTasks.GetBool() ? (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.OrientalShaman, OrientalShaman.OrientalShamanIsSettingNumberOfUniqueTasks.GetBool() ? (OrientalShaman.OrientalShamanCommonTask.GetInt(), OrientalShaman.OrientalShamanShortTask.GetInt(), OrientalShaman.OrientalShamanLongTask.GetInt()) : (0, 0, 0) },
            { RoleId.MadRaccoon, MadRaccoon.CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles) ? (MadRaccoon.CustomOptionData.CommonTask.GetInt(),MadRaccoon.CustomOptionData.ShortTask.GetInt(), MadRaccoon.CustomOptionData.LongTask.GetInt())  : (0, 0, 0) },
            { RoleId.BlackSanta, BlackSanta.IsSettingNumberOfUniqueTasks.GetBool() ? (BlackSanta.CommonTaskOption.GetInt(), BlackSanta.ShortTaskOption.GetInt(), BlackSanta.LongTaskOption.GetInt()) : (0, 0, 0)}
        };

        //テンプレート
        // { RoleId.[RoleId], [役名]IsSettingNumberOfUniqueTasks.GetBool() ? ([役名]CommonTask.GetInt(), [役名]ShortTask.GetInt(), [役名]LongTask.GetInt()) : (0, 0, 0) },

        // RoleIdが辞書に含まれ、タスクの合計値が0以上の場合
        if (taskData.ContainsKey(id))
            if (!(taskData[id].Item1 == 0 && taskData[id].Item2 == 0 && taskData[id].Item3 == 0))
                return taskData[id];

        // タスク数を (0, 0, 0) で返す必要のある役職
        if (id == RoleId.GM) return (0, 0, 0);
        else if (id == RoleId.None) return (0, 0, 0);

        // RoleIdが辞書に含まれない, あるいは合計値が0だった場合, バニラのタスク数を返す
        return (vanillaCommon, vanillaShort, vanillaLong);
    }
}