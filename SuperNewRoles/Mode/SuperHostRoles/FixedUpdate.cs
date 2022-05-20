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
        public static void SetRoleName(PlayerControl player, bool IsUnchecked = false)
        {
            SetRoleName(player, RoleHelpers.IsComms() , IsUnchecked);
        }
        public static void SetRoleName(PlayerControl player, bool commsActive, bool IsUnchecked = false)
        {
            if (player.Data.Disconnected || player.IsBot() || !AmongUsClient.Instance.AmHost) return;

            List<PlayerControl> DiePlayers = new List<PlayerControl>();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.Disconnected && p.PlayerId != 0 && p.PlayerId != player.PlayerId  && p.IsPlayer())
                {
                    if (p.isDead() || p.isRole(RoleId.God))
                    {
                        DiePlayers.Add(p);
                    }
                }
            }
            //必要がないなら、処理しない
            if (player.IsMod() && DiePlayers.Count < 1) return;

            string Name = player.getDefaultName();
            string NewName = "";
            string MySuffix = "";
            Dictionary<byte, string> ChangePlayers = new Dictionary<byte, string>();

            foreach (PlayerControl CelebrityPlayer in RoleClass.Celebrity.CelebrityPlayer)
            {
                ChangePlayers.Add(CelebrityPlayer.PlayerId, ModHelpers.cs(RoleClass.Celebrity.color, CelebrityPlayer.getDefaultName()));
            }

            bool IsMadmateCheck = Madmate.CheckImpostor(player);
            if (IsMadmateCheck)
            {
                foreach (PlayerControl Impostor in PlayerControl.AllPlayerControls)
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

            if (player.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsJackalCheck)
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
                //MadStuntMan.CheckedImpostor.Add(p.PlayerId);
            }

            if (player.isRole(RoleId.Scavenger) && RoleClass.Scavenger.IsCheck)
            {
                foreach (PlayerControl Jackal in RoleClass.Jackal.JackalPlayer)
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
                foreach (PlayerControl Sheriff in RoleClass.Sheriff.SheriffPlayer)
                {
                    if (!ChangePlayers.ContainsKey(Sheriff.PlayerId))
                    {
                        ChangePlayers.Add(Sheriff.PlayerId, ModHelpers.cs(RoleClass.Sheriff.color, Sheriff.getDefaultName()));
                    }
                    else
                    {
                        ChangePlayers[Sheriff.PlayerId] = ModHelpers.cs(RoleClass.Sheriff.color, ChangePlayers[Sheriff.PlayerId]);
                    }
                }
                foreach (PlayerControl Impostor in PlayerControl.AllPlayerControls)
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

            if ((player.isDead() || player.isRole(RoleId.God)) && !IsUnchecked)
            {
                NewName = "(<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + "</size>)" + ModHelpers.cs(introdate.color, Name + MySuffix);
            }
            else if (player.isAlive() || IsUnchecked)
            {
                NewName = "<size=75%>" + ModHelpers.cs(introdate.color, introdate.Name) + TaskText + "</size>\n" + ModHelpers.cs(introdate.color, Name + MySuffix);
            }
            if (!player.IsMod())
            {
                player.RpcSetNamePrivate(NewName);
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
            bool commsActive = RoleHelpers.IsComms();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
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
            else if (PlayerControl.LocalPlayer.isRole(RoleId.MadMaker))
            {
                HudManager.Instance.KillButton.gameObject.SetActive(true);
                PlayerControl.LocalPlayer.Data.Role.CanUseKillButton = true;
                DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(PlayerControlFixedUpdatePatch.setTarget());
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
