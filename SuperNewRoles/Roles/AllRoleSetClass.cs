using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Attribute;

namespace SuperNewRoles;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class RpcSetRoleReplacer
{
    public static bool doReplace = false;
    public static CustomRpcSender sender;
    public static List<(PlayerControl, RoleTypes)> StoragedData = new();
    public static bool Prefix(PlayerControl __instance, RoleTypes roleType)
    {
        Logger.Info($"{__instance.Data.PlayerName} の役職が {roleType} になりました", "RpcSetRole");
        return true;
    }
    public static void Release()
    {
        sender.StartMessage(-1);
        foreach (var pair in StoragedData)
        {
            pair.Item1.SetRole(pair.Item2);
            sender.StartRpc(pair.Item1.NetId, RpcCalls.SetRole)
                .Write((ushort)pair.Item2)
                .EndRpc();
        }
        sender.EndMessage();
        doReplace = false;
    }
    public static void StartReplace(CustomRpcSender sender)
    {
        RpcSetRoleReplacer.sender = sender;
        StoragedData = new();
        doReplace = true;
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
    public static bool IsNotPrefix = false;
    public static bool IsRPCSetRoleOK = false;
    public static bool IsSetRoleRPC = false;
    public static bool IsShapeSet = false;
    public static bool IsNotDesync = false;
    public static bool Prefix(RoleManager __instance)
    {
        AllRoleSetClass.SetPlayerNum();
        IsNotPrefix = false;
        IsSetRoleRPC = false;
        IsRPCSetRoleOK = true;
        IsShapeSet = false;
        IsNotDesync = true;
        if (ModeHandler.IsMode(ModeId.NotImpostorCheck))
        {
            IsNotDesync = false;
        }
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
            FixedUpdate.SetRoleNames();
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
        IsSetRoleRPC = true;
        IsRPCSetRoleOK = false;
        IsNotPrefix = true;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            AllRoleSetClass.AllRoleSet();
        }
        else if (ModeHandler.IsMode(ModeId.NotImpostorCheck))
        {
            Mode.NotImpostorCheck.SelectRolePatch.SetDesync();
        }
        else if (ModeHandler.IsMode(ModeId.Detective))
        {
            Mode.Detective.Main.RoleSelect();
        }
        if (!ModeHandler.IsMode(ModeId.NotImpostorCheck) && !ModeHandler.IsMode(ModeId.BattleRoyal) && !ModeHandler.IsMode(ModeId.Default) && !ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.RpcSetRole(p.Data.Role.Role);
            }
        }
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            new LateTask(() =>
            {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    foreach (var pc in CachedPlayer.AllPlayers)
                    {
                        pc.PlayerControl.RpcSetRole(RoleTypes.Shapeshifter);
                    }
                }
            }, 3f, "SetImpostor");
        }
        AllRoleSetClass.Assigned = true;
    }
}
class AllRoleSetClass
{
    public static List<PlayerControl> impostors;
    public static List<RoleId> Impoonepar;
    public static List<RoleId> Imponotonepar;
    public static List<RoleId> Neutonepar;
    public static List<RoleId> Neutnotonepar;
    public static List<RoleId> Crewonepar;
    public static List<RoleId> Crewnotonepar;
    public static List<PlayerControl> CrewmatePlayers;
    public static List<PlayerControl> ImpostorPlayers;

    public static bool Assigned;

    public static int ImpostorPlayerNum;
    public static int ImpostorGhostRolePlayerNum;
    public static int NeutralPlayerNum;
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
        try
        {
            ImpostorRandomSelect();
        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
        }

        try
        {
            NeutralRandomSelect();
        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
        }

