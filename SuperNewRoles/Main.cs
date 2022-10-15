global using SuperNewRoles.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles
{
    [BepInAutoPlugin("jp.ykundesu.supernewroles","SuperNewRoles")]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Among Us.exe")]
    public partial class SuperNewRolesPlugin : BasePlugin
    {
        public static readonly string VersionString = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        public static bool IsBeta = IsViewText && ThisAssembly.Git.Branch != MasterBranch;

        //プルリク時にfalseなら指摘してください
        public const bool IsViewText = true;

        public const string ModUrl = "ykundesu/SuperNewRoles";
        public const string MasterBranch = "master";
        public const string ModName = "SuperNewRoles";
        public const string ColorModName = "<color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";
        public const string DiscordServer = "https://discord.gg/hXbDgQzSuK";
        public const string Twitter1 = "https://twitter.com/SNRDevs";
        public const string Twitter2 = "https://twitter.com/SuperNewRoles";


        public static Version ThisVersion = System.Version.Parse($"{Assembly.GetExecutingAssembly().GetName().Version}");
        public static BepInEx.Logging.ManualLogSource Logger;
        public static Sprite ModStamp;
        public static int optionsPage = 1;
        public Harmony Harmony { get; } = new Harmony("jp.ykundesu.supernewroles");
        public static SuperNewRolesPlugin Instance;
        public static Dictionary<string, Dictionary<int, string>> StringDATE;
        public static bool IsUpdate = false;
        public static string NewVersion = "";
        public static string thisname;

        public override void Load()
        {
            Logger = Log;
            Instance = this;
            // All Load() Start
            ModTranslation.Load();
            ChacheManager.Load();
            CustomCosmetics.CustomColors.Load();
            ConfigRoles.Load();
            CustomOptions.Load();
            Patches.FreeNamePatch.Initialize();
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

            SuperNewRoles.Logger.Info(ThisAssembly.Git.Branch, "Branch");
            SuperNewRoles.Logger.Info(ThisAssembly.Git.Commit, "Commit");
            SuperNewRoles.Logger.Info(ThisAssembly.Git.Commits, "Commits");
            SuperNewRoles.Logger.Info(ThisAssembly.Git.BaseTag, "BaseTag");
            SuperNewRoles.Logger.Info(ThisAssembly.Git.Tag, "Tag");
            SuperNewRoles.Logger.Info(VersionString, "VersionString");
            SuperNewRoles.Logger.Info(Version, nameof(Version));
            SuperNewRoles.Logger.Info(Application.version, "AmongUsVersion"); // アモングアス本体のバージョン

            Logger.LogInfo(ModTranslation.GetString("\n---------------\nSuperNewRoles\n" + ModTranslation.GetString("StartLogText") + "\n---------------"));

            var assembly = Assembly.GetExecutingAssembly();

            StringDATE = new Dictionary<string, Dictionary<int, string>>();
            Harmony.PatchAll();
            SubmergedCompatibility.Initialize();

            assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
                if (resourceName.EndsWith(".png"))
                    ModHelpers.LoadSpriteFromResources(resourceName, 115f);
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
                SaveManager.chatModeType = 1;
                SaveManager.isGuest = false;
            }
            public static void Postfix(ChatController __instance)
            {
                SaveManager.chatModeType = 1;
                SaveManager.isGuest = false;

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
                if (__instance.IsOpen)
                {
                    __instance.BanButton.MenuButton.enabled = !__instance.animating;
                }
            }
        }
        public static void AgarthaLoad() => Agartha.AgarthaPlugin.Instance.Log.LogInfo("アガルタやで");
    }
}