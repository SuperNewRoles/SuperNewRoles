using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Roles.Attribute;
class HauntedWolf
{
    internal static class CustomOptionData
    {
        const int optionId = 500400;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption IsAssignMadAndFriendRoles;
        public static CustomOption IsReverseSheriffDecision;
        public static CustomOption IsNotDuplication;

        internal static void SetUpCustomRoleOptions()
        {
            Option = SetupCustomRoleOption(optionId, true, RoleId.HauntedWolf, CustomOptionType.Modifier);
            PlayerCount = Create(optionId + 1, true, CustomOptionType.Modifier, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option);
            IsAssignMadAndFriendRoles = Create(optionId + 2, true, CustomOptionType.Modifier, "HauntedWolfIsAssignMadAndFriendRoles", true, Option);
            IsReverseSheriffDecision = Create(optionId + 3, true, CustomOptionType.Modifier, "HauntedWolfIsReverseSheriffDecision", true, Option);
            IsNotDuplication = Create(optionId + 4, true, CustomOptionType.Modifier, "HauntedWolfIsNotDuplication", false, Option);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(123, 108, 62, byte.MaxValue);
        public static void ClearAndReload()
        {
            Player = new();
        }
    }

    internal static class Assign
    {
        internal static void RandomSelect()
        {
            if (CustomOptionData.Option.GetSelection() == 0) return;
            if (CustomOptionData.Option.GetSelection() != 10)
            {
                List<string> lottery = new();
                var assignLate = CustomOptionData.Option.GetSelection();
                for (int i = 0; i < assignLate; i++)
                {
                    lottery.Add("Suc");
                }
                for (int i = 0; i < 10 - assignLate; i++)
                {
                    lottery.Add("No");
                }
                if (ModHelpers.GetRandom(lottery) == "No")
                {
                    return;
                }
            }
            List<PlayerControl> SelectPlayers = new();
            var isNotDuplication = CustomOptionData.IsNotDuplication.GetBool();
            var isAssignMadAndFriendRoles = CustomOptionData.IsAssignMadAndFriendRoles.GetBool();
            foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            {
                if (!p.IsCrew() || p.IsBot()) continue;
                if (isNotDuplication) // 重複して配布しない時
                {
                    if (p.GetRole() != RoleId.DefaultRole) continue;
                }
                if (isAssignMadAndFriendRoles && (p.IsMadRoles() || p.IsFriendRoles())) continue;
                SelectPlayers.Add(p);
            }
            for (int i = 0; i < CustomOptionData.PlayerCount.GetFloat(); i++)
            {
                if (SelectPlayers.Count != 0)
                {
                    List<PlayerControl> listData = new();
                    var playerIndex = ModHelpers.GetRandomIndex(SelectPlayers);
                    var playerControl = SelectPlayers[playerIndex];
                    SelectPlayers.RemoveAt(playerIndex);

                    SetHauntedWolf(playerControl);
                    SetHauntedWolfRPC(playerControl);
                }
            }
            CacheManager.ResetHauntedWolfCache();
        }

        internal static void SetHauntedWolf(PlayerControl player)
        {
            Logger.Info($"{player.name} を 狼憑きにします。");
            RoleData.Player.Add(player);
            CacheManager.ResetHauntedWolfCache();
        }

        internal static void SetHauntedWolfRPC(PlayerControl player)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetHauntedWolf, SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
    }
}