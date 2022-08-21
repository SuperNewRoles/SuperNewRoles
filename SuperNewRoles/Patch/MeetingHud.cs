using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using UnhollowerBaseLib;
using static MeetingHud;

namespace SuperNewRoles.Patch
{
    /*
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
    class PlayerVoteAreaCosmetics
    {
        private static Sprite blankNameplate = null;
        public static void updateNameplate(PlayerVoteArea pva, byte playerId = Byte.MaxValue)
        {
            blankNameplate = blankNameplate ?? HatManager.Instance.AllNamePlates[0].viewData.viewData.Image;

            var nameplate = blankNameplate;
            var p = ModHelpers.PlayerById(playerId != byte.MaxValue ? playerId : pva.TargetPlayerId);
            var nameplateId = p?.CurrentOutfit?.NamePlateId;
            nameplate = HatManager.Instance.GetNamePlateById(nameplateId)?.viewData.viewData.Image;
            pva.Background.sprite = nameplate;
        }
        static void Postfix(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
        {
            updateNameplate(__instance, playerInfo.PlayerId);
        }
    }*/
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    class VotingComplete
    {
        public static void Prefix(MeetingHud __instance, [HarmonyArgument(0)] VoterState[] states, [HarmonyArgument(1)] ref GameData.PlayerInfo exiled, [HarmonyArgument(2)] bool tie)
        {
            if (exiled != null && exiled.Object.IsBot() && RoleClass.Assassin.TriggerPlayer == null && Main.RealExiled == null)
            {
                exiled = null;
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class CheckForEndVotingPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            try
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                if (ModeHandler.IsMode(ModeId.Detective) && Mode.Detective.Main.IsNotDetectiveVote)
                {
                    foreach (var ps in __instance.playerStates)
                    {
                        if (ps.TargetPlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId && !ps.DidVote)
                        {
                            return false;
                        }
                        else if (ps.TargetPlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId && ps.DidVote)
                        {
                            VoterState[] statesdetective;
                            GameData.PlayerInfo exiledPlayerdetective = CachedPlayer.LocalPlayer.Data;
                            bool tiedetective = false;

                            List<VoterState> statesListdetective = new();
                            if (ps.VotedFor != ps.TargetPlayerId)
                            {
                                statesListdetective.Add(new VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = ps.VotedFor
                                });
                                statesdetective = statesListdetective.ToArray();

                                var VotingDatadetective = __instance.CustomCalculateVotes();

                                exiledPlayerdetective = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == ps.VotedFor);

                                __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                            }
                            else
                            {
                                statesListdetective.Add(new VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = 253
                                });
                                statesdetective = statesListdetective.ToArray();

                                var VotingDatadetective = __instance.CustomCalculateVotes();
                                exiledPlayerdetective = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == 253);

                                __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                            }
                            return false;
                        }
                    }
                }
                else if (ModeHandler.IsMode(ModeId.Werewolf))
                {
                    if (Mode.Werewolf.Main.IsAbility)
                    {
                        foreach (var ps in __instance.playerStates)
                        {
                            PlayerControl player = ModHelpers.PlayerById(ps.TargetPlayerId);
                            if (!ps.AmDead && !ps.DidVote && (player.IsImpostor() || (!player.IsRole(RoleId.DefaultRole) && !player.IsRole(RoleId.MadMate) && !player.IsRole(RoleId.SpiritMedium) && !(player.PlayerId == Mode.Werewolf.Main.HunterExilePlayer.PlayerId && Mode.Werewolf.Main.HunterPlayers.IsCheckListPlayerControl(player)))))
                                return false;
                        }
                        for (var i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea ps = __instance.playerStates[i];
                            PlayerControl player = ModHelpers.PlayerById(ps.TargetPlayerId);
                            PlayerControl VoteTarget = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => info.PlayerId == ps.VotedFor)?.Object;
                            if (ps.VotedFor != 253 && ps.VotedFor != 254 && VoteTarget != null)
                            {
                                if (player.IsImpostor())
                                    Mode.Werewolf.Main.WolfKillPlayers.Add(VoteTarget.PlayerId);
                                else if (player.IsRole(RoleId.SoothSayer))
                                    Mode.Werewolf.Main.SoothRoles.Add(player.PlayerId, VoteTarget.PlayerId);
                                else if (player.PlayerId == Mode.Werewolf.Main.HunterExilePlayer.PlayerId && Mode.Werewolf.Main.HunterPlayers.IsCheckListPlayerControl(player))
                                    Mode.Werewolf.Main.SoothRoles.Add(player.PlayerId, VoteTarget.PlayerId);
                            }
                        }
                        __instance.RpcVotingComplete(new List<VoterState>().ToArray(), null, false);
                        return false;
                    }
                    else
                    {
                        foreach (var ps in __instance.playerStates)
                        {
                            if (!(ps.AmDead || ps.DidVote))//死んでいないプレイヤーが投票していない
                                return false;
                        }
                        VoterState[] states1;
                        GameData.PlayerInfo exiledPlayer1 = CachedPlayer.LocalPlayer.Data;
                        bool tie1 = false;

                        List<VoterState> statesList1 = new();
                        for (var i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea ps = __instance.playerStates[i];
                            if (ModeHandler.IsMode(ModeId.BattleRoyal))
                            {
                                if (ps != null && ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Sheriff)) { }
                            }
                            else
                            {
                                if (ps == null) continue;
                                var voter = ModHelpers.PlayerById(ps.TargetPlayerId);
                                if (voter == null || voter.Data == null || voter.Data.Disconnected) continue;
                                statesList1.Add(new VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = ps.VotedFor
                                });
                            }
                        }
                        states1 = statesList1.ToArray();
                        var VotingData1 = __instance.CustomCalculateVotes();
                        byte exileId1 = byte.MaxValue;
                        int max1 = 0;
                        foreach (var data in VotingData1)
                        {
                            if (data.Value > max1)
                            {
                                exileId1 = data.Key;
                                max1 = data.Value;
                                tie1 = false;
                            }
                            else if (data.Value == max1)
                            {
                                exileId1 = byte.MaxValue;
                                tie1 = true;
                            }
                        }
                        exiledPlayer1 = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie1 && info.PlayerId == exileId1);
                        if (exiledPlayer1 != null && Mode.Werewolf.Main.HunterPlayers.IsCheckListPlayerControl(exiledPlayer1.Object))
                        {
                            Mode.Werewolf.Main.HunterExilePlayer = exiledPlayer1.Object;
                            __instance.RpcVotingComplete(states1, null, false); //RPC
                        }
                        else
                        {
                            __instance.RpcVotingComplete(states1, exiledPlayer1, tie1); //RPC
                        }
                        return false;
                    }
                }
                else if (RoleClass.Assassin.TriggerPlayer != null)
                {
                    var (isVoteEnd, voteFor, voteArea) = AssassinVoteState(__instance);

                    SuperNewRolesPlugin.Logger.LogInfo(isVoteEnd + "、" + voteFor);
                    if (isVoteEnd)
                    {
                        //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                        Il2CppStructArray<MeetingHud.VoterState> array =
                            new(
                                __instance.playerStates.Length);

                        for (int i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                            playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId ? voteFor : (byte)254;
                            __instance.SetDirtyBit(1U);

                            array[i] = new VoterState
                            {
                                VoterId = playerVoteArea.TargetPlayerId,
                                VotedForId = playerVoteArea.VotedFor
                            };
                        }
                        GameData.PlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                        GameData.PlayerInfo exileplayer = null;
                        if (target != null && target.Object.PlayerId != RoleClass.Assassin.TriggerPlayer.PlayerId && target.Object.IsPlayer())
                        {
                            var outfit = target.DefaultOutfit;
                            exileplayer = target;
                            PlayerControl exile = null;
                            Main.RealExiled = target.Object;
                            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                            {
                                foreach (PlayerControl p in BotManager.AllBots)
                                {
                                    if (p.IsDead())
                                    {
                                        exileplayer = p.Data;
                                        exile = p;
                                        p.RpcSetColor((byte)outfit.ColorId);
                                        p.RpcSetName(target.Object.GetDefaultName() +
                                            ModTranslation.GetString(target.Object.IsRole(RoleId.Marine) ?
                                            "AssassinSucsess" :
                                            "AssassinFail")
                                            + "<size=0%>");
                                        p.RpcSetHat(outfit.HatId);
                                        p.RpcSetVisor(outfit.VisorId);
                                        p.RpcSetSkin(outfit.SkinId);
                                        break;
                                    }
                                }
                            }
                            RoleClass.Assassin.MeetingEndPlayers.Add(RoleClass.Assassin.TriggerPlayer.PlayerId);
                            if (target.Object.IsRole(RoleId.Marine))
                                RoleClass.Assassin.IsImpostorWin = true;
                            else
                                RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
                            new LateTask(() =>
                            {
                                if (exile != null)
                                {
                                    exile.RpcSetName(exile.GetDefaultName());
                                    exile.RpcSetColor(1);
                                    exile.RpcSetHat("hat_NoHat");
                                    exile.RpcSetPet("peet_EmptyPet");
                                    exile.RpcSetVisor("visor_EmptyVisor");
                                    exile.RpcSetNamePlate("nameplate_NoPlate");
                                    exile.RpcSetSkin("skin_None");
                                }
                            }, 5f);
                        }
                        new LateTask(() => __instance.RpcVotingComplete(array, exileplayer, true), 0.2f);
                    }
                    return false;
                }
                else if (RoleClass.Revolutionist.MeetingTrigger != null)
                {
                    var (isVoteEnd, voteFor, voteArea) = RevolutionistVoteState(__instance);

                    SuperNewRolesPlugin.Logger.LogInfo(isVoteEnd + "、" + voteFor);
                    if (isVoteEnd)
                    {
                        //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                        Il2CppStructArray<MeetingHud.VoterState> array =
                            new(
                                __instance.playerStates.Length);

                        for (int i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                            playerVoteArea.VotedFor = playerVoteArea.TargetPlayerId == RoleClass.Revolutionist.MeetingTrigger.PlayerId ? voteFor : (byte)254;
                            __instance.SetDirtyBit(1U);

                            array[i] = new VoterState
                            {
                                VoterId = playerVoteArea.TargetPlayerId,
                                VotedForId = playerVoteArea.VotedFor
                            };
                        }
                        GameData.PlayerInfo target = GameData.Instance.GetPlayerById(voteFor);
                        GameData.PlayerInfo exileplayer = null;
                        if (target != null && target.Object.PlayerId != RoleClass.Revolutionist.MeetingTrigger.PlayerId && target.Object.IsPlayer())
                        {
                            var outfit = target.DefaultOutfit;
                            exileplayer = target;
                            if (target.Object.IsRole(RoleId.Dictator))
                                RoleClass.Revolutionist.WinPlayer = RoleClass.Revolutionist.MeetingTrigger;
                        }
                        new LateTask(() => __instance.RpcVotingComplete(array, exileplayer, true), 0.2f);
                    }
                    return false;
                }
                else
                {
                    foreach (var ps in __instance.playerStates)
                    {
                        if (!(ps.AmDead || ps.DidVote) && ModHelpers.PlayerById(ps.TargetPlayerId) != null && ModHelpers.PlayerById(ps.TargetPlayerId).IsPlayer())//死んでいないプレイヤーが投票していない
                            return false;
                    }
                }
                VoterState[] states;
                GameData.PlayerInfo exiledPlayer = CachedPlayer.LocalPlayer.Data;
                bool tie = false;

                List<VoterState> statesList = new();
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea ps = __instance.playerStates[i];
                    if (AmongUsClient.Instance.GameMode != GameModes.FreePlay || ps.TargetPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (ModeHandler.IsMode(ModeId.BattleRoyal))
                        {
                            if (ps != null && ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Sheriff)) { }
                        }
                        else
                        {
                            if (ps == null) continue;
                            var voter = ModHelpers.PlayerById(ps.TargetPlayerId);
                            if (voter == null || voter.Data == null || voter.Data.Disconnected || voter.IsBot() || voter.IsDead() || ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Neet)) continue;
                            //BOT・ニートならスキップ判定
                            if ((ps.VotedFor != 253 && ps.VotedFor != 254 && ModHelpers.PlayerById(ps.VotedFor).IsBot()) || ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Neet))
                            {
                                ps.VotedFor = 253;
                            }
                            statesList.Add(new VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                            if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Mayor))
                            {
                                for (var i2 = 0; i2 < RoleClass.Mayor.AddVote - 1; i2++)
                                {
                                    statesList.Add(new VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                            else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.MadMayor))
                            {
                                for (var i2 = 0; i2 < RoleClass.MadMayor.AddVote - 1; i2++)
                                {
                                    statesList.Add(new VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                            else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.MayorFriends))
                            {
                                for (var i2 = 0; i2 < RoleClass.MayorFriends.AddVote - 1; i2++)
                                {
                                    statesList.Add(new VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                            else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Dictator))
                            {
                                for (var i2 = 0; i2 < RoleClass.Dictator.VoteCount - 1; i2++)
                                {
                                    statesList.Add(new VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                        }
                    }
                }
                states = statesList.ToArray();

                var VotingData = __instance.CustomCalculateVotes();
                byte exileId = byte.MaxValue;
                int max = 0;
                foreach (var data in VotingData)
                {
                    if (data.Value > max)
                    {
                        exileId = data.Key;
                        max = data.Value;
                        tie = false;
                    }
                    else if (data.Value == max)
                    {
                        exileId = byte.MaxValue;
                        tie = true;
                    }
                }

                exiledPlayer = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie && info.PlayerId == exileId);

                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Assassin))
                    {
                        Main.RealExiled = exiledPlayer.Object;
                        PlayerControl exile = null;
                        PlayerControl defaultexile = exiledPlayer.Object;
                        var outfit = defaultexile.Data.DefaultOutfit;
                        foreach (PlayerControl p in BotManager.AllBots)
                        {
                            if (p.IsDead())
                            {
                                exiledPlayer = p.Data;
                                exile = p;
                                exile.RpcSetColor((byte)outfit.ColorId);
                                exile.RpcSetName(defaultexile.GetDefaultName());
                                exile.RpcSetHat(outfit.HatId);
                                exile.RpcSetVisor(outfit.VisorId);
                                exile.RpcSetSkin(outfit.SkinId);
                                break;
                            }
                        }
                        new LateTask(() =>
                        {
                            if (exile != null)
                            {
                                exile.RpcSetName(exile.GetDefaultName());
                                exile.RpcSetColor(1);
                                exile.RpcSetHat("hat_NoHat");
                                exile.RpcSetPet("peet_EmptyPet");
                                exile.RpcSetVisor("visor_EmptyVisor");
                                exile.RpcSetNamePlate("nameplate_NoPlate");
                                exile.RpcSetSkin("skin_None");
                            }
                        }, 5f);
                    }
                    if (Bakery.BakeryAlive())
                    {
                        if (exiledPlayer == null)
                        {
                            foreach (PlayerControl p in BotManager.AllBots)
                            {
                                if (p.IsDead())
                                {
                                    exiledPlayer = p.Data;
                                    foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                                    {
                                        if (p2.IsPlayer() && !p2.Data.Disconnected && !p2.IsMod())
                                        {
                                            p.RpcSetNamePrivate("<size=300%>" + ModTranslation.GetString("BakeryExileText") + "\n" + FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip) + "</size><size=0%>", p2);
                                        }
                                    }
                                    new LateTask(() => p.RpcSetName(p.GetDefaultName()), 5f);
                                    break;
                                }
                            }
                        }
                        else if (exiledPlayer.Object.IsBot()) { }
                        else
                        {
                            foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                            {
                                if (p2.IsPlayer() && !p2.Data.Disconnected && !p2.IsMod())
                                {
                                    exiledPlayer.Object.RpcSetNamePrivate("<size=300%>" + ModTranslation.GetString("BakeryExileText") + "\n" + exiledPlayer.Object.GetDefaultName(), p2);
                                }
                            }
                            new LateTask(() => exiledPlayer.Object.RpcSetName(exiledPlayer.Object.GetDefaultName()), 5f);
                        }
                    }
                }

                if (exiledPlayer != null && exiledPlayer.Object.IsRole(RoleId.Dictator))
                {
                    bool Flag = false;
                    if (!RoleClass.Dictator.SubExileLimitData.ContainsKey(exiledPlayer.Object.PlayerId))
                    {
                        RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId] = RoleClass.Dictator.SubExileLimit;
                    }
                    if (RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId] > 0)
                    {
                        RoleClass.Dictator.SubExileLimitData[exiledPlayer.Object.PlayerId]--;
                        Flag = true;
                    }
                    if (Flag)
                    {
                        List<PlayerControl> DictatorSubExileTargetList = PlayerControl.AllPlayerControls.ToArray().ToList();
                        DictatorSubExileTargetList.RemoveAll(p =>
                        {
                            return p.IsDead() || p.PlayerId == exiledPlayer.PlayerId;
                        });
                        exiledPlayer = ModHelpers.GetRandom(DictatorSubExileTargetList)?.Data;
                    }
                }

                __instance.RpcVotingComplete(states, exiledPlayer, tie); //RPC

                /*
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        foreach (var pc in CachedPlayer.AllPlayers)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.IsDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(19f);
                    }
                    else
                    {
                        foreach (var pc in CachedPlayer.AllPlayers)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.IsDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(15f);
                    }
                }
                */
                return false;
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + ex);
                throw;
            }
        }
        public static bool IsMayor()
        {/*
            var player = CachedPlayer.AllPlayers.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            if (player == null) return false;
            */
            return false;
        }
        private static Tuple<bool, byte, PlayerVoteArea> AssassinVoteState(MeetingHud __instance)
        {
            bool isVoteEnd = false;
            byte voteFor = byte.MaxValue;
            PlayerVoteArea area = null;

            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
                {
                    isVoteEnd = playerVoteArea.DidVote || playerVoteArea.AmDead;
                    voteFor = playerVoteArea.VotedFor;
                    area = playerVoteArea;
                    break;
                }
            }
            return Tuple.Create(isVoteEnd, voteFor, area);
        }
        private static Tuple<bool, byte, PlayerVoteArea> RevolutionistVoteState(MeetingHud __instance)
        {
            bool isVoteEnd = false;
            byte voteFor = byte.MaxValue;
            PlayerVoteArea area = null;

            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.TargetPlayerId == RoleClass.Revolutionist.MeetingTrigger.PlayerId)
                {
                    isVoteEnd = playerVoteArea.DidVote || playerVoteArea.AmDead;
                    voteFor = playerVoteArea.VotedFor;
                    area = playerVoteArea;
                    break;
                }
            }
            return Tuple.Create(isVoteEnd, voteFor, area);
        }
    }

    static class ExtendedMeetingHud
    {
        public static Dictionary<byte, int> CustomCalculateVotes(this MeetingHud __instance)
        {
            Dictionary<byte, int> dic = new();
            //| 投票された人 | 投票された回数 |
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea ps = __instance.playerStates[i];
                if (ps == null) continue;
                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay && ps.TargetPlayerId != CachedPlayer.LocalPlayer.PlayerId) continue;
                if (ps != null && ModHelpers.PlayerById(ps.TargetPlayerId) != null && ps.VotedFor != 252 && ps.VotedFor != byte.MaxValue && ps.VotedFor != 254 && ModHelpers.PlayerById(ps.TargetPlayerId).IsAlive() && ModHelpers.PlayerById(ps.TargetPlayerId).IsPlayer())
                {
                    int VoteNum = 1;
                    if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Mayor)) VoteNum = RoleClass.Mayor.AddVote;
                    else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.MadMayor)) VoteNum = RoleClass.MadMayor.AddVote;
                    else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.MayorFriends)) VoteNum = RoleClass.MayorFriends.AddVote;
                    else if (ModHelpers.PlayerById(ps.TargetPlayerId).IsRole(RoleId.Dictator)) VoteNum = RoleClass.Dictator.VoteCount;
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out int num) ? VoteNum : num + VoteNum;
                }
            }
            return dic;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
    class MeetingHudSetForegroundForDeadPatch
    {
        public static bool Prefix()
        {
            return (RoleClass.Assassin.TriggerPlayer == null || !RoleClass.Assassin.TriggerPlayer.AmOwner) && (RoleClass.Revolutionist.MeetingTrigger == null || !RoleClass.Revolutionist.MeetingTrigger.AmOwner);
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class MeetingHudUpdateButtonsPatch
    {
        public static bool PreFix(MeetingHud __instance)
        {
            if (RoleClass.Assassin.TriggerPlayer == null && RoleClass.Revolutionist.MeetingTrigger) { return true; }

            if (AmongUsClient.Instance.AmHost)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    GameData.PlayerInfo PlayerById = GameData.Instance.GetPlayerById(
                        playerVoteArea.TargetPlayerId);
                    if (PlayerById == null)
                    {
                        playerVoteArea.SetDisabled();
                    }
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Prefix()
        {
        }
        public static void Postfix()
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                new LateTask(() =>
                {
                    SyncSetting.CustomSyncSettings();
                }, 3f, "StartMeeting_CustomSyncSetting");
            }
        }
    }
}