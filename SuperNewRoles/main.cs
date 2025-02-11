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
        task.Wait();
        Logger.LogInfo("SuperNewRoles loaded");
        Logger.LogInfo("--------------------------------");
        Logger.LogInfo(ModTranslation.GetString("WelcomeNextSuperNewRoles"));
        Logger.LogInfo("--------------------------------");
    }
    private static void RegisterCustomObjects()
    {
        ClassInjector.RegisterTypeInIl2Cpp<OptionsMenuSelectorData>();
        var rightClickDetectorOptions = new RegisterTypeOptions { Interfaces = new[] { typeof(IPointerClickHandler) } };
        ClassInjector.RegisterTypeInIl2Cpp<RightClickDetector>();
    }
}