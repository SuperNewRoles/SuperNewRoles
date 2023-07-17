using System.Collections.Generic;
using System;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Linq;

namespace SuperNewRoles.Modules;

public static class MatchMaker
{
    public static string BaseURL = "https://supermatchmaker.vercel.app/";
    public static Dictionary<string, string> CreateBaseData()
    {
        var data = new Dictionary<string, string>();
        data["friendcode"] = PlayerControl.LocalPlayer.Data.FriendCode;
        data["roomid"] = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        return data;
    }
    public static void EndInviting()
    {
        var data = CreateBaseData();
        data["type"] = "endinvite";
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void KeepAlive()
    {
        var data = CreateBaseData();
        data["type"] = "keepalive";
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void UpdatePlayerCount(bool Is = false)
    {
        var data = CreateBaseData();
        data["type"] = "updateplayer";
        data["MaxPlayer"] = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers.ToString();
        data["NowPlayer"] = ((Is ? 1 : 0) + GameData.Instance.PlayerCount).ToString();
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void UpdateOption()
    {
        var data = CreateBaseData();
        data["type"] = "updateoption";
        string ActiveRole = "";
        List<string> ActivateRoles = new();
        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions)
        {
            if (opt.GetSelection() == 0) continue;
            if (opt.IsHidden()) continue;
            CustomOption countopt = CustomOption.options.FirstOrDefault(x => x.id == (opt.id + 1));
            for (int i = 0; i < (countopt.GetSelection() + 1); i++)
            {
                ActivateRoles.Add(opt.RoleId.ToString());
            }
        }
        string ActiveOptions = "";
        List<string> ActivateOptions = new();
        foreach (CustomOption option in CustomOption.options)
        {
            bool enabled = true;
            if (AmongUsClient.Instance?.AmHost == false && CustomOptionHolder.hideSettings.GetBool())
            {
                enabled = false;
            }

            if (option.IsHidden())
            {
                enabled = false;
            }
            CustomOption parent = option.parent;

            while (parent != null && enabled)
            {
                enabled = parent.Enabled;
                parent = parent.parent;
            }
            if (enabled)
            {
                ActivateOptions.Add(option.id + ":" + option.GetSelection());
            }
        }
        data["roles"] = string.Join(',', ActivateRoles);
        data["options"] = string.Join(',', ActivateOptions);
        data["mode"] = GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ? "HNS" : ModeHandler.GetMode(false).ToString();
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/update_state", data.GetString()).WrapToIl2Cpp());
    }
    public static void CreateRoom()
    {
        var data = CreateBaseData();
        string ActiveRole = "";
        List<string> ActivateRoles = new();
        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions)
        {
            if (opt.GetSelection() == 0) continue;
            if (opt.IsHidden()) continue;
            ActivateRoles.Add(opt.RoleId.ToString());
        }
        string ActiveOptions = "";
        List<string> ActivateOptions = new();
        foreach (CustomOption option in CustomOption.options)
        {
            bool enabled = true;
            if (AmongUsClient.Instance?.AmHost == false && CustomOptionHolder.hideSettings.GetBool())
            {
                enabled = false;
            }

            if (option.IsHidden())
            {
                enabled = false;
            }
            CustomOption parent = option.parent;

            while (parent != null && enabled)
            {
                enabled = parent.Enabled;
                parent = parent.parent;
            }
            if (enabled)
            {
                ActivateOptions.Add(option.id + ":" + option.GetSelection());
            }
        }
        data["Roles"] = string.Join(',', ActivateRoles);
        data["Options"] = string.Join(',', ActivateOptions);
        data["Mode"] = GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ? "HNS" : ModeHandler.GetMode(false).ToString();
        data["NowPlayer"] = GameData.Instance.PlayerCount.ToString();
        data["MaxPlayer"] = GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers.ToString();
        data["Version"] = SuperNewRolesPlugin.VersionString;
        string server = "NoneServer";
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        switch (n)
        {
            case StringNames.ServerAS:
                server = "0";
                break;
            case StringNames.ServerNA:
                server = "1";
                break;
            case StringNames.ServerEU:
                server = "2";
                break;
        }
        data["Server"] = server;
        AmongUsClient.Instance.StartCoroutine(Analytics.Post(BaseURL + "api/create_room", data.GetString()).WrapToIl2Cpp());
    }
}