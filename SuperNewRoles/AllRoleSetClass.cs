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
using SuperNewRoles.Roles;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRolePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            SuperNewRolesPlugin.Logger.LogInfo(__instance.Data.PlayerName + " => " + roleType);
            if (!AmongUsClient.Instance.AmHost) return true;
            if (RoleManagerSelectRolesPatch.IsShapeSet)
            {
                MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SetRole);
                messageWriter.Write((ushort)roleType);
                messageWriter.EndMessage();
            }
            else
            {
                if (RoleManagerSelectRolesPatch.IsNotDesync)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("SetOK!:" + roleType);
                    if (AmongUsClient.Instance.AmClient)
                        __instance.SetRole(roleType);
                    MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SetRole);
                    messageWriter.Write((ushort)roleType);
                    messageWriter.EndMessage();
                }
                else
                {
                    if (!RoleManagerSelectRolesPatch.IsNotPrefix)
                    {
                        __instance.Data.Role.Role = roleType;
                        DestroyableSingleton<RoleManager>.Instance.SetRole(__instance, roleType);
                    }
                    if (RoleManagerSelectRolesPatch.IsSetRoleRpc)
                    {
                        if (AmongUsClient.Instance.AmClient)
                            __instance.SetRole(roleType);
                        MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SetRole);
                        messageWriter.Write((ushort)roleType);
                        messageWriter.EndMessage();
                    }
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    class startgamepatch
    {
        public static void Postfix()
        {
            RPCHelper.StartRPC(CustomRPC.CustomRPC.StartGameRPC).EndRPC();
            CustomRPC.RPCProcedure.StartGameRPC();

            RoleSelectHandler.SpawnBots();
        }
    }
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        public static bool IsNotPrefix = false;
        public static bool IsRPCSetRoleOK = false;
        public static bool IsSetRoleRpc = false;
        public static bool IsShapeSet = false;
        public static bool IsNotDesync = false;
        public static bool Prefix()
        {
            AllRoleSetClass.SetPlayerNum();
            IsNotPrefix = false;
            IsSetRoleRpc = false;
            IsRPCSetRoleOK = true;
            IsShapeSet = false;
            IsNotDesync = true;
            if (ModeHandler.isMode(ModeId.NotImpostorCheck))
            {
                IsNotDesync = false;
            }
            /*
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                IsNotDesync = false;
            }
            */
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
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
                foreach (PlayerControl player in AllRoleSetClass.impostors)
                {
                    player.RpcSetRole(RoleTypes.Impostor);
                }
                foreach (PlayerControl player in CachedPlayer.AllPlayers)
                {
                    if (!player.Data.Disconnected && !AllRoleSetClass.impostors.IsCheckListPlayerControl(player))
                    {
                        player.RpcSetRole(RoleTypes.Crewmate);
                    }
                }
                RoleSelectHandler.RoleSelect();

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
            else if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                Mode.BattleRoyal.main.ChangeRole.Postfix();
                return false;
            }
            else if (ModeHandler.isMode(ModeId.CopsRobbers))
            {
                Mode.CopsRobbers.RoleSelectHandler.Handler();
                return false;
            }
            return true;
        }
        public static void Postfix()
        {
            IsSetRoleRpc = true;
            IsRPCSetRoleOK = false;
            IsNotPrefix = true;
            if (ModeHandler.isMode(ModeId.Default))
            {
                AllRoleSetClass.AllRoleSet();
            }
            else if (ModeHandler.isMode(ModeId.Werewolf))
            {
                Mode.Werewolf.RoleSelectHandler.RoleSelect();
            }
            else if (ModeHandler.isMode(ModeId.NotImpostorCheck))
            {
                Mode.NotImpostorCheck.SelectRolePatch.SetDesync();
            }
            else if (ModeHandler.isMode(ModeId.Detective))
            {
                Mode.Detective.main.RoleSelect();
            }
            if (!ModeHandler.isMode(ModeId.NotImpostorCheck) && !ModeHandler.isMode(ModeId.BattleRoyal) && !ModeHandler.isMode(ModeId.Default) && !ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    p.RpcSetRole(p.Data.Role.Role);
                }
                /*AmongUsClient.Instance.StartCoroutine(nameof(SetServerRole));*/
            }
            if (!ModeHandler.isMode(ModeId.SuperHostRoles))
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

        public static int ImpostorPlayerNum;
        public static int ImpostorGhostRolePlayerNum;
        public static int NeutralPlayerNum;
        public static int NeutralGhostRolePlayerNum;
        public static int CrewMatePlayerNum;
        public static int CrewMateGhostRolePlayerNum;

        public static void AllRoleSet()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!ModeHandler.isMode(ModeId.SuperHostRoles))
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
            if (ModeHandler.isMode(ModeId.Default))
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
            if (!CustomOption.CustomOptions.QuarreledOption.getBool()) return;
            SuperNewRolesPlugin.Logger.LogInfo("クラードセレクト");
            List<PlayerControl> SelectPlayers = new();
            if (CustomOption.CustomOptions.QuarreledOnlyCrewMate.getBool())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.isImpostor() && !p.isNeutral() && p.IsPlayer())
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
            for (int i = 0; i < CustomOptions.QuarreledTeamCount.getFloat(); i++)
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
            if (!CustomOptions.LoversOption.getBool() || (CustomOptions.LoversPar.getString() == "0%")) return;
            if (!(CustomOptions.LoversPar.getString() == "100%"))
            {
                List<string> a = new();
                var SucPar = int.Parse(CustomOptions.LoversPar.getString().Replace("0%", ""));
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
            bool IsQuarreledDup = CustomOptions.LoversDuplicationQuarreled.getBool();
            if (CustomOptions.LoversOnlyCrewMate.getBool())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.isImpostor() && !p.isNeutral() && !p.isRole(RoleId.truelover) && p.IsPlayer())
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
                        if (!p.isRole(RoleId.truelover))
                        {
                            SelectPlayers.Add(p);
                        }
                    }
                }
            }
            for (int i = 0; i < CustomOptions.LoversTeamCount.getFloat(); i++)
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
            ImpostorPlayerNum = (int)CustomOptions.impostorRolesCountMax.getFloat();
            ImpostorGhostRolePlayerNum = (int)CustomOptions.impostorGhostRolesCountMax.getFloat();
            NeutralPlayerNum = (int)CustomOptions.neutralRolesCountMax.getFloat();
            NeutralGhostRolePlayerNum = (int)CustomOptions.neutralGhostRolesCountMax.getFloat();
            CrewMatePlayerNum = (int)CustomOptions.crewmateRolesCountMax.getFloat();
            CrewMateGhostRolePlayerNum = (int)CustomOptions.crewmateGhostRolesCountMax.getFloat();
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
                            p.setRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;

                    }
                    else if (PlayerCount >= ImpostorPlayers.Count)
                    {
                        foreach (PlayerControl Player in ImpostorPlayers)
                        {
                            ImpostorPlayerNum--;
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            ImpostorPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.setRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                    }
                    Impoonepar.RemoveAt(SelectRoleDateIndex);
                }
                else
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Imponotonepar);
                    RoleId SelectRoleDate = Imponotonepar[SelectRoleDateIndex];
                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= ImpostorPlayerNum)
                    {
                        for (int i = 1; i <= ImpostorPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.setRoleRPC(SelectRoleDate);
                            ImpostorPlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;

                    }
                    else if (PlayerCount >= ImpostorPlayers.Count)
                    {
                        foreach (PlayerControl Player in ImpostorPlayers)
                        {
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            ImpostorPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(ImpostorPlayers);
                            p.setRoleRPC(SelectRoleDate);
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
                SuperNewRolesPlugin.Logger.LogInfo("DATA:\n" + PlayerCount + "\n" + CrewMatePlayerNum + "\n" + CrewMatePlayers.Count);
                if (PlayerCount >= CrewMatePlayerNum)
                {
                    for (int i = 1; i <= CrewMatePlayerNum; i++)
                    {
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.setRoleRPC(RoleId.Marine);
                        CrewMatePlayers.Remove(p);
                    }
                    CrewMatePlayerNum = 0;
                }
                else if (PlayerCount >= CrewMatePlayers.Count)
                {
                    foreach (PlayerControl Player in CrewMatePlayers)
                    {
                        Player.setRoleRPC(RoleId.Marine);
                    }
                    CrewMatePlayerNum = 0;
                }
                else
                {
                    for (int i = 1; i <= PlayerCount; i++)
                    {
                        CrewMatePlayerNum--;
                        PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                        p.setRoleRPC(RoleId.Marine);
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
            while (IsNotEndRandomSelect)
            {
                if (Neutonepar.Count != 0)
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Neutonepar);
                    RoleId SelectRoleDate = Neutonepar[SelectRoleDateIndex];
                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= NeutralPlayerNum)
                    {
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            NeutralPlayerNum--;
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            NeutralPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                    }
                    Neutonepar.RemoveAt(SelectRoleDateIndex);
                }
                else
                {
                    int SelectRoleDateIndex = ModHelpers.GetRandomIndex(Neutnotonepar);
                    RoleId SelectRoleDate = Neutnotonepar[SelectRoleDateIndex];
                    int PlayerCount = (int)GetPlayerCount(SelectRoleDate);
                    if (PlayerCount >= NeutralPlayerNum)
                    {
                        for (int i = 1; i <= NeutralPlayerNum; i++)
                        {
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            NeutralPlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
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
                            p.setRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            CrewMatePlayerNum--;
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            CrewMatePlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
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
                            p.setRoleRPC(SelectRoleDate);
                            CrewMatePlayers.Remove(p);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else if (PlayerCount >= CrewMatePlayers.Count)
                    {
                        foreach (PlayerControl Player in CrewMatePlayers)
                        {
                            Player.setRoleRPC(SelectRoleDate);
                        }
                        IsNotEndRandomSelect = false;
                    }
                    else
                    {
                        for (int i = 1; i <= PlayerCount; i++)
                        {
                            CrewMatePlayerNum--;
                            PlayerControl p = ModHelpers.GetRandom(CrewMatePlayers);
                            p.setRoleRPC(SelectRoleDate);
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
                RoleId.SoothSayer => CustomOptions.SoothSayerPlayerCount.getFloat(),
                RoleId.Jester => CustomOptions.JesterPlayerCount.getFloat(),
                RoleId.Lighter => CustomOptions.LighterPlayerCount.getFloat(),
                RoleId.EvilLighter => CustomOptions.EvilLighterPlayerCount.getFloat(),
                RoleId.EvilScientist => CustomOptions.EvilScientistPlayerCount.getFloat(),
                RoleId.Sheriff => CustomOptions.SheriffPlayerCount.getFloat(),
                RoleId.MeetingSheriff => CustomOptions.MeetingSheriffPlayerCount.getFloat(),
                RoleId.Jackal => CustomOptions.JackalPlayerCount.getFloat(),
                RoleId.Teleporter => CustomOptions.TeleporterPlayerCount.getFloat(),
                RoleId.SpiritMedium => CustomOptions.SpiritMediumPlayerCount.getFloat(),
                RoleId.SpeedBooster => CustomOptions.SpeedBoosterPlayerCount.getFloat(),
                RoleId.EvilSpeedBooster => CustomOptions.EvilSpeedBoosterPlayerCount.getFloat(),
                RoleId.Tasker => CustomOptions.TaskerPlayerCount.getFloat(),
                RoleId.Doorr => CustomOptions.DoorrPlayerCount.getFloat(),
                RoleId.EvilDoorr => CustomOptions.EvilDoorrPlayerCount.getFloat(),
                RoleId.Shielder => CustomOptions.ShielderPlayerCount.getFloat(),
                RoleId.Speeder => CustomOptions.SpeederPlayerCount.getFloat(),
                RoleId.Freezer => CustomOptions.FreezerPlayerCount.getFloat(),
                RoleId.Guesser => CustomOptions.GuesserPlayerCount.getFloat(),
                RoleId.EvilGuesser => CustomOptions.EvilGuesserPlayerCount.getFloat(),
                RoleId.Vulture => CustomOptions.VulturePlayerCount.getFloat(),
                RoleId.NiceScientist => CustomOptions.NiceScientistPlayerCount.getFloat(),
                RoleId.Clergyman => CustomOptions.ClergymanPlayerCount.getFloat(),
                RoleId.MadMate => CustomOptions.MadMatePlayerCount.getFloat(),
                RoleId.Bait => CustomOptions.BaitPlayerCount.getFloat(),
                RoleId.HomeSecurityGuard => CustomOptions.HomeSecurityGuardPlayerCount.getFloat(),
                RoleId.StuntMan => CustomOptions.StuntManPlayerCount.getFloat(),
                RoleId.Moving => CustomOptions.MovingPlayerCount.getFloat(),
                RoleId.Opportunist => CustomOptions.OpportunistPlayerCount.getFloat(),
                RoleId.NiceGambler => CustomOptions.NiceGamblerPlayerCount.getFloat(),
                RoleId.EvilGambler => CustomOptions.EvilGamblerPlayerCount.getFloat(),
                RoleId.Bestfalsecharge => CustomOptions.BestfalsechargePlayerCount.getFloat(),
                RoleId.Researcher => CustomOptions.ResearcherPlayerCount.getFloat(),
                RoleId.SelfBomber => CustomOptions.SelfBomberPlayerCount.getFloat(),
                RoleId.God => CustomOptions.GodPlayerCount.getFloat(),
                RoleId.AllCleaner => CustomOptions.AllCleanerPlayerCount.getFloat(),
                RoleId.NiceNekomata => CustomOptions.NiceNekomataPlayerCount.getFloat(),
                RoleId.EvilNekomata => CustomOptions.EvilNekomataPlayerCount.getFloat(),
                RoleId.JackalFriends => CustomOptions.JackalFriendsPlayerCount.getFloat(),
                RoleId.Doctor => CustomOptions.DoctorPlayerCount.getFloat(),
                RoleId.CountChanger => CustomOptions.CountChangerPlayerCount.getFloat(),
                RoleId.Pursuer => CustomOptions.PursuerPlayerCount.getFloat(),
                RoleId.Minimalist => CustomOptions.MinimalistPlayerCount.getFloat(),
                RoleId.Hawk => CustomOptions.HawkPlayerCount.getFloat(),
                RoleId.Egoist => CustomOptions.EgoistPlayerCount.getFloat(),
                RoleId.NiceRedRidingHood => CustomOptions.NiceRedRidingHoodPlayerCount.getFloat(),
                RoleId.EvilEraser => CustomOptions.EvilEraserPlayerCount.getFloat(),
                RoleId.Workperson => CustomOptions.WorkpersonPlayerCount.getFloat(),
                RoleId.Magaziner => CustomOptions.MagazinerPlayerCount.getFloat(),
                RoleId.Mayor => CustomOptions.MayorPlayerCount.getFloat(),
                RoleId.truelover => CustomOptions.trueloverPlayerCount.getFloat(),
                RoleId.Technician => CustomOptions.TechnicianPlayerCount.getFloat(),
                RoleId.SerialKiller => CustomOptions.SerialKillerPlayerCount.getFloat(),
                RoleId.OverKiller => CustomOptions.OverKillerPlayerCount.getFloat(),
                RoleId.Levelinger => CustomOptions.LevelingerPlayerCount.getFloat(),
                RoleId.EvilMoving => CustomOptions.EvilMovingPlayerCount.getFloat(),
                RoleId.Amnesiac => CustomOptions.AmnesiacPlayerCount.getFloat(),
                RoleId.SideKiller => CustomOptions.SideKillerPlayerCount.getFloat(),
                RoleId.Survivor => CustomOptions.SurvivorPlayerCount.getFloat(),
                RoleId.MadMayor => CustomOptions.MadMayorPlayerCount.getFloat(),
                RoleId.NiceHawk => CustomOptions.NiceHawkPlayerCount.getFloat(),
                RoleId.Bakery => CustomOptions.BakeryPlayerCount.getFloat(),
                RoleId.MadJester => CustomOptions.MadJesterPlayerCount.getFloat(),
                RoleId.MadStuntMan => CustomOptions.MadStuntManPlayerCount.getFloat(),
                RoleId.MadHawk => CustomOptions.MadHawkPlayerCount.getFloat(),
                RoleId.FalseCharges => CustomOptions.FalseChargesPlayerCount.getFloat(),
                RoleId.NiceTeleporter => CustomOptions.NiceTeleporterPlayerCount.getFloat(),
                RoleId.Celebrity => CustomOptions.CelebrityPlayerCount.getFloat(),
                RoleId.Nocturnality => CustomOptions.NocturnalityPlayerCount.getFloat(),
                RoleId.Observer => CustomOptions.ObserverPlayerCount.getFloat(),
                RoleId.Vampire => CustomOptions.VampirePlayerCount.getFloat(),
                RoleId.DarkKiller => CustomOptions.DarkKillerPlayerCount.getFloat(),
                RoleId.Seer => CustomOptions.SeerPlayerCount.getFloat(),
                RoleId.MadSeer => CustomOptions.MadSeerPlayerCount.getFloat(),
                RoleId.EvilSeer => CustomOptions.EvilSeerPlayerCount.getFloat(),
                RoleId.RemoteSheriff => CustomOptions.RemoteSheriffPlayerCount.getFloat(),
                RoleId.Fox => CustomOptions.FoxPlayerCount.getFloat(),
                RoleId.TeleportingJackal => CustomOptions.TeleportingJackalPlayerCount.getFloat(),
                RoleId.MadMaker => CustomOptions.MadMakerPlayerCount.getFloat(),
                RoleId.Demon => CustomOptions.DemonPlayerCount.getFloat(),
                RoleId.TaskManager => CustomOptions.TaskManagerPlayerCount.getFloat(),
                RoleId.SeerFriends => CustomOptions.SeerFriendsPlayerCount.getFloat(),
                RoleId.JackalSeer => CustomOptions.JackalSeerPlayerCount.getFloat(),
                RoleId.Assassin => CustomOptions.AssassinPlayerCount.getFloat(),
                RoleId.Marine => CustomOptions.MarinePlayerCount.getFloat(),
                RoleId.Arsonist => CustomOptions.ArsonistPlayerCount.getFloat(),
                RoleId.Chief => CustomOptions.ChiefPlayerCount.getFloat(),
                RoleId.Cleaner => CustomOptions.CleanerPlayerCount.getFloat(),
                RoleId.MadCleaner => CustomOptions.MadCleanerPlayerCount.getFloat(),
                RoleId.Samurai => CustomOptions.SamuraiPlayerCount.getFloat(),
                RoleId.MayorFriends => CustomOptions.MayorFriendsPlayerCount.getFloat(),
                RoleId.VentMaker => CustomOptions.VentMakerPlayerCount.getFloat(),
                RoleId.GhostMechanic => CustomOptions.GhostMechanicPlayerCount.getFloat(),
                RoleId.EvilHacker => CustomOptions.EvilHackerPlayerCount.getFloat(),
                RoleId.HauntedWolf => CustomOptions.HauntedWolfPlayerCount.getFloat(),
                RoleId.PositionSwapper => CustomOptions.PositionSwapperPlayerCount.getFloat(),
                RoleId.Tuna => CustomOptions.TunaPlayerCount.getFloat(),
                RoleId.Mafia => CustomOptions.MafiaPlayerCount.getFloat(),
                RoleId.BlackCat => CustomOptions.BlackCatPlayerCount.getFloat(),
                RoleId.Spy => CustomOptions.SpyPlayerCount.getFloat(),
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
                    if (Player.isImpostor())
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
                if (intro.RoleId != RoleId.DefaultRole && !intro.IsGhostRole)
                {
                    var option = IntroDate.GetOption(intro.RoleId);
                    if (option == null) continue;
                    var selection = option.getSelection();
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
            var Assassinselection = CustomOptions.AssassinAndMarineOption.getSelection();
            SuperNewRolesPlugin.Logger.LogInfo("アサイン情報:" + Assassinselection + "、" + CrewMatePlayerNum + "、" + CrewMatePlayers.Count);
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
        }
    }
}
