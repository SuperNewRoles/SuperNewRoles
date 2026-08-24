using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

namespace SuperNewRoles.CustomCosmetics;

// カスタムコスメ静的辞書の掃除。生存中プレイヤーのレイヤーは残す。
public static class CustomCosmeticsFixMemory
{
    public static void Remove(CosmeticsLayer cosmeticsLayer)
    {
        CustomCosmeticsLayers.Unregister(cosmeticsLayer);
    }

    public static void CleanupDestroyed()
    {
        CustomCosmeticsLayers.RemoveDestroyedEntries();
    }

    public static void CleanupExceptLivingPlayers()
    {
        CustomCosmeticsLayers.RemoveEntriesExceptLivingPlayers();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class CustomCosmeticsFixMemoryCoStartGamePatch
{
    public static void Prefix()
    {
        CustomCosmeticsFixMemory.CleanupExceptLivingPlayers();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
public static class CustomCosmeticsFixMemoryExitGamePatch
{
    public static void Prefix()
    {
        CustomCosmeticsFixMemory.CleanupDestroyed();
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnDestroy))]
public static class CustomCosmeticsFixMemoryHudDestroyPatch
{
    public static void Prefix()
    {
        CustomCosmeticsFixMemory.CleanupDestroyed();
    }
}
