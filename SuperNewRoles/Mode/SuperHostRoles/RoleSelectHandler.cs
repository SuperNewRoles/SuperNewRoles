using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
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
        public static CustomRpcSender RoleSelect()
        {
            SuperNewRolesPlugin.Logger.LogInfo("ROLESELECT");
            if (!AmongUsClient.Instance.AmHost) return null;
            SuperNewRolesPlugin.Logger.LogInfo("つうか");
            var crs = CustomRpcSender.Create();
            CrewOrImpostorSet();
            OneOrNotListSet();
            AllRoleSetClass.AllRoleSet();
            crs = SetCustomRoles(crs);
            SyncSetting.CustomSyncSettings();
            ChacheManager.ResetChache();
            FixedUpdate.SetRoleNames();
            main.SendAllRoleChat();

            //BotHandler.AddBot(3, "キルされるBot");
            new LateTask(() =>
            {
                if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
                {
                    PlayerControl.LocalPlayer.RpcSetName(PlayerControl.LocalPlayer.getDefaultName());
                    PlayerControl.LocalPlayer.RpcSendChat("＊注意(自動送信)＊\nこのMODは、バグ等がたくさん発生します。\nいろいろな重大なバグがあるため、あくまで自己責任でお願いします。");
                    foreach (var pc in CachedPlayer.AllPlayers)
                    {
                        pc.PlayerControl.RpcSetRole(RoleTypes.Shapeshifter);
                        SuperNewRolesPlugin.Logger.LogInfo("シェイプシフターセット！");
                    }
                }
            }, 3f, "SetImpostor");
            return crs;
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
                        if (i > 0)
                        {
                            bot.RpcSetRole(RoleTypes.Crewmate);
                        }
                    }
                }
                else if (
                  CustomOptions.EgoistOption.getSelection() != 0 ||
                  CustomOptions.SheriffOption.getSelection() != 0 ||
                  CustomOptions.trueloverOption.getSelection() != 0 ||
                  CustomOptions.FalseChargesOption.getSelection() != 0 ||
                  CustomOptions.RemoteSheriffOption.getSelection() != 0 ||
                  CustomOptions.MadMakerOption.getSelection() != 0 ||
                  CustomOptions.SamuraiOption.getSelection() != 0 ||
                  CustomOptions.DemonOption.getSelection() != 0)
                {
                    PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                    bot1.RpcSetRole(RoleTypes.Impostor);

                    PlayerControl bot2 = BotManager.Spawn("暗転対策BOT2");
                    bot2.RpcSetRole(RoleTypes.Crewmate);

                    PlayerControl bot3 = BotManager.Spawn("暗転対策BOT3");
                    bot3.RpcSetRole(RoleTypes.Crewmate);
                } else if (CustomOptions.AssassinAndMarineOption.getSelection() != 0)
                {
                    PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                    bot1.RpcSetRole(RoleTypes.Crewmate);
                }
                if (CustomOptions.BakeryOption.getSelection() != 0)
                {
                    BotManager.Spawn("パン屋BOT").Exiled();
                } else if (CustomOptions.AssassinAndMarineOption.getSelection() != 0)
                {
                    BotManager.Spawn(ModTranslation.getString("AssassinAndMarineName") + "BOT").Exiled();
                }
            }
        }
        public static CustomRpcSender SetCustomRoles(CustomRpcSender crs)
        {
            List<PlayerControl> DesyncImpostors = new List<PlayerControl>();
            DesyncImpostors.AddRange(RoleClass.Jackal.JackalPlayer);
            DesyncImpostors.AddRange(RoleClass.Sheriff.SheriffPlayer);
            DesyncImpostors.AddRange(RoleClass.Demon.DemonPlayer);
            DesyncImpostors.AddRange(RoleClass.truelover.trueloverPlayer);
            DesyncImpostors.AddRange(RoleClass.FalseCharges.FalseChargesPlayer);
            DesyncImpostors.AddRange(RoleClass.MadMaker.MadMakerPlayer);
            //インポスターにDesync


            List<PlayerControl> SetRoleEngineers = new List<PlayerControl>();
            if (RoleClass.Jester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Jester.JesterPlayer);
            if (RoleClass.JackalFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
            if (RoleClass.MadMate.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadMate.MadMatePlayer);
            if (RoleClass.MadStuntMan.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadStuntMan.MadStuntManPlayer);
            if (RoleClass.MadJester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadJester.MadJesterPlayer);
            if (RoleClass.Fox.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Fox.FoxPlayer);
            if (RoleClass.MayorFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MayorFriends.MayorFriendsPlayer);
            SetRoleEngineers.AddRange(RoleClass.Technician.TechnicianPlayer);
            //エンジニアに役職設定


            List<PlayerControl> DesyncShapeshifters = new List<PlayerControl>();
            DesyncShapeshifters.AddRange(RoleClass.Arsonist.ArsonistPlayer);
            DesyncShapeshifters.AddRange(RoleClass.RemoteSheriff.RemoteSheriffPlayer);
            //シェイプシフターにDesync

            foreach (PlayerControl Player in DesyncImpostors)
            {
                if (!Player.IsMod())
                {
                    Player.RpcSetRoleDesync(RoleTypes.Impostor);
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
            }
            foreach (PlayerControl Player in DesyncShapeshifters)
            {
                if (!Player.IsMod())
                {
                    Player.RpcSetRoleDesync(RoleTypes.Shapeshifter);
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
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
            }
            foreach (PlayerControl p in RoleClass.Egoist.EgoistPlayer)
            {
                if (!p.IsMod())
                {
                    p.RpcSetRole(RoleTypes.Impostor);
                    foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                    {
                        if (p2.PlayerId != p.PlayerId && !p.isRole(RoleId.Sheriff) && !p.isRole(RoleId.truelover) && p.IsPlayer())
                        {
                            p2.RpcSetRoleDesync(RoleTypes.Scientist, p);
                        }
                    }
                }
                else
                {
                    p.RpcSetRoleDesync(RoleTypes.Crewmate);
                    p.RpcSetRole(RoleTypes.Impostor);
                }
                //p.Data.IsDead = true;
            }

            foreach (PlayerControl p in SetRoleEngineers)
            {
                if (!p.IsMod()) {
                    p.RpcSetRole(RoleTypes.Engineer);
                }
            }
            foreach (PlayerControl p in RoleClass.SelfBomber.SelfBomberPlayer)
            {
                p.RpcSetRole(RoleTypes.Shapeshifter);
            }
            foreach (PlayerControl p in RoleClass.Samurai.SamuraiPlayer)
            {
                p.RpcSetRole(RoleTypes.Shapeshifter);
            }
            return crs;
        }
        public static void CrewOrImpostorSet()
        {
            AllRoleSetClass.CrewMatePlayers = new List<PlayerControl>();
            AllRoleSetClass.ImpostorPlayers = new List<PlayerControl>();
            foreach (PlayerControl Player in CachedPlayer.AllPlayers)
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

            foreach (IntroDate intro in IntroDate.IntroDatas)
            {
                if (intro.RoleId != RoleId.DefaultRole)
                {
                    var option = IntroDate.GetOption(intro.RoleId);
                    if (option == null || !option.isSHROn) continue;
                    var selection = option.getSelection();
                    if (selection != 0)
                    {
                        if (selection == 10)
                        {
                            switch (intro.Team)
                            {
                                case TeamRoleType.Crewmate:
                                    Crewonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Impostor:
                                    Impoonepar.Add(intro.RoleId);
                                    break;
                                case TeamRoleType.Neutral:
                                    Neutonepar.Add(intro.RoleId);
                                    break;
                            }
                        }
                        else
                        {
                            for (int i = 1; i <= selection; i++)
                            {
                                switch (intro.Team)
                                {
                                    case TeamRoleType.Crewmate:
                                        Crewnotonepar.Add(intro.RoleId);
                                        break;
                                    case TeamRoleType.Impostor:
                                        Imponotonepar.Add(intro.RoleId);
                                        break;
                                    case TeamRoleType.Neutral:
                                        Neutnotonepar.Add(intro.RoleId);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            var Assassinselection = CustomOptions.AssassinAndMarineOption.getSelection();
            SuperNewRolesPlugin.Logger.LogInfo("アサイン情報:" + Assassinselection + "、" + AllRoleSetClass.CrewMatePlayerNum + "、" + AllRoleSetClass.CrewMatePlayers.Count);
            if (Assassinselection != 0 && AllRoleSetClass.CrewMatePlayerNum > 0 && AllRoleSetClass.CrewMatePlayers.Count > 0)
            {
                if (Assassinselection == 10)
                {
                    Impoonepar.Add(RoleId.Assassin);
                }
                else
                {
                    for (int i = 1; i <= Assassinselection; i++)
                    {
                        Imponotonepar.Add(RoleId.Assassin);
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
