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
    [BepInProcess("Among Us.exe")]
    public class SuperNewRolesPlugin : BasePlugin
    {
        public const string Id = "jp.ykundesu.supernewroles";

        public const string VersionString = "1.3.6.2";

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
            CustomCosmetics.DownLoadClass.Load();
            ConfigRoles.Load();
            CustomOption.CustomOptions.Load();
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
        }



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
            public static void Postfix()
            {
                    SaveManager.chatModeType = 1;
                    SaveManager.isGuest = false;

            }
        }
    }
}