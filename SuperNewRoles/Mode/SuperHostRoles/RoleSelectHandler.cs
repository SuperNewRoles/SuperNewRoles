using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class RoleSelectHandler
{
    public static CustomRpcSender sender = null;
    /// <summary>
    /// 追放メッセージを表記する為のBot
    /// </summary>
    /// <value>現在はパン屋Bot 又は 詐欺師Botのみ</value>
    public static PlayerControl ConfirmImpostorSecondTextBot = null;

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
        SyncSetting.CustomSyncSettings(out var modified);
        ChacheManager.ResetChache();
        return sender;
    }
    public static void SpawnBots()
    {
        if (ReplayManager.IsReplayMode) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !ModeHandler.IsMode(ModeId.HideAndSeek, ModeId.VanillaHns))
        {
            int impostor = GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors;
            int crewmate = 0;
            //ジャッカルがいるなら
            if (CustomOptionHolder.JackalOption.GetSelection() != 0 || CustomOptionHolder.JackalSeerOption.GetSelection() != 0)
            {
                for (int i = 0; i < (GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors + 2); i++)
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
                CustomOptionHolder.EgoistOption.GetSelection() != 0 ||
                CustomOptionHolder.SheriffOption.GetSelection() != 0 ||
                CustomOptionHolder.trueloverOption.GetSelection() != 0 ||
                CustomOptionHolder.FalseChargesOption.GetSelection() != 0 ||
                CustomOptionHolder.RemoteSheriffOption.GetSelection() != 0 ||
                CustomOptionHolder.MadMakerOption.GetSelection() != 0 ||
                CustomOptionHolder.SamuraiOption.GetSelection() != 0 ||
                CustomOptionHolder.DemonOption.GetSelection() != 0 ||
                CustomOptionHolder.ToiletFanOption.GetSelection() != 0 ||
                CustomOptionHolder.NiceButtonerOption.GetSelection() != 0 ||
                Worshiper.CustomOptionData.Option.GetSelection() != 0)
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
            else if (CustomOptionHolder.AssassinAndMarlinOption.GetSelection() != 0)
            {
                PlayerControl bot1 = BotManager.Spawn("暗転対策BOT1");
                bot1.RpcSetRole(RoleTypes.Crewmate);
                crewmate++;
            }
            if (CustomOptionHolder.SpyOption.GetSelection() != 0)
            {
                for (int i = 0; i < CustomOptionHolder.SpyPlayerCount.GetFloat() - (crewmate - (impostor - GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors)) + 1; i++)
                {
                    PlayerControl bot1 = BotManager.Spawn("暗転対策BOT");
                    bot1.RpcSetRole(RoleTypes.Crewmate);
                    crewmate++;
                }
            }
            ConfirmImpostorSecondTextBot = null; // 2つ目の追放メッセージ用Botの初期化
            if (CustomOptionHolder.BakeryOption.GetSelection() != 0 || Crook.CustomOptionData.Option.GetSelection() != 0)
            {
                string name = CustomOptionHolder.BakeryOption.GetSelection() != 0 ? "パン屋BOT" : "詐欺師BOT";
                ConfirmImpostorSecondTextBot = BotManager.Spawn(name);
                ConfirmImpostorSecondTextBot.Exiled();
            }
            else if (CustomOptionHolder.AssassinAndMarlinOption.GetSelection() != 0)
            {
                BotManager.Spawn(ModTranslation.GetString("AssassinAndMarlinName") + "BOT").Exiled();
            }
        }
        else if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            BattleRoyal.Main.SpawnBots();
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
        if (RoleClass.Madmate.IsUseVent) SetVanillaRole(RoleClass.Madmate.MadmatePlayer, RoleTypes.Engineer);
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
        if (Pokerface.CustomOptionData.CanUseVent.GetBool()) SetVanillaRole(Pokerface.RoleData.Player, RoleTypes.Engineer);
        /*============エンジニアに役職設定============*/

        /*============科学者に役職設定============*/
        if (PoliceSurgeon.RoleData.HaveVital) SetVanillaRole(PoliceSurgeon.RoleData.Player, RoleTypes.Scientist, false);
        /*============科学者に役職設定============*/

        /*============シェイプシフターDesync============*/
        SetRoleDesync(RoleClass.Arsonist.ArsonistPlayer, RoleTypes.Shapeshifter);
        SetRoleDesync(RoleClass.RemoteSheriff.RemoteSheriffPlayer, RoleTypes.Shapeshifter);
        SetRoleDesync(RoleClass.ToiletFan.ToiletFanPlayer, RoleTypes.Shapeshifter);
        SetRoleDesync(RoleClass.NiceButtoner.NiceButtonerPlayer, RoleTypes.Shapeshifter);
        SetRoleDesync(Worshiper.RoleData.Player, RoleTypes.Shapeshifter);
        SetRoleDesync(MadRaccoon.RoleData.Player, RoleTypes.Shapeshifter);
        /*============シェイプシフターDesync============*/


        /*============シェイプシフター役職設定============*/
        SetVanillaRole(RoleClass.SelfBomber.SelfBomberPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Samurai.SamuraiPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.EvilButtoner.EvilButtonerPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.SuicideWisher.SuicideWisherPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Doppelganger.DoppelggerPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Camouflager.CamouflagerPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(EvilSeer.RoleData.Player, RoleTypes.Shapeshifter, false);
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
            }
            else
            {
                if (Player.PlayerId != 0) sender.RpcSetRole(Player, RoleTypes.Crewmate, Player.GetClientId());
                else Player.SetRole(RoleTypes.Crewmate);
            }
            if (ModeHandler.GetMode() == ModeId.SuperHostRoles)
            {
                //他視点で科学者にするループ
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.PlayerId == Player.PlayerId) continue;
                    if (!pc.IsMod())
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
        AllRoleSetClass.CrewmatePlayers = new();
        AllRoleSetClass.ImpostorPlayers = new();
        foreach (PlayerControl Player in CachedPlayer.AllPlayers)
        {
            if (!Player.IsBot())
            {
                if (Player.IsImpostor()) AllRoleSetClass.ImpostorPlayers.Add(Player);
                else AllRoleSetClass.CrewmatePlayers.Add(Player);
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

        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (!intro.IsGhostRole && AllRoleSetClass.CanRoleIdElected(intro.RoleId))
            {
                var option = IntroData.GetOption(intro.RoleId);
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

        var Assassinselection = CustomOptionHolder.AssassinAndMarlinOption.GetSelection();
        SuperNewRolesPlugin.Logger.LogInfo("[SHR] アサイン情報:" + Assassinselection + "、" + AllRoleSetClass.CrewmatePlayerNum + "、" + AllRoleSetClass.CrewmatePlayers.Count);
        if (Assassinselection != 0 && AllRoleSetClass.CrewmatePlayerNum > 0 && AllRoleSetClass.CrewmatePlayers.Count > 0)
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