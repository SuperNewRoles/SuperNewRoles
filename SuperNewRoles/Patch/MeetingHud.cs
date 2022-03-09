/*
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;

namespace SuperNewRoles.Patch
{
    public static class MeetingHudPatch
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class CheckForEndVotingPatch
        {
            public static bool Prefix(MeetingHud __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return true;
                foreach (var ps in __instance.playerStates)
                {
                    if (!(ps.AmDead || ps.DidVote))//死んでいないプレイヤーが投票していない
                        return false;
                }



                MeetingHud.VoterState[] states;
                GameData.PlayerInfo exiledPlayer = PlayerControl.LocalPlayer.Data;
                bool tie = false;

                List<MeetingHud.VoterState> statesList = new List<MeetingHud.VoterState>();
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea ps = __instance.playerStates[i];
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
                    }
                    
                    statesList.Add(new MeetingHud.VoterState()
                    {
                        VoterId = ps.TargetPlayerId,
                        VotedForId = ps.VotedFor
                    });
                    
                    if (isMayor(ps.TargetPlayerId))//Mayorの投票数
                        for (var i2 = 0; i2 < main.MayorAdditionalVote; i2++)
                        {
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                        }
                }
                    states = statesList.ToArray();

                    var VotingData = __instance.CustomCalculateVotes();
                    byte exileId = byte.MaxValue;
                    int max = 0;
                    //Logger.info("===追放者確認処理開始===");
                    foreach (var data in VotingData)
                    {
                        //Logger.info(data.Key + ": " + data.Value);
                        if (data.Value > max)
                        {
                            //Logger.info(data.Key + "番が最高値を更新(" + data.Value + ")");
                            exileId = data.Key;
                            max = data.Value;
                            tie = false;
                        }
                        else if (data.Value == max)
                        {
                            //Logger.info(data.Key + "番が" + exileId + "番と同数(" + data.Value + ")");
                            exileId = byte.MaxValue;
                            tie = true;
                        }
                        //Logger.info("exileId: " + exileId + ", max: " + max);
                    }

                    //Logger.info("追放者決定: " + exileId);
                    exiledPlayer = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie && info.PlayerId == exileId);

                    __instance.RpcVotingComplete(states, exiledPlayer, tie); //RPC


                }
                if (Mode.ModeHandler.isMode(Mode.ModeId.NotImpostorCheck))
                {
                    Mode.NotImpostorCheck.NotBlackOut.EndMeetingPatch();
                }
                return false;
            }

        }
        public static Dictionary<byte, int> CustomCalculateVotes(this MeetingHud __instance)
        {
            //Logger.info("CustomCalculateVotes開始");
            Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
            for (int i = 0; i < ((Il2CppArrayBase<PlayerVoteArea>)(object)__instance.playerStates).Count; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (!((Object)(object)playerVoteArea == (Object)null) && playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != byte.MaxValue && playerVoteArea.VotedFor != 254)
                {
                    int num = 1;
                    /*
                    if (CheckForEndVotingPatch.isMayor(playerVoteArea.TargetPlayerId))
                    {
                        num = main.MayorAdditionalVote + 1;
                    }
                    dictionary[playerVoteArea.VotedFor] = ((!dictionary.TryGetValue(playerVoteArea.VotedFor, out var value)) ? num : (value + num));
                }
            }
            return dictionary;
        }
    }
}

*/
