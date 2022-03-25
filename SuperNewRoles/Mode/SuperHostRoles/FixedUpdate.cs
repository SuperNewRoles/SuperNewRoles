using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
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
            if (DefaultName.ContainsKey(player.PlayerId))
            {
                return DefaultName[player.PlayerId];
            }
            else
            {
                DefaultName[player.PlayerId] = player.CurrentOutfit.PlayerName;
                return DefaultName[player.PlayerId];
            }
        }
        public static string getsetname(PlayerControl p,bool IsDead = false)
        {
            var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
            string Suffix = "";
            if (p.IsLovers())
            {
                Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
            }
            if (p.isRole(CustomRPC.RoleId.Sheriff))
            {
                if (RoleClass.Sheriff.KillCount.ContainsKey(p.PlayerId))
                {
                    Suffix += "(残り" + RoleClass.Sheriff.KillCount[p.PlayerId] + "発)";
                }
            }
            if (RoleClass.IsMeeting || IsDead)
            {
                return "<size=50%>(" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + ")</size>" + ModHelpers.cs(introdate.color,p.getDefaultName()+Suffix);
            }
            else
            {
                return "<size=75%>" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + "</size>\n" + ModHelpers.cs(introdate.color, p.getDefaultName()+Suffix);
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
        public static void SetRoleNames()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected && p.PlayerId != 0)
                {
                    if (p.isDead() || p.isRole(CustomRPC.RoleId.God))
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            string Suffix = "";
                            if (p2.IsLovers())
                            {
                                Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            }
                            if (p2.isRole(CustomRPC.RoleId.Sheriff))
                            {
                                if (RoleClass.Sheriff.KillCount.ContainsKey(p2.PlayerId))
                                {
                                    Suffix += "(残り" + RoleClass.Sheriff.KillCount[p2.PlayerId] + "発)";
                                }
                            }
                            var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p2.getRole(), p2);
                            if (p2.isDead())
                            {
                                p2.RpcSetNamePrivate("(<size=75%>" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + "</size>)" + ModHelpers.cs(introdate.color, p2.getDefaultName()) + Suffix, p);
                            } else
                            {
                                p2.RpcSetNamePrivate("<size=75%>" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + "</size>\n" + ModHelpers.cs(introdate.color, p2.getDefaultName()) + Suffix, p);
                            }
                        }
                    }
                    else if (p.isAlive() && !p.Data.Disconnected)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected)
                            {
                                p2.RpcSetNamePrivate(p2.getDefaultName(), p);
                            }
                        }
                        string Suffix = "";
                        if (p.IsLovers())
                        {
                            Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            Side.RpcSetNamePrivate(Side.getDefaultName()+Suffix,p);
                        }
                        if (p.isRole(CustomRPC.RoleId.Sheriff))
                        {
                            if (RoleClass.Sheriff.KillCount.ContainsKey(p.PlayerId))
                            {
                                Suffix += "(残り" + RoleClass.Sheriff.KillCount[p.PlayerId] + "発)";
                            }
                        }
                        var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
                        p.RpcSetNamePrivate("<size=75%>" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + "</size>\n" + ModHelpers.cs(introdate.color, p.getDefaultName()+Suffix), p);
                    }
                }
            }
            if (RoleClass.MadMate.IsImpostorCheck)
            {
                var impostorplayers = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor())
                    {
                        impostorplayers.Add(p);
                    }
                }
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in impostorplayers)
                        {
                            string Suffix = "";
                            if (p2.IsLovers() && p2.GetOneSideLovers().PlayerId == p.PlayerId)
                            {
                                Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            }
                            p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                        }
                    }
                    else if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (p.isImpostor() || p.isRole(CustomRPC.RoleId.Egoist))
                            {
                                SetNamesClass.SetPlayerNameColors(p);
                                SetNamesClass.SetPlayerRoleNames(p);
                            }
                        }
                    }
                }
            }
        }
        public static void AllMeetingText()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
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
                                p2.RpcSetNamePrivate("<size=50%>(" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + ")</size>" + ModHelpers.cs(introdate.color, p2.getDefaultName())+Suffix,p);
                            }
                        }
                    } else if (p.isAlive())
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (p.PlayerId != p2.PlayerId && !p2.Data.Disconnected)
                            {
                                p2.RpcSetNamePrivate(p2.getDefaultName(),p);
                            }
                        }
                        string Suffix = "";
                        if (p.IsLovers())
                        {
                            Suffix = ModHelpers.cs(RoleClass.Lovers.color, " ♥");
                            PlayerControl Side = p.GetOneSideLovers();
                            Side.RpcSetNamePrivate(Side.getDefaultName() + Suffix, p);
                        }
                        var introdate = SuperNewRoles.Intro.IntroDate.GetIntroDate(p.getRole(), p);
                        p.RpcSetNamePrivate("<size=50%>(" + ModHelpers.cs(introdate.color, ModTranslation.getString(introdate.NameKey + "Name")) + ")</size>" + ModHelpers.cs(introdate.color, p.getDefaultName())+Suffix);
                    }
                }
            }
            if (RoleClass.MadMate.IsImpostorCheck)
            {
                var impostorplayers = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor())
                    {
                        impostorplayers.Add(p);
                    }
                }
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in impostorplayers)
                        {
                            p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                        }
                    }
                    else if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (p.isImpostor() || p.isRole(CustomRPC.RoleId.Egoist))
                            {
                                SetNamesClass.SetPlayerNameColors(p);
                                SetNamesClass.SetPlayerRoleNames(p);
                            }
                        }
                    }
                }
            }
        }
        public static void Update()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Sheriff)) {
                if (RoleClass.Sheriff.KillMaxCount >= 1)
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(true);
                    PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
                } else
                {
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
                    PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = false;
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                }
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
                        UpdateDate = 10; 
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
        public static void SetNames()
        {
            SetNameUpdate.Postfix(PlayerControl.LocalPlayer);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.isAlive() && !p.isRole(CustomRPC.RoleId.God))
                {
                    if (p.PlayerId != 0)
                    {
                        p.RpcSetNamePrivate(getsetname(p));
                    }
                }
                else if (!p.Data.Disconnected && p.isDead())
                {
                    if (p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            p2.RpcSetNamePrivate(getsetname(p2, p2.isDead()), SeePlayer: p);
                        }
                    }
                }
                else if (!p.Data.Disconnected && p.isRole(CustomRPC.RoleId.God))
                {
                    if (p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            p2.RpcSetNamePrivate(getsetname(p2, true), SeePlayer: p);
                        }
                    }
                }
                
            }
            if (RoleClass.MadMate.IsImpostorCheck)
            {
                var impostorplayers = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p.isImpostor())
                    {
                        impostorplayers.Add(p);
                    }
                }
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in impostorplayers)
                        {
                            p2.RpcSetNamePrivate(ModHelpers.cs(RoleClass.ImpostorRed, p2.getDefaultName()), p);
                        }
                    }
                    else if (p.isAlive() && p.PlayerId != 0)
                    {
                        foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                        {
                            if (p.isImpostor() || p.isRole(CustomRPC.RoleId.Egoist))
                            {
                                SetNamesClass.SetPlayerNameColors(p);
                                SetNamesClass.SetPlayerRoleNames(p);
                            }
                        }
                    }
                }
            }
        }
    }
}
