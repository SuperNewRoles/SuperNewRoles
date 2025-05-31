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
using AmongUs.Data.Player;
using SuperNewRoles.RequestInGame;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace SuperNewRoles;

[BepInAutoPlugin(PluginConfig.Id, PluginConfig.Name)]
[BepInProcess(PluginConfig.ProcessName)]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInIncompatibility("com.emptybottle.townofhost")]
public partial class SuperNewRolesPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(PluginConfig.Id);
    public static SuperNewRolesPlugin Instance;
    public static ManualLogSource Logger { get; private set; }

    public static int MainThreadId { get; private set; }
    private readonly List<Action> _mainThreadActions = new();
    private readonly object _mainThreadActionsLock = new();

    public static Assembly Assembly { get; private set; } = Assembly.GetExecutingAssembly();

    private static string _currentSceneName;

    public static bool IsEpic => Constants.GetPurchasingPlatformType() == PlatformConfig.EpicGamesStoreName;
    public static string BaseDirectory => Path.GetFullPath(Path.Combine(BepInEx.Paths.BepInExRootPath, "../SuperNewRolesNext"));
    public static string SecretDirectory => Path.GetFullPath(Path.Combine(UnityEngine.Application.persistentDataPath, "SuperNewRolesNextSecrets"));
    private static Task TaskRunIfWindows(Action action)
    {
        bool needed = false;
        if (needed && ModHelpers.IsAndroid())
            action();
        else
            return Task.Run(action);
        return Task.Run(() => { });

    }
    // 複数起動中の場合に絶対に重複しない数
    private static int ProcessNumber = 0;

    public static Task HarmonyPatchAllTask;
    public static Task CustomRPCManagerLoadTask;

    public override void Load()
    {
        Assembly = Assembly.GetExecutingAssembly();
        BepInEx.Logging.Logger.Listeners.Add(new SNRLogListener());

        MainThreadId = Thread.CurrentThread.ManagedThreadId;
        Logger = Log;

        SuperNewRolesPlugin.Logger.LogInfo($"BaseDirectory: {BaseDirectory}");
        SuperNewRolesPlugin.Logger.LogInfo($"SecretDirectory: {SecretDirectory}");

        Instance = this;

        RegisterCustomObjects();
        CustomLoadingScreen.Patch(Harmony);
        HarmonyPatchAllTask = TaskRunIfWindows(() => PatchAll(Harmony));

        if (!Directory.Exists(BaseDirectory))
            Directory.CreateDirectory(BaseDirectory);
        if (!Directory.Exists(SecretDirectory))
            Directory.CreateDirectory(SecretDirectory);

        ConfigRoles.Init();
        UpdateCPUProcessorAffinity();
        CustomRoleManager.Load();
        AssetManager.Load();
        ModTranslation.Load();
        var tasks = CustomRPCManager.Load();
        CustomOptionManager.Load();
        SyncVersion.Load();
        EventListenerManager.Load();
        SuperTrophyManager.Load();
        CustomCosmeticsSaver.Load();
        CustomColors.Load();
        ApiServerManager.Initialize();
        RequestInGameManager.Load();

        CustomServer.UpdateRegions();

        CheckStarts();

        CustomRPCManagerLoadTask = TaskRunIfWindows(() =>
        {
            foreach (var task in tasks)
            {
                task();
            }
        });

        Logger.LogInfo("Waiting for Harmony patch");
        if (ModHelpers.IsAndroid())
        {
            HarmonyPatchAllTask?.Wait();
            CustomRPCManagerLoadTask?.Wait();
        }
        Logger.LogInfo("SuperNewRoles loaded");
        Logger.LogInfo("--------------------------------");
        Logger.LogInfo(ModTranslation.GetString("WelcomeNextSuperNewRoles"));
        Logger.LogInfo("--------------------------------");
    }
    public void PatchAll(Harmony harmony)
    {
        var assembly = Assembly;
        if (ModHelpers.IsAndroid())
        {
            harmony.PatchAll(assembly);
            // コルーチンパッチを処理
            HarmonyCoroutinePatchProcessor.ProcessCoroutinePatches(harmony, assembly);
        }
        else
        {
            List<Task> tasks = new();
            AccessTools.GetTypesFromAssembly(assembly).Do(delegate (Type type)
            {
                //tasks.Add(Task.Run(() => ));
                harmony.CreateClassProcessor(type).Patch();
            });
            Task.WhenAll(tasks.ToArray()).Wait();

            // コルーチンパッチを処理
            HarmonyCoroutinePatchProcessor.ProcessCoroutinePatches(harmony, assembly);
        }
    }

    // CPUのコア割当を変更してパフォーマンスを改善する
    public static void UpdateCPUProcessorAffinity()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux()) return;

        ulong affinity = ConfigRoles._ProcessorAffinityMask.Value;
        if (!ConfigRoles._isCPUProcessorAffinity.Value || affinity == 0)
        {
            Logger.LogWarning("UpdateCPUProcessorAffinity: IsCPUProcessorAffinity is false");
            return;
        }

        Logger.LogInfo("Start UpdateCPUProcessorAffinity");
        if (Environment.ProcessorCount > 1)
        {
            Logger.LogInfo($"Environment.ProcessorCount: {Environment.ProcessorCount}");
            Process currentProcess = Process.GetCurrentProcess();
            Logger.LogInfo($"Current ProcessorAffinity: {currentProcess.ProcessorAffinity}");
            // コア数上限突破の場合は全てのコアを使う
            if (Environment.ProcessorCount < System.Numerics.BitOperations.Log2(affinity))
            {
                affinity = 1;
                for (int i = 1; i < Environment.ProcessorCount; i++)
                {
                    affinity |= (ulong)1 << i;
                }
            }
            currentProcess.ProcessorAffinity = (IntPtr)affinity;
        }
        Logger.LogInfo($"UpdatedCPUProcessorAffinity To: {affinity}");
    }

    // 起動中に他クライアントに上書きされないようにDisposeせずに持っておく
    private static FileStream _fs;

    private static void CheckStarts()
    {
        // Androidはよっぽどのことがない限り1起動で済むので
        if (ModHelpers.IsAndroid())
        {
            ProcessNumber = 0;
            return;
        }
        // SuperNewRolesNext/Startsディレクトリのパスを取得
        string startsDir = BaseDirectory + "/Starts";
        // ディレクトリが存在しなければ作成
        if (!Directory.Exists(startsDir))
        {
            Directory.CreateDirectory(startsDir);
        }
        int index = 0;
        while (true)
        {
            try
            {
                // 書き込み可能かチェックするため、独占モードでファイルをオープン
                string filePath = Path.Combine(startsDir, $"{index}.txt");
                _fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);

                // 書き込みテストとして内容を記述する
                byte[] content = Encoding.UTF8.GetBytes("Process check");
                _fs.Write(content, 0, content.Length);

                // 書き込みに成功したので、そのインデックスをProcessNumberに設定
                ProcessNumber = index;
                SuperNewRoles.Logger.Info($"Started AmongUs {index} times");
                break;
            }
            catch (IOException)
            {
                // 書き込み不可の場合は次のファイル番号を試す
                SuperNewRoles.Logger.Warning($"Checking ProcessNumber: {index}");
                index++;
            }
        }
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
        ClassInjector.RegisterTypeInIl2Cpp<CustomAnimationObject>();
        ClassInjector.RegisterTypeInIl2Cpp<DestroyOnAnimationEndObject>();
        ClassInjector.RegisterTypeInIl2Cpp<SelectButtonsMenuCloseAnimation>();
        ClassInjector.RegisterTypeInIl2Cpp<SelectButtonsMenuOpenAnimation>();
        ClassInjector.RegisterTypeInIl2Cpp<LoadingUIComponent>();
        ClassInjector.RegisterTypeInIl2Cpp<ActionOnEsc>();
        ClassInjector.RegisterTypeInIl2Cpp<RocketDeadbody>();
        ClassInjector.RegisterTypeInIl2Cpp<VersionUpdatesComponent>();
        ClassInjector.RegisterTypeInIl2Cpp<ReleaseNoteComponent>();
        ClassInjector.RegisterTypeInIl2Cpp<PatcherUpdaterComponent>();
        // lassInjector.RegisterTypeInIl2Cpp<AddressableReleaseOnDestroy>();
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

    [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
    public static class PlayerBanDataIsBannedPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
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

    [HarmonyPatch(typeof(AbstractUserSaveData), nameof(AbstractUserSaveData.HandleSave))]
    public static class BlockSaveUserDataPatch
    {
        public static bool Prefix() => ProcessNumber == 0;
    }
}