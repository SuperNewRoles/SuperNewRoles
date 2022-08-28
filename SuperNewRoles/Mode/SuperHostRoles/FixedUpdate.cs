using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class FixedUpdate
    {
        public static Dictionary<int, string> DefaultName = new();
        private static int UpdateDate = 0;

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
        class CoShowIntroPatch
        {
            public static void Prefix()
            {
                DefaultName = new Dictionary<int, string>();
                foreach (var pc in CachedPlayer.AllPlayers)
                {
                    //SuperNewRolesPlugin.Logger.LogInfo($"{pc.PlayerId}:{pc.name}:{pc.NameText().text}");
                    DefaultName[pc.PlayerId] = pc.PlayerControl.name;
                    pc.PlayerControl.NameText().text = pc.PlayerControl.name;
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
        public static void RoleFixedUpdate() { }/*
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        public class KilltimerSheriff
        {
            public void Prefix()
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles) && PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff)) { }
            }
        }*/
        //public static Dictionary<byte, float> UpdateTime;
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.PlayerId != 0 && p.PlayerId != player.PlayerId && p.IsPlayer())
                {
                    if (p.IsDead() || p.IsRole(RoleId.God))
                    {
                        DiePlayers.Add(p);
                    }
                }
            }
            //必要がないなら処理しない
            if (player.IsMod() && DiePlayers.Count < 1) return;

            string Name = player.GetDefaultName();
            string NewName = "";
            string MySuffix = "";
            Dictionary<byte, string> ChangePlayers = new();

            foreach (PlayerControl CelebrityPlayer in RoleClass.Celebrity.CelebrityPlayer)
            {
                if (CelebrityPlayer == player) continue;
                ChangePlayers.Add(CelebrityPlayer.PlayerId, ModHelpers.Cs(RoleClass.Celebrity.color, CelebrityPlayer.GetDefaultName()));
            }

            if (Madmate.CheckImpostor(player) ||
                MadMayor.CheckImpostor(player) ||
                player.IsRole(RoleId.Marine) ||
                BlackCat.CheckImpostor(player))
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.IsImpostor() && Impostor.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(Impostor.PlayerId))
                        {
                            ChangePlayers.Add(Impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, Impostor.GetDefaultName()));
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
                        if (!ChangePlayers.ContainsKey(Jackal.PlayerId)) ChangePlayers.Add(Jackal.PlayerId, ModHelpers.Cs(RoleClass.Jackal.color, Jackal.GetDefaultName()));
                        else ChangePlayers[Jackal.PlayerId] = ModHelpers.Cs(RoleClass.Jackal.color, ChangePlayers[Jackal.PlayerId]);
                    }
                }
            }
            else if (player.IsRole(RoleId.Demon))
            {
                if (RoleClass.Demon.IsCheckImpostor)
                {
                    foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                    {
                        if (Impostor.IsImpostor() && Impostor.IsPlayer())
                        {
                            if (!ChangePlayers.ContainsKey(Impostor.PlayerId)) ChangePlayers.Add(Impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, Impostor.GetDefaultName()));
                            else ChangePlayers[Impostor.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers[Impostor.PlayerId]);
                        }
                    }
                }
                foreach (PlayerControl CursePlayer in Demon.GetIconPlayers(player))
                {
                    if (CursePlayer.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(CursePlayer.PlayerId)) ChangePlayers.Add(CursePlayer.PlayerId, CursePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                        else ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    }
                }
            }
            else if (player.IsRole(RoleId.Arsonist))
            {
                foreach (PlayerControl DousePlayer in Arsonist.GetIconPlayers(player))
                {
                    if (DousePlayer.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(DousePlayer.PlayerId)) ChangePlayers.Add(DousePlayer.PlayerId, DousePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                        else ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    }
                }
            }
            else if (player.IsRole(RoleId.SatsumaAndImo))
            {
                foreach (PlayerControl Player in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
                {
                    if (Player.IsPlayer())
                    {
                        if (RoleClass.SatsumaAndImo.TeamNumber == 1)
                        {
                            if (!ChangePlayers.ContainsKey(Player.PlayerId))
                            {
                                ChangePlayers.Add(Player.PlayerId, Player.GetDefaultName() + ModHelpers.Cs(Palette.White, " (C)"));
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
                                ChangePlayers.Add(Player.PlayerId, Player.GetDefaultName() + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                            }
                            else
                            {
                                ChangePlayers[Player.PlayerId] = ChangePlayers[Player.PlayerId] + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)");
                            }
                        }


                    }
                }
            }

            if (player.IsLovers())
            {
                var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
                PlayerControl Side = player.GetOneSideLovers();
                string name = Side.GetDefaultName();
                if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
                else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
                MySuffix += suffix;
            }
            if (player.IsQuarreled())
            {
                var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
                PlayerControl Side = player.GetOneSideQuarreled();
                string name = Side.GetDefaultName();
                if (!ChangePlayers.ContainsKey(Side.PlayerId)) ChangePlayers.Add(Side.PlayerId, Side.GetDefaultName() + suffix);
                else { ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix; }
                MySuffix += suffix;
            }

            if (player.IsRole(RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillCount.ContainsKey(player.PlayerId))
                {
                    MySuffix += "(残り" + RoleClass.Sheriff.KillCount[player.PlayerId] + "発)";
                }
            }
            else if (player.IsRole(RoleId.RemoteSheriff))
            {
                if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId))
                {
                    MySuffix += "(残り" + RoleClass.RemoteSheriff.KillCount[player.PlayerId] + "発)";
                }
            }
            else if (player.IsRole(RoleId.Mafia))
            {
                if (Mafia.IsKillFlag())
                {
                    MySuffix += " (キル可能)";
                }
            }

            var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(player.GetRole(), player);
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
            if ((player.IsDead() || player.IsRole(RoleId.God)) && !IsUnchecked)
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
                NewName = "(<size=75%>" + ModHelpers.Cs(introdate.color, introdate.Name) + TaskText + "</size>)" + ModHelpers.Cs(introdate.color, Name + MySuffix);
            }
            else if (player.IsAlive() || IsUnchecked)
            {
                if (player.IsDead() || player.IsRole(RoleId.God))
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
                NewName = "<size=75%>" + ModHelpers.Cs(introdate.color, introdate.Name) + TaskText + "</size>\n" + ModHelpers.Cs(introdate.color, Name + MySuffix);
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
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.Jackal, RoleId.MadMaker, RoleId.Egoist, RoleId.RemoteSheriff,
                RoleId.Demon, RoleId.Arsonist)
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
                UpdateDate--;
                RoleFixedUpdate();
                /*
                if (UpdateTime != null)
                {
                    foreach (var UpdateTimeData in UpdateTime){
                        UpdateTime[UpdateTimeData.Key] -= Time.fixedDeltaTime;
                    }
                }
                */
                if (AmongUsClient.Instance.AmHost)
                {
                    BlockTool.FixedUpdate();
                    if (UpdateDate <= 0)
                    {
                        UpdateDate = 15;
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
}