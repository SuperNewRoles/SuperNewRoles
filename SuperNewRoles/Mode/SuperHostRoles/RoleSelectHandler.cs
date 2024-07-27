using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class RoleSelectHandler
{
    public static CustomRpcSender sender = null;
    public static bool IsStartingSerialize = false;
    public static Dictionary<byte, byte[]> SetTasksBuffer = new();
    /// <summary>
    /// 追放メッセージを表記する為のBot
    /// </summary>
    /// <value>現在はパン屋Bot 又は 詐欺師Botのみ</value>
    public static PlayerControl ConfirmImpostorSecondTextBot = null;

    public static CustomRpcSender DEBUGOnlySender;

    public static CustomRpcSender RoleSelect(CustomRpcSender send)
    {
        sender = send;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR] ROLESELECT");
        if (!AmongUsClient.Instance.AmHost) return null;
        IsStartingSerialize = true;
        SuperNewRolesPlugin.Logger.LogInfo("[SHR] つうか");
        CrewOrImpostorSet();
        OneOrNotListSet();
        AllRoleSetClass.AllRoleSet();
        SetCustomRoles();
        SyncSetting.CustomSyncSettings();
        CacheManager.ResetCache();
        return sender;
    }
    public static void SpawnBots()
    {
        return;
        if (ReplayManager.IsReplayMode) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !ModeHandler.IsMode(ModeId.HideAndSeek, ModeId.VanillaHns))
        {
            int impostor = GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors;
            int crewmate = 0;
            //ジャッカルがいるなら
            if (Jackal.Optioninfo.RoleOption.GetBool() || CustomOptionHolder.JackalSeerOption.GetSelection() != 0)
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


    /// <summary>
    /// RoleBase移行前の役職の, ISupportSHRの[DesyncRole]及び[IsDesync]の代用。
    /// FIXME : 全てRoleBaseに移行出来たら消す。
    /// </summary>
    public static (bool IsDesync, RoleTypes RoleType) GetDesyncRole(RoleId role)
    {
        return role switch
        {
            RoleId.Sheriff => (true, RoleTypes.Impostor),
            RoleId.Demon => (true, RoleTypes.Impostor),
            RoleId.truelover => (true, RoleTypes.Impostor),
            RoleId.FalseCharges => (true, RoleTypes.Impostor),
            RoleId.MadMaker => (true, RoleTypes.Impostor),
            RoleId.JackalSeer => (true, RoleTypes.Impostor),

            RoleId.Jester => RoleClass.Jester.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.JackalFriends => RoleClass.JackalFriends.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.Madmate => RoleClass.Madmate.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.MadMayor => RoleClass.MadMayor.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.MadJester => RoleClass.MadJester.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.Fox => RoleClass.Fox.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.MayorFriends => RoleClass.MayorFriends.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.Tuna => RoleClass.Tuna.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.Technician => (false, RoleTypes.Engineer),
            RoleId.BlackCat => RoleClass.BlackCat.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.MadSeer => RoleClass.MadSeer.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.SeerFriends => RoleClass.SeerFriends.IsUseVent ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),
            RoleId.Pokerface => Pokerface.CustomOptionData.CanUseVent.GetBool() ? (false, RoleTypes.Engineer) : (false, RoleTypes.Crewmate),

            RoleId.PoliceSurgeon => PoliceSurgeon.RoleData.HaveVital ? (false, RoleTypes.Scientist) : (false, RoleTypes.Crewmate),

            RoleId.Arsonist => (true, RoleTypes.Shapeshifter),
            RoleId.RemoteSheriff => (true, RoleTypes.Shapeshifter),
            RoleId.ToiletFan => (true, RoleTypes.Shapeshifter),
            RoleId.NiceButtoner => (true, RoleTypes.Shapeshifter),
            RoleId.Worshiper => (true, RoleTypes.Shapeshifter),
            RoleId.MadRaccoon => (true, RoleTypes.Shapeshifter),

            RoleId.SelfBomber => (false, RoleTypes.Shapeshifter),
            RoleId.Samurai => (false, RoleTypes.Shapeshifter),
            RoleId.EvilButtoner => (false, RoleTypes.Shapeshifter),
            RoleId.SuicideWisher => (false, RoleTypes.Shapeshifter),
            RoleId.Doppelganger => (false, RoleTypes.Shapeshifter),
            RoleId.Camouflager => (false, RoleTypes.Shapeshifter),

            _ => (false, RoleTypes.Crewmate)
        };
    }

    private static void AddToSyncRoles(this Dictionary<byte, (RoleTypes role, bool isNotModOnly)> syncDictionary, List<PlayerControl> players, RoleTypes roleTypes, bool isNotOnlyMod = false)
    {
        foreach (PlayerControl player in players.AsSpan())
            syncDictionary.AddToSyncRoles(player, roleTypes, isNotOnlyMod);
    }
    private static void AddToSyncRoles(this Dictionary<byte, (RoleTypes role, bool isNotModOnly)> syncDictionary, PlayerControl player, RoleTypes roleTypes, bool isNotOnlyMod = false)
    {
        syncDictionary[player.PlayerId] = (roleTypes, isNotOnlyMod);
    }
    private static void CheckAndAddToSyncRoles(this Dictionary<byte, (RoleTypes role, bool isNotModOnly)> syncDictionary, bool isAdd, List<PlayerControl> players, RoleTypes roleTypes, bool isNotModOnly = false)
    {
        if (isAdd)
            AddToSyncRoles(syncDictionary, players, roleTypes, isNotModOnly);
    }
    private static void AddToDesyncRoles(this Dictionary<byte, RoleTypes> desyncDictionary, List<PlayerControl> players, RoleTypes roleTypes)
    {
        foreach (PlayerControl player in players.AsSpan())
            desyncDictionary.Add(player.PlayerId, roleTypes);
    }

    public static readonly Dictionary<int, RoleTypes> DesyncTable = new()
    {
        { (int)RoleId.Sheriff, RoleTypes.Impostor },
        { (int)RoleId.Demon, RoleTypes.Impostor },
        { (int)RoleId.truelover, RoleTypes.Impostor },
        { (int)RoleId.FalseCharges, RoleTypes.Impostor },
        { (int)RoleId.MadMaker, RoleTypes.Impostor },
        { (int)RoleId.JackalSeer, RoleTypes.Impostor },
        { (int)RoleId.Arsonist, RoleTypes.Shapeshifter },
        { (int)RoleId.RemoteSheriff, RoleTypes.Shapeshifter },
        { (int)RoleId.ToiletFan, RoleTypes.Shapeshifter },
        { (int)RoleId.NiceButtoner, RoleTypes.Shapeshifter },
        { (int)RoleId.Worshiper, RoleTypes.Shapeshifter },
        { (int)RoleId.MadRaccoon, RoleTypes.Shapeshifter }
    };

    public static RoleTypes? GetDesyncRole(PlayerControl player)
    {
        if (player.GetRoleBase() is ISupportSHR playerSHR
            && playerSHR.IsDesync)
            return playerSHR.DesyncRole;
        if (DesyncTable.TryGetValue((int)player.GetRole(), out RoleTypes targetRole))
            return targetRole;
        return null;
    }

    public static void SetCustomRoles()
    {
        Dictionary<byte, (RoleTypes role, bool isNotModOnly)> CrewmateSyncRoles = new();
        Dictionary<byte, RoleTypes> DesyncRoles = new();
        // 役職設定時に最適な方法を使用する
        PlayerControl NotDesyncTarget = null;
        RoleTypes? NotDesyncTargetRole = null;

        /*============インポスターにDesync============*/
        DesyncRoles.AddToDesyncRoles(RoleClass.Sheriff.SheriffPlayer, RoleTypes.Impostor);
        DesyncRoles.AddToDesyncRoles(RoleClass.Demon.DemonPlayer, RoleTypes.Impostor);
        DesyncRoles.AddToDesyncRoles(RoleClass.Truelover.trueloverPlayer, RoleTypes.Impostor);
        DesyncRoles.AddToDesyncRoles(RoleClass.FalseCharges.FalseChargesPlayer, RoleTypes.Impostor);
        DesyncRoles.AddToDesyncRoles(RoleClass.MadMaker.MadMakerPlayer, RoleTypes.Impostor);
        DesyncRoles.AddToDesyncRoles(RoleClass.JackalSeer.JackalSeerPlayer, RoleTypes.Impostor);
        /*============インポスターにDesync============*/


        /*============エンジニアに役職設定============*/
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.Jester.IsUseVent, RoleClass.Jester.JesterPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.JackalFriends.IsUseVent, RoleClass.JackalFriends.JackalFriendsPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.Madmate.IsUseVent, RoleClass.Madmate.MadmatePlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.MadMayor.IsUseVent, RoleClass.MadMayor.MadMayorPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.MadStuntMan.IsUseVent, RoleClass.MadStuntMan.MadStuntManPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.MadJester.IsUseVent, RoleClass.MadJester.MadJesterPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.Fox.IsUseVent, RoleClass.Fox.FoxPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.MayorFriends.IsUseVent, RoleClass.MayorFriends.MayorFriendsPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.Tuna.IsUseVent, RoleClass.Tuna.TunaPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.AddToSyncRoles(RoleClass.Technician.TechnicianPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.BlackCat.IsUseVent, RoleClass.BlackCat.BlackCatPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.MadSeer.IsUseVent, RoleClass.MadSeer.MadSeerPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(RoleClass.SeerFriends.IsUseVent, RoleClass.SeerFriends.SeerFriendsPlayer, RoleTypes.Engineer);
        CrewmateSyncRoles.CheckAndAddToSyncRoles(Pokerface.CustomOptionData.CanUseVent.GetBool(), Pokerface.RoleData.Player, RoleTypes.Engineer);
        /*============エンジニアに役職設定============*/

        /*============科学者に役職設定============*/
        CrewmateSyncRoles.CheckAndAddToSyncRoles(PoliceSurgeon.RoleData.HaveVital, PoliceSurgeon.RoleData.Player, RoleTypes.Scientist, false);
        /*============科学者に役職設定============*/

        /*============シェイプシフターDesync============*/
        DesyncRoles.AddToDesyncRoles(RoleClass.Arsonist.ArsonistPlayer, RoleTypes.Shapeshifter);
        DesyncRoles.AddToDesyncRoles(RoleClass.RemoteSheriff.RemoteSheriffPlayer, RoleTypes.Shapeshifter);
        DesyncRoles.AddToDesyncRoles(RoleClass.ToiletFan.ToiletFanPlayer, RoleTypes.Shapeshifter);
        DesyncRoles.AddToDesyncRoles(RoleClass.NiceButtoner.NiceButtonerPlayer, RoleTypes.Shapeshifter);
        DesyncRoles.AddToDesyncRoles(Worshiper.RoleData.Player, RoleTypes.Shapeshifter);
        DesyncRoles.AddToDesyncRoles(MadRaccoon.RoleData.Player, RoleTypes.Shapeshifter);
        /*============シェイプシフターDesync============*/

        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.GetRoleBase() is not ISupportSHR playerSHR)
                continue;
            if (playerSHR.IsDesync)
            {
                DesyncRoles.AddToDesyncRoles([player], playerSHR.DesyncRole);
                continue;
            }
            if (!playerSHR.RealRole.IsImpostorRole())
                CrewmateSyncRoles.AddToSyncRoles(player, playerSHR.RealRole, playerSHR.IsRealRoleNotModOnly);
            else
                SetVanillaRole(player, playerSHR.RealRole, playerSHR.IsRealRoleNotModOnly);
        }

        // インポスター系の通常設定は一番最初にやる

        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (CrewmateSyncRoles.ContainsKey(player.PlayerId) || DesyncRoles.ContainsKey(player.PlayerId))
                continue;
            if (player.IsImpostor())
                sender.RpcSetRole(player, RoleTypes.Impostor, true);
        }

        /*============シェイプシフター役職設定============*/
        SetVanillaRole(RoleClass.SelfBomber.SelfBomberPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Samurai.SamuraiPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.EvilButtoner.EvilButtonerPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.SuicideWisher.SuicideWisherPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Doppelganger.DoppelggerPlayer, RoleTypes.Shapeshifter, false);
        SetVanillaRole(RoleClass.Camouflager.CamouflagerPlayer, RoleTypes.Shapeshifter, false);
        /*============シェイプシフター役職設定============*/

        foreach (var desyncdata in DesyncRoles)
        {
            PlayerControl player = ModHelpers.PlayerById(desyncdata.Key);
            if (player == null)
            {
                Logger.Error("player is null.", "SHR RoleSelectHandler");
                continue;
            }
            SetRoleDesync(player, desyncdata.Value);
        }
        // CrewmateSyncRole
        foreach (var syncdata in CrewmateSyncRoles)
        {
            PlayerControl player = ModHelpers.PlayerById(syncdata.Key);
            if (player == null)
            {
                Logger.Error("player is null on CrewmateSyncRoles.", "SHR RoleSelectHandler");
                continue;
            }
            SetVanillaRole(player, syncdata.Value.role, syncdata.Value.isNotModOnly);
        }

        foreach(PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.IsImpostor())
                continue;
            if (player.Data.Disconnected)
                continue;
            NotDesyncTarget = player;
            NotDesyncTargetRole = player.Data.Role.Role;
            break;
        }

        
        // 暫定対処、通常は起こり得ないが起こり得た場合にはエラーを出す
        if (NotDesyncTarget == null)
            throw new System.NotImplementedException("Crewmates is all desync role.");

        //new LateTask(() => {
        Logger.Info($"RoleSelectHandler: {NotDesyncTarget.PlayerId}");
        //}, 0.15f, "SetRole Disconnected Task");

        if (RoleClass.Egoist.EgoistPlayer.Count + RoleClass.Spy.SpyPlayer.Count > 0)
            throw new System.NotImplementedException("Egoist and Spy is not working.");
        /*
        foreach (PlayerControl Player in RoleClass.Egoist.EgoistPlayer.AsSpan())
        {
            if (!Player.IsMod())
            {
                int PlayerCID = Player.GetClientId();
                //ただしホスト、お前はDesyncするな。
                sender.RpcSetRole(Player, RoleTypes.Impostor, true);
                //役職者で他プレイヤーを科学者にするループ
                foreach (var pc in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (pc.PlayerId == Player.PlayerId) continue;
                    sender.RpcSetRole(pc, RoleTypes.Scientist, true, PlayerCID);
                }
            }
            else
            {
                //ホストは代わりに普通のクルーにする
                if (Player.PlayerId != 0)
                {
                    sender.RpcSetRole(Player, RoleTypes.Crewmate, true, Player.GetClientId());
                }
                else
                {
                    Player.SetRole(RoleTypes.Crewmate); //ホスト視点用
                }
                sender.RpcSetRole(Player, RoleTypes.Impostor, true);
            }
            //p.Data.IsDead = true;
        }
        foreach (PlayerControl Player in RoleClass.Spy.SpyPlayer.AsSpan())
        {
            if (!Player.IsMod())
            {
                int PlayerCID = Player.GetClientId();
                if (RoleClass.Spy.CanUseVent) sender.RpcSetRole(Player, RoleTypes.Engineer, true, PlayerCID);
                else sender.RpcSetRole(Player, RoleTypes.Crewmate, true, PlayerCID);
                foreach (var pc in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (pc.PlayerId == Player.PlayerId) continue;
                    sender.RpcSetRole(pc, RoleTypes.Scientist, true, PlayerCID);
                }
            }
            else
            {
                if (Player.PlayerId != 0) sender.RpcSetRole(Player, RoleTypes.Crewmate, true, Player.GetClientId());
                else Player.SetRole(RoleTypes.Crewmate);
            }
            if (ModeHandler.GetMode() == ModeId.SuperHostRoles)
            {
                //他視点で科学者にするループ
                foreach (PlayerControl pc in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (pc.PlayerId == Player.PlayerId) continue;
                    if (!pc.IsMod())
                    {
                        if (pc.IsImpostor() || pc.IsRole(RoleId.Spy))
                        {
                            sender.RpcSetRole(Player, RoleTypes.Impostor, true, pc.GetClientId());
                        }
                        else
                        {
                            sender.RpcSetRole(Player, RoleTypes.Scientist, true, pc.GetClientId());
                        }
                    }
                }
            }
        }
        */
        return;
    }

    /// <summary>
    /// Desyncで役職をセットする
    /// </summary>
    /// <param name="player">ターゲット</param>
    /// <param name="roleTypes">Desyncしたい役職(他視点は科学者固定)</param>
    public static void SetRoleDesync(List<PlayerControl> player, RoleTypes roleTypes)
    {
        foreach (PlayerControl Player in player.AsSpan())
        {
            SetRoleDesync(Player, roleTypes);
        }
    }
    public static void SetRoleDesync(PlayerControl Player, RoleTypes roleTypes) => SetRoleDesync(sender, Player, roleTypes);
    public static void SetRoleDesync(this CustomRpcSender sender, PlayerControl Player, RoleTypes roleTypes)
    {
        Logger.Info($"{Player.name}({Player.GetRole()})=>{roleTypes}を実行", "SetRoleDesync");
        if (!Player.IsMod())
        {
            int PlayerCID = Player.GetClientId();
            sender.RpcSetRole(Player, roleTypes, true, PlayerCID);
            foreach (var pc in CachedPlayer.AllPlayers.AsSpan())
            {
                if (pc.PlayerId == Player.PlayerId) continue;
                sender.RpcSetRole(pc, pc.Data.Role.Role.IsImpostorRole() ? RoleTypes.Scientist : pc.Data.Role.Role, true, PlayerCID);
            }
            //他視点で科学者にするループ
            foreach (var pc in CachedPlayer.AllPlayers.AsSpan())
            {
                if (pc.PlayerId == Player.PlayerId) continue;
                if (pc.PlayerId == 0) Player.SetRole(RoleTypes.Crewmate, true); //ホスト視点用
                else sender.RpcSetRole(Player, RoleTypes.Scientist, true, ((PlayerControl)pc).GetClientId());
            }
        }
        else
        {
            //Modクライアントは代わりに普通のクルーにする
            Player.SetRole(RoleTypes.Crewmate, true); //Modクライアント視点用
            sender.RpcSetRole(Player, RoleTypes.Crewmate, true);
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
        foreach (PlayerControl p in player.AsSpan())
        {
            SetVanillaRole(p, roleTypes, isNotModOnly);
        }
    }
    /// <summary>
    /// バニラ役職をセットする
    /// </summary>
    /// <param name="p">ターゲット</param>
    /// <param name="roleTypes">セットする役職</param>
    /// <param name="isNotModOnly">非Mod導入者のみか(概定はtrue)</param>
    public static void SetVanillaRole(PlayerControl p, RoleTypes roleTypes, bool isNotModOnly = true) => SetVanillaRole(sender, p, roleTypes, isNotModOnly);
    public static void SetVanillaRole(this CustomRpcSender sender, PlayerControl p, RoleTypes roleTypes, bool isNotModOnly = true)
    {
        if (p.IsMod() && isNotModOnly)
        {
            Logger.Info($"{p.name}({p.GetRole()})=>{roleTypes}Mod導入者かつ、非導入者のみなので破棄", "SetVanillaRole");
            return;
        }
        Logger.Info($"{p.name}({p.GetRole()})=>{roleTypes}を実行", "SetVanillaRole");
        sender.RpcSetRole(p, roleTypes, true);
    }
    public static void CrewOrImpostorSet()
    {
        AllRoleSetClass.CrewmatePlayers = new();
        AllRoleSetClass.ImpostorPlayers = new();
        foreach (PlayerControl Player in CachedPlayer.AllPlayers.AsSpan())
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
        AllRoleSetClass.AssignTickets = new();
        foreach (IntroData intro in IntroData.Intros.Values)
        {
            if (intro.IsGhostRole || !AllRoleSetClass.CanRoleIdElected(intro.RoleId))
                continue;
            var option = IntroData.GetOption(intro.RoleId);
            if (option == null || !option.isSHROn) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, intro.RoleId, intro.Team);
        }
        foreach (RoleInfo info in RoleInfoManager.RoleInfos.Values)
        {
            if (info.IsGhostRole || !AllRoleSetClass.CanRoleIdElected(info.Role))
                continue;
            var option = IntroData.GetOption(info.Role);
            if (option == null || !option.isSHROn) continue;
            var selection = option.GetSelection();
            AllRoleSetClass.SetChance(selection, info.Role, info.Team);
        }
    }
}