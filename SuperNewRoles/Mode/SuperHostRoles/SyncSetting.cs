using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using SuperNewRoles.SuperNewRolesWeb;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class SyncSetting
{
    public static IGameOptions DefaultOption;
    public static PlayerData<IGameOptions> OptionDatas;
    public static void CustomSyncSettings(this PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers)) return;
        IGameOptions optdata = DefaultOption.DeepCopy();
        bool blackout = false;
        if (MapUtilities.CachedShipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elec))
        {
            SwitchSystem system = elec.CastFast<SwitchSystem>();
            blackout = system != null && system.IsActive;
        }

        if (PlusMode.PlusGameOptions.EnableFirstEmergencyCooldown)
        {
            // 緊急会議のクールタイムの設定取得&送信は別の場所で行い, 此処では, Default設定に上書きされない様 既に設定されている緊急会議クールを再取得&送信している。
            // (別場所で行っている理由 : SyncSettingで送信するとShipStatus.Instance.EmergencyCooldownへの代入が間に合わない & 追放による死亡が判定できない為)
            int emergencyCooldown = OptionDatas[player].DeepCopy().GetInt(Int32OptionNames.EmergencyCooldown);
            optdata.SetInt(Int32OptionNames.EmergencyCooldown, emergencyCooldown);
        }
        if (player.IsCrewVision())
        {
            optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
            if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
        }
        if (player.IsImpostorVision())
        {
            optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
            if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
        }
        if (player.IsZeroCoolEngineer())
        {
            optdata.SetFloat(FloatOptionNames.EngineerCooldown, 0f);
            optdata.SetFloat(FloatOptionNames.EngineerInVentMaxTime, 0f);
        }
        switch (player.GetRole())
        {
            case RoleId.Sheriff:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(CustomOptionHolder.SheriffCoolTime.GetFloat()));
                break;
            case RoleId.Minimalist:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Minimalist.KillCoolTime));
                break;
            case RoleId.Samurai:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Samurai.KillCoolTime));
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.Samurai.SwordCoolTime);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, RoleClass.Samurai.SwordCoolTime);
                break;
            case RoleId.MadMaker:
                if (!player.IsMod())
                {
                    if (!RoleClass.MadMaker.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                    }
                }
                else
                {
                    if (RoleClass.MadMaker.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                    }
                }
                optdata.SetFloat(FloatOptionNames.KillCooldown, RoleClass.MadMaker.CreatePlayers.Contains(player.PlayerId) ? -1f : 0.001f);
                break;
            case RoleId.truelover:
                optdata.SetFloat(FloatOptionNames.KillCooldown, RoleClass.Truelover.CreatePlayers.Contains(player.PlayerId) ? -1f : 0.001f);
                break;
            case RoleId.SerialKiller:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.SerialKiller.KillTime));
                break;
            case RoleId.OverKiller:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.OverKiller.KillCoolTime));
                break;
            case RoleId.FalseCharges:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.FalseCharges.CoolTime));
                break;
            case RoleId.RemoteSheriff:
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0f);
                if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId) && RoleClass.RemoteSheriff.KillCount[player.PlayerId] < 1)
                {
                    optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                    optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, -1f);
                }
                optdata.SetFloat(FloatOptionNames.KillCooldown, player.IsMod() ? KillCoolSet(RoleClass.RemoteSheriff.KillCoolTime) : -1f);
                break;
            case RoleId.Arsonist:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 1f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Arsonist.CoolTime));
                break;
            case RoleId.Nocturnality:
                if (!blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod) / 5);
                else optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod) * 3f);
                break;
            case RoleId.SelfBomber:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.000001f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 0.000001f);
                break;
            case RoleId.Survivor:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Survivor.KillCoolTime));
                break;
            case RoleId.Jackal:
                if (!player.IsMod())
                {
                    if (!RoleClass.Jackal.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                    }
                }
                else
                {
                    if (RoleClass.Jackal.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                    }
                }
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Jackal.KillCooldown));
                break;
            case RoleId.JackalSeer:
                if (!player.IsMod())
                {
                    if (!RoleClass.JackalSeer.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                    }
                }
                else
                {
                    if (RoleClass.JackalSeer.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                    }
                }
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.JackalSeer.KillCooldown));
                break;
            case RoleId.Demon:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Demon.CoolTime));
                break;
            case RoleId.ToiletFan:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.ToiletFan.ToiletCool);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.SatsumaAndImo:
                if (player.GetRoleBase<SatsumaAndImo>()?.TeamState == SatsumaAndImo.SatsumaTeam.Madmate)//クルーじゃないとき
                {
                    optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                    if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                }
                break;
            case RoleId.NiceButtoner:
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.EvilButtoner:
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.Doppelganger:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.Doppelganger.CoolTime);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, RoleClass.Doppelganger.DurationTime);
                break;
            case RoleId.DarkKiller:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(CustomOptionHolder.DarkKillerKillCoolTime.GetFloat()));
                break;
            case RoleId.Camouflager:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.Camouflager.CoolTime >= 5f ? RoleClass.Camouflager.CoolTime : 5f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                if (RoleClass.Camouflager.IsCamouflage)
                {
                    optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown,
                            RoleClass.Camouflager.CoolTime >= 5f ? (RoleClass.Camouflager.CoolTime + RoleClass.Camouflager.DurationTime - 2f) : (3f + RoleClass.Camouflager.DurationTime));
                }
                break;
            case RoleId.Worshiper:
                if (!player.IsMod())
                {
                    if (!Worshiper.RoleData.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                    }
                }
                else
                {
                    if (Worshiper.RoleData.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                    }
                }
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(Worshiper.RoleData.KillSuicideCoolTime));
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, Worshiper.RoleData.AbilitySuicideCoolTime);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.EvilSeer:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.PoliceSurgeon:
                optdata.SetFloat(FloatOptionNames.ScientistCooldown, PoliceSurgeon.CustomOptionData.VitalsDisplayCooldown.GetFloat());
                optdata.SetFloat(FloatOptionNames.ScientistBatteryCharge, PoliceSurgeon.CustomOptionData.BatteryDuration.GetFloat());
                break;
            case RoleId.MadRaccoon:
                if (!player.IsMod())
                {
                    if (!MadRaccoon.RoleData.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                    }
                }
                else
                {
                    if (MadRaccoon.RoleData.IsImpostorLight)
                    {
                        optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod));
                        if (blackout) optdata.SetFloat(FloatOptionNames.CrewLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) * 15);
                    }
                }
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, MadRaccoon.RoleData.ShapeshifterCooldown);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, MadRaccoon.RoleData.ShapeshifterDuration);
                break;
            case RoleId.Madmate:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 60f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            case RoleId.JackalFriends:
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 60f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            default:
                if (player.GetRoleBase() is ISupportSHR supportSHR)
                    supportSHR.BuildSetting(optdata);
                break;
        }

        RoleId ghostRole = player.GetGhostRole();
        if (ghostRole != RoleId.DefaultRole) // バニラ幽霊役職でないなら, クールタイムをセットする
        {
            optdata.SetFloat(FloatOptionNames.GuardianAngelCooldown, GuardianAngelCooldown.SetCooldown(ghostRole));
        }

        optdata.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
        optdata.SetBool(BoolOptionNames.AnonymousVotes, AnonymousVotes.GetAnonymousVotes(player));

        Balancer.InHostMode.SetMeetingSettings(optdata);

        if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
        else optdata.RpcSyncOption(player.GetClientId());
        OptionDatas[player] = optdata.DeepCopy();
    }
    public static float KillCoolSet(float cool) { return cool <= 0 ? 0.001f : cool; }
    public static void MurderSyncSetting(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        IGameOptions optdata = OptionDatas[player].DeepCopy();
        bool blackout = false;
        if (MapUtilities.CachedShipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elec))
        {
            SwitchSystem system = elec.CastFast<SwitchSystem>();
            blackout = system != null && system.IsActive;
        }


        switch (player.GetRole())
        {
            case RoleId.Demon:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Demon.CoolTime) * 2);
                break;
            case RoleId.Arsonist:
                optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Arsonist.CoolTime) * 2);
                optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.CrewLightMod));
                if (blackout) optdata.SetFloat(FloatOptionNames.ImpostorLightMod, optdata.GetFloat(FloatOptionNames.ImpostorLightMod) / 5);
                optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, 1f);
                optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, 1f);
                break;
            default:
                return;
        }
        optdata.SetBool(BoolOptionNames.ShapeshifterLeaveSkin, false);
        if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
        else optdata.RpcSyncOption(player.GetClientId());
    }
    public static void GamblersetCool(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        IGameOptions optdata = OptionDatas[player].DeepCopy();
        optdata.SetFloat(FloatOptionNames.KillCooldown, RoleClass.EvilGambler.GetSuc() ? KillCoolSet(RoleClass.EvilGambler.SucCool) : KillCoolSet(RoleClass.EvilGambler.NotSucCool));
        if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
        else optdata.RpcSyncOption(player.GetClientId());
    }
    public static void DoppelgangerCool(PlayerControl player, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        IGameOptions optdata = OptionDatas[player].DeepCopy();
        optdata.SetFloat(FloatOptionNames.ShapeshifterDuration, RoleClass.Doppelganger.DurationTime);
        optdata.SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.Doppelganger.CoolTime);
        if (RoleClass.Doppelganger.Targets.ContainsKey(player.PlayerId))
        {
            optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Doppelganger.Targets[player.PlayerId].PlayerId == target.PlayerId ?
                                                                        RoleClass.Doppelganger.SucCool : RoleClass.Doppelganger.NotSucCool));
        }
        else optdata.SetFloat(FloatOptionNames.KillCooldown, KillCoolSet(RoleClass.Doppelganger.NotSucCool));
        if (player.AmOwner) GameManager.Instance.LogicOptions.SetGameOptions(optdata);
        else optdata.RpcSyncOption(player.GetClientId());
    }
    public static void CustomSyncSettings()
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR:SyncSettings] CustomSyncSettingsが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (!p.Data.Disconnected && !p.IsBot())
            {
                CustomSyncSettings(p);
            }
        }
    }

    public static IGameOptions DeepCopy(this IGameOptions opt)
    {
        var optByte = GameOptionsManager.Instance.gameOptionsFactory.ToBytes(opt, AprilFoolsMode.IsAprilFoolsModeToggledOn);
        return GameOptionsManager.Instance.gameOptionsFactory.FromBytes(optByte);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public class StartGame
    {
        public static void Postfix()
        {
            var RPD = RoomPlayerData.Instance;
            DefaultOption = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
            OptionDatas = new(defaultvalue: DefaultOption);
            OnGameEndPatch.PlayerData = new();
            ReplayLoader.CoStartGame();
            if (ModeHandler.IsMode(ModeId.BattleRoyal))
                BattleRoyalWebManager.StartGame();
        }
    }
}