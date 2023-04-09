using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine.Networking;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class GameHistoryManager
    {
        public static void Send(Dictionary<byte, FinalStatus> FinalStatuss, Action<long, DownloadHandler> callback = null)
        {
            if (!WebAccountManager.IsLogined) return;
            if (PlayerControl.LocalPlayer == null)
            {
                Logger.Info("LocalPlayerが存在しませんでした。","GameHistoryManager");
                return;
            }
            Dictionary<string, string> SendData = new();
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
            string PlayerIds = "";
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsBot()) continue;
                if (PlayerIds.Contains(player.PlayerId.ToString() + ",")) continue;
                PlayerIds += player.PlayerId.ToString() + ",";
                string PlayerId = player.PlayerId.ToString();
                SendData[PlayerId + "_PlayerName"] = player.GetDefaultName();
                SendData[PlayerId + "_FriendCode"] = player.Data.FriendCode;
                SendData[PlayerId + "_ColorId"] = player.Data.DefaultOutfit.ColorId.ToString();
                SendData[PlayerId + "_RoleName"] = IntroData.GetIntroData(player.GetRole(), player).NameKey;
                var (playerCompleted, playerTotal) = TaskCount.TaskDate(player.Data);
                SendData[PlayerId + "_TotalTask"] = playerTotal.ToString();
                SendData[PlayerId + "_CompletedTask"] = playerCompleted.ToString();
                SendData[PlayerId + "_FinalStatus"] = FinalStatuss[player.PlayerId].ToString();
                SendData[PlayerId + "_IsWin"] = TempData.winners.ToList().Exists(x => x.PlayerName == player.Data.DefaultOutfit.PlayerName) ? "a" : "b";
            }
            SendData["PlayerIds"] = PlayerIds;
            SendData["mode"] = ModeHandler.GetMode(false).ToString();
            SendData["map"] = Constants.MapNames[GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId)].ToString();
            WebApi.SendGameHistory(SendData, callback);
        }
    }
}
