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
            var playerid = player.PlayerId;
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
                    if (!p.IsMod() && p.isAlive())
                    {
                        if (RoleClass.Celebrity.ChangeRoleView)
                        {
                            foreach (PlayerControl Celebrity in RoleClass.Celebrity.ViewPlayers)
                            {
                                Celebrity.RpcSetNamePrivate(ModHelpers.cs(RoleClass.Celebrity.color, p.getDefaultName()));
                            }
                        }
                        else
                        {
                            foreach (PlayerControl Celebrity in RoleClass.Celebrity.CelebrityPlayer)
                            {
                                Celebrity.RpcSetNamePrivate(ModHelpers.cs(RoleClass.Celebrity.color, p.getDefaultName()),p);
                            }
                        }
                        bool IsMadmateCheck = Madmate.CheckImpostor(p);
                        if (IsMadmateCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && p2.isImpostor())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                                }
                            }
                            //Madmate.CheckedImpostor.Add(p.PlayerId);
                        }
                        bool IsMadStuntManCheck = MadStuntMan.CheckImpostor(p);
                        //  SuperNewRolesPlugin.Logger.LogInfo("マッドスタントマンがチェックできるか:"+IsMadStuntManCheck);
                        if (IsMadStuntManCheck)
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
                            //MadStuntMan.CheckedImpostor.Add(p.PlayerId);
                        }
                        bool IsTraitorCheck = Traitor.CheckFox(p);
                        //  SuperNewRolesPlugin.Logger.LogInfo("背信者がチェックできるか:"+IsTraitorCheck);
                        if (IsTraitorCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && !p2.isFox())
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p2.isFox())
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.FoxPurple, p2.getDefaultName()), p);
                                }
                            }
                            //MadStuntMan.CheckedImpostor.Add(p.PlayerId);
                        }
                        if (p.IsLovers() && p.isAlive())
                        {
                            Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            string name = Side.getDefaultName();
                            if (Madmate.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            } else if (Side.isRole(RoleId.Celebrity) || (RoleClass.Celebrity.ChangeRoleView && RoleClass.Celebrity.ViewPlayers.IsCheckListPlayerControl(Side)))
                            {
                                name = ModHelpers.cs(RoleClass.Celebrity.color, name);
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
                    if (!p.isFox())
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
                    if (!p.IsMod())
                    {
                        p.RpcSetNamePrivate(NewName);
                    }
                    foreach (PlayerControl p2 in DiePlayers)
                    {
                        if (p.PlayerId != p2.PlayerId && !p2.IsMod() && !p2.Data.Disconnected)
                        {
                            p.RpcSetNamePrivate(NewName, p2);
                        }
                    }
                }
            }
        }
        public static void Update()
        {
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
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
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
