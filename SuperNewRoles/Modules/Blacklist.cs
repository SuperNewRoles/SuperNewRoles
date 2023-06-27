using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using InnerNet;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;

public static class Blacklist
{
    public class BlackPlayer
    {
        public static List<BlackPlayer> Players = new();
        public string FriendCode;
        public string Reason = "None";
        public string clientId = "";
        public BlackPlayer()
        {
            Players.Add(this);
        }
    }
    /// <summary>
    /// 起動時などで予め取得しておく
    /// </summary>
    /// <returns></returns>
    public static IEnumerator FetchConfig()
    {
        // config.json を GoogleDriveなどに上げる
        var request = UnityWebRequest.Get("https://raw.githubusercontent.com/ykundesu/AmongUs_Blacklist/main/Blacklist.json");
        yield return request.SendWebRequest();
        SuperNewRolesPlugin.Logger.LogInfo("前");
        if (request.isNetworkError || request.isHttpError)
        {
            yield break;
        }
        SuperNewRolesPlugin.Logger.LogInfo("通貨");
        var json = JObject.Parse(request.downloadHandler.text);
        for (var user = json["blockedUsers"].First; user != null; user = user.Next)
        {
            BlackPlayer player = new()
            {
                FriendCode = user["FriendCode"]?.ToString(),
                Reason = user["Reason"]?.ToString(),
                clientId = user["clientId"]?.ToString()
            };
            SuperNewRolesPlugin.Logger.LogInfo(player.FriendCode);
        }
    }
    public static IEnumerator Check(int clientId)
    {
        ClientData clientData;
        do
        {
            yield return null;
            clientData = AmongUsClient.Instance
                                    .allClients
                                    .ToArray()
                                    .FirstOrDefault(client => client.Id == clientId);
            SuperNewRolesPlugin.Logger.LogInfo(clientData);
        } while (clientData == null);
        SuperNewRolesPlugin.Logger.LogInfo(clientData.FriendCode);
        SuperNewRolesPlugin.Logger.LogInfo("回数:" + BlackPlayer.Players.Count);
        foreach (var player in BlackPlayer.Players)
        {
            SuperNewRolesPlugin.Logger.LogInfo(player.FriendCode + " : " + player.FriendCode);
            if (player.FriendCode == clientData.FriendCode || player.clientId == clientId.ToString())
            {
                if (PlayerControl.LocalPlayer.PlayerId == clientData.Character.PlayerId)
                {
                    Application.Quit();
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(clientData.Id, ban: true);
                }
            }
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        __instance.StartCoroutine(Blacklist.Check(__instance.ClientId).WrapToIl2Cpp());
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance,
                                [HarmonyArgument(0)] ClientData client)
    {
        __instance.StartCoroutine(Blacklist.Check(client.Id).WrapToIl2Cpp());
    }
}