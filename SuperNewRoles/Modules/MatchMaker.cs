using System.Collections.Generic;
using System;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Modules;

class MatchMaker
{
    public static string BaseURL = "https://supermatchmakers-1-t2750592.deta.app/";
    public static void CreateRoom(){
        var data = new Dictionary<string, string>();
        data["friendcode"] = PlayerControl.LocalPlayer.Data.FriendCode
        data["roomid"] = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
        string ActiveRole = "";
        List<string> ActivateRoles = new();
        foreach (CustomRoleOption opt in CustomRoleOption.RoleOptions)
        {
            if (opt.GetSelection() == 0) continue;
            ActivateRoles.Add(opt.RoleId);
        }
        string ActiveOptions = "";
        Dictionary<string, string> ActivateOptions = new();
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
            if (enabled){
                ActivateOptions.Add(option.id, option.GetSelection());
            }
        }
        data["Roles"] = string.Join(ActivateRoles, ",");
        data["Options"] = string.Join(ActivateOptions, "")
        Analytics.GetString()
    }
}