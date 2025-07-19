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
    private static ZiplineConsole[] _cachedZiplineConsoles = null;
    
    public static void Initialize()
    {
        if (!MapEditSettingsOptions.TheFungleZiplineOption)
            return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            return;
        
        if (_isInitialized)
        {
            // ログを削除（頻繁な出力を避けるため）
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
                // ジップラインコンソールをキャッシュ
                CacheZiplineConsoles();
                
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

    private static void CacheZiplineConsoles()
    {
        try
        {
            _cachedZiplineConsoles = GameObject.FindObjectsOfType<ZiplineConsole>();
            // 初期化時のみキャッシュ情報をログ出力
            Logger.Info($"Cached {_cachedZiplineConsoles?.Length ?? 0} zipline consoles for cooldown management");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error caching zipline consoles: {ex}");
            _cachedZiplineConsoles = null;
        }
    }

    private static void SetZiplineCooldowns()
    {
        try
        {
            // キャッシュされたコンソールを使用、なければ再取得
            var ziplineConsoles = _cachedZiplineConsoles ?? GameObject.FindObjectsOfType<ZiplineConsole>();
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
                    // 0秒クールダウンの場合は常に0に設定
                    if (cooldownTime <= 0f)
                    {
                        ziplineConsole.CoolDown = 0f;
                    }
                    else
                    {
                        // クールダウンが進行中（設定値より小さい）の場合は設定しない
                        if (ziplineConsole.CoolDown > cooldownTime || ziplineConsole.CoolDown <= 0f)
                        {
                            ziplineConsole.CoolDown = cooldownTime;
                        }
                    }
                    
                    if (ziplineConsole.destination != null)
                    {
                        // 0秒クールダウンの場合は常に0に設定
                        if (cooldownTime <= 0f)
                        {
                            ziplineConsole.destination.CoolDown = 0f;
                        }
                        else
                        {
                            if (ziplineConsole.destination.CoolDown > cooldownTime || ziplineConsole.destination.CoolDown <= 0f)
                            {
                                ziplineConsole.destination.CoolDown = cooldownTime;
                            }
                        }
                    }
                }
            }

            // 最後に設定したクールダウン値を記録
            _lastCooldownValue = cooldownTime;

            // クールダウン設定成功はログ出力しない（頻繁な出力を避けるため）
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
    /// オプション値が変更されたときに即座にクールダウンを更新する
    /// </summary>
    public static void ForceUpdateZiplineCooldowns()
    {
        if (!_isInitialized) return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle)) return;
        
        // 強制更新は重要な操作なのでログを残す
        Logger.Info("Force updating zipline cooldowns due to option change");
        SetZiplineCooldowns();
    }

    /// <summary>
    /// 定期的に設定値をチェックし、変更されていれば更新する
    /// </summary>
    private static void StartPeriodicUpdate()
    {
        if (_periodicUpdateTask != null) return;
        
        ScheduleNextPeriodicCheck();
    }
    
    /// <summary>
    /// 次の定期チェックをスケジュールする
    /// </summary>
    private static void ScheduleNextPeriodicCheck()
    {
        if (!_isInitialized || !ZiplineCoolChangeOption) return;
        
        _periodicUpdateTask = new LateTask(() =>
        {
            PeriodicCheck();
            // 継続的に次回の実行をスケジュール
            ScheduleNextPeriodicCheck();
        }, 0.5f, "ZiplinePeriodicCheck");
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

            // 0秒以下の場合は0にする
            if (currentCooldownSetting <= 0f)
                currentCooldownSetting = 0f;

            // 設定値が変更された場合のみ更新
            if (Mathf.Abs(_lastCooldownValue - currentCooldownSetting) > 0.01f)
            {
                Logger.Info($"Zipline cooldown setting changed from {_lastCooldownValue}s to {currentCooldownSetting}s, updating consoles");
                SetZiplineCooldowns();
            }
            else
            {
                // 設定値が変更されていない場合でも、実際のコンソールのクールダウンを確認して修正する
                // ただし、クールダウンが進行中（設定値より小さい）の場合は更新しない
                // キャッシュされたコンソールを使用してパフォーマンスを改善
                var ziplineConsoles = _cachedZiplineConsoles ?? GameObject.FindObjectsOfType<ZiplineConsole>();
                bool needsUpdate = false;
                
                foreach (var console in ziplineConsoles)
                {
                    // クールダウンが設定値より大きい場合のみ修正が必要
                    if (console != null && console.CoolDown > currentCooldownSetting + 0.01f)
                    {
                        needsUpdate = true;
                        break;
                    }
                    if (console != null && console.destination != null && console.destination.CoolDown > currentCooldownSetting + 0.01f)
                    {
                        needsUpdate = true;
                        break;
                    }
                }
                
                if (needsUpdate)
                {
                    // クールダウン不一致検出はログ出力しない（頻繁な出力を避けるため）
                    SetZiplineCooldowns();
                }
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
        if (_periodicUpdateTask != null)
        {
            _periodicUpdateTask = null;
            // ログを削除（頻繁な出力を避けるため）
        }
        
        // キャッシュをクリア
        _cachedZiplineConsoles = null;
        
        // ログを削除（頻繁な出力を避けるため）
    }
}
