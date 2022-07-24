using System.Collections.Generic;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;

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
            Main.SendAllRoleChat();
            return sender;
        }
        public static void SpawnBots()
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                int impostor = PlayerControl.GameOptions.NumImpostors;
                int crewmate = 0;
                //ジャッカルがいるなら
                if (CustomOptions.JackalOption.GetSelection() != 0)
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
                else if (//bot出す
                    CustomOptions.EgoistOption.GetSelection() != 0 ||
                    CustomOptions.SheriffOption.GetSelection() != 0 ||
                    CustomOptions.trueloverOption.GetSelection() != 0 ||
                    CustomOptions.FalseChargesOption.GetSelection() != 0 ||
                    CustomOptions.RemoteSheriffOption.GetSelection() != 0 ||
                    CustomOptions.MadMakerOption.GetSelection() != 0 ||
                    CustomOptions.SamuraiOption.GetSelection() != 0 ||
                    CustomOptions.DemonOption.GetSelection() != 0 ||
                    CustomOptions.ToiletFanOption.GetSelection() != 0 ||
                    CustomOptions.NiceButtonerOption.GetSelection() != 0)
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
                else if (CustomOptions.AssassinAndMarineOption.GetSelection() != 0)
                {
                    PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                    bot1.RpcSetRole(RoleTypes.Crewmate);
                    crewmate++;
                }
                if (CustomOptions.SpyOption.GetSelection() != 0)
                {
                    for (int i = 0; i < CustomOptions.SpyPlayerCount.GetFloat() - (crewmate - (impostor - PlayerControl.GameOptions.NumImpostors)) + 1; i++)
                    {
                        PlayerControl bot1 = BotManager.Spawn("暗転対策BOT");
                        bot1.RpcSetRole(RoleTypes.Crewmate);
                        crewmate++;
                    }
                }
                if (CustomOptions.BakeryOption.GetSelection() != 0)
                {
                    BotManager.Spawn("パン屋BOT").Exiled();
                }
                else if (CustomOptions.AssassinAndMarineOption.GetSelection() != 0)
                {
                    BotManager.Spawn(ModTranslation.GetString("AssassinAndMarineName") + "BOT").Exiled();
                }
            }
        }
        public static void SetCustomRoles()
        {
            /*============インポスターにDesync============*/
            List<PlayerControl> DesyncImpostors = new();
            DesyncImpostors.AddRange(RoleClass.Jackal.JackalPlayer);
            DesyncImpostors.AddRange(RoleClass.Sheriff.SheriffPlayer);
            DesyncImpostors.AddRange(RoleClass.Demon.DemonPlayer);
            DesyncImpostors.AddRange(RoleClass.Truelover.trueloverPlayer);
            DesyncImpostors.AddRange(RoleClass.FalseCharges.FalseChargesPlayer);
            DesyncImpostors.AddRange(RoleClass.MadMaker.MadMakerPlayer);
            /*============インポスターにDesync============*/


            /*============エンジニアに役職設定============*/
            List<PlayerControl> SetRoleEngineers = new();
            if (RoleClass.Jester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Jester.JesterPlayer);
            if (RoleClass.JackalFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.JackalFriends.JackalFriendsPlayer);
            if (RoleClass.MadMate.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadMate.MadMatePlayer);
            if (RoleClass.MadMayor.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadMayor.MadMayorPlayer);
            if (RoleClass.MadStuntMan.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadStuntMan.MadStuntManPlayer);
            if (RoleClass.MadJester.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MadJester.MadJesterPlayer);
            if (RoleClass.Fox.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Fox.FoxPlayer);
            if (RoleClass.MayorFriends.IsUseVent) SetRoleEngineers.AddRange(RoleClass.MayorFriends.MayorFriendsPlayer);
            if (RoleClass.Tuna.IsUseVent) SetRoleEngineers.AddRange(RoleClass.Tuna.TunaPlayer);
            SetRoleEngineers.AddRange(RoleClass.Technician.TechnicianPlayer);
            if (RoleClass.BlackCat.IsUseVent) SetRoleEngineers.AddRange(RoleClass.BlackCat.BlackCatPlayer);
            /*============エンジニアに役職設定============*/


            /*============シェイプシフターDesync============*/
            List<PlayerControl> DesyncShapeshifters = new();
            DesyncShapeshifters.AddRange(RoleClass.Arsonist.ArsonistPlayer);
            DesyncShapeshifters.AddRange(RoleClass.RemoteSheriff.RemoteSheriffPlayer);
            DesyncShapeshifters.AddRange(RoleClass.ToiletFan.ToiletFanPlayer);
            DesyncShapeshifters.AddRange(RoleClass.NiceButtoner.NiceButtonerPlayer);
            /*============シェイプシフターDesync============*/


            /*============シェイプシフター役職設定============*/
            List<PlayerControl> SetRoleShapeshifters = new();
            SetRoleShapeshifters.AddRange(RoleClass.SelfBomber.SelfBomberPlayer);
            SetRoleShapeshifters.AddRange(RoleClass.Samurai.SamuraiPlayer);
            SetRoleShapeshifters.AddRange(RoleClass.EvilButtoner.EvilButtonerPlayer);
            SetRoleShapeshifters.AddRange(RoleClass.SuicideWisher.SuicideWisherPlayer);
            /*============シェイプシフター役職設定============*/

            foreach (PlayerControl Player in DesyncImpostors)
            {
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.GetClientId();
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
                        else sender.RpcSetRole(Player, RoleTypes.Scientist, pc.GetClientId());
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
                    int PlayerCID = Player.GetClientId();
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
                        else sender.RpcSetRole(Player, RoleTypes.Scientist, pc.GetClientId());
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
                    int PlayerCID = Player.GetClientId();
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
                        sender.RpcSetRole(Player, RoleTypes.Crewmate, Player.GetClientId());
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
                    int PlayerCID = Player.GetClientId();
                    if (RoleClass.Spy.CanUseVent) sender.RpcSetRole(Player, RoleTypes.Engineer, PlayerCID);
                    else sender.RpcSetRole(Player, RoleTypes.Crewmate, PlayerCID);
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
                            if (pc.IsImpostor() || pc.IsRole(RoleId.Spy))
                            {
                                sender.RpcSetRole(Player, RoleTypes.Impostor, pc.GetClientId());
                            }
                            else
                            {
                                sender.RpcSetRole(Player, RoleTypes.Scientist, pc.GetClientId());
                            }
                        }
                    }
                }
                else
                {
                    if (Player.PlayerId != 0) sender.RpcSetRole(Player, RoleTypes.Crewmate, Player.GetClientId());
                    else Player.SetRole(RoleTypes.Crewmate);
                }
            }

            foreach (PlayerControl p in SetRoleEngineers)
            {
                if (!p.IsMod())
                {
                    sender.RpcSetRole(p, RoleTypes.Engineer);
                }
            }
            foreach (PlayerControl p in SetRoleShapeshifters)
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
                    if (Player.IsImpostor()) AllRoleSetClass.ImpostorPlayers.Add(Player);
                    else AllRoleSetClass.CrewMatePlayers.Add(Player);
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
                    var selection = option.GetSelection();
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

            var Assassinselection = CustomOptions.AssassinAndMarineOption.GetSelection();
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