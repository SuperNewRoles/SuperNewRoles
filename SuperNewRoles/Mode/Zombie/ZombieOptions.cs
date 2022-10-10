using Hazel;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Mode.Zombie
{
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
            ZombieMode = CustomOption.Create(489, true, CustomOptionType.Generic, "SettingZombieMode", false, ModeHandler.ModeSetting);
            StartSecondOption = CustomOption.Create(490, true, CustomOptionType.Generic, "ZombieStartSecondSetting", 5f, 2.5f, 30f, 2.5f, ZombieMode);
            ZombieLightOption = CustomOption.Create(491, true, CustomOptionType.Generic, "ZombieZombieLightSetting", 0.5f, 0f, 5f, 0.25f, ZombieMode);
            ZombieSpeedOption = CustomOption.Create(492, true, CustomOptionType.Generic, "ZombieZombieSpeedSetting", 0.75f, 0f, 5f, 0.25f, ZombieMode);
            PoliceLightOption = CustomOption.Create(493, true, CustomOptionType.Generic, "ZombiePoliceLightSetting", 3f, 0f, 5f, 0.25f, ZombieMode);
            PoliceSpeedOption = CustomOption.Create(494, true, CustomOptionType.Generic, "ZombiePoliceSpeedSetting", 1f, 0f, 5f, 0.25f, ZombieMode);
            ZombieCommingLightOption = CustomOption.Create(495, true, CustomOptionType.Generic, "ZombieCommingLightSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
            ZombieCommingSpeedOption = CustomOption.Create(496, true, CustomOptionType.Generic, "ZombieCommingSpeedSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
        }
        static float GetSpeed(float speed) { return speed <= 0 ? 0.001f : speed; }
        public static void FirstChangeSettings()
        {
            var optdata = SyncSetting.OptionData.DeepCopy();
            optdata.CrewLightMod = GetSpeed(ZombieCommingLightOption.GetFloat());
            optdata.ImpostorLightMod = GetSpeed(ZombieCommingSpeedOption.GetFloat());
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.AmOwner) PlayerControl.GameOptions = optdata;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
                writer.WriteBytesAndSize(optdata.ToBytes(5));
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void ChengeSetting(PlayerControl player)
        {
            var optdata = SyncSetting.OptionData.DeepCopy();

            if (player.IsZombie())
            {
                optdata.ImpostorLightMod = GetSpeed(ZombieLight);
                optdata.PlayerSpeedMod = GetSpeed(ZombieSpeed);
                optdata.CrewLightMod = GetSpeed(ZombieLight);
                optdata.PlayerSpeedMod = GetSpeed(ZombieSpeed);
            }
            else
            {
                optdata.CrewLightMod = GetSpeed(PoliceLight);
                optdata.PlayerSpeedMod = GetSpeed(PoliceSpeed);
            }
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static float ZombieLight;
        public static float ZombieSpeed;
        public static float PoliceLight;
        public static float PoliceSpeed;
    }
}