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
                if (!p.Data.Disconnected && p.PlayerId != 0 && p.IsPlayer())
                {
                    if (p.isDead() || p.isRole(RoleId.God))
                    {
                        DiePlayers.Add(p);
                    }
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected && p.IsPlayer())
                {
                    string Suffix = "";
                    if (!p.IsMod() && p.isAlive())
                    {
                        if (RoleClass.Celebrity.ChangeRoleView)
                        {
                            foreach (PlayerControl p2 in RoleClass.Celebrity.ViewPlayers)
                            {
                                if( p.PlayerId != p2.PlayerId )
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.Celebrity.color, p2.getDefaultName()), p);
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerControl p2 in RoleClass.Celebrity.CelebrityPlayer)
                            {
                                if(p.PlayerId != p2.PlayerId)
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.Celebrity.color, p2.getDefaultName()), p);
                                }
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

                        if (p.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsJackalCheck)
                        {
                            foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                            {
                                if (!p2.Data.Disconnected && !p2.isRole(RoleId.Jackal))
                                {
                                    p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                                }
                                else if (!p2.Data.Disconnected && p2.isRole(RoleId.Jackal))
                                {
                                    p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.Jackal.color, p2.getDefaultName()), p);
                                }
                            }
                            //MadStuntMan.CheckedImpostor.Add(p.PlayerId);
                        }
                        if (p.IsLovers() && p.isAlive())
                        {
                            var suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            string name = Side.getDefaultName();
                            if (Madmate.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            } else if (Side.isRole(RoleId.Celebrity) || (RoleClass.Celebrity.ChangeRoleView && RoleClass.Celebrity.ViewPlayers.IsCheckListPlayerControl(Side)))
                            {
                                name = ModHelpers.cs(RoleClass.Celebrity.color, name);
                            } else if (p.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsJackalCheck && Side.isRole(RoleId.Jackal))
                            {
                                name = ModHelpers.cs(RoleClass.Jackal.color, name);
                            }
                            Side.RpcSetNamePrivate(name + suffix, p);
                        }
                        if (p.IsQuarreled() && p.isAlive())
                        {
                            var suffix = ModHelpers.cs(RoleClass.Quarreled.color, "○");
                            PlayerControl Side = p.GetOneSideQuarreled();
                            string name = Side.getDefaultName();
                            if (Madmate.CheckImpostor(p) && (Side.isImpostor() || Side.isRole(RoleId.Egoist)))
                            {
                                name = ModHelpers.cs(RoleClass.ImpostorRed, name);
                            }
                            else if (Side.isRole(RoleId.Celebrity) || (RoleClass.Celebrity.ChangeRoleView && RoleClass.Celebrity.ViewPlayers.IsCheckListPlayerControl(Side)))
                            {
                                name = ModHelpers.cs(RoleClass.Celebrity.color, name);
                            }
                            else if (p.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsJackalCheck && Side.isRole(RoleId.Jackal))
                            {
                                name = ModHelpers.cs(RoleClass.Jackal.color, name);
                            }
                            if (Side.IsLovers())
                            {
                                suffix += ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            }
                            Side.RpcSetNamePrivate(name + suffix, p);
                        }
                    }
                    if (p.IsLovers())
                    {
                        Suffix += ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                    }
                    if (p.IsQuarreled())
                    {
                        Suffix += ModHelpers.cs(RoleClass.Quarreled.color, "○");
                    }
                    if (p.isRole(RoleId.Sheriff))
                    {
                        if (RoleClass.Sheriff.KillCount.ContainsKey(p.PlayerId))
                        {
                            Suffix += "(残り" + RoleClass.Sheriff.KillCount[p.PlayerId] + "発)";
                        }
                    } else if (p.isRole(RoleId.RemoteSheriff))
                    {
                        if (RoleClass.RemoteSheriff.KillCount.ContainsKey(p.PlayerId))
                        {
                            Suffix += "(残り" + RoleClass.RemoteSheriff.KillCount[p.PlayerId] + "発)";
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
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
                    }
                }
                else
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                    PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = false;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                }
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControl.LocalPlayer);
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
                }
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.Egoist))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
            }
            else if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
                }
            }
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (PlayerControl p in BotManager.AllBots)
            {
                p.transform.position = new Vector3(99999, 99999);
            }
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
