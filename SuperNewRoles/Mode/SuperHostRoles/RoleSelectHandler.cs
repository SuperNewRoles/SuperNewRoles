using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleSelectHandler
    {
        public static CustomRpcSender sender = null;
        public static CustomRpcSender RoleSelect(CustomRpcSender send)
        {
            sender = send;
            SuperNewRolesPlugin.Logger.LogInfo("[SHR] ROLESELECT");
            if (!AmongUsClient.Instance.AmHost) return null;
            SuperNewRolesPlugin.Logger.LogInfo("[SHR] つうか");
            CrewOrImpostorSet();
            OneOrNotListSet();
            AllRoleSetClass.AllRoleSet();
            SetCustomRoles();
            SyncSetting.CustomSyncSettings();
            ChacheManager.ResetChache();
            main.SendAllRoleChat();
            return sender;
        }
        public static void SpawnBots()
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                int impostor = PlayerControl.GameOptions.NumImpostors;
                int crewmate = 0;
                //ジャッカルがいるなら
                if (CustomOptions.JackalOption.getSelection() != 0)
                {
                    for (int i = 0; i < (PlayerControl.GameOptions.NumImpostors + 2); i++)
                    {
                        PlayerControl bot = BotManager.Spawn("[SHR] 暗転対策BOT" + (i + 1));
                        if (i == 0)
                        {
                            impostor++;
                            bot.RpcSetRole(RoleTypes.Impostor);
                        }
                        if (i > 0)
                        {
                            crewmate++;
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
                    impostor++;
                    crewmate++;
                    crewmate++;
                }
                else if (CustomOptions.AssassinAndMarineOption.getSelection() != 0)
                {
                    PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                    bot1.RpcSetRole(RoleTypes.Crewmate);
                    crewmate++;
                }
                if (CustomOptions.SpyOption.getSelection() != 0)
                {
                    for (int i = 0; i < CustomOptions.SpyPlayerCount.getFloat() - (crewmate - (impostor - PlayerControl.GameOptions.NumImpostors)) + 1; i++)
                    {
                        PlayerControl bot1 = BotManager.Spawn("暗転対策BOT");
                        bot1.RpcSetRole(RoleTypes.Crewmate);
                        crewmate++;
                    }
                }
                if (CustomOptions.BakeryOption.getSelection() != 0)
                {
                    BotManager.Spawn("パン屋BOT").Exiled();
                }
                else if (CustomOptions.AssassinAndMarineOption.getSelection() != 0)
                {
                    BotManager.Spawn(ModTranslation.getString("AssassinAndMarineName") + "BOT").Exiled();
                }
            }
        }
        public static void SetCustomRoles()
        {
            List<PlayerControl> DesyncImpostors = new();
            DesyncImpostors.AddRange(RoleClass.Jackal.JackalPlayer);
            DesyncImpostors.AddRange(RoleClass.Sheriff.SheriffPlayer);
            DesyncImpostors.AddRange(RoleClass.Demon.DemonPlayer);
            DesyncImpostors.AddRange(RoleClass.truelover.trueloverPlayer);
            DesyncImpostors.AddRange(RoleClass.FalseCharges.FalseChargesPlayer);
            DesyncImpostors.AddRange(RoleClass.MadMaker.MadMakerPlayer);
            //インポスターにDesync
            List<PlayerControl> SetRoleEngineers = new();
            if (RoleClass.Jester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Jester.JesterPlayer);
            if (RoleClass.JackalFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
            if (RoleClass.MadMate.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadMate.MadMatePlayer);
            if (RoleClass.MadStuntMan.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadStuntMan.MadStuntManPlayer);
            if (RoleClass.MadJester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadJester.MadJesterPlayer);
            if (RoleClass.Fox.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Fox.FoxPlayer);
            if (RoleClass.MayorFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MayorFriends.MayorFriendsPlayer);
            if (RoleClass.Tuna.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Tuna.TunaPlayer);
            SetRoleEngineers.AddRange(RoleClass.Technician.TechnicianPlayer);
            if (RoleClass.BlackCat.IsUseVent) SetRoleEngineers.AddRange(RoleClass.BlackCat.BlackCatPlayer);
            //エンジニアに役職設定
            List<PlayerControl> DesyncShapeshifters = new();
            DesyncShapeshifters.AddRange(RoleClass.Arsonist.ArsonistPlayer);
            DesyncShapeshifters.AddRange(RoleClass.RemoteSheriff.RemoteSheriffPlayer);
            //Desync
            foreach (PlayerControl Player in DesyncImpostors)
            {
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.getClientId();
                    sender.RpcSetRole(Player, RoleTypes.Impostor, PlayerCID);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        sender.RpcSetRole(pc, RoleTypes.Scientist, PlayerCID);
                    }
                    //他視点で科学者にするループ
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        if (pc.PlayerId == 0) Player.SetRole(RoleTypes.Scientist); //ホスト視点用
                        else sender.RpcSetRole(Player, RoleTypes.Scientist, pc.getClientId());
                    }
                }
                else
                {
                    //ホストは代わりに普通のクルーにする
                    Player.SetRole(RoleTypes.Crewmate); //ホスト視点用
                    sender.RpcSetRole(Player, RoleTypes.Crewmate);
                }
            }
            foreach (PlayerControl Player in DesyncShapeshifters)
            {
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.getClientId();
                    sender.RpcSetRole(Player, RoleTypes.Shapeshifter, PlayerCID);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        sender.RpcSetRole(pc, RoleTypes.Scientist, PlayerCID);
                    }
                    //他視点で科学者にするループ
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        if (pc.PlayerId == 0) Player.SetRole(RoleTypes.Scientist); //ホスト視点用
                        else sender.RpcSetRole(Player, RoleTypes.Scientist, pc.getClientId());
                    }
                }
                else
                {
                    //ホストは代わりに普通のクルーにする
                    Player.SetRole(RoleTypes.Crewmate); //ホスト視点用
                    sender.RpcSetRole(Player, RoleTypes.Crewmate);
                }
            }
            foreach (PlayerControl Player in RoleClass.Egoist.EgoistPlayer)
            {
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.getClientId();
                    //ただしホスト、お前はDesyncするな。
                    sender.RpcSetRole(Player, RoleTypes.Impostor);
                    //役職者で他プレイヤーを科学者にするループ
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        sender.RpcSetRole(pc, RoleTypes.Scientist, PlayerCID);
                    }
                }
                else
                {
                    //ホストは代わりに普通のクルーにする
                    if (Player.PlayerId != 0)
                    {
                        sender.RpcSetRole(Player, RoleTypes.Crewmate, Player.getClientId());
                    }
                    else
                    {
                        Player.SetRole(RoleTypes.Crewmate); //ホスト視点用
                    }
                    sender.RpcSetRole(Player, RoleTypes.Impostor);
                }
                //p.Data.IsDead = true;
            }
            foreach (PlayerControl Player in RoleClass.Spy.SpyPlayer)
            {
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.getClientId();
                    if (RoleClass.Spy.CanUseVent)
                    {
                        sender.RpcSetRole(Player, RoleTypes.Engineer, PlayerCID);
                    }
                    else
                    {
                        sender.RpcSetRole(Player, RoleTypes.Crewmate, PlayerCID);
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        sender.RpcSetRole(pc, RoleTypes.Scientist, PlayerCID);
                    }
                    //他視点で科学者にするループ
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.PlayerId == Player.PlayerId) continue;
                        if (pc.IsMod()) Player.SetRole(RoleTypes.Scientist); //ホスト視点用
                        else
                        {
                            if (pc.isImpostor() || pc.isRole(RoleId.Spy))
                            {
                                sender.RpcSetRole(Player, RoleTypes.Impostor, pc.getClientId());
                            }
                            else
                            {
                                sender.RpcSetRole(Player, RoleTypes.Scientist, pc.getClientId());
                            }
                        }
                    }
                }
                else
                {
                    if (Player.PlayerId != 0)
                    {
                        sender.RpcSetRole(Player, RoleTypes.Crewmate, Player.getClientId());
                    }
                    else
                    {
                        Player.SetRole(RoleTypes.Crewmate);
                    }
                }
            }

            foreach (PlayerControl p in SetRoleEngineers)
            {
                if (!p.IsMod())
                {
                    sender.RpcSetRole(p, RoleTypes.Engineer);
                }
            }
            foreach (PlayerControl p in RoleClass.SelfBomber.SelfBomberPlayer)
            {
                sender.RpcSetRole(p, RoleTypes.Shapeshifter);
            }
            foreach (PlayerControl p in RoleClass.Samurai.SamuraiPlayer)
            {
                sender.RpcSetRole(p, RoleTypes.Shapeshifter);
            }
            foreach (PlayerControl p in RoleClass.SuicideWisher.SuicideWisherPlayer)
            {
                sender.RpcSetRole(p, RoleTypes.Shapeshifter);
            }
            return;
        }
        public static void CrewOrImpostorSet()
        {
            AllRoleSetClass.CrewMatePlayers = new();
            AllRoleSetClass.ImpostorPlayers = new();
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
            List<RoleId> Impoonepar = new();
            List<RoleId> Imponotonepar = new();
            List<RoleId> Neutonepar = new();
            List<RoleId> Neutnotonepar = new();
            List<RoleId> Crewonepar = new();
            List<RoleId> Crewnotonepar = new();

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
            SuperNewRolesPlugin.Logger.LogInfo("[SHR] アサイン情報:" + Assassinselection + "、" + AllRoleSetClass.CrewMatePlayerNum + "、" + AllRoleSetClass.CrewMatePlayers.Count);
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
