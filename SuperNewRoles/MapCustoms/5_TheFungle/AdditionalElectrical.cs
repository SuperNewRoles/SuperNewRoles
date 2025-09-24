using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.MapCustoms;
public static class FungleAdditionalElectrical
{
    private static bool _isInitialized = false;
    public static void CreateElectrical()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
            !MapEditSettingsOptions.TheFunglePowerOutageSabotage)
        {
            // ログを削除（頻繁な出力を避けるため）
            return;
        }

        if (_isInitialized)
        {
            // ログを削除（頻繁な出力を避けるため）
            return;
        }

        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus == null)
        {
            Logger.Warning("Failed to get FungleShipStatus for electrical system");
            return;
        }

        try
        {
            // 電気系統の初期化を確実に行う
            SwitchSystem system = new();
            fungleShipStatus.Systems[SystemTypes.Electrical] = system.TryCast<ISystemType>();
            var sabotageSystem = fungleShipStatus.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
            if (sabotageSystem != null)
            {
                sabotageSystem.specials.Add(system.TryCast<IActivatable>());
            }
            
            Logger.Info("Successfully initialized electrical system for The Fungle");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error initializing electrical system: {ex}");
        }

        MapLoader.LoadMap(MapNames.Airship, (ship) =>
        {
            if (ship == null)
            {
                Logger.Warning("Failed to load Airship map for electrical system");
                return;
            }

            try
            {
                // 既存の電気修理コンソールをチェック
                var existingElectricalConsoles = ShipStatus.Instance.AllConsoles
                    .Where(c => c != null && c.TaskTypes != null && c.TaskTypes.Any(t => t == TaskTypes.FixLights))
                    .ToList();


                // タスクリストの追加
                List<PlayerTask> Tasks = ShipStatus.Instance.SpecialTasks.ToList();
                var fixLightsTask = ship.SpecialTasks.FirstOrDefault(x => x.TaskType == TaskTypes.FixLights);
                if (fixLightsTask != null && !Tasks.Any(t => t.TaskType == TaskTypes.FixLights))
                {
                    Tasks.Add(fixLightsTask);
                    ShipStatus.Instance.SpecialTasks = new(Tasks.ToArray());
                    Logger.Info("Successfully added FixLights task to The Fungle");
                }
                else if (fixLightsTask == null)
                {
                    Logger.Warning("FixLights task not found in Airship map");
                }

                // 既存のコンソールが1つでも存在する場合は生成をスキップ（FungleAdditionalAdminと同様の安全な実装）
                if (existingElectricalConsoles.Count > 0)
                {
                    // ログを削除（頻繁な出力を避けるため）
                    _isInitialized = true;
                    return;
                }

                // 電気修理コンソールの作成（既存がない場合のみ）
                var electricalPrefab = ship.transform.FindChild("Storage/task_lightssabotage (cargo)");
                if (electricalPrefab == null)
                {
                    Logger.Warning("Failed to find electrical prefab from Airship");
                    return;
                }

                // 3つのコンソールを全て作成
                List<Console> newConsoles = new();
                Vector3[] positions = new Vector3[]
                {
                    new(-16.2f, 7.67f, 0),
                    new(-5.7f, -7.7f, -1.008f),
                    new(21.48f, 4.27f, 0f)
                };

                for (int i = 0; i < positions.Length; i++)
                {
                    Console console = GameObject.Instantiate(electricalPrefab, fungleShipStatus.transform).GetComponent<Console>();
                    console.transform.localPosition = positions[i];
                    console.ConsoleId = i;
                    newConsoles.Add(console);
                }

                // コンソールリストに追加
                if (newConsoles.Count > 0)
                {
                    List<Console> Consoles = ShipStatus.Instance.AllConsoles.ToList();
                    Consoles.AddRange(newConsoles);
                    ShipStatus.Instance.AllConsoles = Consoles.ToArray();
                    
                    Logger.Info($"Successfully created {newConsoles.Count} electrical consoles for The Fungle");
                    Logger.Info($"Current console count: {ShipStatus.Instance.AllConsoles.Length}");
                }
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating electrical consoles: {ex}");
            }
        });
    }

    public static void Reset()
    {
        _isInitialized = false;
        // ログを削除（頻繁な出力を避けるため）
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
    class MapBehaviourAwakePatch
    {
        public static void Postfix(MapBehaviour __instance)
        {
            if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
                !MapEditSettingsOptions.TheFunglePowerOutageSabotage)
                return;

            MapLoader.LoadMap(MapNames.Airship, (ship) =>
            {
                if (ship == null)
                {
                    // ログを削除（頻繁な出力を避けるため）
                    return;
                }

                try
                {
                    var electricalRoom = ship.MapPrefab.infectedOverlay.rooms.FirstOrDefault(x => x.room == SystemTypes.Electrical);
                    if (electricalRoom == null)
                    {
                        // ログを削除（頻繁な出力を避けるため）
                        return;
                    }

                    MapRoom mapRoom = GameObject.Instantiate(electricalRoom, __instance.infectedOverlay.transform);
                    mapRoom.Parent = __instance.infectedOverlay;
                    mapRoom.transform.localPosition = new(-0.83f, -1.8f, -1f);
                    
                    var buttonBehavior = mapRoom.GetComponentInChildren<ButtonBehavior>();
                    if (buttonBehavior != null)
                    {
                        var buttons = __instance.infectedOverlay.allButtons.ToList();
                        buttons.Add(buttonBehavior);
                        __instance.infectedOverlay.allButtons = buttons.ToArray();
                    }
                    
                    var buttons2 = __instance.infectedOverlay.rooms.ToList();
                    buttons2.Add(mapRoom);
                    __instance.infectedOverlay.rooms = buttons2.ToArray();

                    Logger.Info("Successfully added electrical room to The Fungle map UI");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error adding electrical room to map UI: {ex}");
                }
            });
        }
    }
}