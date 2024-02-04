using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;

namespace SuperNewRoles;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRole))]
class SetRoleLogger
{
    public static void Postfix(PlayerControl __instance, RoleTypes role)
    {
        Logger.Info($"{__instance.Data.PlayerName} の役職が {role} になりました", "SetRole");
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class RpcSetRoleReplacer
{
    public static void Postfix(PlayerControl __instance, RoleTypes roleType)
    {
        Logger.Info($"{__instance.Data.PlayerName} の役職が {roleType} になりました", "RpcSetRole");
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
class Startgamepatch
{
    public static void Postfix()
    {
        RPCHelper.StartRPC(CustomRPC.StartGameRPC).EndRPC();
        RPCProcedure.StartGameRPC();

        RoleSelectHandler.SpawnBots();
    }
}
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
class RoleManagerSelectRolesPatch
{
    public static bool IsSetRoleRPC = false;
    public static bool Prefix(RoleManager __instance)
    {
        ReplayLoader.AllRoleSet();
        if (ReplayManager.IsReplayMode)
            return false;
        AllRoleSetClass.SetPlayerNum();
        IsSetRoleRPC = false;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);
            List<PlayerControl> SelectPlayers = new();
            AllRoleSetClass.impostors = new();
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (!player.Data.Disconnected && !player.IsBot())
                {
                    SelectPlayers.Add(player);
                }
            }
            for (int i = 0; i < GameManager.Instance.LogicOptions.NumImpostors; i++)
            {
                if (SelectPlayers.Count >= 1)
                {
                    var newimpostor = ModHelpers.GetRandom(SelectPlayers);
                    AllRoleSetClass.impostors.Add(newimpostor);
                    newimpostor.Data.Role.Role = RoleTypes.Impostor;
                    newimpostor.Data.Role.TeamType = RoleTeamTypes.Impostor;
                    SelectPlayers.RemoveAll(a => a.PlayerId == newimpostor.PlayerId);
                }
            }
            sender = RoleSelectHandler.RoleSelect(sender);

            foreach (PlayerControl player in AllRoleSetClass.impostors)
            {
                sender.RpcSetRole(player, RoleTypes.Impostor);
            }
            RoleTypes CrewRoleTypes = ModeHandler.IsMode(ModeId.VanillaHns) ? RoleTypes.Engineer : RoleTypes.Crewmate;
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (!player.Data.Disconnected && !player.IsImpostor())
                {
                    sender.RpcSetRole(player, CrewRoleTypes);
                }
            }

            //サーバーの役職判定をだます
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetRole)
                    .Write((ushort)RoleTypes.Shapeshifter)
                    .EndRpc();
            }
            //RpcSetRoleReplacerの送信処理
            sender.SendMessage();

            try
            {
                HauntedWolf.Assign.RandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }

            try
            {
                AllRoleSetClass.QuarreledRandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }

            try
            {
                AllRoleSetClass.LoversRandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }
            ChangeName.SetRoleNames();
            return false;
        }
        else if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            Mode.BattleRoyal.Main.ChangeRole.Postfix();
            return false;
        }
        else if (ModeHandler.IsMode(ModeId.PantsRoyal))
        {
            Mode.PantsRoyal.main.AssignRole();
            return false;
        }
        else if (ModeHandler.IsMode(ModeId.CopsRobbers))
        {
            Mode.CopsRobbers.RoleSelectHandler.Handler();
            return false;
        }
        else if (ModeHandler.IsMode(ModeId.Default))
        {
            GM.AssignGM();
        }
        return true;
    }
    public static void Postfix()
    {
        if (ReplayManager.IsReplayMode)
            return;
        IsSetRoleRPC = true;
        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
                AllRoleSetClass.AllRoleSet();
                break;
            case ModeId.NotImpostorCheck:
                Mode.NotImpostorCheck.SelectRolePatch.SetDesync();
                break;
            case ModeId.Detective:
                Mode.Detective.Main.RoleSelect();
                goto default;
            case ModeId.BattleRoyal:
            case ModeId.SuperHostRoles:
                break;
            default:
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    p.RpcSetRole(p.Data.Role.Role);
                }
                break;
        }
        AllRoleSetClass.Assigned = true;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            return;
        new LateTask(() =>
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started)
                return;
            foreach (var pc in CachedPlayer.AllPlayers)
            {
                pc.PlayerControl.RpcSetRole(RoleTypes.Shapeshifter);
            }
        }, 3f, "SetImpostor");
    }
}
class AllRoleSetClass
{
    public enum AssignType
    {
        Impostor = 0x001,
        Neutral = 0x002,
        Crewmate = 0x004,

