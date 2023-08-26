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
        const int optionId = 405600;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        internal static void SetUpCustomRoleOptions()
        {
            Option = SetupCustomRoleOption(optionId, true, RoleId.HauntedWolf);
            PlayerCount = Create(optionId + 1, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(50, 0, 25, byte.MaxValue);
        public static void ClearAndReload()
        {
            Player = new();
        }
    }

    internal static class Assign
    {
        internal static void RandomSelect()
        {
            if (!CustomOptionData.Option.GetBool()) return;
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsCrew() && !p.IsBot()) SelectPlayers.Add(p);
            }
            for (int i = 0; i < CustomOptionData.PlayerCount.GetFloat(); i++)
            {
                if (SelectPlayers.Count is not (1 or 0))
                {
                    List<PlayerControl> listData = new();
                    for (int i2 = 0; i2 < 2; i2++)
                    {
                        var player = ModHelpers.GetRandomIndex(SelectPlayers);
                        listData.Add(SelectPlayers[player]);
                        SelectPlayers.RemoveAt(player);
                    }
                    RoleHelpers.SetHauntedWolf(listData[0], listData[1]);
                    RoleHelpers.SetHauntedWolfRPC(listData[0], listData[1]);
                }
            }
            ChacheManager.ResetHauntedWolfChache();
        }
    }
}