using HarmonyLib;
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
            if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return;
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
                    var switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystem != null && switchSystem.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.KillCooldown = KillCoolSet(CustomOptions.SheriffCoolTime.getFloat());
                    break;
                case RoleId.Minimalist:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Minimalist.KillCoolTime);
                    break;
                case RoleId.Samurai:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Samurai.KillCoolTime);
                    optdata.RoleOptions.ShapeshifterCooldown = RoleClass.Samurai.SwordCoolTime;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
                case RoleId.God:
                    optdata.AnonymousVotes = !RoleClass.God.IsVoteView;
                    break;
                case RoleId.Observer:
                    optdata.AnonymousVotes = !RoleClass.Observer.IsVoteView;
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
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
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
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
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
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.MadJester:
                    if (RoleClass.MadJester.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.MadJester.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.MadMaker:
                    if (!RoleClass.MadMaker.IsImpostorLight)
                    {
                        optdata.ImpostorLightMod = optdata.CrewLightMod;
                        var switchSystemMadMaker = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystemMadMaker != null && switchSystemMadMaker.IsActive)
                        {
                            optdata.ImpostorLightMod /= 5;
                        }
                    }
                    if (RoleClass.MadMaker.CreatePlayers.Contains(player.PlayerId))
                    {
                        optdata.KillCooldown = -1f;
                    }
                    else
                    {
                        optdata.KillCooldown = 0.001f;
                    }
                    break;
                case RoleId.JackalFriends:
                    if (RoleClass.JackalFriends.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.JackalFriends.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.Fox:
                    if (RoleClass.Fox.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.Fox.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.truelover:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemtruelover = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
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
                case RoleId.FalseCharges:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemFalseCharges = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemFalseCharges != null && switchSystemFalseCharges.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.killCooldown = KillCoolSet(RoleClass.FalseCharges.CoolTime);
                    break;
                case RoleId.RemoteSheriff:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemRemoteSheriff = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemRemoteSheriff != null && switchSystemRemoteSheriff.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    optdata.RoleOptions.ShapeshifterCooldown = KillCoolSet(RoleClass.RemoteSheriff.KillCoolTime);
                    if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId) && RoleClass.RemoteSheriff.KillCount[player.PlayerId] < 1)
                    {
                        optdata.RoleOptions.ShapeshifterDuration = 1f;
                        optdata.RoleOptions.ShapeshifterCooldown = -1f;
                    }
                    if (player.IsMod())
                    {
                        optdata.killCooldown = KillCoolSet(RoleClass.RemoteSheriff.KillCoolTime);
                    }
                    else
                    {
                        optdata.killCooldown = -1f;
                    }
                    break;
                case RoleId.Arsonist:
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemArsonist = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemArsonist != null && switchSystemArsonist.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.RoleOptions.ShapeshifterCooldown = 1f;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    optdata.KillCooldown = KillCoolSet(RoleClass.Arsonist.CoolTime);
                    break;
                case RoleId.Nocturnality:
                    var switchSystemNocturnality = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemNocturnality == null || !switchSystemNocturnality.IsActive)
                    {
                        optdata.CrewLightMod /= 5;
                    }
                    else
                    {
                        optdata.CrewLightMod *= 5;
                    }
                    break;
                case RoleId.SelfBomber:
                    optdata.RoleOptions.ShapeshifterCooldown = 0.000001f;
                    optdata.RoleOptions.ShapeshifterDuration = 0.000001f;
                    break;
                case RoleId.Survivor:
                    optdata.killCooldown = KillCoolSet(RoleClass.Survivor.KillCoolTime);
                    break;
                case RoleId.Jackal:
                    if (!RoleClass.Jackal.IsImpostorLight)
                    {
                        optdata.ImpostorLightMod = optdata.CrewLightMod;
                        var switchSystemJackal = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();

                        if (switchSystemJackal != null && switchSystemJackal.IsActive)
                        {
                            optdata.ImpostorLightMod /= 5;
                        }
                    }
                    if (player.IsMod())
                    {
                        if (RoleClass.Jackal.IsImpostorLight)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod;
                            var switchSystem2 = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (switchSystem2 != null && switchSystem2.IsActive)
                            {
                                optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                            }
                        }
                    }
                        optdata.KillCooldown = KillCoolSet(RoleClass.Jackal.KillCoolDown);
                    break;
                case RoleId.Demon:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Demon.CoolTime);
                    break;
                case RoleId.MayorFriends:
                    if (RoleClass.MayorFriends.IsUseVent)
                    {
                        optdata.RoleOptions.EngineerCooldown = 0f;
                        optdata.RoleOptions.EngineerInVentMaxTime = 0f;
                    }
                    if (RoleClass.MayorFriends.IsImpostorLight)
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
            }
            if (player.isDead()) optdata.AnonymousVotes = false;
            optdata.RoleOptions.ShapeshifterLeaveSkin = false;
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.getClientId());
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
        public static void MurderSyncSetting(PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return;
            var role = player.getRole();
            var optdata = OptionData.DeepCopy();
            switch (role)
            {
                case RoleId.Demon:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Demon.CoolTime) * 2;
                    break;
                case RoleId.Arsonist:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Arsonist.CoolTime) * 2;
                    optdata.ImpostorLightMod = optdata.CrewLightMod;
                    var switchSystemArsonist = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemArsonist != null && switchSystemArsonist.IsActive)
                    {
                        optdata.ImpostorLightMod /= 5;
                    }
                    optdata.RoleOptions.ShapeshifterCooldown = 1f;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
                default:
                    return;
            }
            if (player.isDead()) optdata.AnonymousVotes = false;
            optdata.RoleOptions.ShapeshifterLeaveSkin = false;
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.getClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, p.getClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void CustomSyncSettings()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.Data.Disconnected && p.IsPlayer())
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
