using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles.Roles;
using SuperNewRoles.Patch;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class FixedUpdate
    {
        public static Dictionary<int, string> DefaultName = new Dictionary<int, string>();
        private static int UpdateDate = 0;

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
        class CoShowIntroPatch
        {
            public static void Prefix(HudManager __instance)
            {
                DefaultName = new Dictionary<int, string>();
                foreach (var pc in CachedPlayer.AllPlayers)
                {
                    //SuperNewRolesPlugin.Logger.LogInfo($"{pc.PlayerId}:{pc.name}:{pc.nameText.text}");
                    DefaultName[pc.PlayerId] = pc.PlayerControl.name;
                    pc.PlayerControl.nameText.text = pc.PlayerControl.name;
                }
            }
        }
        public static string getDefaultName(this PlayerControl player)
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
        public static void RoleFixedUpdate()
        {


        }/*
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        public class KilltimerSheriff
        {
            public void Prefix()
            {
                if (ModeHandler.isMode(ModeId.SuperHostRoles) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Sheriff))
                {

                }
            }
        }*/
        //public static Dictionary<byte, float> UpdateTime;
        private static int a = 0;
        public static void SetRoleName(PlayerControl player, bool IsUnchecked = false)
        {

            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo(player.name + "への(IsCommsなしの)SetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");
            SetRoleName(player, RoleHelpers.IsComms() , IsUnchecked);
        }

        //短時間で何回も呼ばれると重くなるため更新可能までの時間を指定
        const float UpdateDefaultTime = 0.5f;

        public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
        {
            if (!ModeHandler.isMode(ModeId.SuperHostRoles)) return;
            if (player.Data.Disconnected || player.IsBot() || !AmongUsClient.Instance.AmHost) return;

            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo(player.name+"へのSetRoleNameが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

            //if (UpdateTime.ContainsKey(player.PlayerId) && UpdateTime[player.PlayerId] > 0) return;

            //UpdateTime[player.PlayerId] = UpdateDefaultTime;

            List<PlayerControl> DiePlayers = new List<PlayerControl>();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.PlayerId != 0 && p.PlayerId != player.PlayerId  && p.IsPlayer())
                {
                    if (p.isDead() || p.isRole(RoleId.God))
                    {
                        DiePlayers.Add(p);
                    }
                }
            }
            //必要がないなら処理しない
            if (player.IsMod() && DiePlayers.Count < 1) return;

            string Name = player.getDefaultName();
            string NewName = "";
            string MySuffix = "";
            Dictionary<byte, string> ChangePlayers = new Dictionary<byte, string>();

            foreach (PlayerControl CelebrityPlayer in RoleClass.Celebrity.CelebrityPlayer)
            {
                if (CelebrityPlayer == player) continue;
                ChangePlayers.Add(CelebrityPlayer.PlayerId, ModHelpers.cs(RoleClass.Celebrity.color, CelebrityPlayer.getDefaultName()));
            }

            if (Madmate.CheckImpostor(player))
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.isImpostor() && Impostor.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(Impostor.PlayerId))
                        {
                            ChangePlayers.Add(Impostor.PlayerId, ModHelpers.cs(RoleClass.ImpostorRed, Impostor.getDefaultName()));
                        }
                    }
                }
            }
            else if (MadMayor.CheckImpostor(player) || player.isRole(RoleId.Marine))
            {
                foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                {
                    if (Impostor.isImpostor() && Impostor.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(Impostor.PlayerId))
                        {
                            ChangePlayers.Add(Impostor.PlayerId, ModHelpers.cs(RoleClass.ImpostorRed, Impostor.getDefaultName()));
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
                        if (!ChangePlayers.ContainsKey(Jackal.PlayerId))
                        {
                            ChangePlayers.Add(Jackal.PlayerId, ModHelpers.cs(RoleClass.Jackal.color, Jackal.getDefaultName()));
                        }
                        else
                        {
                            ChangePlayers[Jackal.PlayerId] = ModHelpers.cs(RoleClass.Jackal.color, ChangePlayers[Jackal.PlayerId]);
                        }
                    }
                }
            }
            else if (player.isRole(RoleId.Demon))
            {
                if (RoleClass.Demon.IsCheckImpostor)
                {
                    foreach (PlayerControl Impostor in CachedPlayer.AllPlayers)
                    {
                        if (Impostor.isImpostor() && Impostor.IsPlayer())
                        {
                            if (!ChangePlayers.ContainsKey(Impostor.PlayerId))
                            {
                                ChangePlayers.Add(Impostor.PlayerId, ModHelpers.cs(RoleClass.ImpostorRed, Impostor.getDefaultName()));
                            }
                            else
                            {
                                ChangePlayers[Impostor.PlayerId] = ModHelpers.cs(RoleClass.ImpostorRed, ChangePlayers[Impostor.PlayerId]);
                            }
                        }
                    }
                }
                foreach (PlayerControl CursePlayer in Demon.GetIconPlayers(player))
                {
                    if (CursePlayer.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(CursePlayer.PlayerId))
                        {
                            ChangePlayers.Add(CursePlayer.PlayerId, CursePlayer.getDefaultName() + ModHelpers.cs(RoleClass.Demon.color, " ▲"));
                        }
                        else
                        {
                            ChangePlayers[CursePlayer.PlayerId] = ChangePlayers[CursePlayer.PlayerId] + ModHelpers.cs(RoleClass.Demon.color, " ▲");
                        }
                    }
                }
            }
            else if (player.isRole(RoleId.Arsonist))
            {
                foreach (PlayerControl DousePlayer in Arsonist.GetIconPlayers(player))
                {
                    if (DousePlayer.IsPlayer())
                    {
                        if (!ChangePlayers.ContainsKey(DousePlayer.PlayerId))
                        {
                            ChangePlayers.Add(DousePlayer.PlayerId, DousePlayer.getDefaultName() + ModHelpers.cs(RoleClass.Arsonist.color, " §"));
                        }
                        else
                        {
                            ChangePlayers[DousePlayer.PlayerId] = ChangePlayers[DousePlayer.PlayerId] + ModHelpers.cs(RoleClass.Arsonist.color, " §");
                        }
                    }
                }
            }

            if (player.IsLovers())
            {
                var suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                PlayerControl Side = player.GetOneSideLovers();
                string name = Side.getDefaultName();
                if (!ChangePlayers.ContainsKey(Side.PlayerId))
                {
                    ChangePlayers.Add(Side.PlayerId, Side.getDefaultName() + suffix);
                }
                else
                {
                    ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix;
                }
                MySuffix += suffix;
            }
            if (player.IsQuarreled())
            {
                var suffix = ModHelpers.cs(RoleClass.Quarreled.color, "○");
                PlayerControl Side = player.GetOneSideQuarreled();
                string name = Side.getDefaultName();
                if (!ChangePlayers.ContainsKey(Side.PlayerId))
                {
                    ChangePlayers.Add(Side.PlayerId, Side.getDefaultName() + suffix);
                }
                else
                {
                    ChangePlayers[Side.PlayerId] = ChangePlayers[Side.PlayerId] + suffix;
                }
                MySuffix += suffix;
            }

            if (player.isRole(RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillCount.ContainsKey(player.PlayerId))
                {
                    MySuffix += "(残り" + RoleClass.Sheriff.KillCount[player.PlayerId] + "発)";
                }
            }
            else if (player.isRole(RoleId.RemoteSheriff))
            {
                if (RoleClass.RemoteSheriff.KillCount.ContainsKey(player.PlayerId))
                {
                    MySuffix += "(残り" + RoleClass.RemoteSheriff.KillCount[player.PlayerId] + "発)";
                }
            }

            var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(player.getRole(), player);
            string TaskText = "";
            if (!player.isImpostor())
            {
                try
                {
                    if (commsActive)
                    {
                        TaskText = ModHelpers.cs(Color.yellow, "(?/" + TaskCount.TaskDateNoClearCheck(player.Data).Item2 + ")");
                    }
                    else
                    {
                        var (complate, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                        TaskText = ModHelpers.cs(Color.yellow, "(" + complate + "/" + all + ")");
                    }
                }
                catch
                {

                }
            }
            bool IsDemonVIew = false;
            bool IsArsonistVIew = false;
            if ((player.isDead() || player.isRole(RoleId.God)) && !IsUnchecked)
            {
                if (Demon.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.cs(RoleClass.Demon.color, " ▲");
                    IsDemonVIew = true;
                }
                if (Arsonist.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.cs(RoleClass.Arsonist.color, " §");
                    IsArsonistVIew = true;
                }
                NewName = "(<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + "</size>)" + ModHelpers.cs(introdate.color, Name + MySuffix);
            }
            else if (player.isAlive() || IsUnchecked)
            {
                if ((player.isDead() || player.isRole(RoleId.God)))
                {
                    if (Demon.IsViewIcon(player))
                    {
                        MySuffix += ModHelpers.cs(RoleClass.Demon.color, " ▲");
                        IsDemonVIew = true;
                    }
                    if (Arsonist.IsViewIcon(player))
                    {
                        MySuffix += ModHelpers.cs(RoleClass.Arsonist.color, " §");
                        IsArsonistVIew = true;
                    }
                }
                NewName = "<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + "</size>\n" + ModHelpers.cs(introdate.color, Name + MySuffix);
                SuperNewRolesPlugin.Logger.LogInfo(NewName);
            }
            if (!player.IsMod())
            {
                player.RpcSetNamePrivate(NewName);
                if (player.isAlive())
                {
                    foreach (var ChangePlayerData in ChangePlayers)
                    {
                        PlayerControl ChangePlayer = ModHelpers.playerById(ChangePlayerData.Key);
                        if (ChangePlayer != null)
                        {
                            ChangePlayer.RpcSetNamePrivate(ChangePlayerData.Value, player);
                            SuperNewRolesPlugin.Logger.LogInfo(ChangePlayerData.Value);
                        }
                    }
                }
            }
            string DieSuffix = "";
            if (!IsDemonVIew && Demon.IsViewIcon(player))
            {
                DieSuffix += ModHelpers.cs(RoleClass.Demon.color, " ▲");
            }
            if (!IsArsonistVIew && Arsonist.IsViewIcon(player))
            {
                DieSuffix += ModHelpers.cs(RoleClass.Arsonist.color, " §");
            }
            NewName += DieSuffix;
            foreach (PlayerControl DiePlayer in DiePlayers)
            {
                if (player.PlayerId != DiePlayer.PlayerId && !DiePlayer.IsMod() && !DiePlayer.Data.Disconnected)
                {
                    player.RpcSetNamePrivate(NewName, DiePlayer);
                }
            }
        }

        public static void SetRoleNames(bool IsUnchecked = false)
        {
            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            SuperNewRolesPlugin.Logger.LogInfo("SetRoleNamesが" + callerClassName + "." + callerMethodName + "から呼び出されました。");

            bool commsActive = RoleHelpers.IsComms();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                SetRoleName(p, commsActive, IsUnchecked);
            }
        }
        public static void Update()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillMaxCount >= 1)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                    CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
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
            else if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal) ||
                PlayerControl.LocalPlayer.isRole(RoleId.MadMaker) ||
                PlayerControl.LocalPlayer.isRole(RoleId.Egoist) ||
                PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff) ||
                PlayerControl.LocalPlayer.isRole(RoleId.Demon) ||
                PlayerControl.LocalPlayer.isRole(RoleId.Arsonist)
                )
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
                CachedPlayer.LocalPlayer.Data.Role.CanUseKillButton = true;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
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
                p.RpcSetName(p.getDefaultName());
            }
        }
    }
}
