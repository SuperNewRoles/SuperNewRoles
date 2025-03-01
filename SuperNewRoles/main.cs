using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AmongUs.Data;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;
using BepInEx.Logging;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomOptions;
using UnityEngine.EventSystems;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.HelpMenus;

namespace SuperNewRoles;

[BepInAutoPlugin(PluginConfig.Id, PluginConfig.Name)]
[BepInProcess(PluginConfig.ProcessName)]
public partial class SuperNewRolesPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(PluginConfig.Id);
    public static SuperNewRolesPlugin Instance;
    public static ManualLogSource Logger { get; private set; }

    public static bool IsEpic => Constants.GetPurchasingPlatformType() == PlatformConfig.EpicGamesStoreName;

    public override void Load()
    {
        Logger = Log;
        Instance = this;
        RegisterCustomObjects();
        Task task = Task.Run(() => Harmony.PatchAll());
        if (!Directory.Exists("./SuperNewRolesNext"))
        {
            Directory.CreateDirectory("./SuperNewRolesNext");
        }
        CustomRoleManager.Load();
        AssetManager.Load();
        ModTranslation.Load();
        CustomRPCManager.Load();
        CustomOptionManager.Load();
        SyncVersion.Load();
        EventListenerManager.Load();
        task.Wait();
        Logger.LogInfo("SuperNewRoles loaded");
        Logger.LogInfo("--------------------------------");
        Logger.LogInfo(ModTranslation.GetString("WelcomeNextSuperNewRoles"));
        Logger.LogInfo("--------------------------------");
    }
    private static void RegisterCustomObjects()
    {
        var rightClickDetectorOptions = new RegisterTypeOptions { Interfaces = new[] { typeof(IPointerClickHandler) } };
        ClassInjector.RegisterTypeInIl2Cpp<RightClickDetector>();
        ClassInjector.RegisterTypeInIl2Cpp<FadeCoroutine>();
    }


    // https://github.com/yukieiji/ExtremeRoles/blob/master/ExtremeRoles/Patches/Manager/AuthManagerPatch.cs
    [HarmonyPatch(typeof(AuthManager), nameof(AuthManager.CoConnect))]
    public static class AuthManagerCoConnectPatch
    {
        public static bool Prefix(AuthManager __instance)
        {
            if (!ModHelpers.IsCustomServer() ||
                FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.Servers.Any(x => x.UseDtls))
                return true;
            if (__instance.connection != null)
                __instance.connection.Dispose();
            __instance.connection = null;
            return false;
        }
    }
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
    class GetBroadcastVersionPatch
    {
        public static void Postfix(ref int __result)
        {
            if (AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame or NetworkModes.FreePlay) return;
            __result += 25;
        }
    }
    [HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
    public static class ConstantsVersionModdedPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
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

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class PlayerCountChange
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }
}