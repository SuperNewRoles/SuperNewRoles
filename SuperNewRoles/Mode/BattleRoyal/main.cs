
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System.Collections.Generic;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode.BattleRoyal
{
    class main
    {
        public static void FixedUpdate()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (IsStart)
            {
                CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
                if (!IsTeamBattle)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(Buttons.HudManagerStartPatch.setTarget());
                }
                int alives = 0;
                int allplayer = 0;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    allplayer++;
                    if (p.isAlive())
                    {
                        alives++;
                    }
                }
                if (AlivePlayer != alives || AllPlayer != allplayer)
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            p.RpcSetNamePrivate("(" + alives + "/" + allplayer + ")");
                        }
                    }
                    AlivePlayer = alives;
                    AllPlayer = allplayer;
                }
            } else
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
                            p.RpcSetNamePrivate("キルができるようになるまで残り" + ((int)StartSeconds + 1) + "秒");
                        }
                    }
                    UpdateTime += 1f;
                }
                if (StartSeconds <= 0)
                {
                    IsStart = true;
                    foreach (List<PlayerControl> team in Teams)
                    {
                        if (team.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                        {
                            foreach (PlayerControl p in team)
                            {
                                if (p.PlayerId != 0)
                                {
                                    PlayerControl.LocalPlayer.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed,"プレイヤー"),p);
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
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
        class CoExitVentPatch
        {
            public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
            {
                VentData[__instance.myPlayer.PlayerId] = null;
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
                    /*
                    
                    */
                    if (ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie) || ModeHandler.isMode(ModeId.CopsRobbers))
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                        writer.WritePacked(127);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        new LateTask(() =>
                        {
                            int clientId = __instance.myPlayer.getClientId();
                            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                            writer2.Write(id);
                            AmongUsClient.Instance.FinishRpcImmediately(writer2);
                            __instance.myPlayer.inVent = false;
                        }, 0.5f, "Anti Vent");
                        return false;
                    }
                    else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                    {
                        bool data = CoEnterVent.Prefix(__instance, id);
                        if (data)
                        {
                            VentData[__instance.myPlayer.PlayerId] = id;
                        }
                        return data;
                    }
                }
                VentData[__instance.myPlayer.PlayerId] = id;
                return true;
            }
        }
        public static Dictionary<byte, int?> VentData;
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
        class RepairSystemPatch
        {
            public static bool Prefix(ShipStatus __instance,
                [HarmonyArgument(0)] SystemTypes systemType,
                [HarmonyArgument(1)] PlayerControl player,
                [HarmonyArgument(2)] byte amount)
            {
                if (PlusModeHandler.isMode(PlusModeId.NotSabotage))
                {
                    return false;
                }
                if ((ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.Zombie) || ModeHandler.isMode(ModeId.HideAndSeek) || ModeHandler.isMode(ModeId.CopsRobbers)) && (systemType == SystemTypes.Sabotage || systemType == SystemTypes.Doors)) return false;
                if (systemType == SystemTypes.Electrical && 0 <= amount && amount <= 4 && player.isRole(CustomRPC.RoleId.MadMate))
                {
                    return false;
                }
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    bool returndata = MorePatch.RepairSystem(__instance, systemType, player, amount);
                    return returndata;
                }
                return true;
            }
            public static void Postfix(ShipStatus __instance,
                [HarmonyArgument(0)] SystemTypes systemType,
                [HarmonyArgument(1)] PlayerControl player,
                [HarmonyArgument(2)] byte amount)
            {
                new LateTask(() =>
                {
                    if (!RoleHelpers.IsSabotage())
                    {
                        foreach (PlayerControl p in RoleClass.Technician.TechnicianPlayer)
                        {
                            if (p.inVent && p.isAlive() && VentData.ContainsKey(p.PlayerId) && VentData[p.PlayerId] != null)
                            {
                                p.MyPhysics.RpcBootFromVent((int)VentData[p.PlayerId]);
                            }
                        }
                    }
                }, 0.1f, "TecExitVent");
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    SyncSetting.CustomSyncSettings();
                }
            }
        }
        public static List<PlayerControl> Winners;
        public static bool IsViewAlivePlayer;
        public static bool EndGameCheck(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (IsTeamBattle)
            {
                if (!IsSeted) return false;
                List<PlayerControl> players = new List<PlayerControl>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.isAlive())
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
                    if (teams.IsCheckListPlayerControl(players[0])){
                        foreach (PlayerControl p in players)
                        {
                            if (!teams.IsCheckListPlayerControl(p))
                            {
                                return false;
                            }
                        }
                    }
                }
                Winners = new List<PlayerControl>();
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
                                    var writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.ShareWinner);
                                    writer.Write(p.PlayerId);
                                    writer.EndRPC();
                                }
                            }
                        }
                    }
                }
                catch { SuperNewRolesPlugin.Logger.LogInfo("Winnersエラー"); }
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            } else
            {
                var alives = 0;
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.isAlive())
                    {
                        alives++;
                    }
                }
                if (alives == 1)
                {
                    __instance.enabled = false;
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p.isAlive())
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
            IsViewAlivePlayer = BROption.IsViewAlivePlayer.getBool();
            AlivePlayer = 0;
            AllPlayer = 0;
            IsStart = false;
            StartSeconds = BROption.StartSeconds.getFloat()+4.5f;
            IsCountOK = false;
            UpdateTime = 0f;
            IsTeamBattle = BROption.IsTeamBattle.getBool();
            Teams = new List<List<PlayerControl>>();
            IsSeted = false;
            Winners = new List<PlayerControl>();
        }
        public static class ChangeRole
        {
            public static void Postfix()
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (IsTeamBattle)
                    {
                        float count = BROption.TeamAmount.getFloat();
                        var oneteamcount = Mathf.CeilToInt(CachedPlayer.AllPlayers.Count / count);
                        List<PlayerControl> target = new List<PlayerControl>();
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            target.Add(p);
                        }
                        List<PlayerControl> TempTeam = new List<PlayerControl>();
                        var counttemp = target.Count;
                        for (int i = 0;i < counttemp; i++)
                        {
                            SuperNewRolesPlugin.Logger.LogInfo("oneTeamCount:"+oneteamcount);
                            SuperNewRolesPlugin.Logger.LogInfo("index:"+i);
                            if (target.Count > 0)
                            {
                                var index = ModHelpers.GetRandomIndex(target);
                                TempTeam.Add(target[index]);
                                target.RemoveAt(index);
                                SuperNewRolesPlugin.Logger.LogInfo("ついか");
                                SuperNewRolesPlugin.Logger.LogInfo("てんぷちーむ:"+ TempTeam.Count);
                                if (TempTeam.Count >= oneteamcount)
                                {
                                    Teams.Add(TempTeam);
                                    TempTeam = new List<PlayerControl>();
                                    SuperNewRolesPlugin.Logger.LogInfo("リセット");
                                }
                            }
                        }
                        if (TempTeam.Count > 0)
                        {
                            Teams.Add(TempTeam);
                            TempTeam = new List<PlayerControl>();
                            SuperNewRolesPlugin.Logger.LogInfo("リセット");
                        }
                        SuperNewRolesPlugin.Logger.LogInfo("チーム数:"+Teams.Count);
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
                                            SuperNewRolesPlugin.Logger.LogInfo("セットチーム内");
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
                                } else
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
                        p.getDefaultName();
                        p.RpcSetName("");//Playing on SuperNewRoles!");
                    }
                    new LateTask(() => {
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
