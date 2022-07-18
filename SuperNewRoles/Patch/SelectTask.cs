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
            public static void Prefix(GameData __instance,
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
                    var TasksList = ModHelpers.generateTasks(commont, shortt, longt);
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
            if (p.isRole(RoleId.MadMate))
            {
                if (CustomOptions.MadMateIsCheckImpostor.GetBool())
                {
                    int commont = (int)CustomOptions.MadMateCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.MadMateShortTask.GetFloat();
                    int longt = (int)CustomOptions.MadMateLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MadMayor))
            {
                if (CustomOptions.MadMayorIsCheckImpostor.GetBool())
                {
                    int commont = (int)CustomOptions.MadMayorCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.MadMayorShortTask.GetFloat();
                    int longt = (int)CustomOptions.MadMayorLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MadSeer))
            {
                if (CustomOptions.MadSeerIsCheckImpostor.GetBool())
                {
                    int commont = (int)CustomOptions.MadSeerCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.MadSeerShortTask.GetFloat();
                    int longt = (int)CustomOptions.MadSeerLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.BlackCat))
            {
                if (CustomOptions.BlackCatIsCheckImpostor.GetBool())
                {
                    int commont = (int)CustomOptions.BlackCatCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.BlackCatShortTask.GetFloat();
                    int longt = (int)CustomOptions.BlackCatLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.JackalFriends))
            {
                if (CustomOptions.JackalFriendsIsCheckJackal.GetBool())
                {
                    int commont = (int)CustomOptions.JackalFriendsCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.JackalFriendsShortTask.GetFloat();
                    int longt = (int)CustomOptions.JackalFriendsLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.SeerFriends))
            {
                if (CustomOptions.SeerFriendsIsCheckJackal.GetBool())
                {
                    int commont = (int)CustomOptions.SeerFriendsCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.SeerFriendsShortTask.GetFloat();
                    int longt = (int)CustomOptions.SeerFriendsLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MayorFriends))
            {
                if (CustomOptions.MayorFriendsIsCheckJackal.GetBool())
                {
                    int commont = (int)CustomOptions.MayorFriendsCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.MayorFriendsShortTask.GetFloat();
                    int longt = (int)CustomOptions.MayorFriendsLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.Jester))
            {
                if (CustomOptions.JesterIsWinCleartask.GetBool())
                {
                    int commont = (int)CustomOptions.JesterCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.JesterShortTask.GetFloat();
                    int longt = (int)CustomOptions.JesterLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MadJester))
            {
                if (CustomOptions.IsMadJesterTaskClearWin.GetBool())
                {
                    int commont = (int)CustomOptions.MadJesterCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.MadJesterShortTask.GetFloat();
                    int longt = (int)CustomOptions.MadJesterLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.God))
            {
                if (CustomOptions.GodIsEndTaskWin.GetBool())
                {
                    int commont = (int)CustomOptions.GodCommonTask.GetFloat();
                    int shortt = (int)CustomOptions.GodShortTask.GetFloat();
                    int longt = (int)CustomOptions.GodLongTask.GetFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.Workperson))
            {
                int commont = (int)CustomOptions.WorkpersonCommonTask.GetFloat();
                int shortt = (int)CustomOptions.WorkpersonShortTask.GetFloat();
                int longt = (int)CustomOptions.WorkpersonLongTask.GetFloat();
                if (!(commont == 0 && shortt == 0 && longt == 0))
                {
                    return (commont, shortt, longt);
                }
            }
            else if (p.isRole(RoleId.TaskManager))
            {
                int commont = (int)CustomOptions.TaskManagerCommonTask.GetFloat();
                int shortt = (int)CustomOptions.TaskManagerShortTask.GetFloat();
                int longt = (int)CustomOptions.TaskManagerLongTask.GetFloat();
                if (!(commont == 0 && shortt == 0 && longt == 0))
                {
                    return (commont, shortt, longt);
                }
            }
            else if (p.IsLovers() && !p.isImpostor())
            {
                int commont = (int)CustomOptions.LoversCommonTask.GetFloat();
                int shortt = (int)CustomOptions.LoversShortTask.GetFloat();
                int longt = (int)CustomOptions.LoversLongTask.GetFloat();
                if (!(commont == 0 && shortt == 0 && longt == 0))
                {
                    return (commont, shortt, longt);
                }
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