using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class FixedUpdate
{
    public static Dictionary<int, string> DefaultName = new();
    private static int UpdateData = 0;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
    class CoShowIntroPatch
    {
        public static void Prefix()
        {
            DefaultName = new Dictionary<int, string>();
            foreach (PlayerControl pc in CachedPlayer.AllPlayers)
            {
                //SuperNewRolesPlugin.Logger.LogInfo($"{pc.PlayerId}:{pc.name}:{pc.NameText().text}");
                DefaultName[pc.PlayerId] = pc.Data.PlayerName;
                pc.NameText().text = pc.Data.PlayerName;
            }
        }
    }
    public static string GetDefaultName(this PlayerControl player)
    {
        var playerid = player.PlayerId;
        if (DefaultName.ContainsKey(playerid))
        {
            return DefaultName[playerid];
        }
        else
        {
            DefaultName[playerid] = player.Data.PlayerName;
            return DefaultName[playerid];
        }
    }
    public static void RoleFixedUpdate() { }
    public static void SetRoleName(PlayerControl player, bool IsUnchecked = false)
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR:FixedUpdate]" + player.name + "への(IsCommsなしの)SetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        SetRoleName(player, RoleHelpers.IsComms(), IsUnchecked);
    }

    //短時間で何回も呼ばれると重くなるため更新可能までの時間を指定
    const float UpdateDefaultTime = 0.5f;

    static string GetPlayerName(this PlayerControl player) => ModeHandler.IsMode(ModeId.HideAndSeek)
            ? (player.IsImpostor() ? ModHelpers.Cs(RoleClass.ImpostorRed, player.GetDefaultName())
            : player.GetDefaultName()) : player.GetDefaultName();

    public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (player.IsBot() || !AmongUsClient.Instance.AmHost) return;

        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR: FixedUpdate]" + player.name + "へのSetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

        //if (UpdateTime.ContainsKey(player.PlayerId) && UpdateTime[player.PlayerId] > 0) return;

        //UpdateTime[player.PlayerId] = UpdateDefaultTime;

        List<PlayerControl> DiePlayers = new();
        List<PlayerControl> AlivePlayers = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if (p.PlayerId != 0 && p.PlayerId != player.PlayerId && !p.IsBot())
            {
                if (SetNamesClass.DefaultGhostSeeRoles() || p.IsRole(RoleId.God))
                {
                    DiePlayers.Add(p);
                }
                else
                {
                    AlivePlayers.Add(p);
                }
            }
        }
        bool IsHideAndSeek = ModeHandler.IsMode(ModeId.HideAndSeek);
        //必要がないなら処理しない
        if (player.IsMod() && DiePlayers.Count < 1 && (!IsHideAndSeek || !player.IsImpostor())) return;

        var introData = IntroData.GetIntroData(player.GetRole(), player);

        string Name = player.GetDefaultName();
        string NewName = "";
        string MySuffix = "";
        string RoleNameText = ModHelpers.Cs(introData.color, introData.Name);
        Dictionary<byte, string> ChangePlayers = new();

        foreach (PlayerControl CelebrityPlayer in RoleClass.Celebrity.CelebrityPlayer)
        {
            if (CelebrityPlayer == player) continue;
            if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(CelebrityPlayer.PlayerId, ModHelpers.Cs(RoleClass.Celebrity.color, CelebrityPlayer.GetDefaultName()));
        }

        if (Madmate.CheckImpostor(player) ||
            MadMayor.CheckImpostor(player) ||
            player.IsRole(RoleId.Marlin) ||
            BlackCat.CheckImpostor(player))
        {
            foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
            {
                if (Impostor.IsImpostor() && !Impostor.IsBot())
                {
                    if (!ChangePlayers.ContainsKey(Impostor.PlayerId))
                    {
                        if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(Impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, Impostor.GetDefaultName()));
                    }
                }
            }
        }
        else if (JackalFriends.CheckJackal(player))
        {
            foreach (PlayerControl Jackal in RoleClass.Jackal.JackalPlayer)
            {
                if (!Jackal.Data.Disconnected)
                {
                    if (!RoleClass.Camouflager.IsCamouflage)
                    {
                        if (!ChangePlayers.ContainsKey(Jackal.PlayerId)) ChangePlayers.Add(Jackal.PlayerId, ModHelpers.Cs(RoleClass.Jackal.color, Jackal.GetDefaultName()));
                        else ChangePlayers[Jackal.PlayerId] = ModHelpers.Cs(RoleClass.Jackal.color, ChangePlayers[Jackal.PlayerId]);
                    }
                }
            }
        }
        else if (player.IsRole(RoleId.Demon))
        {
            if (RoleClass.Demon.IsCheckImpostor)
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.IsImpostor() && !Impostor.IsBot() && !RoleClass.Camouflager.IsCamouflage)
                    {
                        if (!ChangePlayers.ContainsKey(Impostor.PlayerId)) ChangePlayers.Add(Impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, Impostor.GetPlayerName()));
                        else ChangePlayers[Impostor.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers[Impostor.PlayerId]);
                    }
                }
            }
            foreach (PlayerControl CursePlayer in Demon.GetIconPlayers(player))
            {
                if (!CursePlayer.IsBot())
                {
                    if (!ChangePlayers.ContainsKey(CursePlayer.PlayerId)) ChangePlayers.Add(CursePlayer.PlayerId, CursePlayer.GetPlayerName() + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                    else ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                }
                if (!RoleClass.Camouflager.IsCamouflage)
                {
                    if (!ChangePlayers.ContainsKey(CursePlayer.PlayerId)) ChangePlayers.Add(CursePlayer.PlayerId, CursePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                    else ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                }
                else if (RoleClass.Camouflager.DemonMark)
                {
                    if (!ChangePlayers.ContainsKey(CursePlayer.PlayerId)) ChangePlayers.Add(CursePlayer.PlayerId, ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                    else ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                }
            }
        }
        else if (player.IsRole(RoleId.Arsonist))
        {
            foreach (PlayerControl DousePlayer in Arsonist.GetIconPlayers(player))
            {
                if (!DousePlayer.IsBot())
                {
                    if (!ChangePlayers.ContainsKey(DousePlayer.PlayerId)) ChangePlayers.Add(DousePlayer.PlayerId, DousePlayer.GetPlayerName() + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                    else ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                }
                if (!RoleClass.Camouflager.IsCamouflage)
                {
                    if (!ChangePlayers.ContainsKey(DousePlayer.PlayerId)) ChangePlayers.Add(DousePlayer.PlayerId, DousePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                    else ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                }
                else if (RoleClass.Camouflager.ArsonistMark)
                {
                    if (!ChangePlayers.ContainsKey(DousePlayer.PlayerId)) ChangePlayers.Add(DousePlayer.PlayerId, ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                    else ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                }
            }
        }
        else if (player.IsRole(RoleId.SatsumaAndImo))
        {
            foreach (PlayerControl Player in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
            {
                if (!Player.IsBot())
                {
                    if (RoleClass.SatsumaAndImo.TeamNumber == 1)
                    {
                        if (!ChangePlayers.ContainsKey(Player.PlayerId))
                        {
                            if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(Player.PlayerId, Player.GetDefaultName() + ModHelpers.Cs(Palette.White, " (C)"));
                            else ChangePlayers.Add(Player.PlayerId, ModHelpers.Cs(Palette.White, " (C)"));
                        }
                        else
                        {
                            ChangePlayers[Player.PlayerId] = ChangePlayers[Player.PlayerId] + ModHelpers.Cs(Palette.White, " (C)");
                        }
                    }
                    else
                    {
                        if (!ChangePlayers.ContainsKey(Player.PlayerId))
                        {
                            if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(Player.PlayerId, Player.GetDefaultName() + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                            else ChangePlayers.Add(Player.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                        }
                        else
                        {
                            ChangePlayers[Player.PlayerId] = ChangePlayers[Player.PlayerId] + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)");
                        }
                    }
                }
            }
        }
        else if (player.IsRole(RoleId.Finder) && RoleClass.Finder.KillCounts.ContainsKey(player.PlayerId) && RoleClass.Finder.KillCounts[player.PlayerId] >= RoleClass.Finder.CheckMadmateKillCount)
        {
            foreach (PlayerControl Player in CachedPlayer.AllPlayers)
            {
                if (!Player.IsBot() && Player.IsMadRoles())
                {
                    if (!ChangePlayers.ContainsKey(Player.PlayerId))
                    {
                        ChangePlayers.Add(Player.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, Player.GetDefaultName()));
                    }
                    else
                    {
                        ChangePlayers[Player.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers[Player.PlayerId]);
                    }
                }
            }
        }

        if (player.IsLovers() &&
            ((RoleClass.Camouflager.LoversMark && RoleClass.Camouflager.IsCamouflage) || !RoleClass.Camouflager.IsCamouflage))
        {
            var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
            PlayerControl Side = player.GetOneSideLovers();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetPlayerName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        if (player.IsQuarreled() &&
            ((RoleClass.Camouflager.QuarreledMark && RoleClass.Camouflager.IsCamouflage) || !RoleClass.Camouflager.IsCamouflage))
        {
            var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
            PlayerControl Side = player.GetOneSideQuarreled();
            string name = Side.GetDefaultName();
            if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetPlayerName() + suffix);
            else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
            MySuffix += suffix;
        }
        if (player.IsRole(RoleId.Sheriff))
        {
            if (RoleClass.Sheriff.KillCount.ContainsKey(player.PlayerId))
            {
                RoleNameText += ModHelpers.Cs(introData.color, $"{RoleClass.Sheriff.KillCount[player.PlayerId]}");
            }
        }
        else if (player.IsRole(RoleId.RemoteSheriff))
        {
            if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId))
            {
                RoleNameText += ModHelpers.Cs(introData.color, $"{RoleClass.RemoteSheriff.KillCount[player.PlayerId]}");
            }
        }
        else if (player.IsRole(RoleId.Mafia))
        {
            if (Mafia.IsKillFlag())
            {
                RoleNameText += " (OK)";
            }
        }

        string TaskText = "";
        if (!player.IsClearTask())
        {
            try
            {
                if (commsActive) TaskText = ModHelpers.Cs(Color.yellow, "(?/" + TaskCount.TaskDateNoClearCheck(player.Data).Item2 + ")");
                else
                {
                    var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                    TaskText = ModHelpers.Cs(Color.yellow, "(" + Complete + "/" + all + ")");
                }
            }
            catch { }
        }
        bool IsDemonVIew = false;
        bool IsArsonistVIew = false;
        if ((SetNamesClass.DefaultGhostSeeRoles() || player.IsRole(RoleId.God)) && !IsUnchecked)
        {
            if (Demon.IsViewIcon(player))
            {
                MySuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                IsDemonVIew = true;
            }
            if (Arsonist.IsViewIcon(player))
            {
                MySuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                IsArsonistVIew = true;
            }
            if (player.IsRole(RoleId.SatsumaAndImo))
            {
                if (RoleClass.SatsumaAndImo.TeamNumber == 1) { MySuffix += ModHelpers.Cs(Palette.White, " (C)"); }
                else { MySuffix += ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"); }
            }
            if (!RoleClass.Camouflager.IsCamouflage) NewName = "(<size=75%>" + ModHelpers.Cs(introData.color, introData.Name) + TaskText + "</size>)" + ModHelpers.Cs(introData.color, Name + MySuffix);
            else NewName = "(<size=75%>" + ModHelpers.Cs(introData.color, introData.Name) + TaskText + "</size>)" + ModHelpers.Cs(introData.color, MySuffix);
        }
        else if (player.IsAlive() || IsUnchecked)
        {
            if (SetNamesClass.DefaultGhostSeeRoles() || player.IsRole(RoleId.God))
            {
                if (Demon.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    IsDemonVIew = true;
                }
                if (Arsonist.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    IsArsonistVIew = true;
                }
            }
            if (!RoleClass.Camouflager.IsCamouflage) NewName = "<size=75%>" + RoleNameText + TaskText + "</size>\n" + ModHelpers.Cs(introData.color, Name + MySuffix);
            else NewName = "<size=75%>" + RoleNameText + TaskText + "</size>\n" + ModHelpers.Cs(introData.color, MySuffix);
            SuperNewRolesPlugin.Logger.LogInfo(NewName);
        }
        if (!player.IsMod())
        {
            player.RpcSetNamePrivate(NewName);
            if (player.IsAlive())
            {
                foreach (var ChangePlayerData in ChangePlayers)
                {
                    PlayerControl ChangePlayer = ModHelpers.PlayerById(ChangePlayerData.Key);
                    if (ChangePlayer != null)
                    {
                        ChangePlayer.RpcSetNamePrivate(ChangePlayerData.Value, player);
                    }
                }
            }
        }
        if (player.IsImpostor() && IsHideAndSeek)
        {
            foreach (PlayerControl AlivePlayer in AlivePlayers)
            {
                if (AlivePlayer.IsMod()) continue;
                player.RpcSetNamePrivate(ModHelpers.Cs(RoleClass.ImpostorRed, player.GetDefaultName()), AlivePlayer);
            }
        }
        string DieSuffix = "";
        if (!IsDemonVIew && Demon.IsViewIcon(player)) { DieSuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲"); }
        if (!IsArsonistVIew && Arsonist.IsViewIcon(player)) { DieSuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §"); }
        NewName += DieSuffix;
        foreach (PlayerControl DiePlayer in DiePlayers)
        {
            if (player.PlayerId != DiePlayer.PlayerId && !DiePlayer.IsMod() && !DiePlayer.Data.Disconnected) player.RpcSetNamePrivate(NewName, DiePlayer);
        }
    }

    public static void SetRoleNames(bool IsUnchecked = false)
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR:FixedUpdate] SetRoleNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

        bool commsActive = RoleHelpers.IsComms();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            SetRoleName(p, commsActive, IsUnchecked);
        }
    }
    public static void Update()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff))
        {
            if (RoleClass.Sheriff.KillMaxCount >= 1)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.SetTarget());
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
                }
            }
            else
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
                CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = false;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            }
        }
        else if
            (PlayerControl.LocalPlayer.IsRole
                (
                    RoleId.Jackal,
                    RoleId.JackalSeer,
                    RoleId.MadMaker,
                    RoleId.Egoist,
                    RoleId.Demon,
                    RoleId.Arsonist
                )
            )
        {
            FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
            CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.SetTarget());
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
            }
        }
        SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (PlayerControl p in BotManager.AllBots)
        {
            p.NetTransform.RpcSnapTo(new Vector2(99999, 99999));
        }
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
        {
            UpdateData--;
            RoleFixedUpdate();

            if (AmongUsClient.Instance.AmHost)
            {
                BlockTool.FixedUpdate();
                if (UpdateData <= 0)
                {
                    UpdateData = 15;
                    if (RoleClass.IsMeeting)
                    {
                        //SetDefaultNames();
                    }
                    else
                    {
                        //SetRoleNames();
                    }
                }
            }
        }
    }
    public static void SetDefaultNames()
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("SetDefaultNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            p.RpcSetName(p.GetDefaultName());
        }
    }
}