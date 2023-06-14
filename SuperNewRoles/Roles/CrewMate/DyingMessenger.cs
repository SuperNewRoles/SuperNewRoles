using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class DyingMessenger
{
    private const int OptionId = 402700;
    public static CustomRoleOption DyingMessengerOption;
    public static CustomOption DyingMessengerPlayerCount;
    public static CustomOption DyingMessengerGetRoleTime;
    public static CustomOption DyingMessengerGetLightAndDarkerTime;
    public static void SetupCustomOptions()
    {
        DyingMessengerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.DyingMessenger);
        DyingMessengerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], DyingMessengerOption);
        DyingMessengerGetRoleTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "DyingMessengerGetRoleTimeSetting", 20f, 0f, 60f, 1f, DyingMessengerOption);
        DyingMessengerGetLightAndDarkerTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "DyingMessengerGetLightAndDarkerTimeSetting", 2f, 0f, 60f, 1f, DyingMessengerOption);
    }

    public static List<PlayerControl> DyingMessengerPlayer;
    public static Color32 color = new(191, 197, 202, byte.MaxValue);
    public static Dictionary<byte, (DateTime, PlayerControl)> ActualDeathTime;
    public static void ClearAndReload()
    {
        DyingMessengerPlayer = new();
        ActualDeathTime = new();
    }
}