using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
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
                    if (p.IsDead() || p.IsRole(RoleId.God))
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
            string newName = "";
            string MySuffix = "";
            string RoleNameText = ModHelpers.Cs(introData.color, introData.Name);
            Dictionary<byte, string> ChangePlayers = new();

            foreach (PlayerControl celebrityPlayer in RoleClass.Celebrity.CelebrityPlayer)
            {
                if (celebrityPlayer == player) continue;
                if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(celebrityPlayer.PlayerId, ModHelpers.Cs(RoleClass.Celebrity.color, celebrityPlayer.GetDefaultName()));
            }

            if (Madmate.CheckImpostor(player) ||
                MadMayor.CheckImpostor(player) ||
                player.IsRole(RoleId.Marine) ||
                BlackCat.CheckImpostor(player))
            {
                foreach (PlayerControl impostor in CachedPlayer.AllPlayers)
                {
                    if (impostor.IsImpostor() && !impostor.IsBot())
                    {
                        if (!ChangePlayers.ContainsKey(impostor.PlayerId))
                        {
                            if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, impostor.GetDefaultName()));
                        }
                    }
                }
            }
            else if (JackalFriends.CheckJackal(player))
            {
                foreach (PlayerControl jackal in RoleClass.Jackal.JackalPlayer)
                {
                    if (!jackal.Data.Disconnected)
                    {
                        if (!RoleClass.Camouflager.IsCamouflage)
                        {
                            if (!ChangePlayers.ContainsKey(jackal.PlayerId)) ChangePlayers.Add(jackal.PlayerId, ModHelpers.Cs(RoleClass.Jackal.color, jackal.GetDefaultName()));
                            else ChangePlayers[jackal.PlayerId] = ModHelpers.Cs(RoleClass.Jackal.color, ChangePlayers[jackal.PlayerId]);
                        }
                    }
                }
            }
            else if (player.IsRole(RoleId.Demon))
            {
                if (RoleClass.Demon.IsCheckImpostor)
                {
                    foreach (PlayerControl impostor in CachedPlayer.AllPlayers)
                    {
                        if (impostor.IsImpostor() && !impostor.IsBot() && !RoleClass.Camouflager.IsCamouflage)
                        {
                            if (!ChangePlayers.ContainsKey(impostor.PlayerId)) ChangePlayers.Add(impostor.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, impostor.GetPlayerName()));
                            else ChangePlayers[impostor.PlayerId] = ModHelpers.Cs(RoleClass.ImpostorRed, ChangePlayers[impostor.PlayerId]);
                        }
                    }
                }
                foreach (PlayerControl cursePlayer in Demon.GetIconPlayers(player))
                {
                    if (!cursePlayer.IsBot())
                    {
                        if (!ChangePlayers.ContainsKey(cursePlayer.PlayerId)) ChangePlayers.Add(cursePlayer.PlayerId, cursePlayer.GetPlayerName() + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                        else ChangePlayers[cursePlayer.PlayerId] = ChangePlayers[cursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    }
                    if (!RoleClass.Camouflager.IsCamouflage)
                    {
                        if (!ChangePlayers.ContainsKey(cursePlayer.PlayerId)) ChangePlayers.Add(cursePlayer.PlayerId, cursePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                        else ChangePlayers[cursePlayer.PlayerId] = ChangePlayers[cursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    }
                    else if (RoleClass.Camouflager.DemonMark)
                    {
                        if (!ChangePlayers.ContainsKey(cursePlayer.PlayerId)) ChangePlayers.Add(cursePlayer.PlayerId, ModHelpers.Cs(RoleClass.Demon.color, " ▲"));
                        else ChangePlayers[cursePlayer.PlayerId] = ChangePlayers[cursePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    }
                }
            }
            else if (player.IsRole(RoleId.Arsonist))
            {
                foreach (PlayerControl dousePlayer in Arsonist.GetIconPlayers(player))
                {
                    if (!dousePlayer.IsBot())
                    {
                        if (!ChangePlayers.ContainsKey(dousePlayer.PlayerId)) ChangePlayers.Add(dousePlayer.PlayerId, dousePlayer.GetPlayerName() + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                        else ChangePlayers[dousePlayer.PlayerId] = ChangePlayers[dousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    }
                    if (!RoleClass.Camouflager.IsCamouflage)
                    {
                        if (!ChangePlayers.ContainsKey(dousePlayer.PlayerId)) ChangePlayers.Add(dousePlayer.PlayerId, dousePlayer.GetDefaultName() + ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                        else ChangePlayers[dousePlayer.PlayerId] = ChangePlayers[dousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    }
                    else if (RoleClass.Camouflager.ArsonistMark)
                    {
                        if (!ChangePlayers.ContainsKey(dousePlayer.PlayerId)) ChangePlayers.Add(dousePlayer.PlayerId, ModHelpers.Cs(RoleClass.Arsonist.color, " §"));
                        else ChangePlayers[dousePlayer.PlayerId] = ChangePlayers[dousePlayer.PlayerId] + ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    }
                }
            }
            else if (player.IsRole(RoleId.SatsumaAndImo))
            {
                foreach (PlayerControl p in RoleClass.SatsumaAndImo.SatsumaAndImoPlayer)
                {
                    if (!p.IsBot())
                    {
                        if (RoleClass.SatsumaAndImo.TeamNumber == 1)
                        {
                            if (!ChangePlayers.ContainsKey(p.PlayerId))
                            {
                                if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(p.PlayerId, p.GetDefaultName() + ModHelpers.Cs(Palette.White, " (C)"));
                                else ChangePlayers.Add(p.PlayerId, ModHelpers.Cs(Palette.White, " (C)"));
                            }
                            else
                            {
                                ChangePlayers[p.PlayerId] = ChangePlayers[p.PlayerId] + ModHelpers.Cs(Palette.White, " (C)");
                            }
                        }
                        else
                        {
                            if (!ChangePlayers.ContainsKey(p.PlayerId))
                            {
                                if (!RoleClass.Camouflager.IsCamouflage) ChangePlayers.Add(p.PlayerId, p.GetDefaultName() + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                                else ChangePlayers.Add(p.PlayerId, ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"));
                            }
                            else
                            {
                                ChangePlayers[p.PlayerId] = ChangePlayers[p.PlayerId] + ModHelpers.Cs(RoleClass.ImpostorRed, " (M)");
                            }
                        }
                    }
                }
            }

            if (player.IsLovers() &&
                ((RoleClass.Camouflager.LoversMark && RoleClass.Camouflager.IsCamouflage) || !RoleClass.Camouflager.IsCamouflage))
            {
                var suffix = ModHelpers.Cs(RoleClass.Lovers.color, " ♥");
                PlayerControl side = player.GetOneSideLovers();
                string name = side.GetDefaultName();
                if (!ChangePlayers.ContainsKey(side.PlayerId)) ChangePlayers.Add(side.PlayerId, side.GetPlayerName() + suffix);
                else { ChangePlayers[side.PlayerId] = ChangePlayers[side.PlayerId] + suffix; }
                MySuffix += suffix;
            }
            if (player.IsQuarreled() &&
                ((RoleClass.Camouflager.QuarreledMark && RoleClass.Camouflager.IsCamouflage) || !RoleClass.Camouflager.IsCamouflage))
            {
                var suffix = ModHelpers.Cs(RoleClass.Quarreled.color, "○");
                PlayerControl side = player.GetOneSideQuarreled();
                string name = side.GetDefaultName();
                if (!ChangePlayers.ContainsKey(side.PlayerId)) ChangePlayers.Add(side.PlayerId, side.GetPlayerName() + suffix);
                else { ChangePlayers[side.PlayerId] = ChangePlayers[side.PlayerId] + suffix; }
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

            string taskText = "";
            if (!player.IsClearTask())
            {
                try
                {
                    if (commsActive) taskText = ModHelpers.Cs(Color.yellow, "(?/" + TaskCount.TaskDateNoClearCheck(player.Data).Item2 + ")");
                    else
                    {
                        var (Complete, all) = TaskCount.TaskDateNoClearCheck(player.Data);
                        taskText = ModHelpers.Cs(Color.yellow, "(" + Complete + "/" + all + ")");
                    }
                }
                catch { }
            }
            bool isDemonVIew = false;
            bool isArsonistVIew = false;
            if ((player.IsDead() || player.IsRole(RoleId.God)) && !IsUnchecked)
            {
                if (Demon.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                    isDemonVIew = true;
                }
                if (Arsonist.IsViewIcon(player))
                {
                    MySuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                    isArsonistVIew = true;
                }
                if (player.IsRole(RoleId.SatsumaAndImo))
                {
                    if (RoleClass.SatsumaAndImo.TeamNumber == 1) { MySuffix += ModHelpers.Cs(Palette.White, " (C)"); }
                    else { MySuffix += ModHelpers.Cs(RoleClass.ImpostorRed, " (M)"); }
                }
                if (!RoleClass.Camouflager.IsCamouflage) newName = "(<size=75%>" + ModHelpers.Cs(introData.color, introData.Name) + taskText + "</size>)" + ModHelpers.Cs(introData.color, Name + MySuffix);
                else newName = "(<size=75%>" + ModHelpers.Cs(introData.color, introData.Name) + taskText + "</size>)" + ModHelpers.Cs(introData.color, MySuffix);
            }
            else if (player.IsAlive() || IsUnchecked)
            {
                if (player.IsDead() || player.IsRole(RoleId.God))
                {
                    if (Demon.IsViewIcon(player))
                    {
                        MySuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲");
                        isDemonVIew = true;
                    }
                    if (Arsonist.IsViewIcon(player))
                    {
                        MySuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §");
                        isArsonistVIew = true;
                    }
                }
                if (!RoleClass.Camouflager.IsCamouflage) newName = "<size=75%>" + RoleNameText + taskText + "</size>\n" + ModHelpers.Cs(introData.color, Name + MySuffix);
                else newName = "<size=75%>" + RoleNameText + taskText + "</size>\n" + ModHelpers.Cs(introData.color, MySuffix);
                SuperNewRolesPlugin.Logger.LogInfo(newName);
            }
            if (!player.IsMod())
            {
                player.RpcSetNamePrivate(newName);
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
            string dieSuffix = "";
            if (!isDemonVIew && Demon.IsViewIcon(player)) { dieSuffix += ModHelpers.Cs(RoleClass.Demon.color, " ▲"); }
            if (!isArsonistVIew && Arsonist.IsViewIcon(player)) { dieSuffix += ModHelpers.Cs(RoleClass.Arsonist.color, " §"); }
            newName += dieSuffix;
            foreach (PlayerControl diePlayer in DiePlayers)
            {
                if (player.PlayerId != diePlayer.PlayerId && !diePlayer.IsMod() && !diePlayer.Data.Disconnected) player.RpcSetNamePrivate(newName, diePlayer);
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
                UpdateData--;
                RoleFixedUpdate();

                if (AmongUsClient.Instance.AmHost)
                {
                    BlockTool.FixedUpdate();
                    if (UpdateData <= 0)
                    {
                        UpdateData = 15;
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