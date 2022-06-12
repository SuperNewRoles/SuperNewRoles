using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using SuperNewRoles.Patches;
namespace SuperNewRoles
{
    public static class ConfigRoles
    {
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }
        public static ConfigEntry<bool> StreamerMode { get; set; }
        public static ConfigEntry<bool> AutoUpdate { get; set; }
        public static ConfigEntry<bool> AutoCopyGameCode { get; set; }
        public static ConfigEntry<bool> DebugMode { get; set; }
        public static ConfigEntry<bool> CustomProcessDown { get; set; }
        public static ConfigEntry<bool> IsVersionErrorView { get; set; }
        public static ConfigEntry<bool> IsShareCosmetics { get; set; }
        public static ConfigEntry<string> ShareCosmeticsNamePlatesURL { get; set; }
        public static ConfigEntry<bool> IsAutoRoomCreate { get; set; }
        public static ConfigEntry<bool> HideTaskArrows { get; set; }
        public static ConfigEntry<bool> EnableHorseMode { get; set; }
        public static void Load()
        {
            StreamerMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Enable Streamer Mode", false);
            AutoUpdate = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Auto Update", true);
            DebugMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Debug Mode", false);
            AutoCopyGameCode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Auto Copy Game Code", true);
            CustomProcessDown = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "CustomProcessDown", false);
            IsVersionErrorView = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsVersionErrorView", true);
            HideTaskArrows = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "HideTaskArrows", false);
            ShareCosmeticsNamePlatesURL = SuperNewRolesPlugin.Instance.Config.Bind("ShareCosmetics", "NamePlateURL", "");
            IsAutoRoomCreate = SuperNewRolesPlugin.Instance.Config.Bind("Custom","AutoRoomCreate",true); ;
            EnableHorseMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "EnableHorseMode", false);
            Ip = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server Port", (ushort)22023);
            IntroPatch.ShouldAlwaysHorseAround.isHorseMode = ConfigRoles.EnableHorseMode.Value;
            Patch.RegionMenuOpenPatch.defaultRegions = ServerManager.DefaultRegions;
            Patch.RegionMenuOpenPatch.UpdateRegions();
        }
    }
}