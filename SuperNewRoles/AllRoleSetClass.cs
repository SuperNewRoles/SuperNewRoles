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

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.RpcSetRole))]
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
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.Disconnected)
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
                Mode.SuperHostRoles.RoleSelectHandler.RoleSelect();
                foreach (PlayerControl player in AllRoleSetClass.impostors)
                {
                    player.RpcSetRole(RoleTypes.Impostor);
                }
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (!player.Data.Disconnected && !AllRoleSetClass.impostors.IsCheckListPlayerControl(player))
                    {
                        player.RpcSetRole(RoleTypes.Crewmate);
                    }
                }
                return false;
            } else if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                Mode.BattleRoyal.main.ChangeRole.Postfix();
                new LateTask(() => {
                    if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetRole(RoleTypes.Shapeshifter);
                        }
                    }
                }, 3f, "SetImpostor");
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
                AllRoleSetClass.OneOrNotListSet();
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
            if (!ModeHandler.isMode(ModeId.NotImpostorCheck) && !ModeHandler.isMode(ModeId.BattleRoyal) && !ModeHandler.isMode(ModeId.Default) && !ModeHandler.isMode(ModeId.SuperHostRoles)) {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    p.RpcSetRole(p.Data.Role.Role);
                }
                /*AmongUsClient.Instance.StartCoroutine(nameof(SetServerRole));*/
            }
            if (!ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                new LateTask(() => {
                    if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetRole(RoleTypes.Shapeshifter);
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
        public static int NeutralPlayerNum;
        public static int CrewMatePlayerNum;

        public static void AllRoleSet()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetPlayerNum();
            if (!ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                CrewOrImpostorSet();
            }
            try
            {
                ImpostorRandomSelect();
            } catch (Exception e)
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
        public static void QuarreledRandomSelect()
        {
            if (!CustomOption.CustomOptions.QuarreledOption.getBool()) return;
            List<PlayerControl> SelectPlayers = new List<PlayerControl>();
            if (CustomOption.CustomOptions.QuarreledOnlyCrewMate.getBool())
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.Role.IsImpostor && !p.isNeutral())
                    {
                        SelectPlayers.Add(p);
                    }
                }
            } else
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    SelectPlayers.Add(p);
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
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.isImpostor() && !p.isNeutral() && !p.isRole(RoleId.truelover))
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
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!IsQuarreledDup || !p.IsQuarreled())
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
            ImpostorPlayerNum = (int)CustomOption.CustomOptions.impostorRolesCountMax.getFloat();
            NeutralPlayerNum = (int)CustomOption.CustomOptions.neutralRolesCountMax.getFloat();
            CrewMatePlayerNum = (int)CustomOption.CustomOptions.crewmateRolesCountMax.getFloat();
        }
        public static void ImpostorRandomSelect()
        {
            if (ImpostorPlayerNum == 0 || (Impoonepar.Count == 0 && Imponotonepar.Count == 0))
            {
                return;
            }
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
                        } catch
                        {

                        }
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

                    } else if (PlayerCount >= ImpostorPlayers.Count)
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
                } else
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

                    } else if (PlayerCount >= ImpostorPlayers.Count) {
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
                case (RoleId.Sealdor):
                    return CustomOption.CustomOptions.SealdorPlayerCount.getFloat();
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
                    //プレイヤーカウント
            }
            return 1;
        }
        public static void CrewOrImpostorSet()
        {
            CrewMatePlayers = new List<PlayerControl>();
            ImpostorPlayers = new List<PlayerControl>();
            foreach (PlayerControl Player in PlayerControl.AllPlayerControls)
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
            if (!(CustomOption.CustomOptions.SoothSayerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SoothSayerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SoothSayer;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Jester;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.LighterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.LighterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Lighter;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            /**
            if (!(CustomOption.CustomOptions.EvilLighterOption.getString().Replace("0%", "") == ""))
            {
                SuperNewRolesPlugin.Logger.LogInfo("EvilLighterSelected!!!!");
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilLighterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilLighter;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            */
            if (!(CustomOption.CustomOptions.EvilScientistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilScientistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilScientist;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Sheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MeetingSheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MeetingSheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MeetingSheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JackalOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JackalOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Jackal;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.TeleporterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TeleporterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Teleporter;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SpiritMediumOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpiritMediumOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SpiritMedium;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SpeedBoosterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpeedBoosterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SpeedBooster;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilSpeedBoosterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilSpeedBoosterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilSpeedBooster;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            /**
            if (!(CustomOption.CustomOptions.TaskerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TaskerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Tasker;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            **/
            if (!(CustomOption.CustomOptions.DoorrOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.DoorrOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Doorr;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilDoorrOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilDoorrOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilDoorr;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            /**
            if (!(CustomOption.CustomOptions.SealdorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SealdorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Sealdor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SpeederOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SpeederOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Speeder;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.FreezerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FreezerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Freezer;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.GuesserOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.GuesserOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Guesser;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilGuesserOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilGuesserOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilGuesser;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.VultureOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.VultureOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Vulture;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            */
            if (!(CustomOption.CustomOptions.NiceScientistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceScientistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceScientist;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.ClergymanOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ClergymanOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Clergyman;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMate;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bait;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.HomeSecurityGuard;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.StuntMan;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MovingOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MovingOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Moving;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Opportunist;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            /**
            if (!(CustomOption.CustomOptions.NiceGamblerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceGamblerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceGambler;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            **/
            if (!(CustomOption.CustomOptions.EvilGamblerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilGamblerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilGambler;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bestfalsecharge;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            /*
        if (!(CustomOption.CustomOptions.ResearcherOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ResearcherOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Researcher;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            **/
            if (!(CustomOption.CustomOptions.SelfBomberOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SelfBomberOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SelfBomber;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.GodOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.GodOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.God;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            /**
            if (!(CustomOption.CustomOptions.AllCleanerOption.getString().Replace("0%", "") == ""))
                {
                    int OptionDate = int.Parse(CustomOption.CustomOptions.AllCleanerOption.getString().Replace("0%", ""));
                    RoleId ThisRoleId = RoleId.AllCleaner;
                    if (OptionDate == 10)
                    {
                        Impoonepar.Add(ThisRoleId);
                    }
                    else
                    {
                        for (int i = 1; i <= OptionDate; i++)
                        {
                            Imponotonepar.Add(ThisRoleId);
                        }
                    }
                }
                **/
            if (!(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceNekomata;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilNekomata;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JackalFriendsOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JackalFriendsOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.JackalFriends;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.DoctorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.DoctorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Doctor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.CountChangerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.CountChangerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.CountChanger;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.PursuerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.PursuerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Pursuer;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Minimalist;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.HawkOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.HawkOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Hawk;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Egoist;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NiceRedRidingHoodOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceRedRidingHoodOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceRedRidingHood;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilEraserOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilEraserOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilEraser;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.WorkpersonOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.WorkpersonOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Workperson;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MagazinerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MagazinerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Magaziner;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MayorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MayorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Mayor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.trueloverOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.trueloverOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.truelover;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.TechnicianOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TechnicianOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Technician;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SerialKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SerialKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SerialKiller;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.OverKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.OverKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.OverKiller;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.LevelingerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.LevelingerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Levelinger;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilMovingOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilMovingOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilMoving;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.AmnesiacOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.AmnesiacOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Amnesiac;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SideKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SideKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SideKiller;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SurvivorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SurvivorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Survivor;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadMayorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMayorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMayor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NiceHawkOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceHawkOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceHawk;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.BakeryOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BakeryOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bakery;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadHawkOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadHawkOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadHawk;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.FalseCharges;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NiceTeleporterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceTeleporterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceTeleporter;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadStuntManOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadStuntManOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadStuntMan;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadHawkOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadHawkOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadHawk;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadJesterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadJesterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadJester;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.FalseCharges;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NiceTeleporterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceTeleporterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceTeleporter;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.CelebrityOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.CelebrityOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Celebrity;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }

            }
            if (!(CustomOption.CustomOptions.NocturnalityOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NocturnalityOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Nocturnality;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.ObserverOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ObserverOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Observer;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            //セットクラス
        }
    }
}
