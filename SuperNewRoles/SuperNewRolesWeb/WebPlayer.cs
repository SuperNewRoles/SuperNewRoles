using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SuperNewRoles.SuperNewRolesWeb;
public struct WebPlayer
{
    public PlayerControl currentPlayer;
    public string WebName;
    public string UserId;
    public bool CanOtherPlayerSendData;
    public static WebPlayer Create(PlayerControl player)
    {
        WebPlayer webPlayer = new()
        {
            currentPlayer = player
        };
        WebApi.GetWebPlayerData(player.Data.FriendCode, (code, handler) =>
        {
            if (webPlayer.currentPlayer == null || code != 200)
            {
                if (RoomPlayerData._instance != null)
                {
                    RoomPlayerData._instance.WebPlayers.RemoveAll(x => x.currentPlayer == webPlayer.currentPlayer);
                }
            }
            else
            {
                JToken jobj = JObject.Parse(handler.text);
                if (!jobj.HasValues) return;
                webPlayer.WebName = jobj["name"]?.ToString();
                webPlayer.UserId = jobj["userid"]?.ToString();
                webPlayer.CanOtherPlayerSendData = jobj["CanOtherPlayerSendData"]?.ToString() == "a";
            }
        });
        return webPlayer;
    }
}