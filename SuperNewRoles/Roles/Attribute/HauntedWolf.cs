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
        public static CustomOption Option;
        public static CustomOption AssignLate;
        public static CustomOption PlayerCount;

        internal static void SetUpCustomRoleOptions()
        {
            Option = Create(optionId, true, CustomOptionType.Crewmate, Cs(RoleClass.Lovers.color, "HauntedWolfName"), false, null, isHeader: true);
            AssignLate = Create(optionId + 1, true, CustomOptionType.Crewmate, "AssignLateSetting", rates, Option);
            PlayerCount = Create(optionId + 2, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option);
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
            if (!CustomOptionData.Option.GetBool() || CustomOptionData.AssignLate.GetSelection() == 0) return;
            if (CustomOptionData.AssignLate.GetSelection() != 10)
            {
                List<string> lottery = new();
                var assignLate = CustomOptionData.AssignLate.GetSelection();
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
                    var playerIndex = ModHelpers.GetRandomIndex(SelectPlayers);
                    var playerControl = SelectPlayers[playerIndex];
                    SelectPlayers.RemoveAt(playerIndex);

                    SetHauntedWolf(playerControl);
                    SetHauntedWolfRPC(playerControl);
                }
            }
            ChacheManager.ResetHauntedWolfChache();
        }

        internal static void SetHauntedWolf(PlayerControl player)
        {
            RoleData.Player.Add(player);
            if (player.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
            }
            ChacheManager.ResetLoversChache();
        }

        internal static void SetHauntedWolfRPC(PlayerControl player)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetHauntedWolf, SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
    }
}