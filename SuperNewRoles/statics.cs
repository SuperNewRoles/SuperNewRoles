using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuperNewRoles;

public static class PluginConfig
{
    public const string Id = "jp.ykundesu.supernewrolesnext";
    public const string Name = "SuperNewRoles";
    public const string ProcessName = "Among Us.exe";
}

public static class VersionInfo
{
    private static Version _version;
    public static Version Current => _version ??= SuperNewRolesPlugin.Assembly.GetName().Version;
    public static string VersionString => Current.ToString() + SnapShotVersion.ToString();

    public static bool IsSnapShot => SnapShotVersion != null;
    public static char? SnapShotVersion = 'i';

    public static string NewVersion = "";
    public static bool IsUpdate = false;
    public static readonly string[] SupportedVanillaVersions = new[] { "2024.3.5" };
}

public static class SNRURLs
{
    public const string ReportInGameAgreement = "https://wiki.supernewroles.com/reporting-in-game-terms";
    public const string AnalyticsURL = "https://analytics.supernewroles.com/";
    public const string SNRCS = "https://cs.supernewroles.com";
    public const string ReportInGameAPI = "https://reports-api.supernewroles.com";
    public const string UpdateURL = "https://update.supernewroles.com/";
    public const string GithubAPITags = "https://api.github.com/repos/supernewroles/SuperNewRoles/releases/tags";
    public const string JoinRoomHost = "joinroom.supernewroles.com";
}
public static class BranchConfig
{
    public const string MasterBranch = "master";
    public const bool IsSecretBranch = false;
    public const bool IsHideText = false;
    public static bool IsBeta => ThisAssembly.Git.Branch != MasterBranch && !IsHideText;
}

public static class PlatformConfig
{
    public const string SteamName = "steam";
    public const string EpicGamesStoreName = "egs";
}

public static class UIConfig
{
    public static int OptionsPage = 1;
    public static int OptionsMaxPage = 0;
    public static string ColorModName = $"<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
    public static Sprite ModStamp;
}

public static class SocialLinks
{
    public const string DiscordServer = "https://supernewroles.com/discord";
    public const string TwitterSnrDevs = "https://twitter.com/SNRDevs";
    public const string TwitterSnrOfficials = "https://twitter.com/SNROfficials";
}

public static class Statics
{
    // MOD情報
    public const string ModUrl = "SuperNewRoles/SuperNewRoles";
    public static string ModName => PluginConfig.Name;

    // バージョン情報
    public static Version Version => VersionInfo.Current;
    public static readonly string VersionString = VersionInfo.VersionString;
    public static string NewVersion = "";
    public static bool IsUpdate = false;

    // ブランチ設定
    public const string MasterBranch = "master";
    public static bool IsBeta = BranchConfig.IsBeta;

    // 開発設定
    /// <summary>シークレットブランチフラグ - PRでtrueの場合は要確認</summary>
    public const bool IsSecretBranch = BranchConfig.IsSecretBranch;
    /// <summary>テキスト非表示フラグ - PRでtrueの場合は要確認</summary>
    public const bool IsHideText = BranchConfig.IsHideText;

    // アセンブリ
    private static Assembly _assembly = null;
    public static Assembly Assembly => _assembly ??= SuperNewRolesPlugin.Assembly;


    // プラットフォーム設定
    public const string SteamName = "steam";
    public const string EpicGamesStoreName = "egs";

    // バージョン互換性
    public static readonly string[] SupportVanillaVersion = VersionInfo.SupportedVanillaVersions;
}
