
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;
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
            var p = ModHelpers.playerById(playerId != byte.MaxValue ? playerId : pva.TargetPlayerId);
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
            if (exiled != null && exiled.Object.IsBot() && RoleClass.Assassin.TriggerPlayer == null && main.RealExiled == null)
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
                if (ModeHandler.isMode(ModeId.Detective) && Mode.Detective.main.IsNotDetectiveVote)
                {
                    foreach (var ps in __instance.playerStates)
                    {
                        if (ps.TargetPlayerId == Mode.Detective.main.DetectivePlayer.PlayerId && !ps.DidVote)
                        {
                            return false;
                        }
                        else if (ps.TargetPlayerId == Mode.Detective.main.DetectivePlayer.PlayerId && ps.DidVote)
                        {
                            MeetingHud.VoterState[] statesdetective;
                            GameData.PlayerInfo exiledPlayerdetective = CachedPlayer.LocalPlayer.Data;
                            bool tiedetective = false;

                            List<MeetingHud.VoterState> statesListdetective = new List<MeetingHud.VoterState>();
                            if (ps.VotedFor != ps.TargetPlayerId)
                            {
                                statesListdetective.Add(new MeetingHud.VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = ps.VotedFor
                                });
                                statesdetective = statesListdetective.ToArray();

                                var VotingDatadetective = __instance.CustomCalculateVotes();

                                exiledPlayerdetective = GameData.Instance.AllPlayers.GetFastEnumerator().ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == ps.VotedFor);

                                __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                            }
                            else
                            {

                                statesListdetective.Add(new MeetingHud.VoterState()
                                {
                                    VoterId = ps.TargetPlayerId,
                                    VotedForId = 253
                                });
                                statesdetective = statesListdetective.ToArray();

                                var VotingDatadetective = __instance.CustomCalculateVotes();
                                exiledPlayerdetective = GameData.Instance.AllPlayers.GetFastEnumerator().ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == 253);

                                __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                            }
                            return false;
                        }
                    }
                }
                else if (ModeHandler.isMode(ModeId.Werewolf))
                {
                    if (Mode.Werewolf.main.IsAbility)
                    {
                        foreach (var ps in __instance.playerStates)
                        {
                            PlayerControl player = ModHelpers.playerById(ps.TargetPlayerId);
                            if (!ps.AmDead && !ps.DidVote && (player.isImpostor() || (!player.isRole(CustomRPC.RoleId.DefaultRole) && !player.isRole(CustomRPC.RoleId.MadMate) && !player.isRole(CustomRPC.RoleId.SpiritMedium) && !(player.PlayerId == Mode.Werewolf.main.HunterExilePlayer.PlayerId && Mode.Werewolf.main.HunterPlayers.IsCheckListPlayerControl(player)))))
                                return false;
                        }
                        for (var i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea ps = __instance.playerStates[i];
                            PlayerControl player = ModHelpers.playerById(ps.TargetPlayerId);
                            PlayerControl VoteTarget = GameData.Instance.AllPlayers.GetFastEnumerator().ToArray().FirstOrDefault(info => info.PlayerId == ps.VotedFor)?.Object;
                            if (ps.VotedFor != 253 && ps.VotedFor != 254 && VoteTarget != null)
                            {
                                if (player.isImpostor())
                                {
                                    Mode.Werewolf.main.WolfKillPlayers.Add(VoteTarget.PlayerId);
                                }
                                else if (player.isRole(RoleId.SoothSayer))
                                {
                                    Mode.Werewolf.main.SoothRoles.Add(player.PlayerId, VoteTarget.PlayerId);
                                }
                                else if (player.PlayerId == Mode.Werewolf.main.HunterExilePlayer.PlayerId && Mode.Werewolf.main.HunterPlayers.IsCheckListPlayerControl(player))
                                {
                                    Mode.Werewolf.main.SoothRoles.Add(player.PlayerId, VoteTarget.PlayerId);
                                }
                            }
                        }
                        __instance.RpcVotingComplete(new List<MeetingHud.VoterState>().ToArray(), null, false);
                        return false;
                    }
                    else
                    {
                        foreach (var ps in __instance.playerStates)
                        {
                            if (!(ps.AmDead || ps.DidVote))//死んでいないプレイヤーが投票していない
                                return false;
                        }
                        MeetingHud.VoterState[] states1;
                        GameData.PlayerInfo exiledPlayer1 = CachedPlayer.LocalPlayer.Data;
                        bool tie1 = false;

                        List<MeetingHud.VoterState> statesList1 = new List<MeetingHud.VoterState>();
                        for (var i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea ps = __instance.playerStates[i];
                            if (ModeHandler.isMode(ModeId.BattleRoyal))
                            {
                                if (ps != null && ModHelpers.playerById(ps.TargetPlayerId).isRole(CustomRPC.RoleId.Sheriff))
                                {

                                }
                            }
                            else
                            {
                                if (ps == null) continue;
                                var voter = ModHelpers.playerById(ps.TargetPlayerId);
                                if (voter == null || voter.Data == null || voter.Data.Disconnected) continue;
                                statesList1.Add(new MeetingHud.VoterState()
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
                        exiledPlayer1 = GameData.Instance.AllPlayers.GetFastEnumerator().ToArray().FirstOrDefault(info => !tie1 && info.PlayerId == exileId1);
                        if (exiledPlayer1 != null && Mode.Werewolf.main.HunterPlayers.IsCheckListPlayerControl(exiledPlayer1.Object))
                        {
                            Mode.Werewolf.main.HunterExilePlayer = exiledPlayer1.Object;
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
                    var (isVoteEnd, voteFor, voteArea) = assassinVoteState(__instance);

                    SuperNewRolesPlugin.Logger.LogInfo(isVoteEnd+"、"+voteFor);
                    if (isVoteEnd)
                    {
                        //GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
                        Il2CppStructArray<MeetingHud.VoterState> array =
                            new Il2CppStructArray<MeetingHud.VoterState>(
                                __instance.playerStates.Length);

                        for (int i = 0; i < __instance.playerStates.Length; i++)
                        {
                            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                            if (playerVoteArea.TargetPlayerId == RoleClass.Assassin.TriggerPlayer.PlayerId)
                            {
                                playerVoteArea.VotedFor = voteFor;
                            }
                            else
                            {
                                playerVoteArea.VotedFor = 254;
                            }
                            __instance.SetDirtyBit(1U);

                            array[i] = new MeetingHud.VoterState
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
                            main.RealExiled = target.Object;
                            if (ModeHandler.isMode(ModeId.SuperHostRoles))
                            {
                                foreach (PlayerControl p in BotManager.AllBots)
                                {
                                    if (p.isDead())
                                    {
                                        exileplayer = p.Data;
                                        exile = p;
                                        p.RpcSetColor((byte)outfit.ColorId);
                                        p.RpcSetName(target.Object.getDefaultName() + 
                                            ModTranslation.getString(target.Object.isRole(RoleId.Marine) ? 
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
                            if (target.Object.isRole(RoleId.Marine))
                            {
                                RoleClass.Assassin.IsImpostorWin = true;
                            }
                            else
                            {
                                RoleClass.Assassin.DeadPlayer = RoleClass.Assassin.TriggerPlayer;
                            }
                            new LateTask(() =>
                            {
                                if (exile != null)
                                {
                                    exile.RpcSetName(exile.getDefaultName());
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
                else
                {
                    foreach (var ps in __instance.playerStates)
                    {
                        if (!(ps.AmDead || ps.DidVote) && ModHelpers.playerById(ps.TargetPlayerId).IsPlayer())//死んでいないプレイヤーが投票していない
                            return false;
                    }
                }
                MeetingHud.VoterState[] states;
                GameData.PlayerInfo exiledPlayer = CachedPlayer.LocalPlayer.Data;
                bool tie = false;

                List<MeetingHud.VoterState> statesList = new List<MeetingHud.VoterState>();
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea ps = __instance.playerStates[i];
                    if (AmongUsClient.Instance.GameMode != GameModes.FreePlay || ps.TargetPlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        if (ModeHandler.isMode(ModeId.BattleRoyal))
                        {
                            if (ps != null && ModHelpers.playerById(ps.TargetPlayerId).isRole(CustomRPC.RoleId.Sheriff))
                            {

                            }
                        }
                        else
                        {
                            if (ps == null) continue;
                            var voter = ModHelpers.playerById(ps.TargetPlayerId);
                            if (voter == null || voter.Data == null || voter.Data.Disconnected || voter.IsBot() || voter.isDead()) continue;
                            //BOTならスキップ判定
                            if (ps.VotedFor != 253 && ps.VotedFor != 254 && ModHelpers.playerById(ps.VotedFor).IsBot())
                            {
                                ps.VotedFor = 253;
                            }
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                            if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.Mayor))
                            {
                                for (var i2 = 0; i2 < RoleClass.Mayor.AddVote - 1; i2++)
                                {
                                    statesList.Add(new MeetingHud.VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                            else if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.MadMayor))
                            {
                                for (var i2 = 0; i2 < RoleClass.MadMayor.AddVote - 1; i2++)
                                {
                                    statesList.Add(new MeetingHud.VoterState()
                                    {
                                        VoterId = ps.TargetPlayerId,
                                        VotedForId = ps.VotedFor
                                    });
                                }
                            }
                            else if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.MayorFriends))
                            {
                                for (var i2 = 0; i2 < RoleClass.MayorFriends.AddVote - 1; i2++)
                                {
                                    statesList.Add(new MeetingHud.VoterState()
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

                exiledPlayer = GameData.Instance.AllPlayers.GetFastEnumerator().ToArray().FirstOrDefault(info => !tie && info.PlayerId == exileId);

                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (exiledPlayer != null && exiledPlayer.Object.isRole(RoleId.Assassin))
                    {
                        main.RealExiled = exiledPlayer.Object;
                        PlayerControl exile = null;
                        PlayerControl defaultexile = exiledPlayer.Object;
                        var outfit = defaultexile.Data.DefaultOutfit;
                        foreach (PlayerControl p in BotManager.AllBots)
                        {
                            if (p.isDead())
                            {
                                exiledPlayer = p.Data;
                                exile = p;
                                exile.RpcSetColor((byte)outfit.ColorId);
                                exile.RpcSetName(defaultexile.getDefaultName());
                                exile.RpcSetHat(outfit.HatId);
                                exile.RpcSetVisor(outfit.VisorId);
                                exile.RpcSetSkin(outfit.SkinId);
                                break;
                            }
                        }
                        new LateTask(() => {
                            if (exile != null)
                            {
                                exile.RpcSetName(exile.getDefaultName());
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
                                if (p.isDead())
                                {
                                    p.getDefaultName();
                                    exiledPlayer = p.Data;
                                    foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                                    {
                                        if (p2.IsPlayer() && !p2.Data.Disconnected && !p2.IsMod())
                                        {
                                            p.RpcSetNamePrivate("<size=300%>" + ModTranslation.getString("BakeryExileText") + "\n" + FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip) + "</size><size=0%>", p2);
                                        }
                                    }
                                    new LateTask(() => p.RpcSetName(p.getDefaultName()),5f);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (PlayerControl p2 in CachedPlayer.AllPlayers)
                            {
                                if (p2.IsPlayer() && !p2.Data.Disconnected && !p2.IsMod())
                                {
                                    exiledPlayer.Object.RpcSetNamePrivate("<size=300%>" + ModTranslation.getString("BakeryExileText") + "\n" + exiledPlayer.Object.getDefaultName(), p2);
                                }
                                new LateTask(() => exiledPlayer.Object.RpcSetName(p2.getDefaultName()), 5f);
                            }
                        }
                    }
                }

                __instance.RpcVotingComplete(states, exiledPlayer, tie); //RPC

                /*
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        foreach (var pc in CachedPlayer.AllPlayers)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.isDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(19f);
                    }
                    else
                    {
                        foreach (var pc in CachedPlayer.AllPlayers)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.isDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(15f);
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
        public static bool isMayor(byte id)
        {/*
            var player = CachedPlayer.AllPlayers.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            if (player == null) return false;
            */
            return false;
        }
        private static Tuple<bool, byte, PlayerVoteArea> assassinVoteState(MeetingHud __instance)
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
    }

    static class ExtendedMeetingHud
    {
        public static Dictionary<byte, int> CustomCalculateVotes(this MeetingHud __instance)
        {
            Dictionary<byte, int> dic = new Dictionary<byte, int>();
            //| 投票された人 | 投票された回数 |
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea ps = __instance.playerStates[i];
                if (ps == null) continue;
                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay && ps.TargetPlayerId != CachedPlayer.LocalPlayer.PlayerId) continue;
                if (ps != null && ModHelpers.playerById(ps.TargetPlayerId) != null && ps.VotedFor != 252 && ps.VotedFor != byte.MaxValue && ps.VotedFor != (byte)254 && ModHelpers.playerById(ps.TargetPlayerId).isAlive() && ModHelpers.playerById(ps.TargetPlayerId).IsPlayer())
                {
                    int num;
                    int VoteNum = 1;
                    if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.Mayor)) VoteNum = RoleClass.Mayor.AddVote;
                    else if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.MadMayor)) VoteNum = RoleClass.MadMayor.AddVote;
                    else if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.MayorFriends)) VoteNum = RoleClass.MayorFriends.AddVote;
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out num) ? VoteNum : num + VoteNum;
                }
            }
            return dic;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
    class MeetingHudSetForegroundForDeadPatch
    {
        public static bool Prefix(
            MeetingHud __instance)
        {

            if (RoleClass.Assassin.TriggerPlayer == null) { return true; }

            if (!RoleClass.Assassin.TriggerPlayer.AmOwner)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class MeetingHudUpdateButtonsPatch
    {
        public static bool PreFix(MeetingHud __instance)
        {
            if (RoleClass.Assassin.TriggerPlayer == null) { return true; }

            if (AmongUsClient.Instance.AmHost)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(
                        playerVoteArea.TargetPlayerId);
                    if (playerById == null)
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
        public static void Prefix(MeetingHud __instance)
        {
        }
        public static void Postfix(MeetingHud __instance)
        {
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                new LateTask(() =>
                {
                    SyncSetting.CustomSyncSettings();
                }, 3f, "StartMeeting_CustomSyncSetting");
            }
        }
    }
}