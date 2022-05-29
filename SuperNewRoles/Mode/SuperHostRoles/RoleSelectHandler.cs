using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleSelectHandler
    {
        public static void RoleSelect()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            OneOrNotListSet();
            CrewOrImpostorSet();
            AllRoleSetClass.AllRoleSet();
            SetCustomRoles();
            SyncSetting.CustomSyncSettings();
            ChacheManager.ResetChache();
            FixedUpdate.SetRoleNames();
            main.SendAllRoleChat();
            
            //BotHandler.AddBot(3, "キルされるBot");
            new LateTask(() => {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.getDefaultName());
                    PlayerControl.LocalPlayer.RpcSendChat("＊注意(自動送信)＊\nこのMODは、バグ等がたくさん発生します。\nいろいろな重大なバグがあるため、あくまで自己責任でお願いします。");
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetRole(RoleTypes.Shapeshifter);
                        SuperNewRolesPlugin.Logger.LogInfo("シェイプシフターセット！");
                    }
                }
            }, 3f, "SetImpostor");
        }
        public static void SpawnBots()
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {

                bool IsJackalSpawned = false;
                //ジャッカルがいるなら
                if (CustomOptions.JackalOption.getSelection() != 0)
                {
                    IsJackalSpawned = true;
                    for (int i = 0; i < (1 * PlayerControl.GameOptions.NumImpostors + 2); i++)
                    {
                        PlayerControl bot = BotManager.Spawn("暗転対策BOT" + (i + 1));
                        if (i == 0)
                        {
                            bot.RpcSetRole(RoleTypes.Impostor);
                        }
                        if (i > 0) {
                            bot.RpcSetRole(RoleTypes.Crewmate);
                        }
                    }
                } else
                {
                bool flag = !IsJackalSpawned && (
                    CustomOptions.EgoistOption.getSelection() != 0 ||
                    CustomOptions.SheriffOption.getSelection() != 0 ||
                    CustomOptions.trueloverOption.getSelection() != 0 ||
                    CustomOptions.FalseChargesOption.getSelection() != 0 ||
                    CustomOptions.RemoteSheriffOption.getSelection() != 0 ||
                    CustomOptions.MadMakerOption.getSelection() != 0 ||
                    CustomOptions.DemonOption.getSelection() != 0 ||
                    CustomOptions.ArsonistOption.getSelection() != 0
                    );
                    if (flag)
                    {
                        PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                        bot1.RpcSetRole(RoleTypes.Impostor);

                        PlayerControl bot2 = BotManager.Spawn("暗転対策BOT2");
                        bot2.RpcSetRole(RoleTypes.Crewmate);

                        PlayerControl bot3 = BotManager.Spawn("暗転対策BOT3");
                        bot3.RpcSetRole(RoleTypes.Crewmate);
                    }
                }
            } else if (ModeHandler.isMode(ModeId.LevelUp))
            {
                BotManager.Spawn("キルされてしまうかわいそうなBOT");
            }
        }
        public static void SetCustomRoles() {
            List<PlayerControl> DesyncImpostorPlayers = new List<PlayerControl>();
            foreach (PlayerControl JackalPlayer in RoleClass.Jackal.JackalPlayer)
            {
                if (!JackalPlayer.IsMod())
                {
                    JackalPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != JackalPlayer.PlayerId && p.IsPlayer())
                        {
                            JackalPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, JackalPlayer);
                        }
                    }
                }
                else
                {
                    JackalPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //SheriffPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl SheriffPlayer in RoleClass.Sheriff.SheriffPlayer)
            {
                if (!SheriffPlayer.IsMod())
                {
                    SheriffPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != SheriffPlayer.PlayerId && p.IsPlayer())
                        {
                            SheriffPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, SheriffPlayer);
                        }
                    }
                } else
                {
                    SheriffPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //SheriffPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl DemonPlayer in RoleClass.Demon.DemonPlayer)
            {
                if (!DemonPlayer.IsMod())
                {
                    DemonPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != DemonPlayer.PlayerId && p.IsPlayer())
                        {
                            DemonPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, DemonPlayer);
                        }
                    }
                }
                else
                {
                    DemonPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //DemonPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl RemoteSheriffPlayer in RoleClass.RemoteSheriff.RemoteSheriffPlayer)
            {
                if (!RemoteSheriffPlayer.IsMod())
                {
                    RemoteSheriffPlayer.RpcSetRoleDesync(RoleTypes.Shapeshifter);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != RemoteSheriffPlayer.PlayerId && p.IsPlayer())
                        {
                            RemoteSheriffPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, RemoteSheriffPlayer);
                        }
                    }
                }
                else
                {
                    RemoteSheriffPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //SheriffPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl trueloverPlayer in RoleClass.truelover.trueloverPlayer)
            {
                if (!trueloverPlayer.IsMod())
                {
                    trueloverPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != trueloverPlayer.PlayerId && p.IsPlayer())
                        {
                            trueloverPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, trueloverPlayer);
                        }
                    }
                }
                else
                {
                    trueloverPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //trueloverPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl Player in RoleClass.FalseCharges.FalseChargesPlayer)
            {
                if (!Player.IsMod())
                {
                    Player.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != Player.PlayerId && p.IsPlayer())
                        {
                            Player.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, Player);
                        }
                    }
                }
                else
                {
                    Player.RpcSetRole(RoleTypes.Crewmate);
                }
                //trueloverPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl ArsonistPlayer in RoleClass.Arsonist.ArsonistPlayer)
            {
                if (!ArsonistPlayer.IsMod())
                {
                    ArsonistPlayer.RpcSetRoleDesync(RoleTypes.Shapeshifter);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != ArsonistPlayer.PlayerId && p.IsPlayer())
                        {
                            ArsonistPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, ArsonistPlayer);
                        }
                    }
                }
                else
                {
                    ArsonistPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
                //SheriffPlayer.Data.IsDead = true;
            }
            foreach (PlayerControl MadMakerPlayer in RoleClass.MadMaker.MadMakerPlayer)
            {
                if (!MadMakerPlayer.IsMod())
                {
                    MadMakerPlayer.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.PlayerId != MadMakerPlayer.PlayerId && p.IsPlayer())
                        {
                            MadMakerPlayer.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            p.RpcSetRoleDesync(RoleTypes.Scientist, MadMakerPlayer);
                        }
                    }
                }
                else
                {
                    MadMakerPlayer.RpcSetRole(RoleTypes.Crewmate);
                }
            }
            if (RoleClass.Jester.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.Jester.JesterPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }
            if (RoleClass.JackalFriends.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.JackalFriends.JackalFriendsPlayer)
                {
                    if (!p.IsMod())
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }
            if (RoleClass.MadMate.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.MadMate.MadMatePlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }
            if (RoleClass.MadStuntMan.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.MadStuntMan.MadStuntManPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }
            if (RoleClass.MadJester.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.MadJester.MadJesterPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }
            if (RoleClass.Fox.IsUseVent)
            {
                foreach (PlayerControl p in RoleClass.Fox.FoxPlayer)
                {
                    if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(p.getClientId()))
                    {
                        p.RpcSetRoleDesync(RoleTypes.Engineer);
                    }
                }
            }

            foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
            {
                if (!p.IsMod())
                {
                    p.RpcSetRole(RoleTypes.Impostor);
                    foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                    {
                        if (p2.PlayerId != p.PlayerId && !p.isRole(RoleId.Sheriff) && !p.isRole(RoleId.truelover) && p.IsPlayer())
                        {
                            p2.RpcSetRoleDesync(RoleTypes.Scientist, p);
                        }
                    }
                } else
                {
                    p.RpcSetRoleDesync(RoleTypes.Crewmate);
                    p.RpcSetRole(RoleTypes.Impostor);
                }
                //p.Data.IsDead = true;
            }
            foreach (PlayerControl p in RoleClass.Technician.TechnicianPlayer)
            {
                if (!p.IsMod())
                {
                    p.RpcSetRoleDesync(RoleTypes.Engineer);
                }
            }
            foreach (PlayerControl p in RoleClass.SelfBomber.SelfBomberPlayer)
            {
                p.RpcSetRole(RoleTypes.Shapeshifter);
            }
            /*
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                p.Data.IsDead = false;
            }
            */
        }
        public static void CrewOrImpostorSet()
        {
            AllRoleSetClass.CrewMatePlayers = new List<PlayerControl>();
            AllRoleSetClass.ImpostorPlayers = new List<PlayerControl>();
            foreach (PlayerControl Player in PlayerControl.AllPlayerControls)
            {
                if (Player.IsPlayer())
                {
                    if (AllRoleSetClass.impostors.IsCheckListPlayerControl(Player))
                    {
                        AllRoleSetClass.ImpostorPlayers.Add(Player);
                    }
                    else
                    {
                        AllRoleSetClass.CrewMatePlayers.Add(Player);
                    }
                }
            }
        }
        public static void OneOrNotListSet()
        {
            var Impoonepar = new List<RoleId>();
            var Imponotonepar = new List<RoleId>();
            var Neutonepar = new List<RoleId>();
            var Neutnotonepar = new List<RoleId>();
            var Crewonepar = new List<RoleId>();
            var Crewnotonepar = new List<RoleId>();
            if (!(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.OpportunistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Opportunist;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.GodOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.GodOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.God;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JesterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Jester;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NiceNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.NiceNekomata;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilNekomataOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilNekomata;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BestfalsechargeOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bestfalsecharge;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.BaitOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Bait;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.HomeSecurityGuardOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.HomeSecurityGuard;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMateOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMate;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.StuntManOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.StuntMan;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EgoistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Egoist;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Sheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.RemoteSheriffOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.RemoteSheriffOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.RemoteSheriff;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MinimalistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Minimalist;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MayorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MayorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Mayor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadMayorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMayorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMayor;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.trueloverOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.trueloverOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.truelover;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.EvilGamblerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.EvilGamblerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.EvilGambler;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.TechnicianOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TechnicianOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Technician;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SerialKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SerialKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SerialKiller;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.OverKillerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.OverKillerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.OverKiller;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.WorkpersonOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.WorkpersonOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Workperson;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadJesterOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadJesterOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadJester;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FalseChargesOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.FalseCharges;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
            }
            if (!(CustomOption.CustomOptions.CelebrityOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.CelebrityOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Celebrity;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.NocturnalityOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.NocturnalityOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Nocturnality;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutnotonepar.Add(ThisRoleId);
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.ObserverOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ObserverOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Observer;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.SelfBomberOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SelfBomberOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.SelfBomber;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.FoxOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.FoxOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Fox;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                } else {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                       Neutonepar.Add(ThisRoleId);
                    }                        
                }
            }
            if (!(CustomOption.CustomOptions.SurvivorOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.SurvivorOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Survivor;
                if (OptionDate == 10)
                {
                    Impoonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Imponotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JackalOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JackalOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Jackal;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.JackalFriendsOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.JackalFriendsOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.JackalFriends;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.MadMakerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.MadMakerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.MadMaker;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.DemonOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.DemonOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Demon;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.TaskManagerOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.TaskManagerOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.TaskManager;
                if (OptionDate == 10)
                {
                    Crewonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Crewnotonepar.Add(ThisRoleId);
                    }
                }
            }
            if (!(CustomOption.CustomOptions.ArsonistOption.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ArsonistOption.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.Arsonist;
                if (OptionDate == 10)
                {
                    Neutonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        Neutonepar.Add(ThisRoleId);
                    }
                }
            }
            AllRoleSetClass.Impoonepar = Impoonepar;
            AllRoleSetClass.Imponotonepar = Imponotonepar;
            AllRoleSetClass.Neutonepar = Neutonepar;
            AllRoleSetClass.Neutnotonepar = Neutnotonepar;
            AllRoleSetClass.Crewonepar = Crewonepar;
            AllRoleSetClass.Crewnotonepar = Crewnotonepar;
        }
    }
}
