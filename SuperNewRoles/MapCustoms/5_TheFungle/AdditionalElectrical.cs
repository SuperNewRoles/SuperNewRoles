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
    public static void CreateElectrical()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) ||
            !MapEditSettingsOptions.TheFunglePowerOutageSabotage)
            return;

        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus == null)
            return;

        // 電気系統の初期化を確実に行う
        SwitchSystem system = new();
        fungleShipStatus.Systems[SystemTypes.Electrical] = system.TryCast<ISystemType>();
        var sabotageSystem = fungleShipStatus.Systems[SystemTypes.Sabotage].TryCast<SabotageSystemType>();
        if (sabotageSystem != null)
        {
            sabotageSystem.specials.Add(system.TryCast<IActivatable>());
        }

        MapLoader.LoadMap(MapNames.Airship, (ship) =>
        {
            if (ship == null)
            {
                Logger.Info("Failed to load Airship map for electrical system");
                return;
            }

            try
            {
                // タスクリストの追加
                List<PlayerTask> Tasks = ShipStatus.Instance.SpecialTasks.ToList();
                var fixLightsTask = ship.SpecialTasks.FirstOrDefault(x => x.TaskType == TaskTypes.FixLights);
                if (fixLightsTask != null)
                {
                    Tasks.Add(fixLightsTask);
                    ShipStatus.Instance.SpecialTasks = new(Tasks.ToArray());
                }

                // 電気修理コンソールの作成
                var electricalPrefab = ship.transform.FindChild("Storage/task_lightssabotage (cargo)");
                if (electricalPrefab == null)
                {
                    Logger.Info("Failed to find electrical prefab from Airship");
                    return;
                }

                Console console1 = GameObject.Instantiate(electricalPrefab, fungleShipStatus.transform).GetComponent<Console>();
                console1.transform.localPosition = new(-16.2f, 7.67f, 0);
                console1.ConsoleId = 0;

                Console console2 = GameObject.Instantiate(electricalPrefab, fungleShipStatus.transform).GetComponent<Console>();
                console2.transform.localPosition = new(-5.7f, -7.7f, -1.008f);
                console2.ConsoleId = 1;

                Console console3 = GameObject.Instantiate(electricalPrefab, fungleShipStatus.transform).GetComponent<Console>();
                console3.transform.localPosition = new(21.48f, 4.27f, 0f);
                console3.ConsoleId = 2;

                // コンソールリストに追加
                List<Console> Consoles = ShipStatus.Instance.AllConsoles.ToList();
                Consoles.Add(console1);
                Consoles.Add(console2);
                Consoles.Add(console3);
                ShipStatus.Instance.AllConsoles = Consoles.ToArray();

                Logger.Info("Successfully created 3 electrical consoles for The Fungle");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating electrical consoles: {ex}");
            }
        });
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
                    Logger.Info("Failed to load Airship map for electrical map UI");
                    return;
                }

                try
                {
                    var electricalRoom = ship.MapPrefab.infectedOverlay.rooms.FirstOrDefault(x => x.room == SystemTypes.Electrical);
                    if (electricalRoom == null)
                    {
                        Logger.Info("Failed to find electrical room from Airship map");
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