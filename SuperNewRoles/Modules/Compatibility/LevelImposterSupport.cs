using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SuperNewRoles.MapDatabase;

namespace SuperNewRoles.Modules.Compatibility;

/// <summary>
/// LevelImposter 導入時だけ有効な検出キャッシュ。DLL 参照は持たない。
/// ホットパスでは <see cref="IsCustomMap"/> の bool だけを見る。
/// </summary>
public static class LevelImposterSupport
{
    public const string PluginGuid = "com.DigiWorm.LevelImposter";
    public const string ReactorPluginGuid = "gg.reactor.api";
    /// <summary>LevelImposter の <c>MapType.LevelImposter</c>。</summary>
    public const byte CustomMapId = 7;

    private static bool _customMapFromShip;

    public static bool IsPluginLoaded { get; private set; }

    /// <summary>
    /// 現在 LI カスタムマップか。プラグイン未導入なら常に false。
    /// ShipStatus がある間は Awake 時のキャッシュ、ロビーでは MapId を見る。
    /// </summary>
    public static bool IsCustomMap
    {
        get
        {
            if (!IsPluginLoaded)
                return false;
            if (ShipStatus.Instance != null)
                return _customMapFromShip;
            return TryGetCurrentMapId(out byte mapId) && mapId == CustomMapId;
        }
    }

    public static void Initialize()
    {
        RefreshPluginLoaded();
        IL2CPPChainloader chainloader = IL2CPPChainloader.Instance;
        if (chainloader != null)
            chainloader.Finished += RefreshPluginLoaded;
        Logger.Info($"LevelImposterSupport: pluginLoaded={IsPluginLoaded}");
    }

    private static void RefreshPluginLoaded()
    {
        IL2CPPChainloader chainloader = IL2CPPChainloader.Instance;
        IsPluginLoaded = chainloader?.Plugins != null && chainloader.Plugins.ContainsKey(PluginGuid);
    }

    public static void RefreshFromShip(ShipStatus ship)
    {
        if (!IsPluginLoaded || ship == null)
        {
            _customMapFromShip = false;
            return;
        }

        bool byType = (byte)ship.Type == CustomMapId;
        bool byOptions = TryGetCurrentMapId(out byte mapId) && mapId == CustomMapId;
        _customMapFromShip = byType || byOptions;
    }

    public static void ClearShipState()
    {
        _customMapFromShip = false;
        PhysicsMapDatabase.InvalidateBoundsCache();
    }

    public static bool TryGetCurrentMapId(out byte mapId)
    {
        mapId = 0;
        GameOptionsManager manager = GameOptionsManager.Instance;
        if (manager?.CurrentGameOptions == null)
            return false;
        mapId = manager.CurrentGameOptions.GetByte(ByteOptionNames.MapId);
        return true;
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public static class ShipStatusAwakePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            RefreshFromShip(__instance);
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
    public static class ShipStatusOnDestroyPatch
    {
        public static void Prefix()
        {
            ClearShipState();
        }
    }

    /// <summary>
    /// LI はマップを非同期構築するため、Intro 開始時点で境界キャッシュを取り直す。
    /// </summary>
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    public static class IntroCutsceneCoBeginPatch
    {
        public static void Postfix()
        {
            if (!IsCustomMap)
                return;
            PhysicsMapDatabase.InvalidateBoundsCache();
        }
    }
}
