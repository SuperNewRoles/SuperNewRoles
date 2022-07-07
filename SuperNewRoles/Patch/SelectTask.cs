using System;
using System.Collections.Generic;
using System.Text;
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
                if (ModeHandler.isMode(ModeId.SuperHostRoles, ModeId.Default) && AmongUsClient.Instance.GameMode != GameModes.FreePlay))
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
                if (CustomOptions.MadMateIsCheckImpostor.getBool())
                {
                    int commont = (int)CustomOptions.MadMateCommonTask.getFloat();
                    int shortt = (int)CustomOptions.MadMateShortTask.getFloat();
                    int longt = (int)CustomOptions.MadMateLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MadMayor))
            {
                if (CustomOptions.MadMayorIsCheckImpostor.getBool())
                {
                    int commont = (int)CustomOptions.MadMayorCommonTask.getFloat();
                    int shortt = (int)CustomOptions.MadMayorShortTask.getFloat();
                    int longt = (int)CustomOptions.MadMayorLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MadSeer))
            {
                if (CustomOptions.MadSeerIsCheckImpostor.getBool())
                {
                    int commont = (int)CustomOptions.MadSeerCommonTask.getFloat();
                    int shortt = (int)CustomOptions.MadSeerShortTask.getFloat();
                    int longt = (int)CustomOptions.MadSeerLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.BlackCat))
            {
                if (CustomOptions.BlackCatIsCheckImpostor.getBool())
                {
                    int commont = (int)CustomOptions.BlackCatCommonTask.getFloat();
                    int shortt = (int)CustomOptions.BlackCatShortTask.getFloat();
                    int longt = (int)CustomOptions.BlackCatLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.JackalFriends))
            {
                if (CustomOptions.JackalFriendsIsCheckJackal.getBool())
                {
                    int commont = (int)CustomOptions.JackalFriendsCommonTask.getFloat();
                    int shortt = (int)CustomOptions.JackalFriendsShortTask.getFloat();
                    int longt = (int)CustomOptions.JackalFriendsLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.SeerFriends))
            {
                if (CustomOptions.SeerFriendsIsCheckJackal.getBool())
                {
                    int commont = (int)CustomOptions.SeerFriendsCommonTask.getFloat();
                    int shortt = (int)CustomOptions.SeerFriendsShortTask.getFloat();
                    int longt = (int)CustomOptions.SeerFriendsLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.MayorFriends))
            {
                if (CustomOptions.MayorFriendsIsCheckJackal.getBool())
                {
                    int commont = (int)CustomOptions.MayorFriendsCommonTask.getFloat();
                    int shortt = (int)CustomOptions.MayorFriendsShortTask.getFloat();
                    int longt = (int)CustomOptions.MayorFriendsLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.Jester))
            {
                if (CustomOptions.JesterIsWinCleartask.getBool())
                {
                    int commont = (int)CustomOptions.JesterCommonTask.getFloat();
                    int shortt = (int)CustomOptions.JesterShortTask.getFloat();
                    int longt = (int)CustomOptions.JesterLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.God))
            {
                if (CustomOptions.GodIsEndTaskWin.getBool())
                {
                    int commont = (int)CustomOptions.GodCommonTask.getFloat();
                    int shortt = (int)CustomOptions.GodShortTask.getFloat();
                    int longt = (int)CustomOptions.GodLongTask.getFloat();
                    if (!(commont == 0 && shortt == 0 && longt == 0))
                    {
                        return (commont, shortt, longt);
                    }
                }
            }
            else if (p.isRole(RoleId.Workperson))
            {
                int commont = (int)CustomOptions.WorkpersonCommonTask.getFloat();
                int shortt = (int)CustomOptions.WorkpersonShortTask.getFloat();
                int longt = (int)CustomOptions.WorkpersonLongTask.getFloat();
                if (!(commont == 0 && shortt == 0 && longt == 0))
                {
                    return (commont, shortt, longt);
                }
            }
            else if (p.isRole(RoleId.TaskManager))
            {
                int commont = (int)CustomOptions.TaskManagerCommonTask.getFloat();
                int shortt = (int)CustomOptions.TaskManagerShortTask.getFloat();
                int longt = (int)CustomOptions.TaskManagerLongTask.getFloat();
                if (!(commont == 0 && shortt == 0 && longt == 0))
                {
                    return (commont, shortt, longt);
                }
            }
            else if (p.IsLovers() && !p.isImpostor())
            {
                int commont = (int)CustomOptions.LoversCommonTask.getFloat();
                int shortt = (int)CustomOptions.LoversShortTask.getFloat();
                int longt = (int)CustomOptions.LoversLongTask.getFloat();
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
