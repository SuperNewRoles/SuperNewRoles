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
}