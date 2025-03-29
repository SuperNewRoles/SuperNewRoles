using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
using SuperNewRoles.MapCustoms;
using SuperNewRoles.SuperTrophies;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.CustomObject;
using SuperNewRoles.API;

namespace SuperNewRoles;

[BepInAutoPlugin(PluginConfig.Id, PluginConfig.Name)]
[BepInProcess(PluginConfig.ProcessName)]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInIncompatibility("com.emptybottle.townofhost")]
// [BepInIncompatibility("jp.ykundesu.agartha")]
public partial class SuperNewRolesPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(PluginConfig.Id);
    public static SuperNewRolesPlugin Instance;
    public static ManualLogSource Logger { get; private set; }

    public static int MainThreadId { get; private set; }
    private readonly List<Action> _mainThreadActions = new();
    private readonly object _mainThreadActionsLock = new();

    public static bool IsEpic => Constants.GetPurchasingPlatformType() == PlatformConfig.EpicGamesStoreName;

    public override void Load()
    {
        MainThreadId = Thread.CurrentThread.ManagedThreadId;
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
        SuperTrophyManager.Load();
        CustomCosmeticsSaver.Load();
        CustomColors.Load();
        ApiServerManager.Initialize();

        Logger.LogInfo("Waiting for Harmony patch");
        task.Wait();
        Logger.LogInfo("SuperNewRoles loaded");
        Logger.LogInfo("--------------------------------");
        Logger.LogInfo(ModTranslation.GetString("WelcomeNextSuperNewRoles"));
        Logger.LogInfo("--------------------------------");
    }
    private static void RegisterCustomObjects()
    {
        ClassInjector.RegisterTypeInIl2Cpp<RightClickDetector>();
        ClassInjector.RegisterTypeInIl2Cpp<FadeCoroutine>();
        ClassInjector.RegisterTypeInIl2Cpp<HelpMenuObjectComponent>();
        ClassInjector.RegisterTypeInIl2Cpp<GotTrophyUI.SlideAnimator>();
        ClassInjector.RegisterTypeInIl2Cpp<CustomCosmeticsCostumeSlot>();
        ClassInjector.RegisterTypeInIl2Cpp<CustomHatLayer>();
        ClassInjector.RegisterTypeInIl2Cpp<CustomVisorLayer>();
        ClassInjector.RegisterTypeInIl2Cpp<PushedPlayerDeadbody>();
        ClassInjector.RegisterTypeInIl2Cpp<CustomPlayerAnimationSimple>();
    }

    public void ExecuteInMainThread(Action action)
    {
        if (Thread.CurrentThread.ManagedThreadId == MainThreadId)
        {
            action();
        }
        else
        {
            lock (_mainThreadActionsLock)
            {
                _mainThreadActions.Add(action);
            }
        }
    }

    public void Update()
    {
        if (_mainThreadActions.Count > 0)
        {
            List<Action> actionsToExecute;
            lock (_mainThreadActionsLock)
            {
                actionsToExecute = new List<Action>(_mainThreadActions);
                _mainThreadActions.Clear();
            }

            foreach (var action in actionsToExecute)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error executing action on main thread: {e}");
                }
            }
        }
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