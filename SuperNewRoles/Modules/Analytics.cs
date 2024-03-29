using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Mode;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperNewRoles.Modules;
public static class Analytics
{
    public const string AnalyticsUrl = "https://snranaly-1-z3859836.deta.app/";
    public const string SendDataUrl = "SendData";
    public const string SendClientDataUrl = "SendClientData";

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public class MainMenuManagerLateUpdatePatch
    {
        private static GenericPopup currentPopup;
        public static void Postfix(MainMenuManager __instance)
        {
            if (!FastDestroyableSingleton<EOSManager>.Instance.HasFinishedLoginFlow())
                return;
            if (currentPopup == null)
                currentPopup = null;
            if (!ConfigRoles.IsSendAnalyticsPopupViewd)
            {
                ConfigRoles.IsSendAnalyticsPopupViewd = true;
                GenericPopup Popup = GameObject.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
                Popup.gameObject.SetActive(true);
                Popup.transform.FindChild("Background").localScale = new(2, 2.8f, 1);
                Popup.transform.FindChild("ExitGame").localPosition = new(0f, -2f, -0.5f);
                Popup.transform.FindChild("ExitGame").GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("AnalyticsOK");
                TextMeshPro Title = GameObject.Instantiate(Popup.TextAreaTMP, Popup.transform);
                Title.text = ModTranslation.GetString("Analytics");
                Title.transform.localPosition = new(0.15f, 2, -0.5f);
                Title.transform.localScale = Vector3.one * 4.5f;
                Popup.TextAreaTMP.transform.localPosition = new(0, 0.8f, -0.5f);
                Popup.TextAreaTMP.transform.localScale = Vector3.one * 1.4f;
                Popup.TextAreaTMP.text = ModTranslation.GetString("AnalyticsText");
                Popup.destroyOnClose = true;
                currentPopup = Popup;
                return;
            }
            if (currentPopup == null && !ConfigRoles.IsViewdApril2024Popup.Value && AprilFoolsManager.IsApril(2024))
            {
                ConfigRoles.IsViewdApril2024Popup.Value = true;
                GenericPopup Popup = GameObject.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
                Popup.gameObject.SetActive(true);
                Popup.transform.FindChild("Background").localScale = new(2, 2f, 1);
                Popup.transform.FindChild("ExitGame").localPosition = new(0f, -1.5f, -0.5f);
                GameObject.Destroy(Popup.transform.FindChild("ExitGame").GetComponentInChildren<TextTranslatorTMP>());
                Popup.transform.FindChild("ExitGame").GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString("AprilFool2024HackedByEvilHackerPopupOK");
                Popup.transform.FindChild("ExitGame").GetComponentInChildren<TextMeshPro>().transform.localPosition = new(0.04f,0,0);
                TextMeshPro Title = GameObject.Instantiate(Popup.TextAreaTMP, Popup.transform);
                Title.text = ModTranslation.GetString("AprilFool2024HackedByEvilHackerPopupTitle");
                Title.transform.localPosition = new(0.07f, 1.285f, -0.5f);
                Title.transform.localScale = Vector3.one * 2.8f;
                Popup.TextAreaTMP.transform.localPosition = new(0.05f, -0.05f, -0.5f);
                Popup.TextAreaTMP.transform.localScale = Vector3.one * 1.5f;
                Popup.TextAreaTMP.text = ModTranslation.GetString("AprilFool2024HackedByEvilHackerPopupText");
                Popup.destroyOnClose = true;
                currentPopup = Popup;
                if (!AprilFoolsManager.isLastAprilFool)
                {
                    AprilFoolsManager._enums = null;
                    AprilFoolsManager.SetRandomModMode();
                }
            }
        }
    }
    public static void PostSendClientData()
    {
        Dictionary<string, string> data = new();
        data.Add("FriendCode", PlayerControl.LocalPlayer.Data.FriendCode);
        data.Add("Mode", ModeHandler.GetMode().ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", SuperNewRolesPlugin.ThisVersion.ToString());
        GameData.PlayerInfo Host = null;
        foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers) if (p.PlayerId == 0) Host = p;
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
        foreach (GameData.PlayerInfo player in GameData.Instance is null ? new() : GameData.Instance.AllPlayers)
        {
            if (player.PlayerId == PlayerControl.LocalPlayer.Data.PlayerId) continue;
            PlayerDatas += $"{player.FriendCode},";
        }
        if (PlayerDatas.Length > 1)
        {
            PlayerDatas = PlayerDatas.Substring(0, PlayerDatas.Length - 1);
        }

        foreach (CustomOption opt in CustomOption.options)
        {
            if (opt.GetSelection() == 0) continue;
            Options += $"{opt.id}:{opt.GetSelection()},";
        }
        if (Options.Length >= 1)
        {
            Options = Options.Substring(0, Options.Length - 1);
        }

        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions.Values)
        {
            if (opt.GetSelection() == 0) continue;
            ActivateRole += $"{opt.RoleId},";
        }
        if (ActivateRole.Length >= 1)
        {
            ActivateRole = ActivateRole.Substring(0, ActivateRole.Length - 1);
        }
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (RealActivateRoleList.Contains(p.GetRole())) continue;
            RealActivateRoleList.Add(p.GetRole());
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
        data.Add("Mode", ModeHandler.GetMode().ToString());
        data.Add("GameId", AmongUsClient.Instance.GameId.ToString());
        data.Add("Version", SuperNewRolesPlugin.ThisVersion.ToString());
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