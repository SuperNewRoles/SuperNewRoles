using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRoleReplacer
    {
        public static bool doReplace = false;
        public static CustomRpcSender sender;
        public static List<(PlayerControl, RoleTypes)> StoragedData = new();
        public static bool Prefix()
        {
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
            RPCHelper.StartRPC(CustomRPC.CustomRPC.StartGameRPC).EndRPC();
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
        public static bool Prefix()
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
            /*
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                IsNotDesync = false;
            }
            */
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);
                List<PlayerControl> SelectPlayers = new();
                AllRoleSetClass.impostors = new();
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (!player.Data.Disconnected && player.IsPlayer())
                    {
                        SelectPlayers.Add(player);
                    }
                }
                for (int i = 0; i < PlayerControl.GameOptions.NumImpostors; i++)
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
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (!player.Data.Disconnected && !player.IsImpostor())
                    {
                        sender.RpcSetRole(player, RoleTypes.Crewmate);
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
            else if (ModeHandler.IsMode(ModeId.CopsRobbers))
            {
                Mode.CopsRobbers.RoleSelectHandler.Handler();
                return false;
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
            else if (ModeHandler.IsMode(ModeId.Werewolf))
            {
                Mode.Werewolf.RoleSelectHandler.RoleSelect();
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
                /*AmongUsClient.Instance.StartCoroutine(nameof(SetServerRole));*/
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
        public static List<PlayerControl> CrewMatePlayers;
        public static List<PlayerControl> ImpostorPlayers;

        public static bool Assigned;

        public static int ImpostorPlayerNum;
        public static int ImpostorGhostRolePlayerNum;
        public static int NeutralPlayerNum;
        public static int NeutralGhostRolePlayerNum;
        public static int CrewMatePlayerNum;
        public static int CrewMateGhostRolePlayerNum;

        public static void AllRoleSet()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
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
                CrewMateRandomSelect();
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("RoleSelectError:" + e);
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
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
            if (!CustomOption.CustomOptions.QuarreledOption.GetBool()) return;
            SuperNewRolesPlugin.Logger.LogInfo("クラードセレクト");
            List<PlayerControl> SelectPlayers = new();
            if (CustomOption.CustomOptions.QuarreledOnlyCrewMate.GetBool())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.IsImpostor() && !p.IsNeutral() && p.IsPlayer())
                    {
                        SelectPlayers.Add(p);
                    }
                }
            }
            else
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsPlayer())
                    {
                        SelectPlayers.Add(p);
                    }
                }
            }
            for (int i = 0; i < CustomOptions.QuarreledTeamCount.GetFloat(); i++)
            {
                if (SelectPlayers.Count is not (1 or 0))
                {
                    List<PlayerControl> Listdate = new();
                    for (int i2 = 0; i2 < 2; i2++)
                    {
                        var player = ModHelpers.GetRandomIndex<PlayerControl>(SelectPlayers);
                        Listdate.Add(SelectPlayers[player]);
                        SelectPlayers.RemoveAt(player);
                    }
                    RoleHelpers.SetQuarreled(Listdate[0], Listdate[1]);
                    RoleHelpers.SetQuarreledRPC(Listdate[0], Listdate[1]);
                }
            }
            ChacheManager.ResetQuarreledChache();
        }

        public static void LoversRandomSelect()
        {
            if (!CustomOptions.LoversOption.GetBool() || (CustomOptions.LoversPar.GetString() == "0%")) return;
            if (!(CustomOptions.LoversPar.GetString() == "100%"))
            {
                List<string> a = new();
                var SucPar = int.Parse(CustomOptions.LoversPar.GetString().Replace("0%", ""));
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
            bool IsQuarreledDup = CustomOptions.LoversDuplicationQuarreled.GetBool();
            if (CustomOptions.LoversOnlyCrewMate.GetBool())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.IsImpostor() && !p.IsNeutral() && !p.IsRole(RoleId.truelover) && p.IsPlayer())
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
                    if (!IsQuarreledDup || (!p.IsQuarreled() && p.IsPlayer()))
                    {
                        if (!p.IsRole(RoleId.truelover))
                        {
                            SelectPlayers.Add(p);
                        }
                    }
                }
            }
            for (int i = 0; i < CustomOptions.LoversTeamCount.GetFloat(); i++)
            {
                if (SelectPlayers.Count is not (1 or 0))
                {
                    List<PlayerControl> Listdate = new();
                    for (int i2 = 0; i2 < 2; i2++)
                    {
                        var player = ModHelpers.GetRandomIndex(SelectPlayers);
                        Listdate.Add(SelectPlayers[player]);
                        SelectPlayers.RemoveAt(player);
                    }
                    RoleHelpers.SetLovers(Listdate[0], Listdate[1]);
                    RoleHelpers.SetLoversRPC(Listdate[0], Listdate[1]);
                }
            }
            ChacheManager.ResetLoversChache();
        }
        public static void SetPlayerNum()
        {
            ImpostorPlayerNum = CustomOptions.impostorRolesCountMax.GetInt();
            ImpostorGhostRolePlayerNum = CustomOptions.impostorGhostRolesCountMax.GetInt();
            NeutralPlayerNum = CustomOptions.neutralRolesCountMax.GetInt();
            NeutralGhostRolePlayerNum = CustomOptions.neutralGhostRolesCountMax.GetInt();
            CrewMatePlayerNum = CustomOptions.crewmateRolesCountMax.GetInt();
            CrewMateGhostRolePlayerNum = CustomOptions.crewmateGhostRolesCountMax.GetInt();
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
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Impoonepar);
                    RoleId SelectRoleDate = Impoonepar[SelectRoleDateIndex];

                    if (SelectRoleDate == RoleId.EvilSpeedBooster)
                    {
                        try
                        {
                            for (int i1 = 1; i1 <= 15; i1++)
                            {
                                for (int i = 1; i <= Imponotonepar.Count; i++)
                                {
                                    if (Crewnotonepar[i - 1] == RoleId.SpeedBooster)
                                    {
                                        Crewnotonepar.RemoveAt(i - 1);
                                    }
                                }
                            }
                            Crewonepar.Remove(RoleId.SpeedBooster);
                        }
                        catch
                        {

                        }
                    }
                    else if (SelectRoleDate == RoleId.Assassin)
                    {
                        IsAssassinAssigned = true;
                    }

                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= ImpostorPlayerNum)
                    {
                        for (int i = 1; i <= ImpostorPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;

                    }
                    else if (PlayerCount >= ImpostorPlayers.Count)
                    {
                        foreach (PlayerControl Player in ImpostorPlayers)
                        {
                            ImpostorPlayerNum--;
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            ImpostorPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                    }
                    Impoonepar.RemoveAt(SelectRoleDateIndex);
                }
                else
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Imponotonepar);
                    RoleId SelectRoleDate = Imponotonepar[SelectRoleDateIndex];

                    if (SelectRoleDate == RoleId.EvilSpeedBooster)
                    {
                        try
                        {
                            for (int i1 = 1; i1 <= 15; i1++)
                            {
                                for (int i = 1; i <= Imponotonepar.Count; i++)
                                {
                                    if (Crewnotonepar[i - 1] == RoleId.SpeedBooster)
                                    {
                                        Crewnotonepar.RemoveAt(i - 1);
                                    }
                                }
                            }
                            Crewonepar.Remove(RoleId.SpeedBooster);
                        }
                        catch
                        {

                        }
                    }
                    else if (SelectRoleDate == RoleId.Assassin)
                    {
                        IsAssassinAssigned = true;
                    }

                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= ImpostorPlayerNum)
                    {
                        for (int i = 1; i <= ImpostorPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;

                    }
                    else if (PlayerCount >= ImpostorPlayers.Count)
                    {
                        foreach (PlayerControl Player in ImpostorPlayers)
                        {
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            ImpostorPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                    }
                    for (int i1 = 1; i1 <= 15; i1++)
                    {
                        for (int i = 1; i <= Imponotonepar.Count; i++)
                        {
                            if (Imponotonepar[i - 1] == SelectRoleDate)
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
                int PlayerCount = (int)GetPlayerCount(RoleId.Marine);
                if (PlayerCount >= CrewMatePlayerNum)
                {
                    for (int i = 1; i <= CrewMatePlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.SetRoleRPC(RoleId.Marine);
                        CrewMatePlayers.Remove(p);
                    }
                    CrewMatePlayerNum = 0;
                }
                else if (PlayerCount >= CrewMatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewMatePlayers)
                    {
                        Player.SetRoleRPC(RoleId.Marine);
                    }
                    CrewMatePlayerNum = 0;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        CrewMatePlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.SetRoleRPC(RoleId.Marine);
                        CrewMatePlayers.Remove(p);
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
            bool IsRevolutionistAssigned = false;
            while (IsNotEndRandomSelect)
            {
                if (Neutonepar.Count != 0)
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Neutonepar);
                    RoleId SelectRoleDate = Neutonepar[SelectRoleDateIndex];

                    if (SelectRoleDate == RoleId.Revolutionist)
                    {
                        IsRevolutionistAssigned = true;
                    }

                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= NeutralPlayerNum)
                    {
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            NeutralPlayerNum--;
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            NeutralPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                    }
                    Neutonepar.RemoveAt(SelectRoleDateIndex);
                }
                else
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Neutnotonepar);
                    RoleId SelectRoleDate = Neutnotonepar[SelectRoleDateIndex];

                    if (SelectRoleDate == RoleId.Revolutionist)
                    {
                        IsRevolutionistAssigned = true;
                    }

                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= NeutralPlayerNum)
                    {
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            NeutralPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                    }
                    for (int i1 = 1; i1 <= 15; i1++)
                    {
                        for (int i = 1; i <= Neutnotonepar.Count; i++)
                        {
                            if (Neutnotonepar[i - 1] == SelectRoleDate)
                            {
                                Neutnotonepar.RemoveAt(i - 1);
                            }
                        }
                    }
                }
            }

            //革命者を選ぶ
            if (IsRevolutionistAssigned)
            {
                int PlayerCount = (int)GetPlayerCount(RoleId.Dictator);
                if (PlayerCount >= CrewMatePlayerNum)
                {
                    for (int i = 1; i <= CrewMatePlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.SetRoleRPC(RoleId.Dictator);
                        CrewMatePlayers.Remove(p);
                    }
                    CrewMatePlayerNum = 0;
                }
                else if (PlayerCount >= CrewMatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewMatePlayers)
                    {
                        Player.SetRoleRPC(RoleId.Dictator);
                    }
                    CrewMatePlayerNum = 0;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        CrewMatePlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.SetRoleRPC(RoleId.Dictator);
                        CrewMatePlayers.Remove(p);
                    }
                }
            }
        }
        public static void CrewMateRandomSelect()
        {
            if (CrewMatePlayerNum <= 0 || (Crewonepar.Count <= 0 && Crewnotonepar.Count <= 0))
            {
                return;
            }
            bool IsNotEndRandomSelect = true;
            while (IsNotEndRandomSelect)
            {
                if (Crewonepar.Count > 0)
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Crewonepar);
                    RoleId SelectRoleDate = Crewonepar[SelectRoleDateIndex];
                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= CrewMatePlayerNum)
                    {
                        for (int i = 1; i <= CrewMatePlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            CrewMatePlayerNum--;
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            CrewMatePlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                    }
                    Crewonepar.RemoveAt(SelectRoleDateIndex);
                }
                else
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Crewnotonepar);
                    RoleId SelectRoleDate = Crewnotonepar[SelectRoleDateIndex];
                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= CrewMatePlayerNum)
                    {
                        for (int i = 1; i <= CrewMatePlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            Player.SetRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            CrewMatePlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.SetRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                    }
                    for (int i1 = 1; i1 <= 15; i1++)
                    {
                        for (int i = 1; i <= Crewnotonepar.Count; i++)
                        {
                            if (Crewnotonepar[i - 1] == SelectRoleDate)
                            {
                                Crewnotonepar.RemoveAt(i - 1);
                            }
                        }
                    }
                }
            }
        }
        public static float GetPlayerCount(RoleId RoleDate)
        {
            return RoleDate switch
            {
                RoleId.SoothSayer => CustomOptions.SoothSayerPlayerCount.GetFloat(),
                RoleId.Jester => CustomOptions.JesterPlayerCount.GetFloat(),
                RoleId.Lighter => CustomOptions.LighterPlayerCount.GetFloat(),
                RoleId.EvilLighter => CustomOptions.EvilLighterPlayerCount.GetFloat(),
                RoleId.EvilScientist => CustomOptions.EvilScientistPlayerCount.GetFloat(),
                RoleId.Sheriff => CustomOptions.SheriffPlayerCount.GetFloat(),
                RoleId.MeetingSheriff => CustomOptions.MeetingSheriffPlayerCount.GetFloat(),
                RoleId.Jackal => CustomOptions.JackalPlayerCount.GetFloat(),
                RoleId.Teleporter => CustomOptions.TeleporterPlayerCount.GetFloat(),
                RoleId.SpiritMedium => CustomOptions.SpiritMediumPlayerCount.GetFloat(),
                RoleId.SpeedBooster => CustomOptions.SpeedBoosterPlayerCount.GetFloat(),
                RoleId.EvilSpeedBooster => CustomOptions.EvilSpeedBoosterPlayerCount.GetFloat(),
                RoleId.Tasker => CustomOptions.TaskerPlayerCount.GetFloat(),
                RoleId.Doorr => CustomOptions.DoorrPlayerCount.GetFloat(),
                RoleId.EvilDoorr => CustomOptions.EvilDoorrPlayerCount.GetFloat(),
                RoleId.Shielder => CustomOptions.ShielderPlayerCount.GetFloat(),
                RoleId.Speeder => CustomOptions.SpeederPlayerCount.GetFloat(),
                RoleId.Freezer => CustomOptions.FreezerPlayerCount.GetFloat(),
                RoleId.Guesser => CustomOptions.GuesserPlayerCount.GetFloat(),
                RoleId.EvilGuesser => CustomOptions.EvilGuesserPlayerCount.GetFloat(),
                RoleId.Vulture => CustomOptions.VulturePlayerCount.GetFloat(),
                RoleId.NiceScientist => CustomOptions.NiceScientistPlayerCount.GetFloat(),
                RoleId.Clergyman => CustomOptions.ClergymanPlayerCount.GetFloat(),
                RoleId.MadMate => CustomOptions.MadMatePlayerCount.GetFloat(),
                RoleId.Bait => CustomOptions.BaitPlayerCount.GetFloat(),
                RoleId.HomeSecurityGuard => CustomOptions.HomeSecurityGuardPlayerCount.GetFloat(),
                RoleId.StuntMan => CustomOptions.StuntManPlayerCount.GetFloat(),
                RoleId.Moving => CustomOptions.MovingPlayerCount.GetFloat(),
                RoleId.Opportunist => CustomOptions.OpportunistPlayerCount.GetFloat(),
                RoleId.NiceGambler => CustomOptions.NiceGamblerPlayerCount.GetFloat(),
                RoleId.EvilGambler => CustomOptions.EvilGamblerPlayerCount.GetFloat(),
                RoleId.Bestfalsecharge => CustomOptions.BestfalsechargePlayerCount.GetFloat(),
                RoleId.Researcher => CustomOptions.ResearcherPlayerCount.GetFloat(),
                RoleId.SelfBomber => CustomOptions.SelfBomberPlayerCount.GetFloat(),
                RoleId.God => CustomOptions.GodPlayerCount.GetFloat(),
                RoleId.AllCleaner => CustomOptions.AllCleanerPlayerCount.GetFloat(),
                RoleId.NiceNekomata => CustomOptions.NiceNekomataPlayerCount.GetFloat(),
                RoleId.EvilNekomata => CustomOptions.EvilNekomataPlayerCount.GetFloat(),
                RoleId.JackalFriends => CustomOptions.JackalFriendsPlayerCount.GetFloat(),
                RoleId.Doctor => CustomOptions.DoctorPlayerCount.GetFloat(),
                RoleId.CountChanger => CustomOptions.CountChangerPlayerCount.GetFloat(),
                RoleId.Pursuer => CustomOptions.PursuerPlayerCount.GetFloat(),
                RoleId.Minimalist => CustomOptions.MinimalistPlayerCount.GetFloat(),
                RoleId.Hawk => CustomOptions.HawkPlayerCount.GetFloat(),
                RoleId.Egoist => CustomOptions.EgoistPlayerCount.GetFloat(),
                RoleId.NiceRedRidingHood => CustomOptions.NiceRedRidingHoodPlayerCount.GetFloat(),
                RoleId.EvilEraser => CustomOptions.EvilEraserPlayerCount.GetFloat(),
                RoleId.Workperson => CustomOptions.WorkpersonPlayerCount.GetFloat(),
                RoleId.Magaziner => CustomOptions.MagazinerPlayerCount.GetFloat(),
                RoleId.Mayor => CustomOptions.MayorPlayerCount.GetFloat(),
                RoleId.truelover => CustomOptions.trueloverPlayerCount.GetFloat(),
                RoleId.Technician => CustomOptions.TechnicianPlayerCount.GetFloat(),
                RoleId.SerialKiller => CustomOptions.SerialKillerPlayerCount.GetFloat(),
                RoleId.OverKiller => CustomOptions.OverKillerPlayerCount.GetFloat(),
                RoleId.Levelinger => CustomOptions.LevelingerPlayerCount.GetFloat(),
                RoleId.EvilMoving => CustomOptions.EvilMovingPlayerCount.GetFloat(),
                RoleId.Amnesiac => CustomOptions.AmnesiacPlayerCount.GetFloat(),
                RoleId.SideKiller => CustomOptions.SideKillerPlayerCount.GetFloat(),
                RoleId.Survivor => CustomOptions.SurvivorPlayerCount.GetFloat(),
                RoleId.MadMayor => CustomOptions.MadMayorPlayerCount.GetFloat(),
                RoleId.NiceHawk => CustomOptions.NiceHawkPlayerCount.GetFloat(),
                RoleId.Bakery => CustomOptions.BakeryPlayerCount.GetFloat(),
                RoleId.MadJester => CustomOptions.MadJesterPlayerCount.GetFloat(),
                RoleId.MadStuntMan => CustomOptions.MadStuntManPlayerCount.GetFloat(),
                RoleId.MadHawk => CustomOptions.MadHawkPlayerCount.GetFloat(),
                RoleId.FalseCharges => CustomOptions.FalseChargesPlayerCount.GetFloat(),
                RoleId.NiceTeleporter => CustomOptions.NiceTeleporterPlayerCount.GetFloat(),
                RoleId.Celebrity => CustomOptions.CelebrityPlayerCount.GetFloat(),
                RoleId.Nocturnality => CustomOptions.NocturnalityPlayerCount.GetFloat(),
                RoleId.Observer => CustomOptions.ObserverPlayerCount.GetFloat(),
                RoleId.Vampire => CustomOptions.VampirePlayerCount.GetFloat(),
                RoleId.DarkKiller => CustomOptions.DarkKillerPlayerCount.GetFloat(),
                RoleId.Seer => CustomOptions.SeerPlayerCount.GetFloat(),
                RoleId.MadSeer => CustomOptions.MadSeerPlayerCount.GetFloat(),
                RoleId.EvilSeer => CustomOptions.EvilSeerPlayerCount.GetFloat(),
                RoleId.RemoteSheriff => CustomOptions.RemoteSheriffPlayerCount.GetFloat(),
                RoleId.Fox => CustomOptions.FoxPlayerCount.GetFloat(),
                RoleId.TeleportingJackal => CustomOptions.TeleportingJackalPlayerCount.GetFloat(),
                RoleId.MadMaker => CustomOptions.MadMakerPlayerCount.GetFloat(),
                RoleId.Demon => CustomOptions.DemonPlayerCount.GetFloat(),
                RoleId.TaskManager => CustomOptions.TaskManagerPlayerCount.GetFloat(),
                RoleId.SeerFriends => CustomOptions.SeerFriendsPlayerCount.GetFloat(),
                RoleId.JackalSeer => CustomOptions.JackalSeerPlayerCount.GetFloat(),
                RoleId.Assassin => CustomOptions.AssassinPlayerCount.GetFloat(),
                RoleId.Marine => CustomOptions.MarinePlayerCount.GetFloat(),
                RoleId.Arsonist => CustomOptions.ArsonistPlayerCount.GetFloat(),
                RoleId.Chief => CustomOptions.ChiefPlayerCount.GetFloat(),
                RoleId.Cleaner => CustomOptions.CleanerPlayerCount.GetFloat(),
                RoleId.MadCleaner => CustomOptions.MadCleanerPlayerCount.GetFloat(),
                RoleId.Samurai => CustomOptions.SamuraiPlayerCount.GetFloat(),
                RoleId.MayorFriends => CustomOptions.MayorFriendsPlayerCount.GetFloat(),
                RoleId.VentMaker => CustomOptions.VentMakerPlayerCount.GetFloat(),
                RoleId.GhostMechanic => CustomOptions.GhostMechanicPlayerCount.GetFloat(),
                RoleId.EvilHacker => CustomOptions.EvilHackerPlayerCount.GetFloat(),
                RoleId.HauntedWolf => CustomOptions.HauntedWolfPlayerCount.GetFloat(),
                RoleId.PositionSwapper => CustomOptions.PositionSwapperPlayerCount.GetFloat(),
                RoleId.Tuna => CustomOptions.TunaPlayerCount.GetFloat(),
                RoleId.Mafia => CustomOptions.MafiaPlayerCount.GetFloat(),
                RoleId.BlackCat => CustomOptions.BlackCatPlayerCount.GetFloat(),
                RoleId.SecretlyKiller => CustomOptions.SecretlyKillerPlayerCount.GetFloat(),
                RoleId.Spy => CustomOptions.SpyPlayerCount.GetFloat(),
                RoleId.Kunoichi => CustomOptions.KunoichiPlayerCount.GetFloat(),
                RoleId.DoubleKiller => CustomOptions.DoubleKillerPlayerCount.GetFloat(),
                RoleId.Smasher => CustomOptions.SmasherPlayerCount.GetFloat(),
                RoleId.SuicideWisher => CustomOptions.SuicideWisherPlayerCount.GetFloat(),
                RoleId.Neet => CustomOptions.NeetPlayerCount.GetFloat(),
                RoleId.ToiletFan => CustomOptions.ToiletFanPlayerCount.GetFloat(),
                RoleId.EvilButtoner => CustomOptions.EvilButtonerPlayerCount.GetFloat(),
                RoleId.NiceButtoner => CustomOptions.NiceButtonerPlayerCount.GetFloat(),
                RoleId.Finder => CustomOptions.FinderPlayerCount.GetFloat(),
                RoleId.Revolutionist => CustomOptions.RevolutionistPlayerCount.GetFloat(),
                RoleId.Dictator => CustomOptions.DictatorPlayerCount.GetFloat(),
                RoleId.Spelunker => CustomOptions.SpelunkerPlayerCount.GetFloat(),
                RoleId.SuicidalIdeation => CustomOptions.SuicidalIdeationPlayerCount.GetFloat(),
                RoleId.Hitman => CustomOptions.HitmanPlayerCount.GetFloat(),
                RoleId.Matryoshka => CustomOptions.MatryoshkaPlayerCount.GetFloat(),
                RoleId.Nun => CustomOptions.NunPlayerCount.GetFloat(),
                RoleId.PartTimer => CustomOptions.PartTimerPlayerCount.GetFloat(),
                RoleId.SatsumaAndImo => CustomOptions.SatsumaAndImoPlayerCount.GetFloat(),
                RoleId.Painter => CustomOptions.PainterPlayerCount.GetFloat(),
                RoleId.Psychometrist => CustomOptions.PsychometristPlayerCount.GetFloat(),
                RoleId.SeeThroughPerson => CustomOptions.SeeThroughPersonPlayerCount.GetFloat(),
                RoleId.Photographer => CustomOptions.PhotographerPlayerCount.GetFloat(),
                RoleId.Stefinder => CustomOptions.StefinderPlayerCount.GetFloat(),
                RoleId.Slugger => CustomOptions.SluggerPlayerCount.GetFloat(),
                //プレイヤーカウント
                _ => 1,
            };
        }
        public static void CrewOrImpostorSet()
        {
            CrewMatePlayers = new();
            ImpostorPlayers = new();
            foreach (PlayerControl Player in CachedPlayer.AllPlayers)
            {
                if (Player.Data.Role.IsSimpleRole)
                {
                    if (Player.IsImpostor())
                    {
                        ImpostorPlayers.Add(Player);
                    }
                    else
                    {
                        CrewMatePlayers.Add(Player);
                    }
                }
            }
        }
        public static void OneOrNotListSet()
        {
            Impoonepar = new();
            Imponotonepar = new();
            Neutonepar = new();
            Neutnotonepar = new();
            Crewonepar = new();
            Crewnotonepar = new();
            foreach (IntroDate intro in IntroDate.IntroDatas)
            {
                if (intro.RoleId != RoleId.DefaultRole && (intro.RoleId != RoleId.Nun || (MapNames)PlayerControl.GameOptions.MapId == MapNames.Airship) && !intro.IsGhostRole)
                {
                    var option = IntroDate.GetOption(intro.RoleId);
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
            var Assassinselection = CustomOptions.AssassinAndMarineOption.GetSelection();
            if (Assassinselection != 0 && CrewMatePlayerNum > 0 && CrewMatePlayers.Count > 0)
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
            if (CustomOptions.RevolutionistAndDictatorOption.GetSelection() != 0 && CrewMatePlayerNum > 0 && CrewMatePlayers.Count > 1)
            {
                if (CustomOptions.RevolutionistAndDictatorOption.GetSelection() == 10)
                {
                    Neutonepar.Add(RoleId.Revolutionist);
                }
                else
                {
                    for (int i = 1; i <= CustomOptions.RevolutionistAndDictatorOption.GetSelection(); i++)
                    {
                        Neutnotonepar.Add(RoleId.Revolutionist);
                    }
                }
            }
        }
    }
}