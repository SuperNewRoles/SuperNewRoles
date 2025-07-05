using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;
using static SuperNewRoles.CustomOptions.Categories.MapSettingOptions;

namespace SuperNewRoles.MapCustoms;
public static class ZiplineUpdown
{
    private static bool _isInitialized = false;
    public static void Initialize()
    {
        if (!MapEditSettingsOptions.TheFungleZiplineOption)
            return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            return;
        
        if (_isInitialized)
        {
            Logger.Info("The Fungle zipline already initialized, skipping");
            return;
        }
        
        try
        {
            FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
            if (fungleShipStatus == null)
            {
                Logger.Warning("Failed to get FungleShipStatus for zipline modifications");
                return;
            }
            
            if (fungleShipStatus.Zipline == null)
            {
                Logger.Warning("Zipline component is null on FungleShipStatus");
                return;
            }
            
            fungleShipStatus.Zipline.upTravelTime = MapEditSettingsOptions.TheFungleZiplineUpTime;
            fungleShipStatus.Zipline.downTravelTime = MapEditSettingsOptions.TheFungleZiplineDownTime;
            
            Logger.Info($"Successfully set zipline times: up={MapEditSettingsOptions.TheFungleZiplineUpTime}s, down={MapEditSettingsOptions.TheFungleZiplineDownTime}s");
            
            // Set initial cooldown values for zipline consoles if cooldown change is enabled
            if (ZiplineCoolChangeOption)
            {
                // 即座に設定
                SetZiplineCooldowns();
                
                // 少し遅延してもう一度設定（確実にするため）
                new LateTask(() => SetZiplineCooldowns(), 0.5f, "SetZiplineCooldowns");
            }
            
            _isInitialized = true;
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error setting zipline travel times: {ex}");
        }
    }

    private static void SetZiplineCooldowns()
    {
        try
        {
            var ziplineConsoles = GameObject.FindObjectsOfType<ZiplineConsole>();
            if (ziplineConsoles == null || ziplineConsoles.Length == 0)
            {
                Logger.Warning("No zipline consoles found to set cooldowns");
                return;
            }

            float cooldownTime = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                cooldownTime = ZiplineImpostorCoolTimeOption;

            // 0秒以下の場合は0にする
            if (cooldownTime <= 0f)
                cooldownTime = 0f;

            foreach (var ziplineConsole in ziplineConsoles)
            {
                if (ziplineConsole != null)
                {
                    ziplineConsole.CoolDown = cooldownTime;
                    if (ziplineConsole.destination != null)
                    {
                        ziplineConsole.destination.CoolDown = cooldownTime;
                    }
                }
            }

            Logger.Info($"Successfully set zipline cooldowns: {cooldownTime}s for {ziplineConsoles.Length} zipline consoles");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error setting zipline cooldowns: {ex}");
        }
    }

    public static void Reset()
    {
        _isInitialized = false;
        Logger.Info("The Fungle zipline initialization flag reset");
    }
}
