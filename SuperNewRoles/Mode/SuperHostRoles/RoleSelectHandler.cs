using System.Collections.Generic;
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
            return sender;
        }
        public static void SpawnBots()
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !ModeHandler.IsMode(ModeId.HideAndSeek))
            {
                int impostor = PlayerControl.GameOptions.NumImpostors;
                int crewmate = 0;
                //ジャッカルがいるなら
                if (CustomOptions.JackalOption.GetSelection() != 0 || CustomOptions.JackalSeerOption.GetSelection() != 0)
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
            SetRoleDesync(RoleClass.Jackal.JackalPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.Sheriff.SheriffPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.Demon.DemonPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.Truelover.trueloverPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.FalseCharges.FalseChargesPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.MadMaker.MadMakerPlayer, RoleTypes.Impostor);
            SetRoleDesync(RoleClass.JackalSeer.JackalSeerPlayer, RoleTypes.Impostor);
            /*============インポスターにDesync============*/


            /*============エンジニアに役職設定============*/
            if (RoleClass.Jester.IsUseVent) SetVanillaRole(RoleClass.Jester.JesterPlayer, RoleTypes.Engineer);
            if (RoleClass.JackalFriends.IsUseVent) SetVanillaRole(RoleClass.JackalFriends.JackalFriendsPlayer, RoleTypes.Engineer);
            if (RoleClass.MadMate.IsUseVent) SetVanillaRole(RoleClass.MadMate.MadMatePlayer, RoleTypes.Engineer);
            if (RoleClass.MadMayor.IsUseVent) SetVanillaRole(RoleClass.MadMayor.MadMayorPlayer, RoleTypes.Engineer);
            if (RoleClass.MadStuntMan.IsUseVent) SetVanillaRole(RoleClass.MadStuntMan.MadStuntManPlayer, RoleTypes.Engineer);
            if (RoleClass.MadJester.IsUseVent) SetVanillaRole(RoleClass.MadJester.MadJesterPlayer, RoleTypes.Engineer);
            if (RoleClass.Fox.IsUseVent) SetVanillaRole(RoleClass.Fox.FoxPlayer, RoleTypes.Engineer);
            if (RoleClass.MayorFriends.IsUseVent) SetVanillaRole(RoleClass.MayorFriends.MayorFriendsPlayer, RoleTypes.Engineer);
            if (RoleClass.Tuna.IsUseVent) SetVanillaRole(RoleClass.Tuna.TunaPlayer, RoleTypes.Engineer);
            SetVanillaRole(RoleClass.Technician.TechnicianPlayer, RoleTypes.Engineer);
            if (RoleClass.BlackCat.IsUseVent) SetVanillaRole(RoleClass.BlackCat.BlackCatPlayer, RoleTypes.Engineer);
            if (RoleClass.MadSeer.IsUseVent) SetVanillaRole(RoleClass.MadSeer.MadSeerPlayer, RoleTypes.Engineer);
            if (RoleClass.SeerFriends.IsUseVent) SetVanillaRole(RoleClass.SeerFriends.SeerFriendsPlayer, RoleTypes.Engineer);
            /*============エンジニアに役職設定============*/


            /*============シェイプシフターDesync============*/
            SetRoleDesync(RoleClass.Arsonist.ArsonistPlayer, RoleTypes.Shapeshifter);
            SetRoleDesync(RoleClass.RemoteSheriff.RemoteSheriffPlayer, RoleTypes.Shapeshifter);
            SetRoleDesync(RoleClass.ToiletFan.ToiletFanPlayer, RoleTypes.Shapeshifter);
            SetRoleDesync(RoleClass.NiceButtoner.NiceButtonerPlayer, RoleTypes.Shapeshifter);
            /*============シェイプシフターDesync============*/


            /*============シェイプシフター役職設定============*/
            SetVanillaRole(RoleClass.SelfBomber.SelfBomberPlayer, RoleTypes.Shapeshifter, false);
            SetVanillaRole(RoleClass.Samurai.SamuraiPlayer, RoleTypes.Shapeshifter, false);
            SetVanillaRole(RoleClass.EvilButtoner.EvilButtonerPlayer, RoleTypes.Shapeshifter, false);
            SetVanillaRole(RoleClass.SuicideWisher.SuicideWisherPlayer, RoleTypes.Shapeshifter, false);
            SetVanillaRole(RoleClass.Doppelganger.DoppelggerPlayer, RoleTypes.Shapeshifter, false);
            SetVanillaRole(RoleClass.Camouflager.CamouflagerPlayer, RoleTypes.Shapeshifter, false);
            /*============シェイプシフター役職設定============*/

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
            return;
        }
        /// <summary>
        /// Desyncで役職をセットする
        /// </summary>
        /// <param name="player">ターゲット</param>
        /// <param name="roleTypes">Desyncしたい役職(他視点は科学者固定)</param>
        public static void SetRoleDesync(List<PlayerControl> player, RoleTypes roleTypes)
        {
            foreach (PlayerControl Player in player)
            {
                Logger.Info($"{Player.name}({Player.GetRole()})=>{roleTypes}を実行", "SetRoleDesync");
                if (!Player.IsMod())
                {
                    int PlayerCID = Player.GetClientId();
                    sender.RpcSetRole(Player, roleTypes, PlayerCID);
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
                    //Modクライアントは代わりに普通のクルーにする
                    Player.SetRole(RoleTypes.Crewmate); //Modクライアント視点用
                    sender.RpcSetRole(Player, RoleTypes.Crewmate);
                }
            }
        }
        /// <summary>
        /// バニラ役職をセットする
        /// </summary>
        /// <param name="player">ターゲット</param>
        /// <param name="roleTypes">セットする役職</param>
        /// <param name="isNotModOnly">非Mod導入者のみか(概定はtrue)</param>
        public static void SetVanillaRole(List<PlayerControl> player, RoleTypes roleTypes, bool isNotModOnly = true)
        {
            foreach (PlayerControl p in player)
            {
                if (p.IsMod() && isNotModOnly)
                {
                    Logger.Info($"{p.name}({p.GetRole()})=>{roleTypes}Mod導入者かつ、非導入者のみなので破棄", "SetVanillaRole");
                    return;
                }
                Logger.Info($"{p.name}({p.GetRole()})=>{roleTypes}を実行", "SetVanillaRole");
                sender.RpcSetRole(p, roleTypes);
            }
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