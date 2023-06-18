using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public static class EvilMechanic
{
    private const int OptionId = 202200;
    public static CustomRoleOption EvilMechanicOption;
    public static CustomOption EvilMechanicPlayerCount;
    public static CustomOption EvilMechanicCoolTime;
    public static CustomOption EvilMechanicDurationTime;
    public static void SetupCustomOptions()
    {
        EvilMechanicOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.EvilMechanic);
        EvilMechanicPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], EvilMechanicOption);
        EvilMechanicCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Impostor, "NiceScientistCooldownSetting", 30f, 2.5f, 60f, 2.5f, EvilMechanicOption);
        EvilMechanicDurationTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Impostor, "NiceScientistDurationSetting", 10f, 2.5f, 30f, 2.5f, EvilMechanicOption, format: "unitSeconds");

    }

    public static List<PlayerControl> EvilMechanicPlayer;
    public static Color32 color = RoleClass.ImpostorRed;
    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MechanicButton_Evil.png", 115f);
    public static void ClearAndReload()
    {
        EvilMechanicPlayer = new();
    }

    // ここにコードを書きこんでください
}