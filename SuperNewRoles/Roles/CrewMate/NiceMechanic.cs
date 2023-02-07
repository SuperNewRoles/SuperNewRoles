using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public static class NiceMechanic
{
    private const int OptionId = 1207;
    public static CustomRoleOption NiceMechanicOption;
    public static CustomOption NiceMechanicPlayerCount;
    public static CustomOption NiceMechanicCoolTime;
    public static CustomOption NiceMechanicDurationTime;
    public static CustomOption NiceMechanicUseVent;
    public static void SetupCustomOptions()
    {
        NiceMechanicOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.NiceMechanic);
        NiceMechanicPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], NiceMechanicOption);
        NiceMechanicCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, NiceMechanicOption);
        NiceMechanicDurationTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, NiceMechanicOption, format: "unitSeconds");
        NiceMechanicUseVent = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "JackalUseVentSetting", true, NiceMechanicOption);
    }

    public static List<PlayerControl> NiceMechanicPlayer;
    public static Color32 color = new Color32(82, 108, 173, byte.MaxValue);
    public static void ClearAndReload()
    {
        NiceMechanicPlayer = new();
    }
    
    // ここにコードを書きこんでください
}