        //100%
        TenPar = 0x008,
        //100%以外
        NotTenPar = 0x016,
    }
    public static List<PlayerControl> impostors;
    public static Dictionary<AssignType, List<RoleId>> AssignTickets;
    public static List<PlayerControl> CrewmatePlayers;
    public static List<PlayerControl> ImpostorPlayers;

    public static bool Assigned;

    public static int ImpostorGhostRolePlayerNum;
    public static int NeutralGhostRolePlayerNum;
    public static int CrewmatePlayerNum;
    public static int CrewmateGhostRolePlayerNum;

    public static void AllRoleSet()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.CopsRobbers))
        {
            CrewOrImpostorSet();
            OneOrNotListSet();
        }

        ImpostorRandomSelect();

        NeutralRandomSelect();

        CrewmateRandomSelect();

        if (ModeHandler.IsMode(ModeId.Default))
        {
            try
            {
                HauntedWolf.Assign.RandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }

            try
            {
                QuarreledRandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }

            try
            {
                LoversRandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }
        }
    }
    public static void QuarreledRandomSelect()
    {
        if (!CustomOptionHolder.QuarreledOption.GetBool()) return;
        SuperNewRolesPlugin.Logger.LogInfo("クラードセレクト");
        List<PlayerControl> SelectPlayers = new();
        if (CustomOptionHolder.QuarreledOnlyCrewmate.GetBool())
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.IsImpostor() && !p.IsNeutral() && !p.IsBot())
                {
                    SelectPlayers.Add(p);
                }
            }
        }
        else
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.IsBot())
                {
                    SelectPlayers.Add(p);
                }
            }
        }
        for (int i = 0; i < CustomOptionHolder.QuarreledTeamCount.GetFloat(); i++)
        {
            if (SelectPlayers.Count is not (1 or 0))
            {
                List<PlayerControl> listData = new();
                for (int i2 = 0; i2 < 2; i2++)
                {
                    var player = ModHelpers.GetRandomIndex<PlayerControl>(SelectPlayers);
                    listData.Add(SelectPlayers[player]);
                    SelectPlayers.RemoveAt(player);
                }
                RoleHelpers.SetQuarreled(listData[0], listData[1]);
                RoleHelpers.SetQuarreledRPC(listData[0], listData[1]);
            }
        }
        ChacheManager.ResetQuarreledChache();
    }

    public static void LoversRandomSelect()
    {
        if (!CustomOptionHolder.LoversOption.GetBool() || (CustomOptionHolder.LoversPar.GetString() == "0%")) return;
        if (CustomOptionHolder.LoversPar.GetString() != "100%")
        {
            List<string> a = new();
            var SucPar = int.Parse(CustomOptionHolder.LoversPar.GetString().Replace("0%", ""));
            for (int i = 0; i < SucPar; i++)
            {
                a.Add("Suc");
            }
            for (int i = 0; i < 10 - SucPar; i++)
            {
                a.Add("No");
            }
            if (ModHelpers.GetRandom(a) == "No")
            {
                return;
            }
        }
        List<PlayerControl> SelectPlayers = new();
        bool IsQuarreledDup = CustomOptionHolder.LoversDuplicationQuarreled.GetBool();
        if (CustomOptionHolder.LoversOnlyCrewmate.GetBool())
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.IsImpostor() && !p.IsNeutral() && !p.IsRole(RoleId.truelover, RoleId.LoversBreaker) && !p.IsBot())
                {
                    if (!IsQuarreledDup || !p.IsQuarreled())
                    {
                        SelectPlayers.Add(p);
                    }
                }
            }
        }
        else
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!IsQuarreledDup || (!p.IsQuarreled() && !p.IsBot()))
                {
                    if (!p.IsRole(RoleId.truelover, RoleId.LoversBreaker))
                    {
                        SelectPlayers.Add(p);
                    }
                }
            }
        }
        for (int i = 0; i < CustomOptionHolder.LoversTeamCount.GetFloat(); i++)
        {
            if (SelectPlayers.Count is not (1 or 0))
            {
                List<PlayerControl> listData = new();
                for (int i2 = 0; i2 < 2; i2++)
                {
                    var player = ModHelpers.GetRandomIndex(SelectPlayers);
                    listData.Add(SelectPlayers[player]);
                    SelectPlayers.RemoveAt(player);
                }
                RoleHelpers.SetLovers(listData[0], listData[1]);
                RoleHelpers.SetLoversRPC(listData[0], listData[1]);
            }
        }
        ChacheManager.ResetLoversChache();
    }
    public static void SetPlayerNum()
    {
        ImpostorGhostRolePlayerNum = CustomOptionHolder.impostorGhostRolesCountMax.GetInt();
        NeutralGhostRolePlayerNum = CustomOptionHolder.neutralGhostRolesCountMax.GetInt();
        CrewmatePlayerNum = CustomOptionHolder.crewmateRolesCountMax.GetInt();
        CrewmateGhostRolePlayerNum = CustomOptionHolder.crewmateGhostRolesCountMax.GetInt();
    }
    public static HashSet<RoleId> RandomSelect(AssignType assignType, ref int CanAssignPlayerCount, List<PlayerControl> AssignTargets)
    {
        if (assignType.HasFlag(AssignType.TenPar) || assignType.HasFlag(AssignType.NotTenPar))
        {
            throw new ArgumentException("AssignType has TenPar or Not TenPar.\nAssignTypeにTenParとNotTenParは同時に指定できません");
        }
        HashSet<RoleId> AssignedRoles = new();
        if (CanAssignPlayerCount <= 0)
            return AssignedRoles;
        (bool, bool) isTry = (AssignTickets.TryGetValue(assignType | AssignType.TenPar, out List<RoleId> TenParTickets), AssignTickets.TryGetValue(assignType | AssignType.NotTenPar, out List<RoleId> NotTenParTickets));
        if (!isTry.Item1 && isTry.Item2) return AssignedRoles;
        if (TenParTickets == null && NotTenParTickets == null) return AssignedRoles;
        if (TenParTickets == null)
            TenParTickets = new();
        if (NotTenParTickets == null)
            NotTenParTickets = new();
        Dictionary<RoleId, int> RemainingAssignPlayerCount = new();
        List<RoleId>[] TargetTickets = new List<RoleId>[2] { TenParTickets, NotTenParTickets };
        foreach (List<RoleId> Ticket in TargetTickets)
        {
            while (Ticket.Count > 0 && AssignTargets.Count > 0 && CanAssignPlayerCount > 0)
            {
                CanAssignPlayerCount--;
                AssignedRoles.Add(
                    SelectAndAssignRole(Ticket, RemainingAssignPlayerCount, AssignTargets)
                );
            }
            if (AssignTargets.Count <= 0)
                break;
        }
        return AssignedRoles;
    }
    private static RoleId SelectAndAssignRole(
           List<RoleId> Tickets,
           Dictionary<RoleId, int> RemainingAssignPlayerCount,
           List<PlayerControl> AssignTargets
        )
    {
        HashSet<RoleId> AssignedRoles = new();

        //対象役職を選出
        RoleId selectRole = ModHelpers.GetRandom(Tickets);

        //残り選出可能回数を取得
        if (!RemainingAssignPlayerCount.ContainsKey(selectRole))
            RemainingAssignPlayerCount.Add(selectRole, GetPlayerCount(selectRole));

        //対象を選出
        int TargetIndex = ModHelpers.GetRandomIndex(AssignTargets);
        AssignTargets[TargetIndex].SetRoleRPC(selectRole);
        AssignTargets.RemoveAt(TargetIndex);

        //残り選出可能回数を減らす
        RemainingAssignPlayerCount[selectRole]--;

        //選出可能回数が0になったらリストから削除
        if (RemainingAssignPlayerCount[selectRole] <= 0)
            Tickets.RemoveAll(x => x == selectRole);

        return selectRole;
    }
    public static void ImpostorRandomSelect()
    {
        int count = CustomOptionHolder.impostorRolesCountMax.GetInt();
        HashSet<RoleId> AssignedRoles = RandomSelect(
            AssignType.Impostor,
            ref count,
            ImpostorPlayers);
        //マーリンを選ぶ
        if (AssignedRoles.Contains(RoleId.Assassin) && CrewmatePlayerNum > 0)
        {
            Dictionary<RoleId, int> RemainingAssignPlayerCount = new();
            List<RoleId> Ticket = new() { RoleId.Marlin };
            while (Ticket.Count > 0 && CrewmatePlayers.Count > 0 && CrewmatePlayerNum > 0)
            {
                CrewmatePlayerNum--;
                AssignedRoles.Add(
                    SelectAndAssignRole(Ticket, RemainingAssignPlayerCount, CrewmatePlayers)
                );
            }
        }
    }
    public static void NeutralRandomSelect()
    {
        int count = CustomOptionHolder.neutralRolesCountMax.GetInt();
        HashSet<RoleId> AssignedRoles = RandomSelect(
            AssignType.Neutral,
            ref count,
            CrewmatePlayers);
        if (AssignedRoles.Count <= 0) return;
        //革命者を選ぶ
        if (AssignedRoles.Contains(RoleId.Revolutionist) && CrewmatePlayerNum > 0)
        {
            Dictionary<RoleId, int> RemainingAssignPlayerCount = new();
            List<RoleId> Ticket = new() { RoleId.Dictator };
            while (Ticket.Count > 0 && CrewmatePlayers.Count > 0 && CrewmatePlayerNum > 0)
            {
                CrewmatePlayerNum--;
                AssignedRoles.Add(
                    SelectAndAssignRole(Ticket, RemainingAssignPlayerCount, CrewmatePlayers)
                );
            }
        }
    }
    public static void CrewmateRandomSelect()
    {
        HashSet<RoleId> AssignedRoles = RandomSelect(
            AssignType.Crewmate,
            ref CrewmatePlayerNum,
            CrewmatePlayers);
    }

    #region GetPlayerCount
    public static int GetPlayerCount(RoleId roleData)
    {
        OptionInfo optionInfo = OptionInfo.GetOptionInfo(roleData);
        if (optionInfo != null)
            return optionInfo.PlayerCount;
        return roleData switch
        {
            RoleId.SoothSayer => CustomOptionHolder.SoothSayerPlayerCount.GetInt(),
            RoleId.Jester => CustomOptionHolder.JesterPlayerCount.GetInt(),
            RoleId.Lighter => CustomOptionHolder.LighterPlayerCount.GetInt(),
            RoleId.EvilLighter => CustomOptionHolder.EvilLighterPlayerCount.GetInt(),
            RoleId.EvilScientist => CustomOptionHolder.EvilScientistPlayerCount.GetInt(),
            RoleId.Sheriff => CustomOptionHolder.SheriffPlayerCount.GetInt(),
            RoleId.MeetingSheriff => CustomOptionHolder.MeetingSheriffPlayerCount.GetInt(),
            RoleId.Jackal => CustomOptionHolder.JackalPlayerCount.GetInt(),
            RoleId.Teleporter => CustomOptionHolder.TeleporterPlayerCount.GetInt(),
            RoleId.SpiritMedium => CustomOptionHolder.SpiritMediumPlayerCount.GetInt(),
            RoleId.SpeedBooster => CustomOptionHolder.SpeedBoosterPlayerCount.GetInt(),
            RoleId.EvilSpeedBooster => CustomOptionHolder.EvilSpeedBoosterPlayerCount.GetInt(),
            RoleId.Tasker => CustomOptionHolder.TaskerPlayerCount.GetInt(),
            RoleId.Doorr => CustomOptionHolder.DoorrPlayerCount.GetInt(),
            RoleId.EvilDoorr => CustomOptionHolder.EvilDoorrPlayerCount.GetInt(),
            RoleId.Shielder => CustomOptionHolder.ShielderPlayerCount.GetInt(),
            RoleId.Speeder => CustomOptionHolder.SpeederPlayerCount.GetInt(),
            RoleId.Freezer => CustomOptionHolder.FreezerPlayerCount.GetInt(),
            RoleId.Vulture => CustomOptionHolder.VulturePlayerCount.GetInt(),
            RoleId.NiceScientist => CustomOptionHolder.NiceScientistPlayerCount.GetInt(),
            RoleId.Clergyman => CustomOptionHolder.ClergymanPlayerCount.GetInt(),
            RoleId.Madmate => CustomOptionHolder.MadmatePlayerCount.GetInt(),
            RoleId.Bait => CustomOptionHolder.BaitPlayerCount.GetInt(),
            RoleId.HomeSecurityGuard => CustomOptionHolder.HomeSecurityGuardPlayerCount.GetInt(),
            RoleId.StuntMan => CustomOptionHolder.StuntManPlayerCount.GetInt(),
            RoleId.Moving => CustomOptionHolder.MovingPlayerCount.GetInt(),
            RoleId.Opportunist => CustomOptionHolder.OpportunistPlayerCount.GetInt(),
            RoleId.NiceGambler => CustomOptionHolder.NiceGamblerPlayerCount.GetInt(),
            RoleId.EvilGambler => CustomOptionHolder.EvilGamblerPlayerCount.GetInt(),
            RoleId.Bestfalsecharge => CustomOptionHolder.BestfalsechargePlayerCount.GetInt(),
            RoleId.Researcher => CustomOptionHolder.ResearcherPlayerCount.GetInt(),
            RoleId.SelfBomber => CustomOptionHolder.SelfBomberPlayerCount.GetInt(),
            RoleId.God => CustomOptionHolder.GodPlayerCount.GetInt(),
            RoleId.AllCleaner => CustomOptionHolder.AllCleanerPlayerCount.GetInt(),
            RoleId.NiceNekomata => CustomOptionHolder.NiceNekomataPlayerCount.GetInt(),
            RoleId.EvilNekomata => CustomOptionHolder.EvilNekomataPlayerCount.GetInt(),
            RoleId.JackalFriends => CustomOptionHolder.JackalFriendsPlayerCount.GetInt(),
            RoleId.Doctor => CustomOptionHolder.DoctorPlayerCount.GetInt(),
            RoleId.CountChanger => CustomOptionHolder.CountChangerPlayerCount.GetInt(),
            RoleId.Pursuer => CustomOptionHolder.PursuerPlayerCount.GetInt(),
            RoleId.Minimalist => CustomOptionHolder.MinimalistPlayerCount.GetInt(),
            RoleId.Hawk => CustomOptionHolder.HawkPlayerCount.GetInt(),
            RoleId.Egoist => CustomOptionHolder.EgoistPlayerCount.GetInt(),
            RoleId.NiceRedRidingHood => CustomOptionHolder.NiceRedRidingHoodPlayerCount.GetInt(),
            RoleId.EvilEraser => CustomOptionHolder.EvilEraserPlayerCount.GetInt(),
            RoleId.Workperson => CustomOptionHolder.WorkpersonPlayerCount.GetInt(),
            RoleId.Magaziner => CustomOptionHolder.MagazinerPlayerCount.GetInt(),
            RoleId.Mayor => CustomOptionHolder.MayorPlayerCount.GetInt(),
            RoleId.truelover => CustomOptionHolder.trueloverPlayerCount.GetInt(),
            RoleId.Technician => CustomOptionHolder.TechnicianPlayerCount.GetInt(),
            RoleId.SerialKiller => CustomOptionHolder.SerialKillerPlayerCount.GetInt(),
            RoleId.OverKiller => CustomOptionHolder.OverKillerPlayerCount.GetInt(),
            RoleId.Levelinger => CustomOptionHolder.LevelingerPlayerCount.GetInt(),
            RoleId.EvilMoving => CustomOptionHolder.EvilMovingPlayerCount.GetInt(),
            RoleId.Amnesiac => CustomOptionHolder.AmnesiacPlayerCount.GetInt(),
            RoleId.SideKiller => CustomOptionHolder.SideKillerPlayerCount.GetInt(),
            RoleId.Survivor => CustomOptionHolder.SurvivorPlayerCount.GetInt(),
            RoleId.MadMayor => CustomOptionHolder.MadMayorPlayerCount.GetInt(),
            RoleId.NiceHawk => CustomOptionHolder.NiceHawkPlayerCount.GetInt(),
            RoleId.Bakery => CustomOptionHolder.BakeryPlayerCount.GetInt(),
            RoleId.MadJester => CustomOptionHolder.MadJesterPlayerCount.GetInt(),
            RoleId.MadStuntMan => CustomOptionHolder.MadStuntManPlayerCount.GetInt(),
            RoleId.MadHawk => CustomOptionHolder.MadHawkPlayerCount.GetInt(),
            RoleId.FalseCharges => CustomOptionHolder.FalseChargesPlayerCount.GetInt(),
            RoleId.NiceTeleporter => CustomOptionHolder.NiceTeleporterPlayerCount.GetInt(),
            RoleId.Celebrity => CustomOptionHolder.CelebrityPlayerCount.GetInt(),
            RoleId.Nocturnality => CustomOptionHolder.NocturnalityPlayerCount.GetInt(),
            RoleId.Observer => CustomOptionHolder.ObserverPlayerCount.GetInt(),
            RoleId.Vampire => CustomOptionHolder.VampirePlayerCount.GetInt(),
            RoleId.DarkKiller => CustomOptionHolder.DarkKillerPlayerCount.GetInt(),
            RoleId.Seer => CustomOptionHolder.SeerPlayerCount.GetInt(),
            RoleId.MadSeer => CustomOptionHolder.MadSeerPlayerCount.GetInt(),
            RoleId.RemoteSheriff => CustomOptionHolder.RemoteSheriffPlayerCount.GetInt(),
            RoleId.Fox => CustomOptionHolder.FoxPlayerCount.GetInt(),
            RoleId.TeleportingJackal => CustomOptionHolder.TeleportingJackalPlayerCount.GetInt(),
            RoleId.MadMaker => CustomOptionHolder.MadMakerPlayerCount.GetInt(),
            RoleId.Demon => CustomOptionHolder.DemonPlayerCount.GetInt(),
            RoleId.TaskManager => CustomOptionHolder.TaskManagerPlayerCount.GetInt(),
            RoleId.SeerFriends => CustomOptionHolder.SeerFriendsPlayerCount.GetInt(),
            RoleId.JackalSeer => CustomOptionHolder.JackalSeerPlayerCount.GetInt(),
            RoleId.Assassin => CustomOptionHolder.AssassinPlayerCount.GetInt(),
            RoleId.Marlin => CustomOptionHolder.MarlinPlayerCount.GetInt(),
            RoleId.Arsonist => CustomOptionHolder.ArsonistPlayerCount.GetInt(),
            RoleId.Chief => CustomOptionHolder.ChiefPlayerCount.GetInt(),
            RoleId.Cleaner => CustomOptionHolder.CleanerPlayerCount.GetInt(),
            RoleId.MadCleaner => CustomOptionHolder.MadCleanerPlayerCount.GetInt(),
            RoleId.Samurai => CustomOptionHolder.SamuraiPlayerCount.GetInt(),
            RoleId.MayorFriends => CustomOptionHolder.MayorFriendsPlayerCount.GetInt(),
            RoleId.VentMaker => CustomOptionHolder.VentMakerPlayerCount.GetInt(),
            RoleId.GhostMechanic => CustomOptionHolder.GhostMechanicPlayerCount.GetInt(),
            RoleId.PositionSwapper => CustomOptionHolder.PositionSwapperPlayerCount.GetInt(),
            RoleId.Tuna => CustomOptionHolder.TunaPlayerCount.GetInt(),
            RoleId.Mafia => CustomOptionHolder.MafiaPlayerCount.GetInt(),
            RoleId.BlackCat => CustomOptionHolder.BlackCatPlayerCount.GetInt(),
            RoleId.SecretlyKiller => CustomOptionHolder.SecretlyKillerPlayerCount.GetInt(),
            RoleId.Spy => CustomOptionHolder.SpyPlayerCount.GetInt(),
            RoleId.Kunoichi => CustomOptionHolder.KunoichiPlayerCount.GetInt(),
            RoleId.DoubleKiller => CustomOptionHolder.DoubleKillerPlayerCount.GetInt(),
            RoleId.Smasher => CustomOptionHolder.SmasherPlayerCount.GetInt(),
            RoleId.SuicideWisher => CustomOptionHolder.SuicideWisherPlayerCount.GetInt(),
            RoleId.Neet => CustomOptionHolder.NeetPlayerCount.GetInt(),
            RoleId.ToiletFan => CustomOptionHolder.ToiletFanPlayerCount.GetInt(),
            RoleId.EvilButtoner => CustomOptionHolder.EvilButtonerPlayerCount.GetInt(),
            RoleId.NiceButtoner => CustomOptionHolder.NiceButtonerPlayerCount.GetInt(),
            RoleId.Finder => CustomOptionHolder.FinderPlayerCount.GetInt(),
            RoleId.Revolutionist => CustomOptionHolder.RevolutionistPlayerCount.GetInt(),
            RoleId.Dictator => CustomOptionHolder.DictatorPlayerCount.GetInt(),
            RoleId.Spelunker => CustomOptionHolder.SpelunkerPlayerCount.GetInt(),
            RoleId.SuicidalIdeation => CustomOptionHolder.SuicidalIdeationPlayerCount.GetInt(),
            RoleId.Hitman => CustomOptionHolder.HitmanPlayerCount.GetInt(),
            RoleId.Matryoshka => CustomOptionHolder.MatryoshkaPlayerCount.GetInt(),
            RoleId.Nun => CustomOptionHolder.NunPlayerCount.GetInt(),
            RoleId.PartTimer => CustomOptionHolder.PartTimerPlayerCount.GetInt(),
            RoleId.SatsumaAndImo => CustomOptionHolder.SatsumaAndImoPlayerCount.GetInt(),
            RoleId.Painter => CustomOptionHolder.PainterPlayerCount.GetInt(),
            RoleId.Psychometrist => CustomOptionHolder.PsychometristPlayerCount.GetInt(),
            RoleId.SeeThroughPerson => CustomOptionHolder.SeeThroughPersonPlayerCount.GetInt(),
            RoleId.Photographer => CustomOptionHolder.PhotographerPlayerCount.GetInt(),
            RoleId.Stefinder => CustomOptionHolder.StefinderPlayerCount.GetInt(),
            RoleId.ShiftActor => ShiftActor.ShiftActorPlayerCount.GetInt(),
            RoleId.ConnectKiller => CustomOptionHolder.ConnectKillerPlayerCount.GetInt(),
            RoleId.Cracker => CustomOptionHolder.CrackerPlayerCount.GetInt(),
            RoleId.NekoKabocha => NekoKabocha.NekoKabochaPlayerCount.GetInt(),
            RoleId.Doppelganger => CustomOptionHolder.DoppelgangerPlayerCount.GetInt(),
            RoleId.Werewolf => CustomOptionHolder.WerewolfPlayerCount.GetInt(),
            RoleId.Knight => Knight.KnightPlayerCount.GetInt(),
            RoleId.Pavlovsowner => CustomOptionHolder.PavlovsownerPlayerCount.GetInt(),
            RoleId.WaveCannonJackal => WaveCannonJackal.WaveCannonJackalPlayerCount.GetInt(),
            RoleId.Camouflager => CustomOptionHolder.CamouflagerPlayerCount.GetInt(),
            RoleId.HamburgerShop => CustomOptionHolder.HamburgerShopPlayerCount.GetInt(),
            RoleId.Penguin => CustomOptionHolder.PenguinPlayerCount.GetInt(),
            RoleId.Dependents => CustomOptionHolder.DependentsPlayerCount.GetInt(),
            RoleId.LoversBreaker => CustomOptionHolder.LoversBreakerPlayerCount.GetInt(),
            RoleId.Jumbo => CustomOptionHolder.JumboPlayerCount.GetInt(),
            RoleId.Worshiper => Worshiper.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Safecracker => Safecracker.SafecrackerPlayerCount.GetInt(),
            RoleId.FireFox => FireFox.FireFoxPlayerCount.GetInt(),
            RoleId.Squid => Squid.SquidPlayerCount.GetInt(),
            RoleId.DyingMessenger => DyingMessenger.DyingMessengerPlayerCount.GetInt(),
            RoleId.WiseMan => WiseMan.WiseManPlayerCount.GetInt(),
            RoleId.NiceMechanic => NiceMechanic.NiceMechanicPlayerCount.GetInt(),
            RoleId.EvilMechanic => EvilMechanic.EvilMechanicPlayerCount.GetInt(),
            RoleId.TheFirstLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetInt(),
            RoleId.TheSecondLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetInt(),
            RoleId.TheThirdLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetInt(),
            RoleId.OrientalShaman => OrientalShaman.OrientalShamanPlayerCount.GetInt(),
            RoleId.Balancer => Balancer.BalancerPlayerCount.GetInt(),
            RoleId.Pteranodon => Pteranodon.PteranodonPlayerCount.GetInt(),
            RoleId.BlackHatHacker => BlackHatHacker.BlackHatHackerPlayerCount.GetInt(),
            RoleId.PoliceSurgeon => PoliceSurgeon.CustomOptionData.PlayerCount.GetInt(),
            RoleId.MadRaccoon => MadRaccoon.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Moira => Moira.MoiraPlayerCount.GetInt(),
            RoleId.JumpDancer => JumpDancer.JumpDancerPlayerCount.GetInt(),
            RoleId.Sauner => Sauner.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Bat => Bat.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Rocket => Rocket.CustomOptionData.PlayerCount.GetInt(),
            RoleId.WellBehaver => WellBehaver.WellBehaverPlayerCount.GetInt(),
            RoleId.Pokerface => Pokerface.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Spider => Spider.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Crook => Crook.CustomOptionData.PlayerCount.GetInt(),
            RoleId.Frankenstein => Frankenstein.FrankensteinPlayerCount.GetInt(),
            // プレイヤーカウント
            _ => 1,
        };
    }
    #endregion

    public static void CrewOrImpostorSet()
    {
        CrewmatePlayers = new();
        ImpostorPlayers = new();
        foreach (PlayerControl Player in CachedPlayer.AllPlayers)
        {
            if (!Player.Data.Role.IsSimpleRole || Player.IsRole(RoleId.GM))
                continue;
            if (Player.IsImpostor())
                ImpostorPlayers.Add(Player);
            else
                CrewmatePlayers.Add(Player);
        }
    }
    /// <summary>
    /// 通常の方法で抽選が可能な役職かを判定する。
    /// </summary>
    /// <param name="id">判定対象のRoleId</param>
    /// <returns>true = 通常抽選可能, false = 通常抽選不可 (特殊な抽選, アサイン形式の役) </returns>
    internal static bool CanRoleIdElected(RoleId id)
    {
        return id switch
        {
            RoleId.DefaultRole => false,
            RoleId.GM => false,
            RoleId.HauntedWolf => false,
            RoleId.Sidekick or RoleId.SidekickSeer or RoleId.SidekickWaveCannon => true,
            RoleId.Pavlovsdogs => false,
            RoleId.ShermansServant => false,
            RoleId.Revolutionist => false,
            RoleId.Assassin => false,
            RoleId.Jumbo => false,
            RoleId.Sauner => (MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId == MapNames.Airship, // エアシップならば選出が可能
            RoleId.Nun or RoleId.Pteranodon => UnityEngine.Object.FindAnyObjectByType<MovingPlatformBehaviour>(), // ぬーんがあるならば選出が可能
            RoleId.Werewolf or RoleId.Knight => ModeHandler.IsMode(ModeId.Werewolf),
            _ => true,
        };
    }
    static List<RoleId> GetTeamChanceList(bool IsTen, TeamRoleType Team)
    {
        AssignType assignType = AssignType.Crewmate;
        switch (Team)
        {
            case TeamRoleType.Impostor:
                assignType = AssignType.Impostor;
                break;
            case TeamRoleType.Neutral:
                assignType = AssignType.Neutral;
                break;
        }
        if (IsTen)
            assignType |= AssignType.TenPar;
        else
            assignType |= AssignType.NotTenPar;
        return AssignTickets.TryGetValue(assignType, out List<RoleId> result)
            ? result
            : (AssignTickets[assignType] = new());
    }
    public static void SetChance(int selection, RoleId role, TeamRoleType Team)
    {
        if (selection == 0)
            return;
        List<RoleId> chanceList = GetTeamChanceList(selection == 10, Team);
        if (selection == 10)
        {
            chanceList.Add(role);
            return;
        }
        for (int i = 0; i < selection; i++)
            chanceList.Add(role);
    }
    public static void OneOrNotListSet()
    {
        AssignTickets = new();
        List<(RoleId role, TeamRoleType Team)> TargetRoles = new();
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            //幽霊役職は除外
            if (intro.IsGhostRole)
                continue;
            //選出できないなら除外
            if (!CanRoleIdElected(intro.RoleId))
                continue;
            TargetRoles.Add((intro.RoleId, intro.Team));
        }
        foreach (RoleInfo roleInfo in RoleInfoManager.RoleInfos.Values)
        {
            if (roleInfo.IsGhostRole)
                continue;
            if (!CanRoleIdElected(roleInfo.Role))
                continue;
            TargetRoles.Add((roleInfo.Role, roleInfo.Team));
        }
        foreach ((RoleId role, TeamRoleType team) roledata in TargetRoles)
        {
            var option = IntroData.GetOption(roledata.role);
            if (option == null) continue;
            var selection = option.GetSelection();
            SetChance(selection, roledata.role, roledata.team);
        }
        SetJumboTicket();
        //SetChance(selection, roleInfo.Role, roleInfo.Team);
        /*var Assassinselection = CustomOptionHolder.AssassinAndMarlinOption.GetSelection();
        if (Assassinselection != 0 && CrewmatePlayerNum > 0 && CrewmatePlayers.Count > 0)
        {
            if (Assassinselection == 10)
            {
                Impoonepar.Add(RoleId.Assassin);
            }
            else
            {
                for (int i = 1; i <= Assassinselection; i++)
                {
                    Imponotonepar.Add(RoleId.Assassin);
                }
            }
        }
        if (CustomOptionHolder.RevolutionistAndDictatorOption.GetSelection() != 0 && CrewmatePlayerNum > 0 && CrewmatePlayers.Count > 1)
        {
            if (CustomOptionHolder.RevolutionistAndDictatorOption.GetSelection() == 10)
            {
                Neutonepar.Add(RoleId.Revolutionist);
            }
            else
            {
                for (int i = 1; i <= CustomOptionHolder.RevolutionistAndDictatorOption.GetSelection(); i++)
                {
                    Neutnotonepar.Add(RoleId.Revolutionist);
                }
            }
        }*/
    }
    public static void SetJumboTicket()
    {
        int JumboSelection = CustomOptionHolder.JumboOption.GetSelection();
        bool IsCrewmate = ModHelpers.IsSucsessChance(CustomOptionHolder.JumboCrewmateChance.GetSelection());
        SetChance(JumboSelection, RoleId.Jumbo, IsCrewmate ? TeamRoleType.Crewmate : TeamRoleType.Impostor);
    }
}