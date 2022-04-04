using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;

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
                if (ModeHandler.isMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                {
                    PlayerControl.GameOptions.NumCommonTasks = 100;
                    PlayerControl.GameOptions.NumShortTasks = 100;
                    PlayerControl.GameOptions.NumLongTasks = 100;
                    var (commont, shortt, longt) = GameData.Instance.GetPlayerById(playerId).Object.GetTaskCount();
                    var TasksList = ModHelpers.generateTasks(commont, shortt, longt);
                    taskTypeIds = new UnhollowerBaseLib.Il2CppStructArray<byte>(TasksList.Count);
                    for (int i = 0; i < TasksList.Count; i++)
                    {
                        taskTypeIds[i] = TasksList[i];
                    }
                    PlayerControl.GameOptions.NumCommonTasks = SyncSetting.OptionData.NumCommonTasks;
                    PlayerControl.GameOptions.NumShortTasks = SyncSetting.OptionData.NumShortTasks;
                    PlayerControl.GameOptions.NumLongTasks = SyncSetting.OptionData.NumLongTasks;
                }
            }
        }
        public static (int,int,int) GetTaskCount(this PlayerControl p)
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
            } else if (p.isRole(RoleId.Jester))
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
            } else if (p.isRole(RoleId.God))
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
            } else if (p.isRole(RoleId.Workperson))
            {
                int commont = (int)CustomOptions.WorkpersonCommonTask.getFloat();
                int shortt = (int)CustomOptions.WorkpersonShortTask.getFloat();
                int longt = (int)CustomOptions.WorkpersonLongTask.getFloat();
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
        public static (CustomOption.CustomOption, CustomOption.CustomOption, CustomOption.CustomOption) TaskSetting(int commonid,int shortid,int longid,CustomOption.CustomOption Child = null)
        {
            CustomOption.CustomOption CommonOption = CustomOption.CustomOption.Create(commonid, ModTranslation.getString("GameCommonTasks"), 1, 0, 12, 1, Child);
            CustomOption.CustomOption ShortOption = CustomOption.CustomOption.Create(shortid, ModTranslation.getString("GameShortTasks"), 1, 0, 69, 1, Child); ;
            CustomOption.CustomOption LongOption = CustomOption.CustomOption.Create(longid, ModTranslation.getString("GameLongTasks"), 1, 0, 45, 1, Child);
            return (CommonOption, ShortOption, LongOption);
        }
    }
}
