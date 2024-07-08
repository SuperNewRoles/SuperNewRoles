using System.IO;
using BepInEx.Configuration;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Modules;

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
    public static ConfigEntry<bool> IsModCosmeticsAreNotLoaded { get; set; }
    public static ConfigEntry<bool> IsNotUsingBlood { get; set; }
    public static ConfigEntry<bool> DownloadOtherSkins { get; set; }
    public static ConfigEntry<bool> IsUpdate { get; set; }
    public static ConfigEntry<bool> IsDeleted { get; set; }
    public static ConfigEntry<bool> IsSendAnalytics { get; set; }
    public static ConfigEntry<bool> IsLightAndDarker { get; set; }
    public static ConfigEntry<bool> IsViewd20240618ServerInfo { get; set; }
    public static ConfigEntry<bool> IsMuteLobbyBGM { get; set; }
    public static ConfigEntry<bool> _isCPUProcessorAffinity { get; set; }
    public static ConfigEntry<ulong> _ProcessorAffinityMask { get; set; }
    //リプレイ
    public static ConfigEntry<bool> ReplayEnable { get; set; }
    public static ConfigEntry<float> ReplayQualityTime { get; set; }
    public static bool IsSendAnalyticsPopupViewd;
    public static bool IsUpdated = false;
    public static void Load()
    {
        var issendanaly = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsSendAnalyticsViewd", false);
        IsSendAnalyticsPopupViewd = issendanaly.Value;
        issendanaly.Value = true;
        CustomCosmetics.CustomCosmeticsMenus.Patch.ObjectData.SelectedPreset = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Selected Closet Preset", 0);
        StreamerMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Enable Streamer Mode", false);
        AutoUpdate = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Auto Update", true);
        DebugMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Debug Mode", false);
        AutoCopyGameCode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Auto Copy Game Code", true);
        CustomProcessDown = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "CustomProcessDown", false);
        IsVersionErrorView = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsVersionErrorView", true);
        HideTaskArrows = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "HideTaskArrows", false);
        ShareCosmeticsNamePlatesURL = SuperNewRolesPlugin.Instance.Config.Bind("ShareCosmetics", "NamePlateURL", "");
        IsAutoRoomCreate = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "AutoRoomCreate", true);
        EnableHorseMode = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "EnableHorseMode", false);
        IsModCosmeticsAreNotLoaded = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsModCosmeticsAreNotLoaded", false);
        IsNotUsingBlood = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsNotUsingBlood", false);
        IsSendAnalytics = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsSendAnalytics", true);
        IsLightAndDarker = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsLightAndDarker", true);
        Ip = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
        Port = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server Port", (ushort)22023);
        IsUpdate = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsUpdate", true);
        IsDeleted = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsDeleted", false);
        //リプレイ
        ReplayEnable = SuperNewRolesPlugin.Instance.Config.Bind("Replay", "Enable", false);
        ReplayQualityTime = SuperNewRolesPlugin.Instance.Config.Bind("Replay", "QualityTime", 0.5f);
        if (!IsDeleted.Value)
        {
            if (Directory.Exists(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\") && Directory.Exists(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\"))
            {
                DirectoryInfo di = new(Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\CustomHatsChache\");
                di.Delete(true);
            }
            IsDeleted.Value = true;
        }
        if (IsUpdate.Value)
        {
            SuperNewRolesPlugin.Logger.LogInfo("IsUpdateが有効でした");
            IsUpdated = true;
        }
        IsUpdate.Value = false;
        IsViewd20240618ServerInfo = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IIsViewd20240618ServerInfo", false);
        IsMuteLobbyBGM = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsMutedLobbyBGM", false);
        _isCPUProcessorAffinity = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "CPUProcessorAffinity", true);
        _ProcessorAffinityMask = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "ProcessorAffinityMask", (ulong)3);
        //ShouldAlwaysHorseAround.isHorseMode = EnableHorseMode.Value;
        RegionMenuOpenPatch.defaultRegions = ServerManager.DefaultRegions;
        RegionMenuOpenPatch.UpdateRegions();
    }
}