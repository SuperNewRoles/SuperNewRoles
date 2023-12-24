using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class WebApi
    {
        public static string GetString(this Dictionary<string, string> dict)
        {
            var items = dict.Select(kvp => $"{kvp.Key}={Il2CppSystem.Web.HttpUtility.UrlEncode(kvp.Value)}");
            string txt = "";
            foreach (string text in items)
            {
                txt += text + "&";
            }
            return txt.Substring(0, txt.Length - 1);
        }
        public static void GenerateCode(string FriendCode, Action<long, DownloadHandler> callback)
        {
            Requests.Post(WebConstants.ApiUrl + "generateACcode", new Dictionary<string, string>() { { "friendcode", FriendCode } }.GetString(), callback);
        }
        public static void Login(string userid, string password, Action<long, DownloadHandler> callback)
        {
            Requests.Post(WebConstants.ApiUrl + "amonguslogin", new Dictionary<string, string>() { { "userid", userid }, { "password", password } }.GetString(), callback);
        }
        public static void CheckToken(string token, Action<long, DownloadHandler> callback)
        {
            Requests.Post(WebConstants.ApiUrl + "checktoken", new Dictionary<string, string>() { { "token", token } }.GetString(), callback);
        }
        public static void GetMyAccountByTokenFirst(string token, Action<long, DownloadHandler> callback)
        {
            Requests.Post(WebConstants.ApiUrl + "getmyaccountbytokenfirst", GetString(new() { { "token", token } }), callback);
        }
        public static void SendGameHistory(Dictionary<string, string> data, Action<long, DownloadHandler> callback)
        {
            if (callback == null) callback = (a, b) => { };
            Requests.Post(WebConstants.ApiUrl + "sendgamehistory", GetString(data), callback);
        }
        public static void GetWebPlayerData(string FriendCode, Action<long, DownloadHandler> callback)
        {
            string[] FriendCodes = FriendCode.Split("#");
            if (FriendCodes.Length <= 1)
            {
                Logger.Info("フレコが短い");
                return;
            }
            Requests.Post(WebConstants.CApiUrl + "getwebplayerdata", GetString(new() { { "FriendCode", FriendCodes[0] }, { "FriendCodeNum", FriendCodes[1] } }), callback);
        }
        public static void BRStartgame(Dictionary<string, string> data, Action<long, DownloadHandler> callback)
        {
            data.Add("token", WebAccountManager.Token);
            Requests.Post(WebConstants.ApiUrl + "battleroyal/startgame", GetString(data), callback);
        }
        public static void BREndgame(Dictionary<string, string> data, Action<long, DownloadHandler> callback)
        {
            data.Add("token", WebAccountManager.Token);
            Requests.Post(WebConstants.ApiUrl + "battleroyal/gameend", GetString(data), callback);
        }
    }
}