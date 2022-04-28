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

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                DefaultName = new Dictionary<int, string>();
            }
        }
        public static string getDefaultName(this PlayerControl player)
        {
            byte playerid = 0;
            if (AmongUsClient.Instance.GameMode == GameModes.LocalGame)
            {
                playerid = player.PlayerId;
            }
            else
            {
                playerid = (byte)player.getClientId();
            }
            if (DefaultName.ContainsKey(playerid))
            {
                return DefaultName[playerid];
            }
            else
            {
                DefaultName[playerid] = player.nameText.text;
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
        private static int a = 0;
        public static void SetRoleNames(bool IsUnchecked = false)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
            bool commsActive = false;
            if (RoleClass.Technician.TechnicianPlayer.Count != 0)
            {
                foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
                {
                    if (t.TaskType == TaskTypes.FixComms)
                    {
                        commsActive = true;
                        break;
                    }
                }
            }
            List<PlayerControl> DiePlayers = new List<PlayerControl>();
            List<PlayerControl> AlivePlayers = new List<PlayerControl>();
            if (!RoleClass.IsMeeting)
            {
                a--;
                if (a <= 0)
                {
                    a = 10;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (!p.Data.Disconnected && p.isAlive())
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && p.PlayerId != p2.PlayerId)
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                            }
                        }
                    }
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected && p.PlayerId != 0)
                {
                    if (p.isDead() || p.isRole(RoleId.God))
                    {
                        DiePlayers.Add(p);
                    }
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected)
                {
                    string Suffix = "";
                    if (p.PlayerId != 0 && p.isAlive())
                    {
                        bool IsMadmateCheck = Madmate.CheckImpostor(p);
                        //  SuperNewRolesPlugin.Logger.LogInfo("マッドメイトがチェックできるか:"+IsMadmateCheck);
                        if (IsMadmateCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && !p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                            //Madmate.CheckedImpostor.Add(p.PlayerId);
                        }
                        bool IsMadMayorCheck = MadMayor.CheckImpostor(p);
                        //  SuperNewRolesPlugin.Logger.LogInfo("マッドメイヤーがチェックできるか:"+IsMadMayorCheck);
                        if (IsMadMayorCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && !p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                            //MadMayor.CheckedImpostor.Add(p.PlayerId);
                        }
                        bool IsMadJesterCheck = MadJester.CheckImpostor(p);
                        //  SuperNewRolesPlugin.Logger.LogInfo("マッドメイヤーがチェックできるか:"+IsMadMayorCheck);
                        if (IsMadJesterCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && !p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                            //MadMayor.CheckedImpostor.Add(p.PlayerId);
                        }

                        if (p.IsLovers() && p.isAlive())
                        {
                            Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            string name = Side.getDefaultName();
                            if (Madmate.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            }
                            if (MadMayor.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            }
                            if (MadJester.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            }
                            Side.RpcSetNamePrivate(name + Suffix, p);
                        }
                    }
                    if (p.isRole(RoleId.Sheriff))
                    {
                        if (RoleClass.Sheriff.KillCount.ContainsKey(p.PlayerId))
                        {
                            Suffix += "(残り" + RoleClass.Sheriff.KillCount[p.PlayerId] + "発)";
                        }
                    }
                    var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
                    string TaskText = "";
                    if (!p.isImpostor())
                    {
                        try
                        {
                            if (commsActive)
                            {
                                var all = TaskCount.TaskDateNoClearCheck(p.Data).Item2;
                                TaskText = ModHelpers.cs(Color.yellow, "(?/" + all + ")");
                            }
                            else
                            {
                                var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                                TaskText = ModHelpers.cs(Color.yellow, "(" + complate + "/" + all + ")");
                            }
                        }
                        catch
                        {

                        }
                    }
                    string NewName = "";
                    if ((p.isDead() || p.isRole(RoleId.God)) && !IsUnchecked)
                    {
                        NewName = "(<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + GetRoleTextClass.GetRoleTextPostfix(p) + "</size>)" + ModHelpers.cs(introdate.color, p.getDefaultName() + Suffix);
                    }
                    else if (p.isAlive() || IsUnchecked)
                    {
                        NewName = "<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + GetRoleTextClass.GetRoleTextPostfix(p) + "</size>\n" + ModHelpers.cs(introdate.color, p.getDefaultName() + Suffix);
                    }
                    if (p.PlayerId != 0)
                    {
                        p.RpcSetNamePrivate(NewName);
                    }
                    foreach (PlayerControl p2 in DiePlayers)
                    {
                        if (p.PlayerId != p2.PlayerId && p2.PlayerId != 0 && !p2.Data.Disconnected)
                        {
                            p.RpcSetNamePrivate(NewName, p2);
                        }
                    }
                }
            }
        }/*
        public static void AllMeetingText()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected && p.PlayerId != 0)
                {
                    if (p.isDead() || p.isRole(CustomRPC.RoleId.God))
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (!p2.Data.Disconnected) {
                                string Suffix = "";
                                if (p2.IsLovers())
                                {
                                    Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                                }
                                var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p2.getRole(), p2);
                                string TaskText = "";
                                if (!p2.isImpostor())
                                {
                                    if (commsActive)
                                    {
                                        var all = TaskCount.TaskDateNoClearCheck(p2.Data).Item2;
                                        TaskText = ModHelpers.cs(Color.yellow, "(?/" + all + ")");
                                    }
                                    else
                                    {
                                        var (complate, all) = TaskCount.TaskDateNoClearCheck(p2.Data);
                                        TaskText = ModHelpers.cs(Color.yellow, "(" + complate + "/" + all + ")");
                                    }
                                }
                                p2.RpcSetNamePrivate("<size=50%>(" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + ")</size>" + ModHelpers.cs(introdate.color, p2.getDefaultName())+Suffix,p);
                            }
                        }
                    } else if (p.isAlive())
                    {
                        if (Madmate.CheckImpostor(p) && p.isRole(RoleId.MadMate))
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected && !p.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                        }
                        if (MadMayor.CheckImpostor(p) && p.isRole(RoleId.MadMayor))
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected && !p.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected)
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                            }
                        }
                        string Suffix = "";
                        if (p.IsLovers())
                        {

                            Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            string name = Side.getDefaultName();
                            if (Madmate.CheckImpostor(p) && p.isRole(RoleId.MadMate) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed,name);
                            }
                            if (MadMayor.CheckImpostor(p) && p.isRole(RoleId.MadMayor) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed,name);
                            }
                            Side.RpcSetNamePrivate(name + Suffix, p);
                        }
                        var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
                        string TaskText = "";
                        if (!p.isImpostor())
                        {
                            if (commsActive)
                            {
                                var all = TaskCount.TaskDateNoClearCheck(p.Data).Item2;
                                TaskText = ModHelpers.cs(Color.yellow, "(?/" + all + ")");
                            }
                            else
                            {
                                var (complate, all) = TaskCount.TaskDateNoClearCheck(p.Data);
                                TaskText = ModHelpers.cs(Color.yellow, "(" + complate + "/" + all + ")");
                            }
                        }
                        p.RpcSetNamePrivate("<size=50%>(" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + ")</size>" + ModHelpers.cs(introdate.color, p.getDefaultName())+Suffix);
                    }
                }
            }
        }*/
        public static void Update()
        {
            //Vector3 tr = PlayerControl.LocalPlayer.transform.position;
            //SuperNewRolesPlugin.Logger.LogInfo("x:"+tr.x+"f,"+tr.y+"f,"+tr.z+"f");
            if (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillMaxCount >= 1)
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(true);
                    PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
                }
                else
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                    PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = false;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                }
            } else if (PlayerControl.LocalPlayer.isRole(RoleId.Egoist))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
            }
            if (!AmongUsClient.Instance.AmHost) return;
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                UpdateDate--;
                RoleFixedUpdate();
                if (AmongUsClient.Instance.AmHost)
                {
                    BlockTool.FixedUpdate();
                    if (UpdateDate <= 0)
                    {
                        UpdateDate = 15;
                        if (RoleClass.IsMeeting)
                        {
                            SetDefaultNames();
                        }
                        else
                        {
                            SetRoleNames();
                        }
                    }
                }
            }
        }
        public static void SetDefaultNames()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.RpcSetName(p.getDefaultName());
            }
        }
    }
}
