﻿using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class SyncSetting
    {
        public static GameOptionsData OptionData;
        public static void CustomSyncSettings(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var role = player.getRole();
            var optdata = OptionData.DeepCopy();
            switch (role)
            {
                case RoleId.Jester:
                    if (RoleClass.Jester.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    break;
                case RoleId.Sheriff:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    if (switchSystem != null && switchSystem.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.KillCooldown = KillCoolSet(CustomOptions.SheriffCoolTime.getFloat());
                    break;
                case RoleId.Minimalist:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Minimalist.KillCoolTime);
                    break;
                case RoleId.God:
                    optdata.AnonymousVotes = !RoleClass.God.IsVoteView;
                    break;
                case RoleId.MadMate:
                    if (RoleClass.MadMate.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }                    
                    if (RoleClass.MadMate.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.MadMayor:
                    if (RoleClass.MadMayor.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.MadMayor.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.MadStuntMan:
                    if (RoleClass.MadStuntMan.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.MadStuntMan.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.truelover:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemtruelover = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    if (switchSystemtruelover != null && switchSystemtruelover.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    if (RoleClass.truelover.CreatePlayers.Contains(player.PlayerId))
                    {
                        optdata.KillCooldown = -1f;
                    }
                    else
                    {
                        optdata.KillCooldown = 0.001f;
                    }
                    break;
                case RoleId.Technician:
                    optdata.RoleOptions.EngineerCooldown = 0f;
                    optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    break;
                case RoleId.SerialKiller:
                    optdata.killCooldown = KillCoolSet(RoleClass.SerialKiller.KillTime);
                    break;
                case RoleId.OverKiller:
                    optdata.killCooldown = KillCoolSet(RoleClass.OverKiller.KillCoolTime);
                    break;
            }
            if (player.isDead()) optdata.AnonymousVotes = false;
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, player.getClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static float KillCoolSet(float cool)
        {
            if (cool <= 0)
            {
                return 0.001f;
            } else
            {
                return cool;
            }
        }
        public static void GamblersetCool(PlayerControl p)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var role = p.getRole();
            var optdata = OptionData.DeepCopy();
            if (RoleClass.EvilGambler.GetSuc())
            {
                optdata.KillCooldown = KillCoolSet(RoleClass.EvilGambler.SucCool);
            } else
            {
                optdata.KillCooldown = KillCoolSet(RoleClass.EvilGambler.NotSucCool);
            }
            if (p.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, p.getClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void CustomSyncSettings()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected)
                {
                    CustomSyncSettings(p);
                }
            }
        }
        public static GameOptionsData DeepCopy(this GameOptionsData opt)
        {
            var optByte = opt.ToBytes(5);
            return GameOptionsData.FromBytes(optByte);
        }
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
        public class StartGame
        {
            public static void Prefix()
            {
             //   BotHandler.CreateBot();
            }
            public static void Postfix()
            {
                if (!AmongUsClient.Instance.AmHost) return;
                OptionData = PlayerControl.GameOptions.DeepCopy();
            }
        }
    }
}
