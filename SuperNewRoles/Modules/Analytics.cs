using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;
public static class Analytics
{
    public const string AnalyticsUrl = SNRURLs.AnalyticsURL;
    public const string SendDataUrl = "SendData";
    public const string SendClientDataUrl = "SendClientData";

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public class MainMenuManagerLateUpdatePatch
    {
        private static GameObject currentPopup;
        private static bool isAnalyticsPopupViewd = false;
        public static void Postfix(MainMenuManager __instance)
        {
            if (!FastDestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
                return;
            if (currentPopup == null)
                currentPopup = null;
            if (!isAnalyticsPopupViewd && ConfigRoles.IsSendAnalyticsPopupViewd.Value)
                isAnalyticsPopupViewd = true;
            if (currentPopup == null && !isAnalyticsPopupViewd)
            {
                GameObject Popup = AssetManager.Instantiate("AnalyticsBG", Camera.main.transform);
                Popup.gameObject.SetActive(true);
                Popup.transform.Find("Title").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AnalyticsPopupTitle");
                Popup.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AnalyticsPopupText");
                Popup.transform.localScale = Vector3.one * 0.58f;
                GameObject AnalyticsButton = Popup.transform.Find("AnalyticsButton").gameObject;
                AnalyticsButton.transform.Find("Text").GetComponent<TextMeshPro>().text = ModTranslation.GetString("AnalyticsOK");
                PassiveButton passiveButton = AnalyticsButton.AddComponent<PassiveButton>();
                passiveButton.Colliders = new Collider2D[] { AnalyticsButton.GetComponent<Collider2D>() };
                passiveButton.OnClick = new();
                passiveButton.OnClick.AddListener((UnityAction)(() =>
                {
                    GameObject.Destroy(Popup);
                    ConfigRoles.IsSendAnalyticsPopupViewd.Value = true;
                    isAnalyticsPopupViewd = true;
                    ConfigRoles.IsSendAnalytics.Value = true;
                }));
                passiveButton.OnMouseOver = new();
                passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
                {
                    AnalyticsButton.transform.Find("Selected").gameObject.SetActive(true);
                }));
                passiveButton.OnMouseOut = new();
                passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
                {
                    AnalyticsButton.transform.Find("Selected").gameObject.SetActive(false);
                }));
                currentPopup = Popup;
                return;
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoEndGame))]
    public class AmongUsClientCoEndGamePatch
    {
        public static void Postfix()
        {
            if (ConfigRoles.IsSendAnalytics.Value) return;
            PostSendClientData();
            if (AmongUsClient.Instance.AmHost)
                PostSendData();
        }
    }
    public static void PostSendClientData()
    {
        Dictionary<string, string> data = new();
        data.Add("FriendCode", PlayerControl.LocalPlayer.Data.FriendCode);
        data.Add("Mode", Categories.ModeSettings.ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", Statics.VersionString.ToString());
        NetworkedPlayerInfo Host = null;
        foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers) if (p.PlayerId == 0) Host = p;
        data.Add("HostFriendCode", Host.FriendCode);
        data.Add("PlayerCount", GameData.Instance.AllPlayers.Count.ToString());
        string json = data.GetString();
        AmongUsClient.Instance.StartCoroutine(Post(AnalyticsUrl + SendClientDataUrl, json).WrapToIl2Cpp());
    }
    public static void PostSendData()
    {
        string PlayerDatas = "";
        string Options = "";
        string ActivateRole = "";
        string RealActivateRole = "";
        List<RoleId> RealActivateRoleList = new();
        foreach (NetworkedPlayerInfo player in GameData.Instance is null ? new() : GameData.Instance.AllPlayers)
        {
            if (player.PlayerId == PlayerControl.LocalPlayer.Data.PlayerId) continue;
            PlayerDatas += $"{player.FriendCode},";
        }
        if (PlayerDatas.Length > 1)
        {
            PlayerDatas = PlayerDatas.Substring(0, PlayerDatas.Length - 1);
        }

        if (Options.Length >= 1)
        {
            Options = Options.Substring(0, Options.Length - 1);
        }

        foreach (RoleOptionManager.RoleOption opt in RoleOptionManager.RoleOptions)
        {
            if (opt.NumberOfCrews == 0 || opt.Percentage == 0) continue;
            ActivateRole += $"{opt.RoleId},";
        }
        if (ActivateRole.Length >= 1)
        {
            ActivateRole = ActivateRole.Substring(0, ActivateRole.Length - 1);
        }
        foreach (ExPlayerControl p in ExPlayerControl.ExPlayerControls)
        {
            if (RealActivateRoleList.Contains(p.Role)) continue;
            RealActivateRoleList.Add(p.Role);
        }
        foreach (RoleId role in RealActivateRoleList)
        {
            RealActivateRole += role + ",";
        }
        if (RealActivateRole.Length >= 1)
        {
            RealActivateRole = RealActivateRole.Substring(0, RealActivateRole.Length - 1);
        }

        Dictionary<string, string> data = new();
        data.Add("FriendCode", PlayerControl.LocalPlayer.Data.FriendCode);
        data.Add("Mode", Categories.ModeSettings.ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", Statics.VersionString.ToString());
        data.Add("PlayerDatas", PlayerDatas);
        data.Add("Options", Options);
        data.Add("ActivateRoles", ActivateRole);
        data.Add("RealActivateRoles", RealActivateRole);
        data.Add("MapId", GameOptionsManager.Instance.CurrentGameOptions.MapId.ToString());
        data.Add("GameMode", GameOptionsManager.Instance.currentGameMode.ToString());
        string json = data.GetString();
        Logger.Info(json, "JSON");
        AmongUsClient.Instance.StartCoroutine(Post(AnalyticsUrl + SendDataUrl, json).WrapToIl2Cpp());
    }
    public static string GetString(this IList<string> list)
    {
        string txt = "[]";
        foreach (string text in list)
        {
            txt += text + ",";
        }
        return txt.Substring(0, txt.Length - 1) + "]";
    }
    public static string GetString(this IDictionary<string, string> dict)
    {
        var items = dict.Select(kvp => $"{kvp.Key}={Il2CppSystem.Web.HttpUtility.UrlEncode(kvp.Value)}");
        string txt = "";
        foreach (string text in items)
        {
            txt += text + "&";
        }
        return txt.Substring(0, txt.Length - 1);
    }
    public static IEnumerator Post(string url, string jsonstr)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonstr);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.Send();

        Logger.Info($"Status Code: {request.responseCode}", "Analytics");
        //Logger.Info($"Result:{request.downloadHandler.text}");
    }
}