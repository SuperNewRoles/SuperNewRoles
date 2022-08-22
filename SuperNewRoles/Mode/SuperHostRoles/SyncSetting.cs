using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class SyncSetting
    {
        public static GameOptionsData OptionData;
        public static void CustomSyncSettings(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            var role = player.GetRole();
            var optdata = OptionData.DeepCopy();
            if (player.IsCrewVision())
            {
                optdata.ImpostorLightMod = optdata.CrewLightMod;
                var switchSystemToiletFan = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                if (switchSystemToiletFan != null && switchSystemToiletFan.IsActive)
                {
                    optdata.ImpostorLightMod /= 5;
                }
            }
            if (player.IsImpostorVision())
            {
                optdata.CrewLightMod = optdata.ImpostorLightMod;
                var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                if (switchSystem2 != null && switchSystem2.IsActive)
                {
                    optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                }
            }
            if (player.IsZeroCoolEngineer())
            {
                optdata.RoleOptions.EngineerCooldown = 0f;
                optdata.RoleOptions.EngineerInVentMaxTime = 0f;
            }
            switch (role)
            {
                case RoleId.Sheriff:
                    optdata.KillCooldown = KillCoolSet(CustomOptions.SheriffCoolTime.GetFloat());
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
                case RoleId.MadMaker:
                    if (!player.IsMod())
                    {
                        if (!RoleClass.MadMaker.IsImpostorLight)
                        {
                            optdata.ImpostorLightMod = optdata.CrewLightMod;
                            var switchSystemMadMaker = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (switchSystemMadMaker != null && switchSystemMadMaker.IsActive)
                            {
                                optdata.ImpostorLightMod /= 5;
                            }
                        }
                    }
                    if (player.IsMod())
                    {
                        if (RoleClass.MadMaker.IsImpostorLight)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod;
                            var switchSystem2 = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (switchSystem2 != null && switchSystem2.IsActive)
                            {
                                optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                            }
                        }
                    }
                    optdata.KillCooldown = RoleClass.MadMaker.CreatePlayers.Contains(player.PlayerId) ? -1f : 0.001f;
                    break;
                case RoleId.truelover:
                    optdata.KillCooldown = RoleClass.Truelover.CreatePlayers.Contains(player.PlayerId) ? -1f : 0.001f;
                    break;
                case RoleId.SerialKiller:
                    optdata.killCooldown = KillCoolSet(RoleClass.SerialKiller.KillTime);
                    break;
                case RoleId.OverKiller:
                    optdata.killCooldown = KillCoolSet(RoleClass.OverKiller.KillCoolTime);
                    break;
                case RoleId.FalseCharges:
                    optdata.killCooldown = KillCoolSet(RoleClass.FalseCharges.CoolTime);
                    break;
                case RoleId.RemoteSheriff:
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    optdata.RoleOptions.ShapeshifterCooldown = KillCoolSet(RoleClass.RemoteSheriff.KillCoolTime);
                    if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId) && RoleClass.RemoteSheriff.KillCount[player.PlayerId] < 1)
                    {
                        optdata.RoleOptions.ShapeshifterDuration = 1f;
                        optdata.RoleOptions.ShapeshifterCooldown = -1f;
                    }
                    optdata.killCooldown = player.IsMod() ? KillCoolSet(RoleClass.RemoteSheriff.KillCoolTime) : -1f;
                    break;
                case RoleId.Arsonist:
                    optdata.RoleOptions.ShapeshifterCooldown = 1f;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    optdata.KillCooldown = KillCoolSet(RoleClass.Arsonist.CoolTime);
                    break;
                case RoleId.Nocturnality:
                    var switchSystemNocturnality = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (switchSystemNocturnality == null || !switchSystemNocturnality.IsActive) optdata.CrewLightMod /= 5;
                    else optdata.CrewLightMod *= 5;
                    break;
                case RoleId.SelfBomber:
                    optdata.RoleOptions.ShapeshifterCooldown = 0.000001f;
                    optdata.RoleOptions.ShapeshifterDuration = 0.000001f;
                    break;
                case RoleId.Survivor:
                    optdata.killCooldown = KillCoolSet(RoleClass.Survivor.KillCoolTime);
                    break;
                case RoleId.Jackal:
                    if (!player.IsMod())
                    {
                        if (!RoleClass.Jackal.IsImpostorLight)
                        {
                            optdata.ImpostorLightMod = optdata.CrewLightMod;
                            var switchSystemJackal = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (switchSystemJackal != null && switchSystemJackal.IsActive) optdata.ImpostorLightMod /= 5;
                        }
                    }
                    else
                    {
                        if (RoleClass.Jackal.IsImpostorLight)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod;
                            var switchSystem2 = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (switchSystem2 != null && switchSystem2.IsActive) optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    optdata.KillCooldown = KillCoolSet(RoleClass.Jackal.KillCoolDown);
                    break;
                case RoleId.Demon:
                    optdata.KillCooldown = KillCoolSet(RoleClass.Demon.CoolTime);
                    break;
                case RoleId.ToiletFan:
                    optdata.RoleOptions.ShapeshifterCooldown = RoleClass.ToiletFan.ToiletCool;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
                case RoleId.SatsumaAndImo:
                    if (RoleClass.SatsumaAndImo.TeamNumber != 1)//クルーじゃないとき
                    {
                        optdata.CrewLightMod = optdata.ImpostorLightMod;
                        var switchSystem2 = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                        if (switchSystem2 != null && switchSystem2.IsActive)
                        {
                            optdata.CrewLightMod = optdata.ImpostorLightMod * 15;
                        }
                    }
                    break;
                case RoleId.NiceButtoner:
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
                case RoleId.EvilButtoner:
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
            }
            if (player.IsDead()) optdata.AnonymousVotes = false;
            optdata.RoleOptions.ShapeshifterLeaveSkin = false;
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static float KillCoolSet(float cool) { return cool <= 0 ? 0.001f : cool; }
        public static void MurderSyncSetting(PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            var role = player.GetRole();
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
                    if (switchSystemArsonist != null && switchSystemArsonist.IsActive) optdata.ImpostorLightMod /= 5;
                    optdata.RoleOptions.ShapeshifterCooldown = 1f;
                    optdata.RoleOptions.ShapeshifterDuration = 1f;
                    break;
                default:
                    return;
            }
            if (player.IsDead()) optdata.AnonymousVotes = false;
            optdata.RoleOptions.ShapeshifterLeaveSkin = false;
            if (player.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, player.GetClientId());
            writer.WriteBytesAndSize(optdata.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void GamblersetCool(PlayerControl p)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var role = p.GetRole();
            var optdata = OptionData.DeepCopy();
            optdata.KillCooldown = RoleClass.EvilGambler.GetSuc() ? KillCoolSet(RoleClass.EvilGambler.SucCool) : KillCoolSet(RoleClass.EvilGambler.NotSucCool);
            if (p.AmOwner) PlayerControl.GameOptions = optdata;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.None, p.GetClientId());
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
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
        public class StartGame
        {
            public static void Prefix()
            {
                //   BotHandler.CreateBot();
            }
            public static void Postfix()
            {
                OptionData = PlayerControl.GameOptions.DeepCopy();
            }
        }
    }
}