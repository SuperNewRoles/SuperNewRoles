using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;
using UnityEngine.Networking;
using static MeetingHud;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class GameHistoryManager
    {
        public static void ClearAndReloads()
        {
            MeetingHistories = new();
            SendData = null;
        }
        public class MeetingHistory
        {
            public Dictionary<byte, List<int>> VoteColors = new();
            public List<int> SkipPlayers = new();
            public byte exiledPlayer = 255;
            public List<byte> DeadPlayers = new();
            public byte reporter = 255;
            public byte reportedbody = 255;
            public MeetingHistory(Il2CppStructArray<VoterState> states, GameData.PlayerInfo exiled)
            {
                foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
                {
                    bool IsDead = player.IsDead || player.Disconnected;
                    if (player.Object != null)
                        if (player.Object.IsBot()) continue;
                    if (IsDead)
                    {
                        DeadPlayers.Add(player.PlayerId);
                    }
                    VoteColors.Add(player.PlayerId, new());
                }
                reporter = Instance.reporterId;
                reportedbody = 255;
                if (MeetingRoomManager.Instance != null && MeetingRoomManager.Instance.target != null)
                    reportedbody = MeetingRoomManager.Instance.target.PlayerId;
                if (exiled == null) exiledPlayer = 255;
                else exiledPlayer = exiled.PlayerId;
                foreach (VoterState state in states)
                {
                    if (state.SkippedVote || state.VotedForId > 250)
                        SkipPlayers.Add(state.VoterId);
                    else
                        VoteColors[state.VotedForId].Add(state.VoterId);
                }
                MeetingHistories.Add(this);
            }
        }
        public static List<MeetingHistory> MeetingHistories;
        static Dictionary<string, string> SendData;
        public static void OnGameEndSet(Dictionary<int, FinalStatus> FinalStatuss)
        {
            if (!WebAccountManager.IsLogined) return;
            if (PlayerControl.LocalPlayer == null)
            {
                Logger.Info("LocalPlayerが存在しませんでした。", "GameHistoryManager");
                return;
            }
            SendData = new();
            //認証情報
            SendData["token"] = WebAccountManager.Token;
            SendData["GameId"] = AmongUsClient.Instance.GameId.ToString();
            bool cors = WebAccountManager.CanOtherPlayerSendData;
            bool IsMeOnly = false;
            List<WebPlayer> corsOnPlayers = new();
            List<WebPlayer> corsOffPlayers = new();
            foreach (WebPlayer wp in RoomPlayerData.Instance.WebPlayers)
            {
                if (wp.CanOtherPlayerSendData) corsOnPlayers.Add(wp);
                else corsOffPlayers.Add(wp);
            }
            //自分以外の送信者がいない場合は自分以外にも送信する
            if ((corsOnPlayers.Count <= 0 && corsOffPlayers.Count == 1) ||
                corsOnPlayers.Count == 1 && cors)
            {
                IsMeOnly = false;
            }
            //もし、自分以外にも送信対象者がいる場合、自分がその中で一番PlayerIdが小さかったら送信する。
            else if (corsOnPlayers.Count > 1 && cors)
            {
                foreach (WebPlayer wp in corsOnPlayers)
                {
                    if (wp.currentPlayer == null) continue;
                    if (wp.currentPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (wp.currentPlayer.PlayerId < PlayerControl.LocalPlayer.PlayerId) return;
                }
                IsMeOnly = false;
            }
            //もし、自分以外にも送信対象者がいる場合、自分がその中で一番PlayerIdが小さかったら送信する。
            else if (corsOnPlayers.Count <= 0 && corsOffPlayers.Count > 1 && !cors)
            {
                foreach (WebPlayer wp in corsOffPlayers)
                {
                    if (wp.currentPlayer == null) continue;
                    if (wp.currentPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (wp.currentPlayer.PlayerId < PlayerControl.LocalPlayer.PlayerId) IsMeOnly = true;
                }
            }
            SendData["IsMeOnly"] = IsMeOnly ? "a" : "b";
            SendData["MePlayerId"] = PlayerControl.LocalPlayer.PlayerId.ToString();
            //プレイヤー情報
            string PlayerIds = "";
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                string PlayerName = player.PlayerName;
                RoleId roleId = RoleId.DefaultRole;
                if (player.Object != null)
                {
                    if (player.Object.IsBot()) continue;
                    PlayerName = player.Object.GetDefaultName();
                    roleId = player.Object.GetRole();
                }
                if (PlayerIds.Contains(player.PlayerId.ToString() + ",")) continue;
                PlayerIds += player.PlayerId.ToString() + ",";
                string PlayerId = player.PlayerId.ToString();
                SendData[PlayerId + "_PlayerName"] = PlayerName;
                SendData[PlayerId + "_FriendCode"] = player.FriendCode;
                SendData[PlayerId + "_ColorId"] = player.DefaultOutfit.ColorId.ToString();
                SendData[PlayerId + "_RoleName"] = IntroData.GetIntroData(roleId, player.Object).NameKey;
                var (playerCompleted, playerTotal) = TaskCount.TaskDate(player);
                SendData[PlayerId + "_TotalTask"] = playerTotal.ToString();
                SendData[PlayerId + "_CompletedTask"] = playerCompleted.ToString();
                SendData[PlayerId + "_FinalStatus"] = FinalStatuss.ContainsKey(player.PlayerId) ? FinalStatus.Alive.ToString() : FinalStatuss[player.PlayerId].ToString();
                SendData[PlayerId + "_IsWin"] = TempData.winners.ToList().Exists(x => x.PlayerName == player.DefaultOutfit.PlayerName) ? "a" : "b";
            }
            SendData["PlayerIds"] = PlayerIds;
            //設定情報
            SendData["mode"] = ModeHandler.GetMode(false).ToString();
            SendData["map"] = Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId)].ToString();
            // 会議情報
            SendData["MeetingCount"] = MeetingHistories.Count.ToString();
            int index = 0;
            foreach (MeetingHistory mh in MeetingHistories)
            {
                string indexstr = index.ToString();
                SendData[$"meeting_{indexstr}_exiled"] = mh.exiledPlayer == byte.MaxValue ? "None" : mh.exiledPlayer.ToString();
                SendData[$"meeting_{indexstr}_dead"] = string.Join(",", mh.DeadPlayers);
                SendData[$"meeting_{indexstr}_SkipPlayers"] = string.Join(",", mh.SkipPlayers);
                SendData[$"meeting_{indexstr}_VoteColorTargets"] = string.Join(",", mh.VoteColors.Keys);
                SendData[$"meeting_{indexstr}_reporter"] = mh.reporter.ToString();
                SendData[$"meeting_{indexstr}_reporttarget"] = mh.reportedbody.ToString();
                foreach (var votecolor in mh.VoteColors)
                {
                    SendData[$"meeting_{indexstr}_VoteData_{votecolor.Key}"] = string.Join(",", votecolor.Value);
                }
                index++;
            }
        }
        public static void Send(string WinReasonText, Color32 WinReasonColor, System.Action<long, DownloadHandler> callback = null)
        {
            if (SendData == null) return;
            SendData["WinReasonText"] = WinReasonText;
            SendData["WinReasonColor_0"] = WinReasonColor.r.ToString();
            SendData["WinReasonColor_1"] = WinReasonColor.g.ToString();
            SendData["WinReasonColor_2"] = WinReasonColor.b.ToString();
            SendData["WinReasonColor_3"] = WinReasonColor.a.ToString();
            WebApi.SendGameHistory(SendData, callback);
        }
    }
}