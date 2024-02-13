using System;
using System.Collections.Generic;
using System.Linq;
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
public class PairRoleDetail
{
    public static PairRoleDetail GetPairRoleDetail(RoleId role) {
        return PairRoleDetails.TryGetValue(role, out PairRoleDetail value) ? value : null;
    }
    public static readonly Dictionary<RoleId, PairRoleDetail> PairRoleDetails = new() {
        { RoleId.Assassin, new(RoleId.Assassin, (0,0,1), (TeamRoleType.Crewmate, RoleId.Marlin)) },
        { RoleId.Revolutionist, new(RoleId.Revolutionist, (0,0,1), (TeamRoleType.Crewmate, RoleId.Dictator)) }, 
        { RoleId.TheFirstLittlePig, new(RoleId.TheFirstLittlePig, (0,2,0), TheThreeLittlePigs.TheThreeLittlePigsOnAssigned , (TeamRoleType.Neutral, RoleId.TheSecondLittlePig), (TeamRoleType.Neutral, RoleId.TheThirdLittlePig)) },
        { RoleId.Pokerface, new(RoleId.Pokerface, (0,2,0), Pokerface.OnAssigned, (TeamRoleType.Neutral, RoleId.Pokerface), (TeamRoleType.Neutral, RoleId.Pokerface)) }
    };
    public RoleId role { get; }
    public (TeamRoleType team, RoleId role)[] pairRoles { get; }
    public (int impostor, int neutral, int crewmate) needPlayerCount { get; }
    public Action<List<(RoleId, PlayerControl)>> OnAssigned { get; } = null;
    public PairRoleDetail(RoleId role, (int impostor, int neutral, int crewmate) needPlayerCount, params (TeamRoleType team, RoleId role)[] pairRoles)
    {
        this.role = role;
        this.pairRoles = pairRoles;
        this.needPlayerCount = needPlayerCount;
    }
    public PairRoleDetail(RoleId role, (int impostor, int neutral, int crewmate) needPlayerCount, Action<List<(RoleId, PlayerControl)>> onAssigned, params (TeamRoleType team, RoleId role)[] pairRoles)
    {
        this.role = role;
        this.pairRoles = pairRoles;
        this.needPlayerCount = needPlayerCount;
        this.OnAssigned = onAssigned;
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

    public static int ImpostorPlayerNum;
    public static int NeutralPlayerNum;
    public static int CrewmatePlayerNum;
    public static int ImpostorGhostRolePlayerNum;
    public static int NeutralGhostRolePlayerNum;
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
        ImpostorPlayerNum = CustomOptionHolder.impostorRolesCountMax.GetInt();
        NeutralPlayerNum = CustomOptionHolder.neutralRolesCountMax.GetInt();
        CrewmatePlayerNum = CustomOptionHolder.crewmateRolesCountMax.GetInt();
        ImpostorGhostRolePlayerNum = CustomOptionHolder.impostorGhostRolesCountMax.GetInt();
        NeutralGhostRolePlayerNum = CustomOptionHolder.neutralGhostRolesCountMax.GetInt();
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
                var assigneddata =
                    SelectAndAssignRole(CanAssignPlayerCount, assignType, Ticket, RemainingAssignPlayerCount, AssignTargets);
                CanAssignPlayerCount-= assigneddata.assignedCount;
                foreach (var role in assigneddata.assigned)
                {
                    AssignedRoles.Add(role);
                }
            }
            if (AssignTargets.Count <= 0)
                break;
        }
        return AssignedRoles;
    }
    public static bool CheckCanAssignCount(AssignType assignType, int nowCanAssignCount, (int impostor, int neutral, int crewmate) data)
    {
        int impostorcount = 0;
        int neutralcount = CustomOptionHolder.neutralRolesCountMax.GetInt();
        int crewmatecount = CustomOptionHolder.crewmateRolesCountMax.GetInt();
        int impostorplayercount = ImpostorPlayers.Count;
        int crewmateplayercount = CrewmatePlayers.Count;
        if (assignType.HasFlag(AssignType.Impostor))
        {
            impostorcount = nowCanAssignCount - 1;
        }
        else if (assignType.HasFlag(AssignType.Neutral))
        {
            neutralcount = nowCanAssignCount - 1;
            crewmateplayercount--;
        }
        else if (assignType.HasFlag(AssignType.Crewmate))
        {
            neutralcount = 0;
            crewmatecount = nowCanAssignCount - 1;
            crewmateplayercount--;
        }
        if (impostorcount < data.impostor)
            return false;
        if (neutralcount < data.neutral)
            return false;
        if (crewmatecount < data.crewmate)
            return false;
        if (impostorplayercount < data.impostor)
            return false;
        if (crewmateplayercount < data.neutral)
            return false;
        if (crewmateplayercount < data.crewmate)
            return false;
        return true;
    }
    public static AssignType ToAssignType(TeamRoleType team)
    {
        return team switch
        {
            TeamRoleType.Impostor => AssignType.Impostor,
            TeamRoleType.Neutral => AssignType.Neutral,
            TeamRoleType.Crewmate => AssignType.Crewmate,
            _ => throw new ArgumentException("TeamRoleType is not Impostor, Neutral, Crewmate"),
        };
    }
    public static TeamRoleType ToTeamRoleType(AssignType assignType)
    {
        if (assignType.HasFlag(AssignType.Impostor))
            return TeamRoleType.Impostor;
        if (assignType.HasFlag(AssignType.Neutral))
            return TeamRoleType.Neutral;
        if (assignType.HasFlag(AssignType.Crewmate))
            return TeamRoleType.Crewmate;
        throw new ArgumentException("AssignType is not Impostor, Neutral, Crewmate");
    }
    private static (HashSet<RoleId> assigned, int assignedCount, PlayerControl assignedtarget) SelectAndAssignRole(
           int CanAssignPlayerCount,
           AssignType assignType,
           List<RoleId> Tickets,
           Dictionary<RoleId, int> RemainingAssignPlayerCount,
           List<PlayerControl> AssignTargets,
           bool onPairAssign = false
        )
    {
        int assignedCount = 0;
        HashSet<RoleId> AssignedRoles = new();

        //対象役職を選出
        RoleId selectRole = ModHelpers.GetRandom(Tickets);

        PairRoleDetail pairRoleDetail = PairRoleDetail.GetPairRoleDetail(selectRole);
        List<(RoleId, PlayerControl)> Assigneds = new();
        if (!onPairAssign && pairRoleDetail != null)
        {
            bool canAssign = CheckCanAssignCount(assignType, CanAssignPlayerCount, pairRoleDetail.needPlayerCount);
            if (!canAssign)
            {
                Tickets.RemoveAll(x => x == selectRole);
                return (new(), 0, null);
            }
            Dictionary<RoleId, int> RemainingAssignPlayerCountOnPair = new();
            // foreachで回してSelectAndAssignRoleを呼ぶ
            foreach (var pairRole in pairRoleDetail.pairRoles)
            {
                List<PlayerControl> assignTargets = null;
                if (pairRole.team == TeamRoleType.Impostor)
                    assignTargets = ImpostorPlayers;
                else if (pairRole.team is TeamRoleType.Neutral or TeamRoleType.Crewmate)
                    assignTargets = CrewmatePlayers;
                var pairassigned = SelectAndAssignRole(1, assignType, new() { pairRole.role }, RemainingAssignPlayerCount, assignTargets, true);
                Assigneds.Add((pairRole.role, pairassigned.assignedtarget));
                assignedCount += pairassigned.assignedCount;
                RemainingAssignPlayerCountOnPair[pairRole.role] = GetPlayerCount(pairRole.role) - 1;
                if (RemainingAssignPlayerCountOnPair[pairRole.role] <= 0)
                    RemainingAssignPlayerCountOnPair.Remove(pairRole.role);
                AssignedRoles.Add(pairRole.role);

            }
            //RemainingAssignPlayerCountOnPairを元にアサイン可能であればアサインする
            while (RemainingAssignPlayerCountOnPair.Count > 0)
            {
                var targetrole = ModHelpers.GetRandom(RemainingAssignPlayerCountOnPair.Keys.ToList());
                var count = RemainingAssignPlayerCountOnPair[targetrole];
                if (count <= 0)
                {
                    RemainingAssignPlayerCountOnPair.Remove(targetrole);
                    continue;
                }
                //対象を選出
                var team = pairRoleDetail.pairRoles.FirstOrDefault(x => x.role == targetrole).team;
                if (team switch
                {
                    TeamRoleType.Crewmate => CrewmatePlayerNum,
                    TeamRoleType.Neutral => NeutralPlayerNum,
                    TeamRoleType.Impostor => ImpostorPlayerNum,
                    _ => throw new ArgumentException("TeamRoleType is not Impostor, Neutral, Crewmate"),
                } <= 0)
                {
                    RemainingAssignPlayerCountOnPair.Remove(targetrole);
                    continue;
                }
                List<PlayerControl> _assignTargets = team switch
                {
                    TeamRoleType.Impostor => ImpostorPlayers,
                    TeamRoleType.Neutral => CrewmatePlayers,
                    TeamRoleType.Crewmate => CrewmatePlayers,
                    _ => throw new ArgumentException("TeamRoleType is not Impostor, Neutral, Crewmate"),
                };
                int targetIndex = ModHelpers.GetRandomIndex(_assignTargets);
                _assignTargets[targetIndex].SetRoleRPC(targetrole);
                Assigneds.Add((targetrole, _assignTargets[targetIndex]));
                _assignTargets.RemoveAt(targetIndex);
                RemainingAssignPlayerCountOnPair[targetrole]--;
                assignedCount++;
                if (RemainingAssignPlayerCountOnPair[targetrole] <= 0)
                {
                    RemainingAssignPlayerCountOnPair.Remove(targetrole);
                    continue;
                }
            }
        }

        //残り選出可能回数を取得
        if (!RemainingAssignPlayerCount.ContainsKey(selectRole))
            RemainingAssignPlayerCount.Add(selectRole, GetPlayerCount(selectRole));

        //対象を選出
        int TargetIndex = ModHelpers.GetRandomIndex(AssignTargets);
        var assignedtarget = AssignTargets[TargetIndex];
        assignedtarget.SetRoleRPC(selectRole);
        AssignTargets.RemoveAt(TargetIndex);

        Assigneds.Add((selectRole, assignedtarget));
        if (!onPairAssign && pairRoleDetail != null)
        {
            pairRoleDetail.OnAssigned?.Invoke(Assigneds);
        }

        //残り選出可能回数を減らす
        RemainingAssignPlayerCount[selectRole]--;

        //選出可能回数が0になったらリストから削除
        if (RemainingAssignPlayerCount[selectRole] <= 0)
            Tickets.RemoveAll(x => x == selectRole);

        AssignedRoles.Add(selectRole);
        assignedCount++;

        return (AssignedRoles, assignedCount, assignedtarget);
    }
    public static void ImpostorRandomSelect()
    {
        HashSet<RoleId> AssignedRoles = RandomSelect(
            AssignType.Impostor,
            ref ImpostorPlayerNum,
            ImpostorPlayers);
    }
    public static void NeutralRandomSelect()
    {
        HashSet<RoleId> AssignedRoles = RandomSelect(
            AssignType.Neutral,
            ref NeutralPlayerNum,
            CrewmatePlayers);
        if (AssignedRoles.Count <= 0) return;
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
        foreach (var assigns in AssignTickets)
        {
            Logger.Info("----------------");
            Logger.Info($"{assigns.Key} : {assigns.Value.Count}");
            Logger.Info($"IsImpostor: {assigns.Key.HasFlag(AssignType.Impostor)}");
            Logger.Info($"IsNeutral: {assigns.Key.HasFlag(AssignType.Neutral)}");
            Logger.Info($"IsCrewmate: {assigns.Key.HasFlag(AssignType.Crewmate)}");
            Logger.Info($"IsTenPar: {assigns.Key.HasFlag(AssignType.TenPar)}");
            Logger.Info($"IsNotTenPar: {assigns.Key.HasFlag(AssignType.NotTenPar)}");
            foreach (var assign in assigns.Value)
            {
                Logger.Info($"{assign}");
            }
            Logger.Info("----------------");
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