using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;
using static SuperNewRoles.CustomOptions.Categories.MapSettingOptions;

namespace SuperNewRoles.MapCustoms;
public static class ZiplineUpdown
{
    private static bool _isInitialized = false;
    private static float _lastCooldownValue = -1f;
    private static LateTask _periodicUpdateTask;
    
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
                
                // 設定値の監視を開始
                StartPeriodicUpdate();
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

            // 最後に設定したクールダウン値を記録
            _lastCooldownValue = cooldownTime;

            Logger.Info($"Successfully set zipline cooldowns: {cooldownTime}s for {ziplineConsoles.Length} zipline consoles");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error setting zipline cooldowns: {ex}");
        }
    }

    /// <summary>
    /// 設定値が変更された際にジップラインのクールダウンを動的に更新する
    /// </summary>
    public static void UpdateZiplineCooldowns()
    {
        if (!_isInitialized) return;
        if (!ZiplineCoolChangeOption) return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle)) return;
        
        SetZiplineCooldowns();
    }

    /// <summary>
    /// 定期的に設定値をチェックし、変更されていれば更新する
    /// </summary>
    private static void StartPeriodicUpdate()
    {
        if (_periodicUpdateTask != null) return;
        
        _periodicUpdateTask = new LateTask(() =>
        {
            PeriodicCheck();
            // 2秒間隔で定期チェックを継続
            if (_isInitialized && ZiplineCoolChangeOption)
            {
                _periodicUpdateTask = new LateTask(() => PeriodicCheck(), 2f, "ZiplinePeriodicCheck");
            }
        }, 2f, "ZiplinePeriodicCheck");
    }

    private static void PeriodicCheck()
    {
        if (!_isInitialized) return;
        if (!ZiplineCoolChangeOption) return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle)) return;

        try
        {
            float currentCooldownSetting = ZiplineCoolTimeOption;
            if (ZiplineImpostorCoolChangeOption && ExPlayerControl.LocalPlayer.IsImpostor())
                currentCooldownSetting = ZiplineImpostorCoolTimeOption;

            // 設定値が変更された場合のみ更新
            if (Mathf.Abs(_lastCooldownValue - currentCooldownSetting) > 0.01f)
            {
                Logger.Info($"Zipline cooldown setting changed from {_lastCooldownValue}s to {currentCooldownSetting}s, updating consoles");
                _lastCooldownValue = currentCooldownSetting;
                SetZiplineCooldowns();
            }
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error in zipline periodic check: {ex}");
        }
    }

    public static void Reset()
    {
        _isInitialized = false;
        _lastCooldownValue = -1f;
        
        // 定期タスクを停止
        _periodicUpdateTask = null;
        
        Logger.Info("The Fungle zipline initialization flag reset");
    }
}
