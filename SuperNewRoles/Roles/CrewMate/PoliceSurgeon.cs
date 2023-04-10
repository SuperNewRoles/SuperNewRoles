using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Roles.Crewmate;

public static class PoliceSurgeon
{
    private static int optionId = 1266;
    public static CustomRoleOption PoliceSurgeonOption;
    public static CustomOption PoliceSurgeonPlayerCount;
    public static CustomOption PoliceSurgeonHaveVitalsInTaskPhase;
    public static CustomOption PoliceSurgeonVitalsDisplayCooldown;
    public static CustomOption PoliceSurgeonBatteryDuration;
    public static CustomOption PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn;
    public static CustomOption PoliceSurgeonHowManyTurnAgoTheDied;
    public static CustomOption PoliceSurgeonIncludeAnErrorInTheTimeOfDeath;
    public static CustomOption PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath;

    public static void SetupCustomOptions()
    {
        PoliceSurgeonOption = SetupCustomRoleOption(optionId, true, RoleId.PoliceSurgeon); optionId++;
        PoliceSurgeonPlayerCount = Create(optionId, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], PoliceSurgeonOption); optionId++;
        PoliceSurgeonHaveVitalsInTaskPhase = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHaveVitalsInTaskPhase", false, PoliceSurgeonOption); optionId++;
        PoliceSurgeonVitalsDisplayCooldown = Create(optionId, true, CustomOptionType.Crewmate, "VitalsDisplayCooldown", 15f, 5f, 60f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonBatteryDuration = Create(optionId, true, CustomOptionType.Crewmate, "BatteryDuration", 5f, 5f, 30f, 5f, PoliceSurgeonHaveVitalsInTaskPhase); optionId++;
        PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIndicateTimeOfDeathInSubsequentTurn", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonHowManyTurnAgoTheDied = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonHowManyTurnAgoTheDied", false, PoliceSurgeonOption); optionId++;
        PoliceSurgeonIncludeAnErrorInTheTimeOfDeath = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonIncludeAnErrorInTheTimeOfDeath", true, PoliceSurgeonOption); optionId++;
        PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath = Create(optionId, true, CustomOptionType.Crewmate, "PoliceSurgeonMarginOfErrorToIncludeInTimeOfDeath", 5f, 1f, 15f, 1f,  PoliceSurgeonIncludeAnErrorInTheTimeOfDeath);
    }

    public static List<PlayerControl> PoliceSurgeonPlayer;
    public static Color32 color = new(137, 195, 235, byte.MaxValue);
    public static void ClearAndReload()
    {
        PoliceSurgeonPlayer = new();
    }
}