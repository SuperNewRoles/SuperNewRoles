using System;
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
using SuperNewRoles.Patches;

namespace SuperNewRoles.Modules;
public static class Analytics
{
    public const string AnalyticsUrl = SNRURLs.AnalyticsURL;
    public const string SendDataUrl = "SendData";
    public const string SendClientDataUrl = "SendClientData";

    // Runtime metrics (ring buffers to minimize allocations and CPU)
    private static readonly float[] s_fpsBuffer = new float[1200]; // ~5分分 (0.25秒間隔)
    private static int s_fpsBufferCount = 0;
    private static int s_fpsBufferIndex = 0;
    private static float s_fpsAccumTime = 0f;
    private static int s_fpsAccumFrames = 0;

    private static readonly int[] s_pingBuffer = new int[120]; // ~60秒分 (0.5秒間隔)
    private static int s_pingBufferCount = 0;
    private static int s_pingBufferIndex = 0;
    private static float s_pingAccumTimer = 0f;
    private static DateTime? s_matchStartAtUtc;

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
    public static void SendAnalytics()
    {
        if (!ConfigRoles.IsSendAnalytics.Value) return;
        PostSendClientData();
        if (AmongUsClient.Instance.AmHost)
            PostSendData();
    }
    public static void PostSendClientData()
    {
        Dictionary<string, string> data = new();
        data.Add("FriendCode", ModHelpers.HashMD5(EOSManager.Instance.FriendCode));
        data.Add("Mode", Categories.ModeOption.ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", Statics.VersionString.ToString());
        NetworkedPlayerInfo Host = null;
        foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers) if (p.ClientId == AmongUsClient.Instance.HostId) Host = p;
        data.Add("HostFriendCode", ModHelpers.HashMD5(Host.FriendCode));
        data.Add("PlayerCount", GameData.Instance.AllPlayers.Count.ToString());
        data.Add("Platform", Application.platform.ToString());
        // Additional client metrics
        data.Add("AveragePing", GetAveragePing().ToString());
        data.Add("Fps", GetAverageFpsString());
        data.Add("Fps1PercentLow", GetFps1PercentLowString());
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
            PlayerDatas += $"{ModHelpers.HashMD5(player.FriendCode)},";
        }
        if (PlayerDatas.Length > 1)
        {
            PlayerDatas = PlayerDatas.Substring(0, PlayerDatas.Length - 1);
        }

