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
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                RoleHelpers.DeadCaches[p.PlayerId] = p.PlayerControl.isDead(false);
            }
        }
    }
    [HarmonyPatch(typeof(GameData.PlayerInfo), nameof(GameData.PlayerInfo.IsDead), MethodType.Setter)]
    class DeadPatch
    {
        public static void Postfix(GameData.PlayerInfo __instance)
        {
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                RoleHelpers.DeadCaches[p.PlayerId] = p.PlayerControl.isDead(false);
            }
        }
    }
    [HarmonyPatch(typeof(GameData.PlayerInfo), nameof(GameData.PlayerInfo.Disconnected), MethodType.Setter)]
    class DisconnectPatch
    {
        public static void Postfix(GameData.PlayerInfo __instance)
        {
            foreach (CachedPlayer p in CachedPlayer.AllPlayers)
            {
                RoleHelpers.DeadCaches[p.PlayerId] = p.PlayerControl.isDead(false);
            }
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
                AllRoleSetClass.impostors = new List<PlayerControl>();
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
                        SelectPlayers.RemoveAll(a => a.PlayerId == newimpostor.PlayerId);
                    }
                }
                var crs = RoleSelectHandler.RoleSelect();
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
                crs.SendMessage();
                SuperNewRolesPlugin.Logger.LogInfo(false);
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
                    if (!p.Data.Role.IsImpostor && !p.isNeutral() && p.IsPlayer())
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
                    var Listdate = new List<PlayerControl>();
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
                var a = new List<string>();
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
                    var Listdate = new List<PlayerControl>();
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
            if (CrewMatePlayerNum == 0 || (Crewonepar.Count == 0 && Crewnotonepar.Count == 0))
            {
                return;
            }
            bool IsNotEndRandomSelect = true;
            while (IsNotEndRandomSelect)
            {
                if (Crewonepar.Count != 0)
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
                RoleId.SoothSayer => CustomOption.CustomOptions.SoothSayerPlayerCount.getFloat(),
                RoleId.Jester => CustomOption.CustomOptions.JesterPlayerCount.getFloat(),
                RoleId.Lighter => CustomOption.CustomOptions.LighterPlayerCount.getFloat(),
                RoleId.EvilLighter => CustomOption.CustomOptions.EvilLighterPlayerCount.getFloat(),
                RoleId.EvilScientist => CustomOption.CustomOptions.EvilScientistPlayerCount.getFloat(),
                RoleId.Sheriff => CustomOption.CustomOptions.SheriffPlayerCount.getFloat(),
                RoleId.MeetingSheriff => CustomOption.CustomOptions.MeetingSheriffPlayerCount.getFloat(),
                RoleId.Jackal => CustomOption.CustomOptions.JackalPlayerCount.getFloat(),
                RoleId.Teleporter => CustomOption.CustomOptions.TeleporterPlayerCount.getFloat(),
                RoleId.SpiritMedium => CustomOption.CustomOptions.SpiritMediumPlayerCount.getFloat(),
                RoleId.SpeedBooster => CustomOption.CustomOptions.SpeedBoosterPlayerCount.getFloat(),
                RoleId.EvilSpeedBooster => CustomOption.CustomOptions.EvilSpeedBoosterPlayerCount.getFloat(),
                RoleId.Tasker => CustomOption.CustomOptions.TaskerPlayerCount.getFloat(),
                RoleId.Doorr => CustomOption.CustomOptions.DoorrPlayerCount.getFloat(),
                RoleId.EvilDoorr => CustomOption.CustomOptions.EvilDoorrPlayerCount.getFloat(),
                RoleId.Shielder => CustomOption.CustomOptions.ShielderPlayerCount.getFloat(),
                RoleId.Speeder => CustomOption.CustomOptions.SpeederPlayerCount.getFloat(),
                RoleId.Freezer => CustomOption.CustomOptions.FreezerPlayerCount.getFloat(),
                RoleId.Guesser => CustomOption.CustomOptions.GuesserPlayerCount.getFloat(),
                RoleId.EvilGuesser => CustomOption.CustomOptions.EvilGuesserPlayerCount.getFloat(),
                RoleId.Vulture => CustomOption.CustomOptions.VulturePlayerCount.getFloat(),
                RoleId.NiceScientist => CustomOption.CustomOptions.NiceScientistPlayerCount.getFloat(),
                RoleId.Clergyman => CustomOption.CustomOptions.ClergymanPlayerCount.getFloat(),
                RoleId.MadMate => CustomOption.CustomOptions.MadMatePlayerCount.getFloat(),
                RoleId.Bait => CustomOption.CustomOptions.BaitPlayerCount.getFloat(),
                RoleId.HomeSecurityGuard => CustomOption.CustomOptions.HomeSecurityGuardPlayerCount.getFloat(),
                RoleId.StuntMan => CustomOption.CustomOptions.StuntManPlayerCount.getFloat(),
                RoleId.Moving => CustomOption.CustomOptions.MovingPlayerCount.getFloat(),
                RoleId.Opportunist => CustomOption.CustomOptions.OpportunistPlayerCount.getFloat(),
                RoleId.NiceGambler => CustomOption.CustomOptions.NiceGamblerPlayerCount.getFloat(),
                RoleId.EvilGambler => CustomOption.CustomOptions.EvilGamblerPlayerCount.getFloat(),
                RoleId.Bestfalsecharge => CustomOption.CustomOptions.BestfalsechargePlayerCount.getFloat(),
                RoleId.Researcher => CustomOption.CustomOptions.ResearcherPlayerCount.getFloat(),
                RoleId.SelfBomber => CustomOption.CustomOptions.SelfBomberPlayerCount.getFloat(),
                RoleId.God => CustomOption.CustomOptions.GodPlayerCount.getFloat(),
                RoleId.AllCleaner => CustomOption.CustomOptions.AllCleanerPlayerCount.getFloat(),
                RoleId.NiceNekomata => CustomOption.CustomOptions.NiceNekomataPlayerCount.getFloat(),
                RoleId.EvilNekomata => CustomOption.CustomOptions.EvilNekomataPlayerCount.getFloat(),
                RoleId.JackalFriends => CustomOption.CustomOptions.JackalFriendsPlayerCount.getFloat(),
                RoleId.Doctor => CustomOption.CustomOptions.DoctorPlayerCount.getFloat(),
                RoleId.CountChanger => CustomOption.CustomOptions.CountChangerPlayerCount.getFloat(),
                RoleId.Pursuer => CustomOption.CustomOptions.PursuerPlayerCount.getFloat(),
                RoleId.Minimalist => CustomOption.CustomOptions.MinimalistPlayerCount.getFloat(),
                RoleId.Hawk => CustomOption.CustomOptions.HawkPlayerCount.getFloat(),
                RoleId.Egoist => CustomOption.CustomOptions.EgoistPlayerCount.getFloat(),
                RoleId.NiceRedRidingHood => CustomOption.CustomOptions.NiceRedRidingHoodPlayerCount.getFloat(),
                RoleId.EvilEraser => CustomOption.CustomOptions.EvilEraserPlayerCount.getFloat(),
                RoleId.Workperson => CustomOption.CustomOptions.WorkpersonPlayerCount.getFloat(),
                RoleId.Magaziner => CustomOption.CustomOptions.MagazinerPlayerCount.getFloat(),
                RoleId.Mayor => CustomOption.CustomOptions.MayorPlayerCount.getFloat(),
                RoleId.truelover => CustomOption.CustomOptions.trueloverPlayerCount.getFloat(),
                RoleId.Technician => CustomOption.CustomOptions.TechnicianPlayerCount.getFloat(),
                RoleId.SerialKiller => CustomOption.CustomOptions.SerialKillerPlayerCount.getFloat(),
                RoleId.OverKiller => CustomOption.CustomOptions.OverKillerPlayerCount.getFloat(),
                RoleId.Levelinger => CustomOption.CustomOptions.LevelingerPlayerCount.getFloat(),
                RoleId.EvilMoving => CustomOption.CustomOptions.EvilMovingPlayerCount.getFloat(),
                RoleId.Amnesiac => CustomOption.CustomOptions.AmnesiacPlayerCount.getFloat(),
                RoleId.SideKiller => CustomOption.CustomOptions.SideKillerPlayerCount.getFloat(),
                RoleId.Survivor => CustomOption.CustomOptions.SurvivorPlayerCount.getFloat(),
                RoleId.MadMayor => CustomOption.CustomOptions.MadMayorPlayerCount.getFloat(),
                RoleId.NiceHawk => CustomOption.CustomOptions.NiceHawkPlayerCount.getFloat(),
                RoleId.Bakery => CustomOption.CustomOptions.BakeryPlayerCount.getFloat(),
                RoleId.MadJester => CustomOption.CustomOptions.MadJesterPlayerCount.getFloat(),
                RoleId.MadStuntMan => CustomOption.CustomOptions.MadStuntManPlayerCount.getFloat(),
                RoleId.MadHawk => CustomOption.CustomOptions.MadHawkPlayerCount.getFloat(),
                RoleId.FalseCharges => CustomOption.CustomOptions.FalseChargesPlayerCount.getFloat(),
                RoleId.NiceTeleporter => CustomOption.CustomOptions.NiceTeleporterPlayerCount.getFloat(),
                RoleId.Celebrity => CustomOption.CustomOptions.CelebrityPlayerCount.getFloat(),
                RoleId.Nocturnality => CustomOption.CustomOptions.NocturnalityPlayerCount.getFloat(),
                RoleId.Observer => CustomOption.CustomOptions.ObserverPlayerCount.getFloat(),
                RoleId.Vampire => CustomOption.CustomOptions.VampirePlayerCount.getFloat(),
                RoleId.DarkKiller => CustomOption.CustomOptions.DarkKillerPlayerCount.getFloat(),
                RoleId.Seer => CustomOption.CustomOptions.SeerPlayerCount.getFloat(),
                RoleId.MadSeer => CustomOption.CustomOptions.MadSeerPlayerCount.getFloat(),
                RoleId.EvilSeer => CustomOption.CustomOptions.EvilSeerPlayerCount.getFloat(),
                RoleId.RemoteSheriff => CustomOption.CustomOptions.RemoteSheriffPlayerCount.getFloat(),
                RoleId.Fox => CustomOption.CustomOptions.FoxPlayerCount.getFloat(),
                RoleId.TeleportingJackal => CustomOption.CustomOptions.TeleportingJackalPlayerCount.getFloat(),
                RoleId.MadMaker => CustomOption.CustomOptions.MadMakerPlayerCount.getFloat(),
                RoleId.Demon => CustomOption.CustomOptions.DemonPlayerCount.getFloat(),
                RoleId.TaskManager => CustomOption.CustomOptions.TaskManagerPlayerCount.getFloat(),
                RoleId.SeerFriends => CustomOption.CustomOptions.SeerFriendsPlayerCount.getFloat(),
                RoleId.JackalSeer => CustomOption.CustomOptions.JackalSeerPlayerCount.getFloat(),
                RoleId.Assassin => CustomOption.CustomOptions.AssassinPlayerCount.getFloat(),
                RoleId.Marine => CustomOption.CustomOptions.MarinePlayerCount.getFloat(),
                RoleId.Arsonist => CustomOption.CustomOptions.ArsonistPlayerCount.getFloat(),
                RoleId.Chief => CustomOption.CustomOptions.ChiefPlayerCount.getFloat(),
                RoleId.Cleaner => CustomOption.CustomOptions.CleanerPlayerCount.getFloat(),
                RoleId.MadCleaner => CustomOption.CustomOptions.MadCleanerPlayerCount.getFloat(),
                RoleId.Samurai => CustomOption.CustomOptions.SamuraiPlayerCount.getFloat(),
                RoleId.MayorFriends => CustomOption.CustomOptions.MayorFriendsPlayerCount.getFloat(),
                RoleId.VentMaker => CustomOption.CustomOptions.VentMakerPlayerCount.getFloat(),
                RoleId.GhostMechanic => CustomOption.CustomOptions.GhostMechanicPlayerCount.getFloat(),
                RoleId.EvilHacker => CustomOption.CustomOptions.EvilHackerPlayerCount.getFloat(),
                RoleId.HauntedWolf => CustomOption.CustomOptions.HauntedWolfPlayerCount.getFloat(),
                RoleId.Tuna => CustomOption.CustomOptions.TunaPlayerCount.getFloat(),
                                RoleId.BlackCat => CustomOption.CustomOptions.BlackCatPlayerCount.getFloat(),
                _ => 1,
            };
        }
        public static void CrewOrImpostorSet()
        {
            CrewMatePlayers = new List<PlayerControl>();
            ImpostorPlayers = new List<PlayerControl>();
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
            Impoonepar = new List<RoleId>();
            Imponotonepar = new List<RoleId>();
            Neutonepar = new List<RoleId>();
            Neutnotonepar = new List<RoleId>();
            Crewonepar = new List<RoleId>();
            Crewnotonepar = new List<RoleId>();
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
