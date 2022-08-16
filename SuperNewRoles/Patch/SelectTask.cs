using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Patch
{
    public static class SelectTask
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.RpcSetTasks))]
        class RpcSetTasksPatch
        {
            public static void Prefix(
            [HarmonyArgument(0)] byte playerId,
            [HarmonyArgument(1)] ref UnhollowerBaseLib.Il2CppStructArray<byte> taskTypeIds)
            {
                if (GameData.Instance.GetPlayerById(playerId).Object.IsBot() || taskTypeIds.Length == 0)
                {
                    taskTypeIds = new byte[0];
                    return;
                }
                if (ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.Default) && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                {
                    var (commont, shortt, longt) = GameData.Instance.GetPlayerById(playerId).Object.GetTaskCount();
                    var TasksList = ModHelpers.GenerateTasks(commont, shortt, longt);
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
            if (p.IsRole(RoleId.MadMate))
            {
                if (CustomOptions.MadMateIsCheckImpostor.GetBool())
                {
                    int commont = CustomOptions.MadMateCommonTask.GetInt();
                    int shortt = CustomOptions.MadMateShortTask.GetInt();
                    int longt = CustomOptions.MadMateLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.MadMayor))
            {
                if (CustomOptions.MadMayorIsCheckImpostor.GetBool())
                {
                    int commont = CustomOptions.MadMayorCommonTask.GetInt();
                    int shortt = CustomOptions.MadMayorShortTask.GetInt();
                    int longt = CustomOptions.MadMayorLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.MadSeer))
            {
                if (CustomOptions.MadSeerIsCheckImpostor.GetBool())
                {
                    int commont = CustomOptions.MadSeerCommonTask.GetInt();
                    int shortt = CustomOptions.MadSeerShortTask.GetInt();
                    int longt = CustomOptions.MadSeerLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.BlackCat))
            {
                if (CustomOptions.BlackCatIsCheckImpostor.GetBool())
                {
                    int commont = CustomOptions.BlackCatCommonTask.GetInt();
                    int shortt = CustomOptions.BlackCatShortTask.GetInt();
                    int longt = CustomOptions.BlackCatLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.JackalFriends))
            {
                if (CustomOptions.JackalFriendsIsCheckJackal.GetBool())
                {
                    int commont = CustomOptions.JackalFriendsCommonTask.GetInt();
                    int shortt = CustomOptions.JackalFriendsShortTask.GetInt();
                    int longt = CustomOptions.JackalFriendsLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.SeerFriends))
            {
                if (CustomOptions.SeerFriendsIsCheckJackal.GetBool())
                {
                    int commont = CustomOptions.SeerFriendsCommonTask.GetInt();
                    int shortt = CustomOptions.SeerFriendsShortTask.GetInt();
                    int longt = CustomOptions.SeerFriendsLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.MayorFriends))
            {
                if (CustomOptions.MayorFriendsIsCheckJackal.GetBool())
                {
                    int commont = CustomOptions.MayorFriendsCommonTask.GetInt();
                    int shortt = CustomOptions.MayorFriendsShortTask.GetInt();
                    int longt = CustomOptions.MayorFriendsLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.Jester))
            {
                if (CustomOptions.JesterIsWinCleartask.GetBool())
                {
                    int commont = CustomOptions.JesterCommonTask.GetInt();
                    int shortt = CustomOptions.JesterShortTask.GetInt();
                    int longt = CustomOptions.JesterLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.MadJester))
            {
                if (CustomOptions.IsMadJesterTaskClearWin.GetBool())
                {
                    int commont = CustomOptions.MadJesterCommonTask.GetInt();
                    int shortt = CustomOptions.MadJesterShortTask.GetInt();
                    int longt = CustomOptions.MadJesterLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.God))
            {
                if (CustomOptions.GodIsEndTaskWin.GetBool())
                {
                    int commont = CustomOptions.GodCommonTask.GetInt();
                    int shortt = CustomOptions.GodShortTask.GetInt();
                    int longt = CustomOptions.GodLongTask.GetInt();
                    if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
                }
            }
            else if (p.IsRole(RoleId.Workperson))
            {
                int commont = CustomOptions.WorkpersonCommonTask.GetInt();
                int shortt = CustomOptions.WorkpersonShortTask.GetInt();
                int longt = CustomOptions.WorkpersonLongTask.GetInt();
                if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
            }
            else if (p.IsRole(RoleId.TaskManager))
            {
                int commont = CustomOptions.TaskManagerCommonTask.GetInt();
                int shortt = CustomOptions.TaskManagerShortTask.GetInt();
                int longt = CustomOptions.TaskManagerLongTask.GetInt();
                if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
            }
            else if (p.IsRole(RoleId.SuicidalIdeation))
            {
                int commont = CustomOptions.SuicidalIdeationCommonTask.GetInt();
                int longt = CustomOptions.SuicidalIdeationLongTask.GetInt();
                int shortt = CustomOptions.SuicidalIdeationShortTask.GetInt();
                if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
            }
            else if (p.IsLovers() && !p.IsImpostor())
            {
                int commont = CustomOptions.LoversCommonTask.GetInt();
                int shortt = CustomOptions.LoversShortTask.GetInt();
                int longt = CustomOptions.LoversLongTask.GetInt();
                if (!(commont == 0 && shortt == 0 && longt == 0)) return (commont, shortt, longt);
            }
            return (SyncSetting.OptionData.NumCommonTasks, SyncSetting.OptionData.NumShortTasks, SyncSetting.OptionData.NumLongTasks);
        }
        public static (CustomOption.CustomOption, CustomOption.CustomOption, CustomOption.CustomOption) TaskSetting(int commonid, int shortid, int longid, CustomOption.CustomOption Child = null, CustomOptionType type = CustomOptionType.Generic, bool IsSHROn = false)
        {
            CustomOption.CustomOption CommonOption = CustomOption.CustomOption.Create(commonid, IsSHROn, type, "GameCommonTasks", 1, 0, 12, 1, Child);
            CustomOption.CustomOption ShortOption = CustomOption.CustomOption.Create(shortid, IsSHROn, type, "GameShortTasks", 1, 0, 69, 1, Child);
            CustomOption.CustomOption LongOption = CustomOption.CustomOption.Create(longid, IsSHROn, type, "GameLongTasks", 1, 0, 45, 1, Child);
            return (CommonOption, ShortOption, LongOption);
        }
    }
}