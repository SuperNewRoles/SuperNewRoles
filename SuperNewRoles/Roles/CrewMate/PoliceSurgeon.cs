using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class PoliceSurgeon
{
    private static int optionId = 1266;
    public static CustomRoleOption PoliceSurgeonOption;
    public static CustomOption PoliceSurgeonPlayerCount;
    public static void SetupCustomOptions()
    {
        PoliceSurgeonOption = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.PoliceSurgeon); optionId++;
        PoliceSurgeonPlayerCount = CustomOption.Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PoliceSurgeonOption); optionId++;
    }

    public static List<PlayerControl> PoliceSurgeonPlayer;
    public static Color32 color = new(137, 195, 235, byte.MaxValue);
    public static void ClearAndReload()
    {
        PoliceSurgeonPlayer = new();
    }
}