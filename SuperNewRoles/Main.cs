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

namespace SuperNewRoles
{
    [BepInPlugin(Id, "SuperNewRoles", VersionString)]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Among Us.exe")]
    public class SuperNewRolesPlugin : BasePlugin
    {
        public const string Id = "jp.ykundesu.supernewroles";

        public const string VersionString = "1.4.0.7";

        public static System.Version Version = System.Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public static Sprite ModStamp;
        public static int optionsPage = 1;
        public Harmony Harmony { get; } = new Harmony(Id);
        public static SuperNewRolesPlugin Instance;
        public static Dictionary<string, Dictionary<int, string>> StringDATE;
        public static bool IsUpdate = false;
        public static string NewVersion = "" ;
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
                DirectoryInfo d = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
                string[] files = d.GetFiles("*.dll.old").Select(x => x.FullName).ToArray(); // Getting old versions
                foreach (string f in files)
                    File.Delete(f);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception occured when clearing old versions:\n" + e);
            }

            // Old Delete End

            Logger.LogInfo(ModTranslation.getString("StartLogText"));

            var assembly = Assembly.GetExecutingAssembly();

            StringDATE = new Dictionary<string, Dictionary<int, string>>();
            Harmony.PatchAll();
            SubmergedCompatibility.Initialize();
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
                } else if (Input.GetKeyDown(KeyCode.F2)) {

                    __instance.SetVisible(false);
                    new LateTask(() =>
                    {
                        __instance.SetVisible(true);
                    }, 0f,"AntiChatBag");

                }
                if (__instance.IsOpen)
                {
                    if (__instance.animating)
                    {
                        __instance.BanButton.MenuButton.enabled = false;
                    }
                    else
                    {
                        __instance.BanButton.MenuButton.enabled = true;
                    }
                }
            }
        }
    }
}