        try
        {
            CrewmateRandomSelect();
        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
        }
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
        ImpostorGhostRolePlayerNum = CustomOptionHolder.impostorGhostRolesCountMax.GetInt();
        NeutralPlayerNum = CustomOptionHolder.neutralRolesCountMax.GetInt();
        NeutralGhostRolePlayerNum = CustomOptionHolder.neutralGhostRolesCountMax.GetInt();
        CrewmatePlayerNum = CustomOptionHolder.crewmateRolesCountMax.GetInt();
        CrewmateGhostRolePlayerNum = CustomOptionHolder.crewmateGhostRolesCountMax.GetInt();
    }
    public static void ImpostorRandomSelect()
    {
        if (ImpostorPlayerNum == 0 || (Impoonepar.Count == 0 && Imponotonepar.Count == 0))
        {
            return;
        }
        bool IsAssassinAssigned = false;
        bool IsNotEndRandomSelect = true;
        while (IsNotEndRandomSelect)
        {
            if (Impoonepar.Count != 0)
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Impoonepar);
                RoleId selectRoleData = Impoonepar[selectRoleDataIndex];

                if (selectRoleData == RoleId.EvilSpeedBooster && CustomOptionHolder.EvilSpeedBoosterIsNotSpeedBooster.GetBool())
                {
                    try
                    {
                        int index = 0;
                        foreach (var role in Crewnotonepar.ToArray())
                        {
                            if (role is RoleId.SpeedBooster)
                                Crewnotonepar.RemoveAt(index);
                            index++;
                        }
                        Crewonepar.Remove(RoleId.SpeedBooster);
                    }
                    catch
                    {

                    }
                }
                else if (selectRoleData == RoleId.Assassin)
                {
                    IsAssassinAssigned = true;
                }

                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (PlayerCount >= ImpostorPlayerNum)
                {
                    for (int i = 1; i <= ImpostorPlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                        p.SetRoleRPC(selectRoleData);
                        ImpostorPlayers.Remove(p);
                    }
                    IsNotEndRandomSelect = false;

                }
                else if (PlayerCount >= ImpostorPlayers.Count)
                {
                    foreach (PlayerControl Player in ImpostorPlayers)
                    {
                        ImpostorPlayerNum--;
                        Player.SetRoleRPC(selectRoleData);
                    }
                    IsNotEndRandomSelect = false;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        ImpostorPlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                        p.SetRoleRPC(selectRoleData);
                        ImpostorPlayers.Remove(p);
                    }
                }
                Impoonepar.RemoveAt(selectRoleDataIndex);
            }
            else if (Imponotonepar.Count <= 0)
            {
                IsNotEndRandomSelect = false;
                break;
            }
            else
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Imponotonepar);
                RoleId selectRoleData = Imponotonepar[selectRoleDataIndex];

                if (selectRoleData == RoleId.EvilSpeedBooster && CustomOptionHolder.EvilSpeedBoosterIsNotSpeedBooster.GetBool())
                {
                    try
                    {
                        int index = 0;
                        foreach (var role in Crewnotonepar.ToArray())
                        {
                            if (role is RoleId.SpeedBooster)
                                Crewnotonepar.RemoveAt(index);
                            index++;
                        }
                        Crewonepar.Remove(RoleId.SpeedBooster);
                    }
                    catch
                    {

                    }
                }
                else if (selectRoleData == RoleId.Assassin)
                {
                    IsAssassinAssigned = true;
                }

                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (PlayerCount >= ImpostorPlayerNum)
                {
                    for (int i = 1; i <= ImpostorPlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                        p.SetRoleRPC(selectRoleData);
                        ImpostorPlayers.Remove(p);
                    }
                    IsNotEndRandomSelect = false;

                }
                else if (PlayerCount >= ImpostorPlayers.Count)
                {
                    foreach (PlayerControl Player in ImpostorPlayers)
                    {
                        Player.SetRoleRPC(selectRoleData);
                    }
                    IsNotEndRandomSelect = false;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        ImpostorPlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                        p.SetRoleRPC(selectRoleData);
                        ImpostorPlayers.Remove(p);
                    }
                }
                for (int i1 = 1; i1 <= 15; i1++)
                {
                    for (int i = 1; i <= Imponotonepar.Count; i++)
                    {
                        if (Imponotonepar[i - 1] == selectRoleData)
                        {
                            Imponotonepar.RemoveAt(i - 1);
                        }
                    }
                }
            }
        }

        //マーリンを選ぶ
        if (IsAssassinAssigned)
        {
            int PlayerCount = (int)GetPlayerCount(RoleId.Marlin);
            if (PlayerCount >= CrewmatePlayerNum)
            {
                for (int i = 1; i <= CrewmatePlayerNum; i++)
                {
                    PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                    p.SetRoleRPC(RoleId.Marlin);
                    CrewmatePlayers.Remove(p);
                }
                CrewmatePlayerNum = 0;
            }
            else if (PlayerCount >= CrewmatePlayers.Count)
            {
                foreach (PlayerControl Player in CrewmatePlayers)
                {
                    Player.SetRoleRPC(RoleId.Marlin);
                }
                CrewmatePlayerNum = 0;
            }
            else
            {
                for (int i = 1; i <= PlayerCount; i++)
                {
                    CrewmatePlayerNum--;
                    PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                    p.SetRoleRPC(RoleId.Marlin);
                    CrewmatePlayers.Remove(p);
                }
            }
        }
    }
    public static void NeutralRandomSelect()
    {
        if (NeutralPlayerNum == 0 || (Neutonepar.Count == 0 && Neutnotonepar.Count == 0))
        {
            return;
        }
        bool IsNotEndRandomSelect = true;
        //各役職のフラグ
        bool IsRevolutionistAssigned = false;

        while (IsNotEndRandomSelect)
        {
            if (Neutonepar.Count > 0)
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Neutonepar);
                RoleId selectRoleData = Neutonepar[selectRoleDataIndex];

                if (selectRoleData == RoleId.Revolutionist)
                {
                    IsRevolutionistAssigned = true;
                }

                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (selectRoleData is not RoleId.TheFirstLittlePig)
                {
                    if (PlayerCount >= NeutralPlayerNum)
                    {
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(selectRoleData);
                            CrewmatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewmatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewmatePlayers)
                        {
                            NeutralPlayerNum--;
                            Player.SetRoleRPC(selectRoleData);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            NeutralPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(selectRoleData);
                            CrewmatePlayers.Remove(p);
                        }
                    }
                }
                else if (selectRoleData is RoleId.TheFirstLittlePig)
                {
                    if (PlayerCount * 3 >= NeutralPlayerNum)
                    {
                        int TheThreeLittlePigsTeam = (int)Math.Truncate(NeutralPlayerNum / 3f);
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            List<PlayerControl> TheThreeLittlePigsPlayer = new();
                            // 1番目の仔豚決定
                            PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheFirstLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 2番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheSecondLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 3番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheThirdLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            NeutralPlayerNum = NeutralPlayerNum - 3;
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetTheThreeLittlePigsTeam);
                            writer.Write(TheThreeLittlePigsPlayer[0].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[1].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[2].PlayerId);
                            writer.EndRPC();
                            RPCProcedure.SetTheThreeLittlePigsTeam(TheThreeLittlePigsPlayer[0].PlayerId, TheThreeLittlePigsPlayer[1].PlayerId, TheThreeLittlePigsPlayer[2].PlayerId);
                        }
                        if (0 >= NeutralPlayerNum || 0 >= CrewmatePlayers.Count)
                            IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount * 3 >= CrewmatePlayers.Count)
                    {
                        int TheThreeLittlePigsTeam = (int)Math.Truncate(CrewmatePlayers.Count / 3f);
                        for (int i = 1; i <= CrewmatePlayers.Count; i++)
                        {
                            List<PlayerControl> TheThreeLittlePigsPlayer = new();
                            // 1番目の仔豚決定
                            PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheFirstLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 2番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheSecondLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 3番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheThirdLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            NeutralPlayerNum = NeutralPlayerNum - 3;
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetTheThreeLittlePigsTeam);
                            writer.Write(TheThreeLittlePigsPlayer[0].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[1].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[2].PlayerId);
                            writer.EndRPC();
                            RPCProcedure.SetTheThreeLittlePigsTeam(TheThreeLittlePigsPlayer[0].PlayerId, TheThreeLittlePigsPlayer[1].PlayerId, TheThreeLittlePigsPlayer[2].PlayerId);
                        }
                        if (0 >= NeutralPlayerNum || 0 >= CrewmatePlayers.Count)
                            IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            List<PlayerControl> TheThreeLittlePigsPlayer = new();
                            // 1番目の仔豚決定
                            PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheFirstLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 2番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheSecondLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            // 3番目の仔豚決定
                            p = ModHelpers.GetRandom(CrewmatePlayers);
                            p.SetRoleRPC(RoleId.TheThirdLittlePig);
                            TheThreeLittlePigsPlayer.Add(p);
                            CrewmatePlayers.Remove(p);
                            NeutralPlayerNum = NeutralPlayerNum - 3;
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetTheThreeLittlePigsTeam);
                            writer.Write(TheThreeLittlePigsPlayer[0].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[1].PlayerId);
                            writer.Write(TheThreeLittlePigsPlayer[2].PlayerId);
                            writer.EndRPC();
                            RPCProcedure.SetTheThreeLittlePigsTeam(TheThreeLittlePigsPlayer[0].PlayerId, TheThreeLittlePigsPlayer[1].PlayerId, TheThreeLittlePigsPlayer[2].PlayerId);
                        }
                    }
                }
                Neutonepar.RemoveAt(selectRoleDataIndex);
            }
            else if (Neutnotonepar.Count <= 0)
            {
                IsNotEndRandomSelect = false;
                break;
            }
            else
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Neutnotonepar);
                RoleId selectRoleData = Neutnotonepar[selectRoleDataIndex];

                if (selectRoleData == RoleId.Revolutionist)
                {
                    IsRevolutionistAssigned = true;
                }

                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (PlayerCount >= NeutralPlayerNum)
                {
                    for (int i = 1; i <= NeutralPlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                    IsNotEndRandomSelect = false;
                }
                else if (PlayerCount >= CrewmatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewmatePlayers)
                    {
                        Player.SetRoleRPC(selectRoleData);
                    }
                    IsNotEndRandomSelect = false;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        NeutralPlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                }
                for (int i1 = 1; i1 <= 15; i1++)
                {
                    for (int i = 1; i <= Neutnotonepar.Count; i++)
                    {
                        if (Neutnotonepar[i - 1] == selectRoleData)
                        {
                            Neutnotonepar.RemoveAt(i - 1);
                        }
                    }
                }
            }
        }

        //革命者を選ぶ
        Logger.Info(IsRevolutionistAssigned.ToString(), "Dictator");
        if (IsRevolutionistAssigned)
        {
            int PlayerCount = (int)GetPlayerCount(RoleId.Dictator);
            if (PlayerCount >= CrewmatePlayerNum)
            {
                for (int i = 1; i <= CrewmatePlayerNum; i++)
                {
                    int index = ModHelpers.GetRandomIndex(CrewmatePlayers);
                    PlayerControl p = CrewmatePlayers[index];
                    p.SetRoleRPC(RoleId.Dictator);
                    CrewmatePlayers.RemoveAt(index);
                }
                CrewmatePlayerNum = 0;
            }
            else if (PlayerCount >= CrewmatePlayers.Count)
            {
                foreach (PlayerControl Player in CrewmatePlayers)
                {
                    Player.SetRoleRPC(RoleId.Dictator);
                }
                CrewmatePlayers = new();
                CrewmatePlayerNum = 0;
            }
            else
            {
                for (int i = 1; i <= PlayerCount; i++)
                {
                    CrewmatePlayerNum--;
                    int Index = ModHelpers.GetRandomIndex(CrewmatePlayers);
                    PlayerControl p = CrewmatePlayers[Index];
                    p.SetRoleRPC(RoleId.Dictator);
                    CrewmatePlayers.RemoveAt(Index);
                }
            }
        }
    }
    public static void CrewmateRandomSelect()
    {
        if (CrewmatePlayerNum <= 0 || (Crewonepar.Count <= 0 && Crewnotonepar.Count <= 0))
        {
            return;
        }
        bool IsNotEndRandomSelect = true;
        while (IsNotEndRandomSelect)
        {
            if (Crewonepar.Count > 0)
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Crewonepar);
                RoleId selectRoleData = Crewonepar[selectRoleDataIndex];
                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (PlayerCount >= CrewmatePlayerNum)
                {
                    for (int i = 1; i <= CrewmatePlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                    IsNotEndRandomSelect = false;
                }
                else if (PlayerCount >= CrewmatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewmatePlayers)
                    {
                        CrewmatePlayerNum--;
                        Player.SetRoleRPC(selectRoleData);
                    }
                    IsNotEndRandomSelect = false;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        CrewmatePlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                }
                Crewonepar.RemoveAt(selectRoleDataIndex);
            }
            else if (Crewnotonepar.Count <= 0)
            {
                IsNotEndRandomSelect = false;
                break;
            }
            else
            {
                int selectRoleDataIndex = ModHelpers.GetRandomIndex(Crewnotonepar);
                RoleId selectRoleData = Crewnotonepar[selectRoleDataIndex];
                int PlayerCount = (int)GetPlayerCount(selectRoleData);
                if (PlayerCount >= CrewmatePlayerNum)
                {
                    for (int i = 1; i <= CrewmatePlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                    IsNotEndRandomSelect = false;
                }
                else if (PlayerCount >= CrewmatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewmatePlayers)
                    {
                        Player.SetRoleRPC(selectRoleData);
                    }
                    IsNotEndRandomSelect = false;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        CrewmatePlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewmatePlayers);
                        p.SetRoleRPC(selectRoleData);
                        CrewmatePlayers.Remove(p);
                    }
                }
                for (int i1 = 1; i1 <= 15; i1++)
                {
                    for (int i = 1; i <= Crewnotonepar.Count; i++)
                    {
                        if (Crewnotonepar[i - 1] == selectRoleData)
                        {
                            Crewnotonepar.RemoveAt(i - 1);
                        }
                    }
                }
            }
        }
    }
    public static float GetPlayerCount(RoleId roleData)
    {
        return roleData switch
        {
            RoleId.SoothSayer => CustomOptionHolder.SoothSayerPlayerCount.GetFloat(),
            RoleId.Jester => CustomOptionHolder.JesterPlayerCount.GetFloat(),
            RoleId.Lighter => CustomOptionHolder.LighterPlayerCount.GetFloat(),
            RoleId.EvilLighter => CustomOptionHolder.EvilLighterPlayerCount.GetFloat(),
            RoleId.EvilScientist => CustomOptionHolder.EvilScientistPlayerCount.GetFloat(),
            RoleId.Sheriff => CustomOptionHolder.SheriffPlayerCount.GetFloat(),
            RoleId.MeetingSheriff => CustomOptionHolder.MeetingSheriffPlayerCount.GetFloat(),
            RoleId.Jackal => CustomOptionHolder.JackalPlayerCount.GetFloat(),
            RoleId.Teleporter => CustomOptionHolder.TeleporterPlayerCount.GetFloat(),
            RoleId.SpiritMedium => CustomOptionHolder.SpiritMediumPlayerCount.GetFloat(),
            RoleId.SpeedBooster => CustomOptionHolder.SpeedBoosterPlayerCount.GetFloat(),
            RoleId.EvilSpeedBooster => CustomOptionHolder.EvilSpeedBoosterPlayerCount.GetFloat(),
            RoleId.Tasker => CustomOptionHolder.TaskerPlayerCount.GetFloat(),
            RoleId.Doorr => CustomOptionHolder.DoorrPlayerCount.GetFloat(),
            RoleId.EvilDoorr => CustomOptionHolder.EvilDoorrPlayerCount.GetFloat(),
            RoleId.Shielder => CustomOptionHolder.ShielderPlayerCount.GetFloat(),
            RoleId.Speeder => CustomOptionHolder.SpeederPlayerCount.GetFloat(),
            RoleId.Freezer => CustomOptionHolder.FreezerPlayerCount.GetFloat(),
            RoleId.NiceGuesser => CustomOptionHolder.NiceGuesserPlayerCount.GetFloat(),
            RoleId.EvilGuesser => CustomOptionHolder.EvilGuesserPlayerCount.GetFloat(),
            RoleId.Vulture => CustomOptionHolder.VulturePlayerCount.GetFloat(),
            RoleId.NiceScientist => CustomOptionHolder.NiceScientistPlayerCount.GetFloat(),
            RoleId.Clergyman => CustomOptionHolder.ClergymanPlayerCount.GetFloat(),
            RoleId.Madmate => CustomOptionHolder.MadmatePlayerCount.GetFloat(),
            RoleId.Bait => CustomOptionHolder.BaitPlayerCount.GetFloat(),
            RoleId.HomeSecurityGuard => CustomOptionHolder.HomeSecurityGuardPlayerCount.GetFloat(),
            RoleId.StuntMan => CustomOptionHolder.StuntManPlayerCount.GetFloat(),
            RoleId.Moving => CustomOptionHolder.MovingPlayerCount.GetFloat(),
            RoleId.Opportunist => CustomOptionHolder.OpportunistPlayerCount.GetFloat(),
            RoleId.NiceGambler => CustomOptionHolder.NiceGamblerPlayerCount.GetFloat(),
            RoleId.EvilGambler => CustomOptionHolder.EvilGamblerPlayerCount.GetFloat(),
            RoleId.Bestfalsecharge => CustomOptionHolder.BestfalsechargePlayerCount.GetFloat(),
            RoleId.Researcher => CustomOptionHolder.ResearcherPlayerCount.GetFloat(),
            RoleId.SelfBomber => CustomOptionHolder.SelfBomberPlayerCount.GetFloat(),
            RoleId.God => CustomOptionHolder.GodPlayerCount.GetFloat(),
            RoleId.AllCleaner => CustomOptionHolder.AllCleanerPlayerCount.GetFloat(),
            RoleId.NiceNekomata => CustomOptionHolder.NiceNekomataPlayerCount.GetFloat(),
            RoleId.EvilNekomata => CustomOptionHolder.EvilNekomataPlayerCount.GetFloat(),
            RoleId.JackalFriends => CustomOptionHolder.JackalFriendsPlayerCount.GetFloat(),
            RoleId.Doctor => CustomOptionHolder.DoctorPlayerCount.GetFloat(),
            RoleId.CountChanger => CustomOptionHolder.CountChangerPlayerCount.GetFloat(),
            RoleId.Pursuer => CustomOptionHolder.PursuerPlayerCount.GetFloat(),
            RoleId.Minimalist => CustomOptionHolder.MinimalistPlayerCount.GetFloat(),
            RoleId.Hawk => CustomOptionHolder.HawkPlayerCount.GetFloat(),
            RoleId.Egoist => CustomOptionHolder.EgoistPlayerCount.GetFloat(),
            RoleId.NiceRedRidingHood => CustomOptionHolder.NiceRedRidingHoodPlayerCount.GetFloat(),
            RoleId.EvilEraser => CustomOptionHolder.EvilEraserPlayerCount.GetFloat(),
            RoleId.Workperson => CustomOptionHolder.WorkpersonPlayerCount.GetFloat(),
            RoleId.Magaziner => CustomOptionHolder.MagazinerPlayerCount.GetFloat(),
            RoleId.Mayor => CustomOptionHolder.MayorPlayerCount.GetFloat(),
            RoleId.truelover => CustomOptionHolder.trueloverPlayerCount.GetFloat(),
            RoleId.Technician => CustomOptionHolder.TechnicianPlayerCount.GetFloat(),
            RoleId.SerialKiller => CustomOptionHolder.SerialKillerPlayerCount.GetFloat(),
            RoleId.OverKiller => CustomOptionHolder.OverKillerPlayerCount.GetFloat(),
            RoleId.Levelinger => CustomOptionHolder.LevelingerPlayerCount.GetFloat(),
            RoleId.EvilMoving => CustomOptionHolder.EvilMovingPlayerCount.GetFloat(),
            RoleId.Amnesiac => CustomOptionHolder.AmnesiacPlayerCount.GetFloat(),
            RoleId.SideKiller => CustomOptionHolder.SideKillerPlayerCount.GetFloat(),
            RoleId.Survivor => CustomOptionHolder.SurvivorPlayerCount.GetFloat(),
            RoleId.MadMayor => CustomOptionHolder.MadMayorPlayerCount.GetFloat(),
            RoleId.NiceHawk => CustomOptionHolder.NiceHawkPlayerCount.GetFloat(),
            RoleId.Bakery => CustomOptionHolder.BakeryPlayerCount.GetFloat(),
            RoleId.MadJester => CustomOptionHolder.MadJesterPlayerCount.GetFloat(),
            RoleId.MadStuntMan => CustomOptionHolder.MadStuntManPlayerCount.GetFloat(),
            RoleId.MadHawk => CustomOptionHolder.MadHawkPlayerCount.GetFloat(),
            RoleId.FalseCharges => CustomOptionHolder.FalseChargesPlayerCount.GetFloat(),
            RoleId.NiceTeleporter => CustomOptionHolder.NiceTeleporterPlayerCount.GetFloat(),
            RoleId.Celebrity => CustomOptionHolder.CelebrityPlayerCount.GetFloat(),
            RoleId.Nocturnality => CustomOptionHolder.NocturnalityPlayerCount.GetFloat(),
            RoleId.Observer => CustomOptionHolder.ObserverPlayerCount.GetFloat(),
            RoleId.Vampire => CustomOptionHolder.VampirePlayerCount.GetFloat(),
            RoleId.DarkKiller => CustomOptionHolder.DarkKillerPlayerCount.GetFloat(),
            RoleId.Seer => CustomOptionHolder.SeerPlayerCount.GetFloat(),
            RoleId.MadSeer => CustomOptionHolder.MadSeerPlayerCount.GetFloat(),
            RoleId.EvilSeer => EvilSeer.CustomOptionData.PlayerCount.GetFloat(),
            RoleId.RemoteSheriff => CustomOptionHolder.RemoteSheriffPlayerCount.GetFloat(),
            RoleId.Fox => CustomOptionHolder.FoxPlayerCount.GetFloat(),
            RoleId.TeleportingJackal => CustomOptionHolder.TeleportingJackalPlayerCount.GetFloat(),
            RoleId.MadMaker => CustomOptionHolder.MadMakerPlayerCount.GetFloat(),
            RoleId.Demon => CustomOptionHolder.DemonPlayerCount.GetFloat(),
            RoleId.TaskManager => CustomOptionHolder.TaskManagerPlayerCount.GetFloat(),
            RoleId.SeerFriends => CustomOptionHolder.SeerFriendsPlayerCount.GetFloat(),
            RoleId.JackalSeer => CustomOptionHolder.JackalSeerPlayerCount.GetFloat(),
            RoleId.Assassin => CustomOptionHolder.AssassinPlayerCount.GetFloat(),
            RoleId.Marlin => CustomOptionHolder.MarlinPlayerCount.GetFloat(),
            RoleId.Arsonist => CustomOptionHolder.ArsonistPlayerCount.GetFloat(),
            RoleId.Chief => CustomOptionHolder.ChiefPlayerCount.GetFloat(),
            RoleId.Cleaner => CustomOptionHolder.CleanerPlayerCount.GetFloat(),
            RoleId.MadCleaner => CustomOptionHolder.MadCleanerPlayerCount.GetFloat(),
            RoleId.Samurai => CustomOptionHolder.SamuraiPlayerCount.GetFloat(),
            RoleId.MayorFriends => CustomOptionHolder.MayorFriendsPlayerCount.GetFloat(),
            RoleId.VentMaker => CustomOptionHolder.VentMakerPlayerCount.GetFloat(),
            RoleId.GhostMechanic => CustomOptionHolder.GhostMechanicPlayerCount.GetFloat(),
            RoleId.EvilHacker => CustomOptionHolder.EvilHackerPlayerCount.GetFloat(),
            RoleId.PositionSwapper => CustomOptionHolder.PositionSwapperPlayerCount.GetFloat(),
            RoleId.Tuna => CustomOptionHolder.TunaPlayerCount.GetFloat(),
            RoleId.Mafia => CustomOptionHolder.MafiaPlayerCount.GetFloat(),
            RoleId.BlackCat => CustomOptionHolder.BlackCatPlayerCount.GetFloat(),
            RoleId.SecretlyKiller => CustomOptionHolder.SecretlyKillerPlayerCount.GetFloat(),
            RoleId.Spy => CustomOptionHolder.SpyPlayerCount.GetFloat(),
            RoleId.Kunoichi => CustomOptionHolder.KunoichiPlayerCount.GetFloat(),
            RoleId.DoubleKiller => CustomOptionHolder.DoubleKillerPlayerCount.GetFloat(),
            RoleId.Smasher => CustomOptionHolder.SmasherPlayerCount.GetFloat(),
            RoleId.SuicideWisher => CustomOptionHolder.SuicideWisherPlayerCount.GetFloat(),
            RoleId.Neet => CustomOptionHolder.NeetPlayerCount.GetFloat(),
            RoleId.ToiletFan => CustomOptionHolder.ToiletFanPlayerCount.GetFloat(),
            RoleId.EvilButtoner => CustomOptionHolder.EvilButtonerPlayerCount.GetFloat(),
            RoleId.NiceButtoner => CustomOptionHolder.NiceButtonerPlayerCount.GetFloat(),
            RoleId.Finder => CustomOptionHolder.FinderPlayerCount.GetFloat(),
            RoleId.Revolutionist => CustomOptionHolder.RevolutionistPlayerCount.GetFloat(),
            RoleId.Dictator => CustomOptionHolder.DictatorPlayerCount.GetFloat(),
            RoleId.Spelunker => CustomOptionHolder.SpelunkerPlayerCount.GetFloat(),
            RoleId.SuicidalIdeation => CustomOptionHolder.SuicidalIdeationPlayerCount.GetFloat(),
            RoleId.Hitman => CustomOptionHolder.HitmanPlayerCount.GetFloat(),
            RoleId.Matryoshka => CustomOptionHolder.MatryoshkaPlayerCount.GetFloat(),
            RoleId.Nun => CustomOptionHolder.NunPlayerCount.GetFloat(),
            RoleId.PartTimer => CustomOptionHolder.PartTimerPlayerCount.GetFloat(),
            RoleId.SatsumaAndImo => CustomOptionHolder.SatsumaAndImoPlayerCount.GetFloat(),
            RoleId.Painter => CustomOptionHolder.PainterPlayerCount.GetFloat(),
            RoleId.Psychometrist => CustomOptionHolder.PsychometristPlayerCount.GetFloat(),
            RoleId.SeeThroughPerson => CustomOptionHolder.SeeThroughPersonPlayerCount.GetFloat(),
            RoleId.Photographer => CustomOptionHolder.PhotographerPlayerCount.GetFloat(),
            RoleId.Stefinder => CustomOptionHolder.StefinderPlayerCount.GetFloat(),
            RoleId.Slugger => CustomOptionHolder.SluggerPlayerCount.GetFloat(),
            RoleId.ShiftActor => ShiftActor.ShiftActorPlayerCount.GetFloat(),
            RoleId.ConnectKiller => CustomOptionHolder.ConnectKillerPlayerCount.GetFloat(),
            RoleId.Cracker => CustomOptionHolder.CrackerPlayerCount.GetFloat(),
            RoleId.NekoKabocha => NekoKabocha.NekoKabochaPlayerCount.GetFloat(),
            RoleId.WaveCannon => CustomOptionHolder.WaveCannonPlayerCount.GetFloat(),
            RoleId.Doppelganger => CustomOptionHolder.DoppelgangerPlayerCount.GetFloat(),
            RoleId.Werewolf => CustomOptionHolder.WerewolfPlayerCount.GetFloat(),
            RoleId.Knight => Knight.KnightPlayerCount.GetFloat(),
            RoleId.Pavlovsowner => CustomOptionHolder.PavlovsownerPlayerCount.GetFloat(),
            RoleId.WaveCannonJackal => WaveCannonJackal.WaveCannonJackalPlayerCount.GetFloat(),
            RoleId.Conjurer => Conjurer.PlayerCount.GetFloat(),
            RoleId.Camouflager => CustomOptionHolder.CamouflagerPlayerCount.GetFloat(),
            RoleId.Cupid => CustomOptionHolder.CupidPlayerCount.GetFloat(),
            RoleId.HamburgerShop => CustomOptionHolder.HamburgerShopPlayerCount.GetFloat(),
            RoleId.Penguin => CustomOptionHolder.PenguinPlayerCount.GetFloat(),
            RoleId.Dependents => CustomOptionHolder.DependentsPlayerCount.GetFloat(),
            RoleId.LoversBreaker => CustomOptionHolder.LoversBreakerPlayerCount.GetFloat(),
            RoleId.Jumbo => CustomOptionHolder.JumboPlayerCount.GetFloat(),
            RoleId.Worshiper => Worshiper.CustomOptionData.PlayerCount.GetFloat(),
            RoleId.Safecracker => Safecracker.SafecrackerPlayerCount.GetFloat(),
            RoleId.FireFox => FireFox.FireFoxPlayerCount.GetFloat(),
            RoleId.Squid => Squid.SquidPlayerCount.GetFloat(),
            RoleId.DyingMessenger => DyingMessenger.DyingMessengerPlayerCount.GetFloat(),
            RoleId.WiseMan => WiseMan.WiseManPlayerCount.GetFloat(),
            RoleId.NiceMechanic => NiceMechanic.NiceMechanicPlayerCount.GetFloat(),
            RoleId.EvilMechanic => EvilMechanic.EvilMechanicPlayerCount.GetFloat(),
            RoleId.TheFirstLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetFloat(),
            RoleId.TheSecondLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetFloat(),
            RoleId.TheThirdLittlePig => TheThreeLittlePigs.TheThreeLittlePigsTeamCount.GetFloat(),
            RoleId.OrientalShaman => OrientalShaman.OrientalShamanPlayerCount.GetFloat(),
            RoleId.Balancer => Balancer.BalancerPlayerCount.GetFloat(),
            RoleId.Pteranodon => Pteranodon.PteranodonPlayerCount.GetFloat(),
            RoleId.BlackHatHacker => BlackHatHacker.BlackHatHackerPlayerCount.GetFloat(),
            RoleId.PoliceSurgeon => PoliceSurgeon.CustomOptionData.PlayerCount.GetFloat(),
            RoleId.MadRaccoon => MadRaccoon.CustomOptionData.PlayerCount.GetFloat(),
            RoleId.Moira => Moira.MoiraPlayerCount.GetFloat(),
            // プレイヤーカウント
            _ => 1,
        };
    }
    public static void CrewOrImpostorSet()
    {
        CrewmatePlayers = new();
        ImpostorPlayers = new();
        foreach (PlayerControl Player in CachedPlayer.AllPlayers)
        {
            if (Player.Data.Role.IsSimpleRole && !Player.IsRole(RoleId.GM))
            {
                if (Player.IsImpostor())
                {
                    ImpostorPlayers.Add(Player);
                }
                else
                {
                    CrewmatePlayers.Add(Player);
                }
            }
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
            RoleId.Nun or RoleId.Pteranodon => (MapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId == MapNames.Airship, // エアシップならば選出が可能
            RoleId.Werewolf or RoleId.Knight => ModeHandler.IsMode(ModeId.Werewolf),
            _ => true,
        };
    }
    public static void OneOrNotListSet()
    {
        Impoonepar = new();
        Imponotonepar = new();
        Neutonepar = new();
        Neutnotonepar = new();
        Crewonepar = new();
        Crewnotonepar = new();
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (!intro.IsGhostRole && CanRoleIdElected(intro.RoleId))
            {
                var option = IntroData.GetOption(intro.RoleId);
                if (option == null) continue;
                var selection = option.GetSelection();
                if (selection != 0)
                {
                    if (selection == 10)
                    {
                        switch (intro.Team)
                        {
                            case TeamRoleType.Crewmate:
                                Crewonepar.Add(intro.RoleId);
                                break;
                            case TeamRoleType.Impostor:
                                Impoonepar.Add(intro.RoleId);
                                break;
                            case TeamRoleType.Neutral:
                                Neutonepar.Add(intro.RoleId);
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= selection; i++)
                        {
                            switch (intro.Team)
                            {
                                case TeamRoleType.Crewmate:
                                    Crewnotonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Impostor:
                                    Imponotonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Neutral:
                                    Neutnotonepar.Add(intro.RoleId);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        SetJumboTicket();
        var Assassinselection = CustomOptionHolder.AssassinAndMarlinOption.GetSelection();
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
        }
    }
    public static void SetJumboTicket()
    {
        int JumboSelection = CustomOptionHolder.JumboOption.GetSelection();
        bool IsCrewmate = ModHelpers.IsSucsessChance(CustomOptionHolder.JumboCrewmateChance.GetSelection());
        if (JumboSelection != 0)
        {
            if (JumboSelection == 10)
            {
                if (IsCrewmate) Crewonepar.Add(RoleId.Jumbo);
                else Impoonepar.Add(RoleId.Jumbo);
            }
            else
            {
                for (int i = 1; i <= JumboSelection; i++)
                {
                    if (IsCrewmate) Crewnotonepar.Add(RoleId.Jumbo);
                    else Imponotonepar.Add(RoleId.Jumbo);
                }
            }
        }
    }
}