using System.Collections.Generic;
using HarmonyLib;
using Hazel;

using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class Main
    {
        public static Dictionary<byte, int> KillCount;
        public static void FixedUpdate()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (IsStart)
            {
                CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
                if (!IsTeamBattle)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(Buttons.HudManagerStartPatch.SetTarget());
                }
                int alives = 0;
                int allplayer = 0;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    allplayer++;
                    if (p.IsAlive())
                    {
                        alives++;
                    }
                }
                if ((AlivePlayer != alives || AllPlayer != allplayer) && BROption.IsViewAlivePlayer.GetBool())
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            string EndText = " ";
                            if (BROption.IsKillCountView.GetBool())
                            {
                                if (KillCount.ContainsKey(p.PlayerId)) EndText += KillCount[p.PlayerId];
                                else EndText += "0";

                                if (!BROption.IsKillCountViewSelfOnly.GetBool())
                                {
                                    foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                                    {
                                        if (p2.PlayerId == p.PlayerId) continue;
                                        p.RpcSetNamePrivate(EndText, p2);
                                    }
                                }
                            }
                            p.RpcSetNamePrivate($"({alives}/{allplayer}){EndText}");
                        }
                    }
                    AlivePlayer = alives;
                    AllPlayer = allplayer;
                }
            }
            else
            {
                if (IsCountOK)
                {
                    StartSeconds -= Time.fixedDeltaTime;
                }
                UpdateTime -= Time.fixedDeltaTime;
                if (UpdateTime <= 0)
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            p.RpcSetNamePrivate(ModTranslation.GetString("BattleRoyalRemaining") + ((int)StartSeconds + 1) + ModTranslation.GetString("second"));
                        }
                    }
                    UpdateTime += 1f;
                }
                if (StartSeconds <= 0)
                {
                    IsStart = true;
                    ModeHandler.HideName();
                    foreach (List<PlayerControl> team in Teams)
                    {
                        if (team.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                        {
                            foreach (PlayerControl p in team)
                            {
                                if (p.PlayerId != 0)
                                {
                                    PlayerControl.LocalPlayer.RpcSetNamePrivate(ModHelpers.Cs(RoleClass.ImpostorRed, ModTranslation.GetString("Player")), p);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static int AlivePlayer;
        public static int AllPlayer;
        public static bool IsStart;
        public static void MurderPlayer(PlayerControl source, PlayerControl target)
        {
            if (!KillCount.ContainsKey(source.PlayerId)) KillCount[source.PlayerId] = 0;
            KillCount[source.PlayerId]++;
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
        class CoExitVentPatch
        {
            public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
            {
                VentData[__instance.myPlayer.PlayerId] = null;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.myPlayer.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.ExitVent);
                return true;
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
        class CoEnterVentPatch
        {
            public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (ModeHandler.IsMode(ModeId.BattleRoyal) || ModeHandler.IsMode(ModeId.Zombie) || ModeHandler.IsMode(ModeId.CopsRobbers))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                        writer.WritePacked(127);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        new LateTask(() =>
                        {
                            int clientId = __instance.myPlayer.GetClientId();
                            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                            writer2.Write(id);
                            AmongUsClient.Instance.FinishRpcImmediately(writer2);
                            __instance.myPlayer.inVent = false;
                        }, 0.5f, "Anti Vent");
                        return false;
                    }
                    else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        bool data = CoEnterVent.Prefix(__instance, id);
                        if (data)
                        {
                            VentData[__instance.myPlayer.PlayerId] = id;
                        }
                        return data;
                    }
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.myPlayer.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.InVent);
                VentData[__instance.myPlayer.PlayerId] = id;
                return true;
            }
        }
        public static Dictionary<byte, int?> VentData;
        public static List<PlayerControl> Winners;
        public static bool IsViewAlivePlayer;
        public static bool EndGameCheck(ShipStatus __instance)
        {
            if (IsTeamBattle)
            {
                if (!IsSeted) return false;
                List<PlayerControl> players = new();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsAlive())
                    {
                        players.Add(p);
                    }
                }
                if (players.Count <= 0)
                {
                    __instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                    return true;
                }
                foreach (List<PlayerControl> teams in Teams)
                {
                    if (teams.IsCheckListPlayerControl(players[0]))
                    {
                        foreach (PlayerControl p in players)
                        {
                            if (!teams.IsCheckListPlayerControl(p))
                            {
                                return false;
                            }
                        }
                    }
                }
                Winners = new();
                try
                {
                    foreach (List<PlayerControl> teams in Teams)
                    {
                        if (teams.IsCheckListPlayerControl(players[0]))
                        {
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                p.RpcSetRole(RoleTypes.GuardianAngel);
                                if (teams.IsCheckListPlayerControl(p))
                                {
                                    p.Data.IsDead = false;
                                    Winners.Add(p);
                                    var writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                                    writer.Write(p.PlayerId);
                                    writer.EndRPC();
                                }
                            }
                        }
                    }
                }
                catch { SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal:Error] Winners Erroe"); }
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            else
            {
                var alives = 0;
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsAlive())
                    {
                        alives++;
                    }
                }
                if (alives == 1)
                {
                    __instance.enabled = false;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.IsAlive())
                        {
                            p.RpcSetRole(RoleTypes.Impostor);
                        }
                        else
                        {
                            p.RpcSetRole(RoleTypes.GuardianAngel);
                        }
                    }
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                    return true;
                }
                else if (alives == 0)
                {
                    __instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.HumansByVote, false);
                    return true;
                }
            }
            return false;
        }
        public static float StartSeconds;
        public static bool IsCountOK;
        static float UpdateTime;
        public static bool IsTeamBattle;
        public static List<List<PlayerControl>> Teams;
        static bool IsSeted;
        public static void ClearAndReload()
        {
            IsViewAlivePlayer = BROption.IsViewAlivePlayer.GetBool();
            KillCount = new();
            AlivePlayer = 0;
            AllPlayer = 0;
            IsStart = false;
            StartSeconds = BROption.StartSeconds.GetFloat() + 4.5f;
            IsCountOK = false;
            UpdateTime = 0f;
            IsTeamBattle = BROption.IsTeamBattle.GetBool();
            Teams = new List<List<PlayerControl>>();
            IsSeted = false;
            Winners = new();
        }
        public static class ChangeRole
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (IsTeamBattle)
                    {
                        float count = BROption.TeamAmount.GetFloat();
                        var oneteamcount = Mathf.CeilToInt(CachedPlayer.AllPlayers.Count / count);
                        List<PlayerControl> target = new();
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            target.Add(p);
                        }
                        List<PlayerControl> TempTeam = new();
                        var counttemp = target.Count;
                        for (int i = 0; i < counttemp; i++)
                        {
                            SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] OneTeamCount:" + oneteamcount);
                            SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Index:" + i);
                            if (target.Count > 0)
                            {
                                var index = ModHelpers.GetRandomIndex(target);
                                TempTeam.Add(target[index]);
                                target.RemoveAt(index);
                                SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Add");
                                SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Template Team:" + TempTeam.Count);
                                if (TempTeam.Count >= oneteamcount)
                                {
                                    Teams.Add(TempTeam);
                                    TempTeam = new();
                                    SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Reset");
                                }
                            }
                        }
                        if (TempTeam.Count > 0)
                        {
                            Teams.Add(TempTeam);
                            TempTeam = new();
                            SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Reset");
                        }
                        SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Team Count:" + Teams.Count);
                        foreach (List<PlayerControl> teamlist in Teams)
                        {
                            foreach (PlayerControl p in teamlist)
                            {
                                if (p.PlayerId != 0)
                                {
                                    foreach (PlayerControl p2 in teamlist)
                                    {
                                        if (p2.PlayerId != 0)
                                        {
                                            SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Within a Set Team");
                                            p.RpcSetRoleDesync(RoleTypes.Impostor, p2);
                                        }
                                        else
                                        {
                                            p.SetRole(RoleTypes.Impostor);
                                        }
                                    }
                                    foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                                    {
                                        if (!teamlist.IsCheckListPlayerControl(p2))
                                        {
                                            p2.RpcSetRoleDesync(RoleTypes.Scientist, p);
                                            p.RpcSetRoleDesync(RoleTypes.Scientist, p2);
                                        }
                                    }
                                }
                                else
                                {
                                    p.SetRole(RoleTypes.Impostor);
                                    p.RpcSetRole(RoleTypes.Crewmate);
                                    DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                                    CachedPlayer.LocalPlayer.Data.Role.Role = RoleTypes.Impostor;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
                        {
                            if (p1.PlayerId != 0)
                            {
                                DestroyableSingleton<RoleManager>.Instance.SetRole(p1, RoleTypes.Crewmate);
                                p1.RpcSetRoleDesync(RoleTypes.Impostor);
                                foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                                {
                                    if (p1.PlayerId != p2.PlayerId && p2.PlayerId != 0)
                                    {
                                        p1.RpcSetRoleDesync(RoleTypes.Scientist, p2);
                                        p2.RpcSetRoleDesync(RoleTypes.Scientist, p1);
                                    }
                                }
                            }
                            else
                            {
                                p1.RpcSetRole(RoleTypes.Crewmate);
                            }
                        }
                        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                        CachedPlayer.LocalPlayer.Data.Role.Role = RoleTypes.Impostor;
                    }
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        p.GetDefaultName();
                        p.RpcSetName("");//Playing on SuperNewRoles!");
                    }
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
                    IsSeted = true;
                }
            }
        }
    }
}