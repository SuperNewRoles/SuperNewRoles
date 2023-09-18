using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Pokerface
{
    public static class CustomOptionData
    {
        private static int optionId = 303500;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Pokerface); optionId++;
            PlayerCount = CustomOption.Create(optionId, true, CustomOptionType.Neutral, "SettingTeamCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
        }
    }

    public class PokerfaceTeam
    {
        public readonly PlayerControl[] TeamPlayers = new PlayerControl[3];
        public PokerfaceTeam(PlayerControl player1,PlayerControl player2, PlayerControl player3)
        {
            TeamPlayers[0] = player1;
            TeamPlayers[1] = player2;
            TeamPlayers[2] = player3;
        }
        public bool CanWin()
        {
            byte i = 0;
            foreach (PlayerControl player in TeamPlayers)
            {
                if (i == 255 && player.IsAlive())
                    return false;
                else if (i == 2 && player.IsAlive())
                    return true;
                else if (player.IsAlive())
                    i = 255;
                else if (i != 255)
                    i++;
            }
            return i == 255;
        }
    }
    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static List<PokerfaceTeam> PokerfaceTeams;
        public static Color32 color = new(114, 209, 107, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
        }
    }


    // ここにコードを書きこんでください
}