        foreach (CustomOption opt in CustomOptionManager.CustomOptions)
        {
            if (opt.Selection == 0) continue;
            Options += $"{opt.Id}:{opt.Selection},";
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
        data.Add("FriendCode", ModHelpers.HashMD5(EOSManager.Instance.FriendCode));
        data.Add("Mode", Categories.ModeOption.ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", Statics.VersionString.ToString());
        data.Add("PlayerDatas", PlayerDatas);
        data.Add("Options", Options);
        data.Add("ActivateRoles", ActivateRole);
        data.Add("RealActivateRoles", RealActivateRole);
        data.Add("MapId", GameOptionsManager.Instance.CurrentGameOptions.MapId.ToString());
        data.Add("GameMode", GameOptionsManager.Instance.currentGameMode.ToString());
        data.Add("Platform", Application.platform.ToString());
        // Additional host-side metrics
        data.Add("AveragePing", GetAveragePing().ToString());
        data.Add("Fps", GetAverageFpsString());
        data.Add("Fps1PercentLow", GetFps1PercentLowString());
        data.Add("ServerNumber", GetServerNumberString());
        data.Add("WinningTeam", GetWinningTeamString());
        data.Add("MatchStartAt", GetMatchStartAtString());
        data.Add("IsHaison", EndGameManagerSetUpPatch.endGameCondition != null && EndGameManagerSetUpPatch.endGameCondition.IsHaison ? "1" : "0");
        string json = data.GetString();
        Logger.Info(json, "JSON");
        AmongUsClient.Instance.StartCoroutine(Post(AnalyticsUrl + SendDataUrl, json).WrapToIl2Cpp());
    }

    // Helpers for metrics formatting
    private static string GetAverageFpsString()
    {
        if (s_fpsBufferCount == 0) return "";
        float sum = 0f;
        for (int i = 0; i < s_fpsBufferCount; i++) sum += s_fpsBuffer[i];
        float avg = sum / s_fpsBufferCount;
        return (Mathf.Round(avg * 10f) / 10f).ToString();
    }
    private static string GetFps1PercentLowString()
    {
        if (s_fpsBufferCount == 0) return "";
        // copy and partial sort (small buffer)
        int n = s_fpsBufferCount;
        float[] tmp = new float[n];
        Array.Copy(s_fpsBuffer, tmp, n);
        Array.Sort(tmp);
        int k = Mathf.Clamp(Mathf.FloorToInt(n * 0.01f), 1, n);
        double sum = 0;
        for (int i = 0; i < k; i++) sum += tmp[i];
        float avg = (float)(sum / k);
        return (Mathf.Round(avg * 10f) / 10f).ToString();
    }
    private static int GetAveragePing()
    {
        if (s_pingBufferCount == 0) return AmongUsClient.Instance != null ? AmongUsClient.Instance.Ping : 0;
        long sum = 0;
        for (int i = 0; i < s_pingBufferCount; i++) sum += s_pingBuffer[i];
        return Mathf.RoundToInt((float)sum / s_pingBufferCount);
    }
    private static string GetServerNumberString()
    {
        try
        {
            if (FastDestroyableSingleton<ServerManager>.Instance == null) return "";
            var sm = FastDestroyableSingleton<ServerManager>.Instance;
            if (sm.CurrentRegion == null || sm.CurrentUdpServer == null) return "";
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
                return "0";
            var server = sm.CurrentRegion;
            switch (server.TranslateName)
            {
                case StringNames.ServerNA:
                    return "1";
                case StringNames.ServerEU:
                    return "2";
                case StringNames.ServerAS:
                    return "3";
                case StringNames.ServerSA:
                    return "4";
                default:
                    if (server.Servers.Any(x => x.Ip == "cs.supernewroles.com"))
                        return "5";
                    return "6";
            }
        }
        catch { }
        return "";
    }
    private static string GetWinningTeamString()
    {
        try
        {
            if (EndGameManagerSetUpPatch.endGameCondition != null)
            {
                return EndGameManagerSetUpPatch.endGameCondition.UpperText;
            }
        }
        catch { }
        return "";
    }
    private static string GetMatchStartAtString()
    {
        try
        {
            return s_matchStartAtUtc.HasValue ? s_matchStartAtUtc.Value.ToString("o") : "";
        }
        catch { return ""; }
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
        if (request.responseCode >= 400)
        {
            var errorDetail = request.error ?? request.downloadHandler?.text;
            Logger.Error($"Analytics error: {request.responseCode} - {errorDetail}", "Analytics");
        }
    }

    // Patches to collect metrics
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    private static class HudManagerUpdateAnalyticsPatch
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.IsGameStarted) return;
            // Accumulate frames to reduce sampling frequency; every 0.25s record one FPS sample
            float dt = Time.deltaTime;
            if (dt <= 0f) dt = 0.0001f;
            s_fpsAccumTime += dt;
            s_fpsAccumFrames++;
            if (s_fpsAccumTime >= 0.25f)
            {
                float fps = s_fpsAccumFrames / s_fpsAccumTime;
                s_fpsBuffer[s_fpsBufferIndex] = fps;
                s_fpsBufferIndex = (s_fpsBufferIndex + 1) % s_fpsBuffer.Length;
                if (s_fpsBufferCount < s_fpsBuffer.Length) s_fpsBufferCount++;
                s_fpsAccumTime = 0f;
                s_fpsAccumFrames = 0;
            }

            // Ping sample (~2Hz)
            s_pingAccumTimer += Time.deltaTime;
            if (s_pingAccumTimer >= 0.5f)
            {
                s_pingAccumTimer = 0f;
                int ping = AmongUsClient.Instance.Ping;
                s_pingBuffer[s_pingBufferIndex] = ping;
                s_pingBufferIndex = (s_pingBufferIndex + 1) % s_pingBuffer.Length;
                if (s_pingBufferCount < s_pingBuffer.Length) s_pingBufferCount++;
            }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    private static class CoStartGameAnalyticsPatch
    {
        public static void Postfix()
        {
            Array.Clear(s_fpsBuffer, 0, s_fpsBuffer.Length);
            s_fpsBufferCount = 0;
            s_fpsBufferIndex = 0;
            s_fpsAccumTime = 0f;
            s_fpsAccumFrames = 0;

            Array.Clear(s_pingBuffer, 0, s_pingBuffer.Length);
            s_pingBufferCount = 0;
            s_pingBufferIndex = 0;
            s_pingAccumTimer = 0f;
            s_matchStartAtUtc = DateTime.UtcNow;
        }
    }
}
