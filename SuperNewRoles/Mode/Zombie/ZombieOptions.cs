﻿using System;
using System.Collections.Generic;
using System.Text;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.Zombie
{
    public class ZombieOptions
    {
        public static CustomOption.CustomOption ZombieMode;
        public static CustomOption.CustomOption StartSecondOption;
        public static CustomOption.CustomOption ZombieLightOption;
        public static CustomOption.CustomOption ZombieSpeedOption;
        public static CustomOption.CustomOption PoliceLightOption;
        public static CustomOption.CustomOption PoliceSpeedOption;
        public static CustomOption.CustomOption ZombieCommingLightOption;
        public static CustomOption.CustomOption ZombieCommingSpeedOption;
        public static void Load()
        {
            ZombieMode = CustomOption.CustomOption.Create(195, true, CustomOptionType.Generic,"SettingZombieMode", false, ModeHandler.ModeSetting);
            StartSecondOption = CustomOption.CustomOption.Create(332, true, CustomOptionType.Generic, "ZombieStartSecondSetting", 5f, 2.5f, 30f, 2.5f, ZombieMode);
            ZombieLightOption = CustomOption.CustomOption.Create(196, true, CustomOptionType.Generic, "ZombieZombieLightSetting", 0.5f, 0f, 5f, 0.25f, ZombieMode);
            ZombieSpeedOption = CustomOption.CustomOption.Create(333, true, CustomOptionType.Generic, "ZombieZombieSpeedSetting", 0.75f, 0f, 5f, 0.25f, ZombieMode);
            PoliceLightOption = CustomOption.CustomOption.Create(334, true, CustomOptionType.Generic, "ZombiePoliceLightSetting", 3f, 0f, 5f, 0.25f, ZombieMode);
            PoliceSpeedOption = CustomOption.CustomOption.Create(335, true, CustomOptionType.Generic, "ZombiePoliceSpeedSetting", 1f, 0f, 5f, 0.25f, ZombieMode);
            ZombieCommingLightOption = CustomOption.CustomOption.Create(336, true, CustomOptionType.Generic, "ZombieCommingLightSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
            ZombieCommingSpeedOption = CustomOption.CustomOption.Create(337, true, CustomOptionType.Generic, "ZombieCommingSpeedSetting", 1.5f, 0f, 5f, 0.25f, ZombieMode);
        }
        static float GetSpeed(float speed) { 
            if (speed <= 0)
            {
                return 0.001f;
            }
            return speed;
        }
        public static void FirstChangeSettings()
        {
            var optdata = SyncSetting.OptionData.DeepCopy();
            optdata.CrewLightMod = GetSpeed(ZombieCommingLightOption.getFloat());
            optdata.ImpostorLightMod = GetSpeed(ZombieCommingSpeedOption.getFloat());
            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                if (player.AmOwner) PlayerControl.GameOptions = optdata;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, player.getClientId());
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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, player.getClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static float ZombieLight;
        public static float ZombieSpeed;
        public static float PoliceLight;
        public static float PoliceSpeed;
    }
}
