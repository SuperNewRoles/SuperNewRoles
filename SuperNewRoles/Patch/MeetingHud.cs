
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    

    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
    class PlayerVoteAreaCosmetics
    {
        private static Sprite blankNameplate = null;
        public static void updateNameplate(PlayerVoteArea pva, byte playerId = Byte.MaxValue)
        {
            blankNameplate = blankNameplate ?? HatManager.Instance.AllNamePlates[0].Image;

            var nameplate = blankNameplate;
            var p = ModHelpers.playerById(playerId != byte.MaxValue ? playerId : pva.TargetPlayerId);
            var nameplateId = p?.CurrentOutfit?.NamePlateId;
            nameplate = HatManager.Instance.GetNamePlateById(nameplateId)?.Image;
            pva.Background.sprite = nameplate;
        }
        static void Postfix(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
        {
            updateNameplate(__instance, playerInfo.PlayerId);
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
                        } else if(ps.TargetPlayerId == Mode.Detective.main.DetectivePlayer.PlayerId && ps.DidVote)
                        {
                            MeetingHud.VoterState[] statesdetective;
                            GameData.PlayerInfo exiledPlayerdetective = PlayerControl.LocalPlayer.Data;
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

                                exiledPlayerdetective = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tiedetective && info.PlayerId == ps.VotedFor);

                                __instance.RpcVotingComplete(statesdetective, exiledPlayerdetective, tiedetective); //RPC
                            } else
                            {

                                statesListdetective.Add(new MeetingHud.VoterState()
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
                            PlayerControl VoteTarget = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => info.PlayerId == ps.VotedFor)?.Object;
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
                    } else
                    {
                        foreach (var ps in __instance.playerStates)
                        {
                            if (!(ps.AmDead || ps.DidVote))//死んでいないプレイヤーが投票していない
                                return false;
                        }
                        MeetingHud.VoterState[] states1;
                        GameData.PlayerInfo exiledPlayer1 = PlayerControl.LocalPlayer.Data;
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
                        exiledPlayer1 = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie1 && info.PlayerId == exileId1);
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
                else
                {
                    foreach (var ps in __instance.playerStates)
                    {
                        if (!(ps.AmDead || ps.DidVote))//死んでいないプレイヤーが投票していない
                            return false;
                    }
                }
                MeetingHud.VoterState[] states;
                GameData.PlayerInfo exiledPlayer = PlayerControl.LocalPlayer.Data;
                bool tie = false;

                List<MeetingHud.VoterState> statesList = new List<MeetingHud.VoterState>();
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
                        /*
                        if (ps.VotedFor == 253 && !voter.Data.IsDead)//スキップ
                        {
                            switch (main.whenSkipVote)
                            {
                                case VoteMode.Suicide:
                                    main.ps.setDeathReason(ps.TargetPlayerId, PlayerState.DeathReason.Suicide);
                                    voter.RpcMurderPlayer(voter);
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (ps.VotedFor == 254 && !voter.Data.IsDead)//無投票
                        {
                            switch (main.whenNonVote)
                            {
                                case VoteMode.Suicide:
                                    voter.RpcMurderPlayer(voter);
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    break;
                                default:
                                    break;
                            }
                        }*/
                        if (!ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.Mayor))
                        {
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                        }
                        if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.Mayor))
                        {

                            for (var i2 = 0; i2 < RoleClass.Mayor.AddVote; i2++)
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

                __instance.RpcVotingComplete(states, exiledPlayer, tie); //RPC

                /*
                new LateTask(() => {
                    var reactorId = 3;
                    BotHandler.CreateBot();
                    //BotHandler.bot.NetTransform.RpcSnapTo(new Vector2(-0.7f, -1f));
                    SuperNewRolesPlugin.Logger.LogInfo("TPOK!");
                    try
                    {
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                            SabotageWriter.Write(reactorId);
                            MessageExtensions.WriteNetObject(SabotageWriter, p);
                            SabotageWriter.Write((byte)128);
                            AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                            MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                            SabotageFixWriter.Write(reactorId);
                            MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                            SabotageFixWriter.Write((byte)16);
                            AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                            if (PlayerControl.GameOptions.MapId == 4)
                            {
                                MessageWriter SabotageFixWriter2 = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                                SabotageFixWriter2.Write(reactorId);
                                MessageExtensions.WriteNetObject(SabotageFixWriter2, p);
                                SabotageFixWriter2.Write((byte)17);
                                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter2);
                            }
                        }
                    }
                    catch (Exception e){
                        SuperNewRolesPlugin.Logger.LogInfo("エラー―やん！！！:"+e);
                    }
                    //AmongUsClient.Instance.Spawn(BotHandler.bot, -2, SpawnFlags.None);
                    /*
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("マーダーOK2!!!");
                    var reactorId = 3;
                    SuperNewRolesPlugin.Logger.LogInfo("マーダーOK3!!!");
                    p.RpcMurderPlayer(BotHandler.bot);
                    SuperNewRolesPlugin.Logger.LogInfo("マーダーOK!!!");
                    MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageWriter, p);
                    SabotageWriter.Write((byte)128);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, p);
                    SabotageFixWriter.Write((byte)16);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        MessageWriter SabotageFixWriter2 = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, p.getClientId());
                        SabotageFixWriter2.Write(reactorId);
                        MessageExtensions.WriteNetObject(SabotageFixWriter2, p);
                        SabotageFixWriter2.Write((byte)17);
                        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter2);
                    }
                    /*
                    p.RPCMurderPlayerPrivate(BotHandler.bot);
                    if (p.IsNoBot())
                    {
                    }
                    
                }, 10f, "EndMeetingAntiBlockOut");*/
                if (ModeHandler.isMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.GameOptions.MapId == 4)
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.isDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(19f);
                    } else
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            if (NotBlackOut.IsAntiBlackOut(pc) && (pc.isDead() || pc.PlayerId == exiledPlayer?.PlayerId)) pc.ResetPlayerCam(15f);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:"+ex);
                throw;
            }
            return false;
        }
        public static bool isMayor(byte id)
        {/*
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            if (player == null) return false;
            */
            return false;
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
                if (ps.VotedFor != (byte)252 && ps.VotedFor != byte.MaxValue && ps.VotedFor != (byte)254)
                {
                    int num;
                    int VoteNum = 1;
                    if (ModHelpers.playerById(ps.TargetPlayerId).isRole(RoleId.Mayor)) VoteNum = RoleClass.Mayor.AddVote;
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out num) ? VoteNum : num + VoteNum;
                }
            }
            return dic;
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
                new LateTask(() => {
                    SyncSetting.CustomSyncSettings();
                }, 3f, "StartMeeting_CustomSyncSetting");
            }
        }
    }
}