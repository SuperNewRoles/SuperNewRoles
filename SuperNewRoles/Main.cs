global using SuperNewRoles.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using InnerNet;
using SuperNewRoles.CustomObject;
using SuperNewRoles.SuperNewRolesWeb;
using UnityEngine;

namespace SuperNewRoles;

[BepInAutoPlugin("jp.ykundesu.supernewroles", "SuperNewRoles")]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInProcess("Among Us.exe")]
public partial class SuperNewRolesPlugin : BasePlugin
{
    public static readonly string VersionString = $"{Assembly.GetExecutingAssembly().GetName().Version}";

    public const bool IsBeta = ThisAssembly.Git.Branch != MasterBranch && !IsHideText;

    public const bool IsSecretBranch = false; // プルリク時にtrueなら指摘してください
    public const bool IsHideText = false; // プルリク時にtrueなら指摘してください

    public const string ModUrl = "ykundesu/SuperNewRoles";
    public const string MasterBranch = "master";
    public static string ModName => IsApril() ? "SuperNakanzinoRoles" : "SuperNewRoles";
    public static string ColorModName => $"<color=#ffa500>Super</color><color=#ff0000>{(IsApril() ? "Nakanzino" : "New")}</color><color=#00ff00>Roles</color>";
    public const string DiscordServer = "https://discord.gg/Cqfwx82ynN";
    public const string Twitter1 = "https://twitter.com/SNRDevs";
    public const string Twitter2 = "https://twitter.com/SNROfficials";


    public static Version ThisVersion = System.Version.Parse($"{Assembly.GetExecutingAssembly().GetName().Version}");
    public static BepInEx.Logging.ManualLogSource Logger;
    public static Sprite ModStamp;
    public static int optionsPage = 1;
    public static int optionsMaxPage = 0;
    public Harmony Harmony { get; } = new Harmony("jp.ykundesu.supernewroles");
    public static SuperNewRolesPlugin Instance;
    public static Dictionary<string, Dictionary<int, string>> StringDATA;
    public static bool IsUpdate = false;
    public static string NewVersion = "";
    public static string thisname;
    public static string ThisPluginModName;

    public override void Load()
    {
        Logger = Log;
        Instance = this;
        // All Load() Start
        ConfigRoles.Load();
        WebAccountManager.Load();
        //WebAccountManager.SetToken("XvSwpZ8CsQgEksBg");
        ModTranslation.LoadCsv();
        ChacheManager.Load();
        WebConstants.Load();
        CustomCosmetics.CustomColors.Load();
        ModDownloader.Load();
        CustomOptionHolder.Load();
        AccountLoginMenu.Initialize();
        // All Load() End


        // Old Delete Start

        try
        {
            DirectoryInfo d = new(Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
            string[] files = d.GetFiles("*.dll.old").Select(x => x.FullName).ToArray(); // Getting old versions
            foreach (string f in files)
                File.Delete(f);
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Exception occured when clearing old versions:\n" + e);
        }

        // Old Delete End

        SuperNewRoles.Logger.Info(DateTime.Now.ToString("D"), "DateTime Now"); // 2022年11月24日
        SuperNewRoles.Logger.Info(ThisAssembly.Git.Branch, "Branch");
        SuperNewRoles.Logger.Info(ThisAssembly.Git.Commit, "Commit");
        SuperNewRoles.Logger.Info(ThisAssembly.Git.Commits, "Commits");
        SuperNewRoles.Logger.Info(ThisAssembly.Git.BaseTag, "BaseTag");
        SuperNewRoles.Logger.Info(ThisAssembly.Git.Tag, "Tag");
        SuperNewRoles.Logger.Info(VersionString, "VersionString");
        SuperNewRoles.Logger.Info(Version, nameof(Version));
        SuperNewRoles.Logger.Info($"{Application.version}({Constants.GetPurchasingPlatformType()})", "AmongUsVersion"); // アモングアス本体のバージョン(プレイしているプラットフォーム)
        try
        {
            var directoryPath = Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins";
            SuperNewRoles.Logger.Info($"DirectoryPathが半角のみ:{ModHelpers.IsOneByteOnlyString(directoryPath)}", "IsOneByteOnly path"); // フォルダパスが半角のみで構成されているか
            var di = new DirectoryInfo(directoryPath);
            var pluginFiles = di.GetFiles();
            foreach (var f in pluginFiles)
            {
                var name = f.Name;
                SuperNewRoles.Logger.Info($"---------- {name} -----------", "Data");
                SuperNewRoles.Logger.Info(name, nameof(pluginFiles)); // ファイル名
                SuperNewRoles.Logger.Info($"{f.Length}MB", name); // サイズをバイト単位で取得
            }
        }
        catch (Exception e)
        {
            SuperNewRoles.Logger.Error($"pluginFilesの取得時に例外発生{e.ToString()}", "pluginFiles");
        }

        Logger.LogInfo(ModTranslation.GetString("\n---------------\nSuperNewRoles\n" + ModTranslation.GetString("StartLogText") + "\n---------------"));

        var assembly = Assembly.GetExecutingAssembly();

        StringDATA = new Dictionary<string, Dictionary<int, string>>();
        Harmony.PatchAll();
        assembly = Assembly.GetExecutingAssembly();
        string[] resourceNames = assembly.GetManifestResourceNames();
        foreach (string resourceName in resourceNames)
            if (resourceName.EndsWith(".png"))
                ModHelpers.LoadSpriteFromResources(resourceName, 115f);
        ThisPluginModName = IL2CPPChainloader.Instance.Plugins.FirstOrDefault(x => x.Key == "jp.ykundesu.supernewroles").Value.Metadata.Name;

        //Register Il2cpp
        Logger.LogInfo("a");
        ClassInjector.RegisterTypeInIl2Cpp<CustomAnimation>();
        Logger.LogInfo("b");
        ClassInjector.RegisterTypeInIl2Cpp<SluggerDeadbody>();
        Logger.LogInfo("c");
        ClassInjector.RegisterTypeInIl2Cpp<WaveCannonObject>();
        Logger.LogInfo("d");
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    class GetBroadcastVersionPatch
    {
        static void Postfix(ref int __result)
        {
            if (AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame or NetworkModes.FreePlay) return;
            if (ModHelpers.IsCustomServer()) return;
            __result = Constants.GetVersion(2222, 0, 0, 0);
        }
    }

    public static bool IsApril()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime dateTime = new(utcNow.Year, 3, 31, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTime dateTime2 = dateTime.AddDays(1.0);
        return utcNow >= dateTime && utcNow <= dateTime2;
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result) => __result = false;
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ChatControllerAwakePatch
    {
        public static void Prefix()
        {
            DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
        }
        public static void Postfix(ChatController __instance)
        {
            DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!__instance.isActiveAndEnabled) return;
                __instance.Toggle();
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                __instance.SetVisible(false);
                new LateTask(() =>
                {
                    __instance.SetVisible(true);
                }, 0f, "AntiChatBug");
            }
            if (__instance.IsOpenOrOpening)
            {
                __instance.banButton.MenuButton.enabled = !__instance.IsAnimating;
            }
        }
    }
    public static void AgarthaLoad() => Agartha.AgarthaPlugin.Instance.Log.LogInfo("アガルタやで");
}