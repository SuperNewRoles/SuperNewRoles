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
        if (MadmateIsCheckImpostor.GetBool()) taskData.Add(RoleId.Madmate, MadmateIsSettingNumberOfUniqueTasks.GetBool() ? (MadmateCommonTask.GetInt(), MadmateShortTask.GetInt(), MadmateLongTask.GetInt()) : (0, 0, 0));
        if (MadMayorIsCheckImpostor.GetBool()) taskData.Add(RoleId.MadMayor, MadMayorIsSettingNumberOfUniqueTasks.GetBool() ? (MadMayorCommonTask.GetInt(), MadMayorShortTask.GetInt(), MadMayorLongTask.GetInt()) : (0, 0, 0));
        if (MadSeerIsCheckImpostor.GetBool()) taskData.Add(RoleId.MadSeer, MadSeerIsSettingNumberOfUniqueTasks.GetBool() ? (MadSeerCommonTask.GetInt(), MadSeerShortTask.GetInt(), MadSeerLongTask.GetInt()) : (0, 0, 0));
        if (BlackCatIsCheckImpostor.GetBool()) taskData.Add(RoleId.BlackCat, BlackCatIsSettingNumberOfUniqueTasks.GetBool() ? (BlackCatCommonTask.GetInt(), BlackCatShortTask.GetInt(), BlackCatLongTask.GetInt()) : (0, 0, 0));
        if (JackalFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.JackalFriends, JackalFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (JackalFriendsCommonTask.GetInt(), JackalFriendsShortTask.GetInt(), JackalFriendsLongTask.GetInt()) : (0, 0, 0));
        if (SeerFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.SeerFriends, SeerFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (SeerFriendsCommonTask.GetInt(), SeerFriendsShortTask.GetInt(), SeerFriendsLongTask.GetInt()) : (0, 0, 0));
        if (MayorFriendsIsCheckJackal.GetBool()) taskData.Add(RoleId.MayorFriends, MayorFriendsIsSettingNumberOfUniqueTasks.GetBool() ? (MayorFriendsCommonTask.GetInt(), MayorFriendsShortTask.GetInt(), MayorFriendsLongTask.GetInt()) : (0, 0, 0));
        if (JesterIsWinCleartask.GetBool()) taskData.Add(RoleId.Jester, (JesterCommonTask.GetInt(), JesterShortTask.GetInt(), JesterLongTask.GetInt()));
        if (IsMadJesterTaskClearWin.GetBool() || MadJesterIsCheckImpostor.GetBool()) taskData.Add(RoleId.MadJester, (MadJesterCommonTask.GetInt(), MadJesterShortTask.GetInt(), MadJesterLongTask.GetInt()));
        if (GodIsEndTaskWin.GetBool()) taskData.Add(RoleId.God, (GodCommonTask.GetInt(), GodShortTask.GetInt(), GodLongTask.GetInt()));
        if (Worshiper.CustomOptionData.IsCheckImpostor.GetBool() && !ModeHandler.IsMode(ModeId.SuperHostRoles)) taskData.Add(RoleId.Worshiper, Worshiper.CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool() ? (Worshiper.CustomOptionData.CommonTask.GetInt(), Worshiper.CustomOptionData.ShortTask.GetInt(), Worshiper.CustomOptionData.LongTask.GetInt()) : (0, 0, 0));
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

    internal static int GetTotalTasks(RoleId roleId, int abilityPattern = 0)
    {
        int vanillaCommon = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
        int vanillaLong = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
        int vanillaShort = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
        int vanillaTotalTasks = vanillaCommon + vanillaLong + vanillaShort;

        int roleCommon = 0, roleLong = 0, roleShort = 0, roleTotalTasks = 0;

        switch (roleId)
        {
            case RoleId.Madmate:
                if (MadmateIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = MadmateCommonTask.GetInt();
                    roleLong = MadmateLongTask.GetInt();
                    roleShort = MadmateShortTask.GetInt();
                }
                break;
            case RoleId.MadMayor:
                if (MadMayorIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = MadMayorCommonTask.GetInt();
                    roleLong = MadMayorLongTask.GetInt();
                    roleShort = MadMayorShortTask.GetInt();
                }
                break;
            case RoleId.MadSeer:
                if (MadSeerIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = MadSeerCommonTask.GetInt();
                    roleLong = MadSeerLongTask.GetInt();
                    roleShort = MadSeerShortTask.GetInt();
                }
                break;
            case RoleId.BlackCat:
                if (BlackCatIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = BlackCatCommonTask.GetInt();
                    roleLong = BlackCatLongTask.GetInt();
                    roleShort = BlackCatShortTask.GetInt();
                }
                break;
            case RoleId.MadJester:
                if (MadJesterIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = MadJesterCommonTask.GetInt();
                    roleLong = MadJesterLongTask.GetInt();
                    roleShort = MadJesterShortTask.GetInt();
                }
                break;
            case RoleId.Worshiper:
                if (Worshiper.CustomOptionData.IsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = Worshiper.CustomOptionData.CommonTask.GetInt();
                    roleLong = Worshiper.CustomOptionData.LongTask.GetInt();
                    roleShort = Worshiper.CustomOptionData.ShortTask.GetInt();
                }
                break;
            case RoleId.JackalFriends:
                if (JackalFriendsIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = JackalFriendsCommonTask.GetInt();
                    roleLong = JackalFriendsLongTask.GetInt();
                    roleShort = JackalFriendsShortTask.GetInt();
                }
                break;
            case RoleId.SeerFriends:
                if (SeerFriendsIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = SeerFriendsCommonTask.GetInt();
                    roleLong = SeerFriendsLongTask.GetInt();
                    roleShort = SeerFriendsShortTask.GetInt();
                }
                break;
            case RoleId.MayorFriends:
                if (MayorFriendsIsSettingNumberOfUniqueTasks.GetBool())
                {
                    roleCommon = MayorFriendsCommonTask.GetInt();
                    roleLong = MayorFriendsLongTask.GetInt();
                    roleShort = MayorFriendsShortTask.GetInt();
                }
                break;
            default:
                // RoleIdが存在しないエラー扱い.
                // 固有タスク数設定の合計が0になった時, あるいは設定がオフの時, バニラから取得するようにしている為 通常は0になることはないから。
                return 0;
        }

        roleTotalTasks = roleCommon + roleLong + roleShort;
        if (roleTotalTasks == 0) roleTotalTasks = vanillaTotalTasks;

        return roleTotalTasks;
    }
}