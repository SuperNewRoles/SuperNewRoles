using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class OrientalShaman
{
    private const int OptionId = 1210;
    public static CustomRoleOption OrientalShamanOption;
    public static CustomOption OrientalShamanPlayerCount;
    public static CustomOption OrientalShamanImpostorView;
    public static CustomOption OrientalShamanVentUseCoolTime;
    public static CustomOption OrientalShamanVentDurationTime;
    public static CustomOption OrientalShamanCrewWinHijack;
    public static CustomOption OrientalShamanWinTask;
    public static CustomOption OrientalShamanCommonTask;
    public static CustomOption OrientalShamanShortTask;
    public static CustomOption OrientalShamanLongTask;
    public static void SetupCustomOptions()
    {
        OrientalShamanOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.OrientalShaman);
        OrientalShamanPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], OrientalShamanOption);
        OrientalShamanImpostorView = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", true, OrientalShamanOption);
        var OrientalShamanoption = SelectTask.TaskSetting(443, 444, 445, OrientalShamanOption, CustomOptionType.Neutral, true);
        OrientalShamanCommonTask = OrientalShamanoption.Item1;
        OrientalShamanShortTask = OrientalShamanoption.Item2;
        OrientalShamanLongTask = OrientalShamanoption.Item3;
    }
    
    public static List<PlayerControl> OrientalShamanPlayer;
    public static Color32 color = new Color32(192, 177, 246, byte.MaxValue);
    public static void ClearAndReload()
    {
        OrientalShamanPlayer = new();
    }
    
    // ここにコードを書きこんでください
}