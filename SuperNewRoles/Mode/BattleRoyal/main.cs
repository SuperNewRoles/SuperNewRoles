using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal;

class Main
{
    public static Dictionary<byte, int> KillCount;
    public static bool IsRoleSetted;
    public static List<PlayerControl> RoleSettedPlayers = new();
    public static void SpawnBots()
    {
        CrystalMagician.Bot = BotManager.Spawn(ModTranslation.GetString("CrystalMagicianCrystalBot"));
    }
    public static void FixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return;
        if (!IsRoleSetted) return;
        //SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
        ChangeName.FixedUpdate();
        BattleRoyalRole.BattleRoyalRoles.RemoveAll(x => x.CurrentPlayer == null);
        foreach (BattleRoyalRole role in BattleRoyalRole.BattleRoyalRoles.AsSpan())
        {
            role.FixedUpdate();
        }
        if (IsStart)
        {
            CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
            if (!IsTeamBattle)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(Buttons.HudManagerStartPatch.SetTarget());
            }
            int alives = 0;
            int allplayer = 0;
            foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            {
                allplayer++;
                if (p.IsAlive())
                {
                    alives++;
                }
            }
            if ((AlivePlayer != alives || AllPlayer != allplayer) && BROption.IsViewAlivePlayer.GetBool())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
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
                                foreach (PlayerControl p2 in CachedPlayer.AllPlayers.AsSpan())
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
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (!p.Data.Disconnected)
                    {
                        p.GetDefaultName();
                        p.RpcSetNamePrivate(ModTranslation.GetString("BattleRoyalRemaining") + ((int)StartSeconds + 1) + ModTranslation.GetString("second"));
                    }
                }
                UpdateTime += 1f;
            }
            if (StartSeconds <= 0)
            {
                IsStart = true;
                //ModeHandler.HideName();
                foreach (BattleTeam team in BattleTeam.BattleTeams.AsSpan())
                {
                    if (team.IsTeam(PlayerControl.LocalPlayer))
                    {
                        foreach (PlayerControl p in team.TeamMember.AsSpan())
                        {
                            if (p.PlayerId != 0)
                            {
                                PlayerControl.LocalPlayer.RpcSetNamePrivate(ModHelpers.Cs(RoleClass.ImpostorRed, ModTranslation.GetString("Player")), p);
                            }
                        }
                    }
                }
                ChangeName.UpdateName();
            }
        }
    }
    public static int AlivePlayer;
    public static int AllPlayer;
    public static bool IsStart;
    public static void MurderPlayer(PlayerControl source, PlayerControl target, PlayerAbility targetAbility)
    {
        if (source.IsRole(RoleId.LongKiller) && !PlayerAbility.GetPlayerAbility(target).CanUseKill) return;
        target.Data.IsDead = true;
        if (targetAbility.CanRevive)
        {
            source.RpcSnapTo(target.transform.position);
            GameDataSerializePatch.Is = true;
            RPCHelper.RpcSyncNetworkedPlayer(target.Data);
        }
        else
        {
            source.RpcMurderPlayer(target, true);
        }
        SyncBattleOptions.CustomSyncOptions();
        if (source.IsRole(RoleId.Darknight))
        {
            Darknight.GetDarknightPlayer(source).OnKill(target);
        }
        foreach (Revenger revenger in Revenger.revengers.AsSpan())
        {
            revenger.OnKill(source, target);
        }
        if (!KillCount.ContainsKey(source.PlayerId)) KillCount[source.PlayerId] = 0;
        KillCount[source.PlayerId]++;
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
    class CoExitVentPatch
    {
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
        {
            VentData[__instance.myPlayer.PlayerId] = null;
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.myPlayer.PlayerId) Roles.Crewmate.Painter.Handle(Roles.Crewmate.Painter.ActionType.ExitVent);
            VentInfo.OnExitVent(__instance.myPlayer, id);
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
                if (ModeHandler.IsMode(ModeId.BattleRoyal,
                    ModeId.Zombie, ModeId.CopsRobbers) ||
                    (
                    ModeHandler.IsMode(ModeId.PantsRoyal) &&
                    PantsRoyal.main.IsPantsHaver(__instance.myPlayer.PlayerId)
                    ))
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
                    if (!data)
                        return false;
                    VentData[__instance.myPlayer.PlayerId] = id;
                }
            }
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.myPlayer.PlayerId) Roles.Crewmate.Painter.Handle(Roles.Crewmate.Painter.ActionType.InVent);
            VentData[__instance.myPlayer.PlayerId] = id;
            VentInfo.OnEnterVent(__instance.myPlayer, id);
            return true;
        }
    }
    public static Dictionary<byte, int?> VentData;
    public static List<PlayerControl> Winners;
    public static bool IsViewAlivePlayer;
    public static bool EndGameCheck(ShipStatus __instance)
    {
        if (!IsRoleSetted) return false;
        if (IsTeamBattle)
        {
            if (!IsSeted) return false;
            List<PlayerControl> players = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            {
                if (p.IsBot()) continue;
                if (p.IsAlive())
                {
                    players.Add(p);
                }
            }
            if (players.Count <= 0)
            {
                __instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            bool Flag = false;
            foreach (BattleTeam team in BattleTeam.BattleTeams.AsSpan())
            {
                foreach (PlayerControl player in team.TeamMember.AsSpan())
                {
                    if (player.IsDead()) continue;
                    if (Flag)
                    {
                        return false;
                    }
                    Flag = true;
                    break;
                }
            }
            Winners = new();
            try
            {
                foreach (BattleTeam teams in BattleTeam.BattleTeams.AsSpan())
                {
                    if (teams.IsTeam(players[0]))
                    {
                        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                        {
                            p.RpcSetRole(RoleTypes.GuardianAngel);
                            if (teams.IsTeam(p))
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
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }
        else
        {
            var alives = 0;
            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.gameObject.SetActive(false);
            foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
            {
                if (p.IsBot()) continue;
                if (p.IsAlive())
                {
                    alives++;
                }
            }
            if (alives == 1)
            {
                __instance.enabled = false;
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
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
                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            else if (alives == 0)
            {
                __instance.enabled = false;
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
        }
        return false;
    }
    public static float StartSeconds;
    public static bool IsCountOK;
    static float UpdateTime;
    public static bool IsTeamBattle;
    static bool IsSeted;
    public static bool IsIntroEnded;
    public static void ClearAndReload()
    {
        IsIntroEnded = false;
        BattleTeam.ClearAll();
        PlayerAbility.ClearAll();

        ChangeName.ClearAll();

        BattleRoyalRole.ClearAll();
        Reviver.Clear();
        Guardrawer.Clear();
        KingPoster.Clear();
        LongKiller.Clear();
        Darknight.Clear();
        Revenger.Clear();
        CrystalMagician.Clear();
        GrimReaper.Clear();

        RoleSettedPlayers = new();
        IsRoleSetted = false;
        IsViewAlivePlayer = BROption.IsViewAlivePlayer.GetBool();
        KillCount = new();
        AlivePlayer = 0;
        AllPlayer = 0;
        IsStart = false;
        StartSeconds = BROption.StartSeconds.GetFloat();
        IsCountOK = false;
        UpdateTime = 0f;
        IsTeamBattle = BROption.IsTeamBattle.GetBool();
        IsSeted = false;
        Winners = new();
    }
    public static class ChangeRole
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (!p.IsBot()) continue;
                    p.RpcSetRole(RoleTypes.Scientist);
                }
                if (IsTeamBattle)
                {
                    float count = BROption.TeamAmount.GetFloat();
                    List<PlayerControl> target = new();
                    foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (p.IsBot()) continue;
                        target.Add(p);
                    }
                    var oneteamcount = Mathf.CeilToInt(target.Count / count);
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
                                BattleTeam teamobj = new()
                                {
                                    TeamMember = TempTeam
                                };
                                TempTeam = new();
                                SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Reset");
                            }
                        }
                    }
                    if (TempTeam.Count > 0)
                    {
                        BattleTeam teamobj = new()
                        {
                            TeamMember = TempTeam
                        };
                        TempTeam = new();
                        SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Reset");
                    }
                    SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Team Count:" + BattleTeam.BattleTeams.Count);
                    foreach (BattleTeam teamlist in BattleTeam.BattleTeams.AsSpan())
                    {
                        foreach (PlayerControl p in teamlist.TeamMember.AsSpan())
                        {
                            if (p.PlayerId != 0)
                            {
                                foreach (PlayerControl p2 in teamlist.TeamMember.AsSpan())
                                {
                                    if (p2.PlayerId != 0)
                                    {
                                        SuperNewRolesPlugin.Logger.LogInfo("[BattleRoyal] Within a Set Team");
                                        p.RpcSetRoleDesync(RoleTypes.Shapeshifter, p2);
                                    }
                                    else
                                    {
                                        p.SetRole(RoleTypes.Shapeshifter);
                                    }
                                }
                                foreach (PlayerControl p2 in CachedPlayer.AllPlayers.AsSpan())
                                {
                                    if (!teamlist.IsTeam(p2))
                                    {
                                        p2.RpcSetRoleDesync(RoleTypes.Scientist, p);
                                        p.RpcSetRoleDesync(RoleTypes.Scientist, p2);
                                    }
                                }
                            }
                            else
                            {
                                p.SetRole(RoleTypes.Shapeshifter);
                                p.RpcSetRole(RoleTypes.Crewmate);
                                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                                CachedPlayer.LocalPlayer.Data.Role.Role = RoleTypes.Shapeshifter;
                            }
                        }
                    }
                }
                else
                {
                    foreach (PlayerControl p1 in CachedPlayer.AllPlayers.AsSpan())
                    {
                        if (p1.IsBot()) continue;
                        new BattleTeam()
                        {
                            TeamMember = new() { p1 }
                        };
                        if (p1.PlayerId != 0)
                        {
                            FastDestroyableSingleton<RoleManager>.Instance.SetRole(p1, RoleTypes.Crewmate);
                            p1.RpcSetRoleDesync(RoleTypes.Shapeshifter, false);
                            foreach (PlayerControl p2 in CachedPlayer.AllPlayers.AsSpan())
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
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                    CachedPlayer.LocalPlayer.Data.Role.Role = RoleTypes.Shapeshifter;
                }
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (p.IsBot()) continue;
                    p.GetDefaultName();
                    new PlayerAbility(p);
                }
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    p.Data.RoleType = RoleTypes.Shapeshifter;
                }
                RPCHelper.RpcSyncAllNetworkedPlayer();
                new LateTask(() =>
                {
                    if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                    {
                        foreach (var pc in CachedPlayer.AllPlayers.AsSpan())
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