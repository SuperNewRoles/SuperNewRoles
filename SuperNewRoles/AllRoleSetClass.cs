using SuperNewRoles.CustomRPC;
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Roles;
using SuperNewRoles.Mode;
using System.Collections;
using UnityEngine;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Intro;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
    class RpcSetRolePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
        {
            return true;

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
                List<PlayerControl> SelectPlayers = new List<PlayerControl>();
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
            List<PlayerControl> SelectPlayers = new List<PlayerControl>();
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
                if (!(SelectPlayers.Count == 1 || SelectPlayers.Count == 0))
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
            List<PlayerControl> SelectPlayers = new List<PlayerControl>();
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
                    if (!IsQuarreledDup || !p.IsQuarreled() && p.IsPlayer())
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
                if (!(SelectPlayers.Count == 1 || SelectPlayers.Count == 0))
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
            switch (RoleDate)
            {
                case (RoleId.SoothSayer):
                    return CustomOption.CustomOptions.SoothSayerPlayerCount.getFloat();
                case (RoleId.Jester):
                    return CustomOption.CustomOptions.JesterPlayerCount.getFloat();
                case (RoleId.Lighter):
                    return CustomOption.CustomOptions.LighterPlayerCount.getFloat();
                case (RoleId.EvilLighter):
                    return CustomOption.CustomOptions.EvilLighterPlayerCount.getFloat();
                case (RoleId.EvilScientist):
                    return CustomOption.CustomOptions.EvilScientistPlayerCount.getFloat();
                case (RoleId.Sheriff):
                    return CustomOption.CustomOptions.SheriffPlayerCount.getFloat();
                case (RoleId.MeetingSheriff):
                    return CustomOption.CustomOptions.MeetingSheriffPlayerCount.getFloat();
                case (RoleId.Jackal):
                    return CustomOption.CustomOptions.JackalPlayerCount.getFloat();
                case (RoleId.Teleporter):
                    return CustomOption.CustomOptions.TeleporterPlayerCount.getFloat();
                case (RoleId.SpiritMedium):
                    return CustomOption.CustomOptions.SpiritMediumPlayerCount.getFloat();
                case (RoleId.SpeedBooster):
                    return CustomOption.CustomOptions.SpeedBoosterPlayerCount.getFloat();
                case (RoleId.EvilSpeedBooster):
                    return CustomOption.CustomOptions.EvilSpeedBoosterPlayerCount.getFloat();
                case (RoleId.Tasker):
                    return CustomOption.CustomOptions.TaskerPlayerCount.getFloat();
                case (RoleId.Doorr):
                    return CustomOption.CustomOptions.DoorrPlayerCount.getFloat();
                case (RoleId.EvilDoorr):
                    return CustomOption.CustomOptions.EvilDoorrPlayerCount.getFloat();
                case (RoleId.Shielder):
                    return CustomOption.CustomOptions.ShielderPlayerCount.getFloat();
                case (RoleId.Speeder):
                    return CustomOption.CustomOptions.SpeederPlayerCount.getFloat();
                case (RoleId.Freezer):
                    return CustomOption.CustomOptions.FreezerPlayerCount.getFloat();
                case (RoleId.Guesser):
                    return CustomOption.CustomOptions.GuesserPlayerCount.getFloat();
                case (RoleId.EvilGuesser):
                    return CustomOption.CustomOptions.EvilGuesserPlayerCount.getFloat();
                case (RoleId.Vulture):
                    return CustomOption.CustomOptions.VulturePlayerCount.getFloat();
                case (RoleId.NiceScientist):
                    return CustomOption.CustomOptions.NiceScientistPlayerCount.getFloat();
                case (RoleId.Clergyman):
                    return CustomOption.CustomOptions.ClergymanPlayerCount.getFloat();
                case (RoleId.MadMate):
                    return CustomOption.CustomOptions.MadMatePlayerCount.getFloat();
                case (RoleId.Bait):
                    return CustomOption.CustomOptions.BaitPlayerCount.getFloat();
                case (RoleId.HomeSecurityGuard):
                    return CustomOption.CustomOptions.HomeSecurityGuardPlayerCount.getFloat();
                case (RoleId.StuntMan):
                    return CustomOption.CustomOptions.StuntManPlayerCount.getFloat();
                case (RoleId.Moving):
                    return CustomOption.CustomOptions.MovingPlayerCount.getFloat();
                case (RoleId.Opportunist):
                    return CustomOption.CustomOptions.OpportunistPlayerCount.getFloat();
                case (RoleId.NiceGambler):
                    return CustomOption.CustomOptions.NiceGamblerPlayerCount.getFloat();
                case (RoleId.EvilGambler):
                    return CustomOption.CustomOptions.EvilGamblerPlayerCount.getFloat();
                case (RoleId.Bestfalsecharge):
                    return CustomOption.CustomOptions.BestfalsechargePlayerCount.getFloat();
                case (RoleId.Researcher):
                    return CustomOption.CustomOptions.ResearcherPlayerCount.getFloat();
                case (RoleId.SelfBomber):
                    return CustomOption.CustomOptions.SelfBomberPlayerCount.getFloat();
                case (RoleId.God):
                    return CustomOption.CustomOptions.GodPlayerCount.getFloat();
                case (RoleId.AllCleaner):
                    return CustomOption.CustomOptions.AllCleanerPlayerCount.getFloat();
                case (RoleId.NiceNekomata):
                    return CustomOption.CustomOptions.NiceNekomataPlayerCount.getFloat();
                case (RoleId.EvilNekomata):
                    return CustomOption.CustomOptions.EvilNekomataPlayerCount.getFloat();
                case (RoleId.JackalFriends):
                    return CustomOption.CustomOptions.JackalFriendsPlayerCount.getFloat();
                case (RoleId.Doctor):
                    return CustomOption.CustomOptions.DoctorPlayerCount.getFloat();
                case (RoleId.CountChanger):
                    return CustomOption.CustomOptions.CountChangerPlayerCount.getFloat();
                case (RoleId.Pursuer):
                    return CustomOption.CustomOptions.PursuerPlayerCount.getFloat();
                case (RoleId.Minimalist):
                    return CustomOption.CustomOptions.MinimalistPlayerCount.getFloat();
                case (RoleId.Hawk):
                    return CustomOption.CustomOptions.HawkPlayerCount.getFloat();
                case (RoleId.Egoist):
                    return CustomOption.CustomOptions.EgoistPlayerCount.getFloat();
                case (RoleId.NiceRedRidingHood):
                    return CustomOption.CustomOptions.NiceRedRidingHoodPlayerCount.getFloat();
                case (RoleId.EvilEraser):
                    return CustomOption.CustomOptions.EvilEraserPlayerCount.getFloat();
                case (RoleId.Workperson):
                    return CustomOption.CustomOptions.WorkpersonPlayerCount.getFloat();
                case (RoleId.Magaziner):
                    return CustomOption.CustomOptions.MagazinerPlayerCount.getFloat();
                case (RoleId.Mayor):
                    return CustomOption.CustomOptions.MayorPlayerCount.getFloat();
                case (RoleId.truelover):
                    return CustomOption.CustomOptions.trueloverPlayerCount.getFloat();
                case (RoleId.Technician):
                    return CustomOption.CustomOptions.TechnicianPlayerCount.getFloat();
                case (RoleId.SerialKiller):
                    return CustomOption.CustomOptions.SerialKillerPlayerCount.getFloat();
                case (RoleId.OverKiller):
                    return CustomOption.CustomOptions.OverKillerPlayerCount.getFloat();
                case (RoleId.Levelinger):
                    return CustomOption.CustomOptions.LevelingerPlayerCount.getFloat();
                case (RoleId.EvilMoving):
                    return CustomOption.CustomOptions.EvilMovingPlayerCount.getFloat();
                case (RoleId.Amnesiac):
                    return CustomOption.CustomOptions.AmnesiacPlayerCount.getFloat();
                case (RoleId.SideKiller):
                    return CustomOption.CustomOptions.SideKillerPlayerCount.getFloat();
                case (RoleId.Survivor):
                    return CustomOption.CustomOptions.SurvivorPlayerCount.getFloat();
                case (RoleId.MadMayor):
                    return CustomOption.CustomOptions.MadMayorPlayerCount.getFloat();
                case (RoleId.NiceHawk):
                    return CustomOption.CustomOptions.NiceHawkPlayerCount.getFloat();
                case (RoleId.Bakery):
                    return CustomOption.CustomOptions.BakeryPlayerCount.getFloat();
                case (RoleId.MadJester):
                    return CustomOption.CustomOptions.MadJesterPlayerCount.getFloat();
                case (RoleId.MadStuntMan):
                    return CustomOption.CustomOptions.MadStuntManPlayerCount.getFloat();
                case (RoleId.MadHawk):
                    return CustomOption.CustomOptions.MadHawkPlayerCount.getFloat();
                case (RoleId.FalseCharges):
                    return CustomOption.CustomOptions.FalseChargesPlayerCount.getFloat();
                case (RoleId.NiceTeleporter):
                    return CustomOption.CustomOptions.NiceTeleporterPlayerCount.getFloat();
                case (RoleId.Celebrity):
                    return CustomOption.CustomOptions.CelebrityPlayerCount.getFloat();
                case (RoleId.Nocturnality):
                    return CustomOption.CustomOptions.NocturnalityPlayerCount.getFloat();
                case (RoleId.Observer):
                    return CustomOption.CustomOptions.ObserverPlayerCount.getFloat();
                case (RoleId.Vampire):
                    return CustomOption.CustomOptions.VampirePlayerCount.getFloat();
                case (RoleId.DarkKiller):
                    return CustomOption.CustomOptions.DarkKillerPlayerCount.getFloat();
                case (RoleId.Seer):
                    return CustomOption.CustomOptions.SeerPlayerCount.getFloat();
                case (RoleId.MadSeer):
                    return CustomOption.CustomOptions.MadSeerPlayerCount.getFloat();
                case (RoleId.EvilSeer):
                    return CustomOption.CustomOptions.EvilSeerPlayerCount.getFloat();
                case (RoleId.RemoteSheriff):
                    return CustomOption.CustomOptions.RemoteSheriffPlayerCount.getFloat();
                case (RoleId.Fox):
                    return CustomOption.CustomOptions.FoxPlayerCount.getFloat();
                case (RoleId.TeleportingJackal):
                    return CustomOption.CustomOptions.TeleportingJackalPlayerCount.getFloat();
                case (RoleId.MadMaker):
                    return CustomOption.CustomOptions.MadMakerPlayerCount.getFloat();
                case (RoleId.Demon):
                    return CustomOption.CustomOptions.DemonPlayerCount.getFloat();
                case (RoleId.TaskManager):
                    return CustomOption.CustomOptions.TaskManagerPlayerCount.getFloat();
                case (RoleId.SeerFriends):
                    return CustomOption.CustomOptions.SeerFriendsPlayerCount.getFloat();
                case (RoleId.JackalSeer):
                    return CustomOption.CustomOptions.JackalSeerPlayerCount.getFloat();
                case (RoleId.Assassin):
                    return CustomOption.CustomOptions.AssassinPlayerCount.getFloat();
                case (RoleId.Marine):
                    return CustomOption.CustomOptions.MarinePlayerCount.getFloat();
                case (RoleId.Arsonist):
                    return CustomOption.CustomOptions.ArsonistPlayerCount.getFloat();
                case (RoleId.Chief):
                    return CustomOption.CustomOptions.ChiefPlayerCount.getFloat();
                case (RoleId.Cleaner):
                    return CustomOption.CustomOptions.CleanerPlayerCount.getFloat();
                case (RoleId.MadCleaner):
                    return CustomOption.CustomOptions.MadCleanerPlayerCount.getFloat();
                case (RoleId.Samurai):
                    return CustomOption.CustomOptions.SamuraiPlayerCount.getFloat();
                case (RoleId.MayorFriends):
                    return CustomOption.CustomOptions.MayorFriendsPlayerCount.getFloat();
                case (RoleId.VentMaker):
                    return CustomOption.CustomOptions.VentMakerPlayerCount.getFloat();
                case (RoleId.GhostMechanic):
                    return CustomOption.CustomOptions.GhostMechanicPlayerCount.getFloat();
                case (RoleId.EvilHacker):
                    return CustomOption.CustomOptions.EvilHackerPlayerCount.getFloat();
                case (RoleId.HauntedWolf):
                    return CustomOption.CustomOptions.HauntedWolfPlayerCount.getFloat();
                    //プレイヤーカウント
            }
            return 1;
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
        //セットクラス
        }
    }
}