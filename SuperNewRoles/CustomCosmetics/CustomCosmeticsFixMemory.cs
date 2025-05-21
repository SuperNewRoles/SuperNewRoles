using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics;

public static class CustomCosmeticsFixMemory
{
    public static void UnloadAllAssets()
    {
        return;
        Logger.Info("[CustomCosmeticsFixMemory] Starting to unload all cosmetics assets...");
        int hatCount = 0;
        foreach (var hat in CustomCosmeticsLoader.moddedHats.Values)
        {
            hat.UnloadSprites();
            hatCount++;
        }
        Logger.Info($"[CustomCosmeticsFixMemory] Unloaded {hatCount} modded hats.");

        int visorCount = 0;
        foreach (var visor in CustomCosmeticsLoader.moddedVisors.Values)
        {
            visor.UnloadSprites();
            visorCount++;
        }
        Logger.Info($"[CustomCosmeticsFixMemory] Unloaded {visorCount} modded visors.");

        int nameplateCount = 0;
        foreach (var nameplate in CustomCosmeticsLoader.moddedNamePlates.Values)
        {
            nameplate.UnloadSprites();
            nameplateCount++;
        }
        Logger.Info($"[CustomCosmeticsFixMemory] Unloaded {nameplateCount} modded nameplates.");

        Logger.Info("[CustomCosmeticsFixMemory] Calling Resources.UnloadUnusedAssets()...");
        Resources.UnloadUnusedAssets();
        System.GC.Collect(); // 強制的にガベージコレクションを実行 (デバッグ目的、リリース時には注意)
        Logger.Info("[CustomCosmeticsFixMemory] Finished unloading all cosmetics assets.");
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class AssetManagerUnloadAllAssetsPatch
    {
        public static void Postfix()
        {
            UnloadAllAssets();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationMenu), nameof(PlayerCustomizationMenu.OnDestroy))]
    class PlayerCustomizationMenuOnDestroyPatch
    {
        public static void Postfix()
        {
            UnloadAllAssets();
        }
    }
}