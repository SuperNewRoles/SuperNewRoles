using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;

using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Patches;

public static class SelectTask
{
    [HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
    class RpcSetTasksPatch
    {
        public static void Prefix(
            GameData __instance,
            [HarmonyArgument(0)] byte playerId,
            [HarmonyArgument(1)] ref UnhollowerBaseLib.Il2CppStructArray<byte> taskTypeIds)
        {
            if (GameData.Instance.GetPlayerById(playerId).Object.IsBot() || taskTypeIds.Length == 0)
            {
                taskTypeIds = new byte[0];
                return;
            }
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.Default, ModeId.CopsRobbers) && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
            {
                var (commont, shortt, longt) = GameData.Instance.GetPlayerById(playerId).Object.GetTaskCount();
                var TasksList = ModHelpers.GenerateTasks(__instance.GetPlayerById(playerId).Object, commont, shortt, longt);
                taskTypeIds = new UnhollowerBaseLib.Il2CppStructArray<byte>(TasksList.Count);
                for (int i = 0; i < TasksList.Count; i++)
                {
                    taskTypeIds[i] = TasksList[i];
                }
            }
        }
    }
    public static (int, int, int) GetTaskCount(this PlayerControl p)
    {
        Dictionary<RoleId, (int, int, int)> taskData = new();
        if (MadmateCheckImpostorTask.GetBool()) taskData.Add(RoleId.Madmate, (MadmateCommonTask.GetInt(), MadmateShortTask.GetInt(), MadmateLongTask.GetInt()));
        if (MadMayorIsCheckImpostor.GetBool()) taskData.Add(RoleId.MadMayor, (MadMayorCommonTask.GetInt(), MadMayorShortTask.GetInt(), MadMayorLongTask.GetInt()));
        if (MadSeerIsCheckImpostor.GetBool()) taskData.Add(RoleId.MadSeer, (MadSeerCommonTask.GetInt(), MadSeerShortTask.GetInt(), MadSeerLongTask.GetInt()));
        if (BlackCatIsCheckImpostor.GetBool()) taskData.Add(RoleId.BlackCat, (BlackCatCommonTask.GetInt(), BlackCatShortTask.GetInt(), BlackCatLongTask.GetInt()));
        if (JackalFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.JackalFriends, (JackalFriendsCommonTask.GetInt(), JackalFriendsShortTask.GetInt(), JackalFriendsLongTask.GetInt()));
        if (SeerFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.SeerFriends, (SeerFriendsCommonTask.GetInt(), SeerFriendsShortTask.GetInt(), SeerFriendsLongTask.GetInt()));
        if (MayorFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.MayorFriends, (MayorFriendsCommonTask.GetInt(), MayorFriendsShortTask.GetInt(), MayorFriendsLongTask.GetInt()));
        if (JesterIsWinCleartask.GetBool()) taskData.Add(RoleId.Jester, (JesterCommonTask.GetInt(), JesterShortTask.GetInt(), JesterLongTask.GetInt()));
        if (IsMadJesterTaskClearWin.GetBool() || MadJesterCheckImpostorTask.GetBool()) taskData.Add(RoleId.MadJester, (MadJesterCommonTask.GetInt(), MadJesterShortTask.GetInt(), MadJesterLongTask.GetInt()));
        if (GodIsEndTaskWin.GetBool()) taskData.Add(RoleId.God, (GodCommonTask.GetInt(), GodShortTask.GetInt(), GodLongTask.GetInt()));
        if (Worshiper.WorshiperIsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles)) taskData.Add(RoleId.Worshiper, (Worshiper.WorshiperCommonTask.GetInt(), Worshiper.WorshiperShortTask.GetInt(), Worshiper.WorshiperLongTask.GetInt()));
        taskData.Add(RoleId.Workperson, (WorkpersonCommonTask.GetInt(), WorkpersonShortTask.GetInt(), WorkpersonLongTask.GetInt()));
        taskData.Add(RoleId.TaskManager, (TaskManagerCommonTask.GetInt(), TaskManagerShortTask.GetInt(), TaskManagerLongTask.GetInt()));
        taskData.Add(RoleId.SuicidalIdeation, (SuicidalIdeationCommonTask.GetInt(), SuicidalIdeationShortTask.GetInt(), SuicidalIdeationLongTask.GetInt()));
        taskData.Add(RoleId.Tasker, (TaskerCommonTask.GetInt(), TaskerShortTask.GetInt(), TaskerLongTask.GetInt()));
        taskData.Add(RoleId.HamburgerShop, (HamburgerShopCommonTask.GetInt(), HamburgerShopShortTask.GetInt(), HamburgerShopLongTask.GetInt()));
        taskData.Add(RoleId.Safecracker, (Safecracker.SafecrackerCommonTask.GetInt(), Safecracker.SafecrackerShortTask.GetInt(), Safecracker.SafecrackerLongTask.GetInt()));
        if (TheThreeLittlePigs.TheThreeLittlePigsTask.GetBool())
        {
            taskData.Add(RoleId.TheFirstLittlePig, (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()));
            taskData.Add(RoleId.TheSecondLittlePig, (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()));
            taskData.Add(RoleId.TheThirdLittlePig, (TheThreeLittlePigs.TheThreeLittlePigsCommonTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsShortTask.GetInt(), TheThreeLittlePigs.TheThreeLittlePigsLongTask.GetInt()));
        }
        if (OrientalShaman.OrientalShamanWinTask.GetBool()) taskData.Add(RoleId.OrientalShaman, (OrientalShaman.OrientalShamanCommonTask.GetInt(), OrientalShaman.OrientalShamanShortTask.GetInt(), OrientalShaman.OrientalShamanLongTask.GetInt()));

        //テンプレート
        //taskData.Add(RoleId, (CommonTask.GetInt(), ShortTask.GetInt(), LongTask.GetInt()));

        if (taskData.ContainsKey(p.GetRole())) // pの役職がDictionaryにあるか
        {
            if (taskData[p.GetRole()] != (0, 0, 0)) // pの役職をKeyでValueを取得。が(0,0,0)ではない
                return taskData[p.GetRole()];
        }
        else if (p.IsLovers() && !p.IsImpostor())
        {
            int commont = LoversCommonTask.GetInt();
            int shortt = LoversShortTask.GetInt();
            int longt = LoversLongTask.GetInt();
            if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
        }
        else if (p.IsRole(RoleId.GM))
        {
            return (0, 0, 0);
        }
        return (SyncSetting.OptionData.GetInt(Int32OptionNames.NumCommonTasks), SyncSetting.OptionData.GetInt(Int32OptionNames.NumShortTasks), SyncSetting.OptionData.GetInt(Int32OptionNames.NumLongTasks));
    }
    public static (CustomOption, CustomOption, CustomOption) TaskSetting(int commonid, int shortid, int longid, CustomOption Child = null, CustomOptionType type = CustomOptionType.Generic, bool IsSHROn = false)
    {
        CustomOption CommonOption = CustomOption.Create(commonid, IsSHROn, type, "GameCommonTasks", 1, 0, 12, 1, Child);
        CustomOption ShortOption = CustomOption.Create(shortid, IsSHROn, type, "GameShortTasks", 1, 0, 69, 1, Child);
        CustomOption LongOption = CustomOption.Create(longid, IsSHROn, type, "GameLongTasks", 1, 0, 45, 1, Child);
        return (CommonOption, ShortOption, LongOption);
    }
}