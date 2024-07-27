using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Mode.Zombie;

public class ZombieOptions
{
    public static CustomOption ZombieMode;
    public static CustomOption StartSecondOption;
    public static CustomOption ZombieLightOption;
    public static CustomOption ZombieSpeedOption;
    public static CustomOption PoliceLightOption;
    public static CustomOption PoliceSpeedOption;
    public static CustomOption ZombieCommingLightOption;
    public static CustomOption ZombieCommingSpeedOption;
    public static void Load()
    {
        ZombieMode = CustomOption.Create(101500, true, CustomOptionType.Generic, "SettingZombieMode", false, ModeHandler.ModeSetting);
        StartSecondOption = CustomOption.Create(101501, true, CustomOptionType.Generic, "ZombieStartSecondSetting", 5f, 2.5f, 30f, 2.5f, ZombieMode);
        ZombieLightOption = CustomOption.Create(101502, true, CustomOptionType.Generic, "ZombieZombieLightSetting", 0.5f, 0f, 5f, 0.25f, ZombieMode);
        ZombieSpeedOption = CustomOption.Create(101503, true, CustomOptionType.Generic, "ZombieZombieSpeedSetting", 0.75f, 0f, 5f, 0.25f, ZombieMode);
        PoliceLightOption = CustomOption.Create(101504, true, CustomOptionType.Generic, "ZombiePoliceLightSetting", 3f, 0f, 5f, 0.25f, ZombieMode);
        PoliceSpeedOption = CustomOption.Create(101505, true, CustomOptionType.Generic, "ZombiePoliceSpeedSetting", 1f, 0f, 5f, 0.25f, ZombieMode);
        ZombieCommingLightOption = CustomOption.Create(101506, true, CustomOptionType.Generic, "ZombieCommingLightSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
        ZombieCommingSpeedOption = CustomOption.Create(101507, true, CustomOptionType.Generic, "ZombieCommingSpeedSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
    }
    static float GetSpeed(float speed) { return speed <= 0 ? 0.001f : speed; }
    public static void FirstChangeSettings()
    {
        var optdata = SyncSetting.DefaultOption.DeepCopy();
        optdata.SetFloat(FloatOptionNames.CrewLightMod, GetSpeed(ZombieCommingLightOption.GetFloat()));
        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, GetSpeed(ZombieCommingSpeedOption.GetFloat()));
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
            optdata.RpcSyncOption(player.GetClientId());
        }
    }
    public static void ChengeSetting(PlayerControl player)
    {
        var optdata = SyncSetting.DefaultOption.DeepCopy();

        if (player.IsZombie())
        {
            optdata.SetFloat(FloatOptionNames.ImpostorLightMod, GetSpeed(ZombieLight));
            optdata.SetFloat(FloatOptionNames.PlayerSpeedMod, GetSpeed(ZombieSpeed));
            optdata.SetFloat(FloatOptionNames.CrewLightMod, GetSpeed(ZombieLight));
            optdata.SetFloat(FloatOptionNames.PlayerSpeedMod, GetSpeed(ZombieSpeed));
        }
        else
        {
            optdata.SetFloat(FloatOptionNames.CrewLightMod, GetSpeed(PoliceLight));
            optdata.SetFloat(FloatOptionNames.PlayerSpeedMod, GetSpeed(PoliceSpeed));
        }
        if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
        optdata.RpcSyncOption(player.GetClientId());
    }
    public static float ZombieLight;
    public static float ZombieSpeed;
    public static float PoliceLight;
    public static float PoliceSpeed;
}