using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;
public class FungleAdditionalAdmin
{
    private static bool _isInitialized = false;
    public static void AddAdmin()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
            !MapEditSettingsOptions.TheFungleAdditionalAdmin)
        {
            // ログを削除（頻繁な出力を避けるため）
            return;
        }

        if (_isInitialized)
        {
            // ログを削除（頻繁な出力を避けるため）
            return;
        }

        MapLoader.LoadMap(MapNames.Airship, (ship) =>
        {
            if (ship == null)
            {
                Logger.Warning("Failed to load Airship map for admin console");
                return;
            }

            try
            {
                // 既存のアドミンコンソールをチェック
                var existingAdminConsoles = GameObject.FindObjectsOfType<MapBehaviour>()
                    .Where(mb => mb.name.Contains("panel_cockpit_map"))
                    .ToList();

                if (existingAdminConsoles.Count > 0)
                {
                    // ログを削除（頻繁な出力を避けるため）
                    _isInitialized = true;
                    return;
                }

                var adminPrefab = ship.transform.FindChild("Cockpit/panel_cockpit_map");
                if (adminPrefab == null)
                {
                    Logger.Warning("Failed to find admin console prefab from Airship");
                    return;
                }

                Transform Admin = GameObject.Instantiate(adminPrefab, ShipStatus.Instance.transform);
                Admin.transform.position = new Vector3(-10.3f, 13.5f, 0.1f);
                Admin.transform.Rotate(new(0, 0, 10f));
                
                Logger.Info("Successfully added admin console to The Fungle");
                Logger.Info($"Admin console position: {Admin.transform.position}, rotation: {Admin.transform.rotation}");
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding admin console: {ex}");
            }
        });
    }

    public static void Reset()
    {
        _isInitialized = false;
        // ログを削除（頻繁な出力を避けるため）
    }
}