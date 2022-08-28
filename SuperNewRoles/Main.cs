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
    [BepInPlugin(Id, "SuperNewRoles", VersionString)]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Among Us.exe")]
    public class SuperNewRolesPlugin : BasePlugin
    {
        public const string Id = "jp.ykundesu.supernewroles";

        //バージョンと同時にIsBetaも変える
        public const string VersionString = "1.4.2.1";
        public static bool IsBeta
        {
            get
            {
                return ThisAssembly.Git.Branch != "master";
            }
        }

        public static Version Version = Version.Parse(VersionString);
        public static BepInEx.Logging.ManualLogSource Logger;
        public static Sprite ModStamp;
        public static int optionsPage = 1;
        public Harmony Harmony { get; } = new Harmony(Id);
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
            CustomOption.CustomOptions.Load();
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

            Logger.LogInfo(ModTranslation.GetString("\n---------------\nSuperNewRoles\n" + ModTranslation.GetString("StartLogText") + "\n---------------"));

            var assembly = Assembly.GetExecutingAssembly();

            StringDATE = new Dictionary<string, Dictionary<int, string>>();
            Harmony.PatchAll();
            SubmergedCompatibility.Initialize();

            assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.EndsWith(".png"))
                {
                    ModHelpers.LoadSpriteFromResources(resourceName, 115f);
                }
            }
        }
        /*
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
        class TranslateControllerMessagePatch
        {
            static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
            {
                SuperNewRolesPlugin.Logger.LogInfo(id+":"+__result);
            }
        }*/
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
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
                    }, 0f, "AntiChatBag");
                }
                if (__instance.IsOpen)
                {
                    __instance.BanButton.MenuButton.enabled = !__instance.animating;
                }
            }
        }
    }
}