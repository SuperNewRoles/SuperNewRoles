
using HarmonyLib;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnhollowerBaseLib;

namespace SuperNewRoles.Patch
{
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
                        statesList.Add(new MeetingHud.VoterState()
                        {
                            VoterId = ps.TargetPlayerId,
                            VotedForId = ps.VotedFor
                        });
                        //if (isMayor(ps.TargetPlayerId))//Mayorの投票数
                        /*
                        for (var i2 = 0; i2 < main.MayorAdditionalVote; i2++)
                        {
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                        }
                        */
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

                return false;


            }
            catch (Exception ex)
            {
                throw;
            }
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
                    //投票を1追加 キーが定義されていない場合は1で上書きして定義
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
            foreach (var pva in __instance.playerStates)
            {
            }
        }
    }
}