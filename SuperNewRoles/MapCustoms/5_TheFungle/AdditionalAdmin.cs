using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.MapCustoms;
public class FungleAdditionalAdmin
{
    public static void AddAdmin()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
            !MapEditSettingsOptions.TheFungleAdditionalAdmin)
        {
            Logger.Info("The Fungle admin console creation skipped: not on The Fungle map or option disabled");
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
            }
            catch (Exception ex)
            {
                Logger.Error($"Error adding admin console: {ex}");
            }
        });
    }
}