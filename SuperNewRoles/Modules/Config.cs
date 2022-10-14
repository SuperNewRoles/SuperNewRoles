using System.IO;
using BepInEx.Configuration;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Modules
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
        public static ConfigEntry<bool> DownloadSuperNewNamePlates { get; set; }
        public static ConfigEntry<bool> DownloadOtherSkins { get; set; }
        public static ConfigEntry<bool> IsUpdate { get; set; }
        public static ConfigEntry<bool> IsDeleted { get; set; }
        public static bool IsUpdated = false;
        public static void Load()
        {
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
            DownloadSuperNewNamePlates = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "DownloadSuperNewNamePlates", true);
            Ip = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "Custom Server Port", (ushort)22023);
            IsUpdate = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsUpdate", true);
            IsDeleted = SuperNewRolesPlugin.Instance.Config.Bind("Custom", "IsDeleted", false);
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
            ShouldAlwaysHorseAround.isHorseMode = EnableHorseMode.Value;
            RegionMenuOpenPatch.defaultRegions = ServerManager.DefaultRegions;
            RegionMenuOpenPatch.UpdateRegions();
        }
    }
}