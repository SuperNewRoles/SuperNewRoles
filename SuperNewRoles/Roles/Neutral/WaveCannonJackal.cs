using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;

namespace SuperNewRoles.Roles.Neutral;

class WaveCannonJackal
{
    // CustomOption Start
    private static int OptionId = 1251;
    public static CustomRoleOption WaveCannonJackalOption;
    public static CustomOption WaveCannonJackalPlayerCount;
    public static CustomOption WaveCannonJackalCoolTime;
    public static CustomOption WaveCannonJackalChargeTime;
    public static CustomOption WaveCannonJackalKillCooldown;
    public static CustomOption WaveCannonJackalUseVent;
    public static CustomOption WaveCannonJackalUseSabo;
    public static CustomOption WaveCannonJackalIsImpostorLight;
    public static CustomOption WaveCannonJackalIsSyncKillCoolTime;
    public static CustomOption WaveCannonJackalCreateSidekick;
    public static CustomOption WaveCannonJackalCreateFriend;
    public static CustomOption WaveCannonJackalSKCooldown;
    public static CustomOption WaveCannonJackalNewJackalCreateSidekick;
    public static CustomOption WaveCannonJackalNewJackalHaveWaveCannon;
    public static void SetupCustomOptions()
    {
        WaveCannonJackalOption = SetupCustomRoleOption(OptionId, false, RoleId.WaveCannonJackal, CustomOptionType.Neutral); OptionId++;
        WaveCannonJackalPlayerCount = Create(OptionId, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WaveCannonJackalOption); OptionId++;
        WaveCannonJackalCoolTime = Create(OptionId, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 180f, 2.5f, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalChargeTime = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonChargeTime", 3f, 0.5f, 15f, 0.5f, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalKillCooldown = Create(OptionId, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalOption, format: "unitSeconds"); OptionId++;
        WaveCannonJackalUseVent = Create(OptionId, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalIsImpostorLight = Create(OptionId, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalUseSabo = Create(OptionId, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalIsSyncKillCoolTime = Create(OptionId, false, CustomOptionType.Neutral, "IsSyncKillCoolTime", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalSKCooldown = Create(OptionId, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalCreateSidekick, format: "unitSeconds"); OptionId++;
        WaveCannonJackalNewJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalNewJackalHaveWaveCannon = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalNewJackalHaveWaveCannon", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalCreateFriend = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, WaveCannonJackalOption);
    }
    // CustomOption End

    // RoleClass Start
    public static List<PlayerControl> WaveCannonJackalPlayer;
    public static List<PlayerControl> FakeSidekickWaveCannonPlayer;
    public static Color32 color = RoleClass.JackalBlue;
    public static List<int> CreatePlayers;
    public static bool CreateSidekick;
    public static bool CanCreateSidekick;
    public static bool CanCreateFriend;
    public static void ClearAndReload()
    {
        WaveCannonJackalPlayer = new();
        FakeSidekickWaveCannonPlayer = new();
        CreatePlayers = new();
        CanCreateSidekick = WaveCannonJackalCreateSidekick.GetBool();
        CanCreateFriend = WaveCannonJackalCreateFriend.GetBool();
    }
    // RoleClass End

    public static void ResetCooldowns()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = WaveCannonJackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
    }
}