using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.Data;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches
{
    class SNROnlySearch
    {
        public static byte currentMapId;
        static KeyValueOption MapNameOption;
        public static bool IsSNRSearch = false;
        public static void FixedUpdate()
        {
            //Logger.Info(PlayerControl.GameOptions.MapId.ToString());
            if (AmongUsClient.Instance.AmHost)
            {
                if (CustomOptionHolder.IsSNROnlySearch.GetBool())
                {
                    if (PlayerControl.GameOptions.MapId < 5)
                    {
                        currentMapId = PlayerControl.GameOptions.MapId;
                        PlayerControl.GameOptions.MapId = 6;
                        PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                    }
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetMapId);
                    writer.Write(currentMapId);
                    writer.EndRPC();
                    if (GameSettingMenu.Instance != null)
                    {
                        if (MapNameOption == null)
                            MapNameOption = GameSettingMenu.Instance.transform.FindChild("Game Settings/GameGroup/SliderInner/MapName")?.GetComponent<KeyValueOption>();
                        if (MapNameOption != null)
                        {
                            MapNameOption.oldValue = currentMapId;
                            MapNameOption.ValueText.text = Constants.MapNames[currentMapId];
                        }
                    }
                }
                else
                {
                    if (PlayerControl.GameOptions.MapId > 5)
                    {
                        currentMapId = 0;
                        PlayerControl.GameOptions.MapId = currentMapId;
                        PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.ToggleMapFilter))]
        public static class GameOptionsDataToggleMapFilterPatch
        {
            public static bool Prefix(GameOptionsData __instance, [HarmonyArgument(0)] byte mapId)
            {
                //Logger.Info(__instance.MapId.ToString(), "MapId1");
                __instance.MapId = (byte)(__instance.MapId ^ (byte)(1 << (int)mapId));
                if (__instance.MapId == 0)
                {
                    __instance.MapId = (byte)(__instance.MapId ^ (byte)(1 << (int)mapId));
                }
                Logger.Info($"{mapId} : {__instance.MapId}", "MapId2");
                return false;
            }
        }

        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Awake))]
        public static class CreateOptionsPicker_Awake_Patch
        {
            static SpriteRenderer IsSNROnlyRoomButtonRender;
            public static void Prefix(CreateOptionsPicker __instance)
            {
                Logger.Info($"Awake:{__instance.mode}");
                if (__instance.mode is not SettingsMode.Search)
                {
                    GameOptionsData.GameHostOptions.MapId = 0;
                    return;
                }
                Transform temp;
                GameObject IsSNROnlyRoomButton;
                PassiveButton button;
                (IsSNROnlyRoomButton = GameObject.Instantiate((temp = __instance.MapButtons[3].transform.parent).gameObject, temp.parent)).name = "IsSNROnlyRoom";
                IsSNROnlyRoomButton.transform.position = new(2.96f, 2.04f, -20);
                (IsSNROnlyRoomButtonRender = IsSNROnlyRoomButton.transform.Find("4").GetComponent<SpriteRenderer>()).sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.banner.png", 450f);
                if (GameOptionsData.GameSearchOptions.FilterContainsMap(6))
                {
                    foreach (SpriteRenderer render in __instance.MapButtons)
                        if (render.name != "IsSNROnlyRoom")
                            render.transform.parent.gameObject.SetActive(false);
                    IsSNROnlyRoomButtonRender.color = Color.white;
                } else IsSNROnlyRoomButtonRender.color = Palette.DisabledGrey;
                (button = IsSNROnlyRoomButton.GetComponent<PassiveButton>()).OnClick.RemoveAllListeners();
                button.OnClick.AddListener((UnityAction)(() =>
                {
                    bool IsSNROn = false;
                    if (GameOptionsData.GameSearchOptions.FilterContainsMap(0))
                    {
                        GameOptionsData.GameSearchOptions.ToggleMapFilter(0);
                        IsSNROn = true;
                    }
                    if (GameOptionsData.GameSearchOptions.FilterContainsMap(1))
                    {
                        GameOptionsData.GameSearchOptions.ToggleMapFilter(1);
                        IsSNROn = true;
                    }
                    if (GameOptionsData.GameSearchOptions.FilterContainsMap(2))
                    {
                        GameOptionsData.GameSearchOptions.ToggleMapFilter(2);
                        IsSNROn = true;
                    }
                    if (GameOptionsData.GameSearchOptions.FilterContainsMap(4))
                        GameOptionsData.GameSearchOptions.ToggleMapFilter(4);
                    if (GameOptionsData.GameSearchOptions.FilterContainsMap(6))
                        GameOptionsData.GameSearchOptions.ToggleMapFilter(6);
                    if (IsSNROn)
                    {
                        foreach (SpriteRenderer render in __instance.MapButtons)
                            if (render.name != "IsSNROnlyRoom")
                                render.transform.parent.gameObject.SetActive(false);
                        //他MODで実装する場合はここのIdを変えてください。(同じMOD同士でマッチングするのを防ぐため)
                        //後、他の部分のIdも変えてね
                        __instance.SetMap(6);
                        IsSNROnlyRoomButtonRender.color = Color.white;
                        Logger.Info(GameOptionsData.GameSearchOptions.MapId.ToString(), "現在のMapId:IsSNROn");
                    }
                    else
                    {
                        foreach (SpriteRenderer render in __instance.MapButtons)
                            if (render.name != "IsSNROnlyRoom")
                                render.transform.parent.gameObject.SetActive(true);
                        __instance.SetMap(0);
                        __instance.SetMap(1);
                        __instance.SetMap(2);
                        if (!GameOptionsData.GameSearchOptions.FilterContainsMap(4))
                            __instance.SetMap(4);
                        IsSNROnlyRoomButtonRender.color = Palette.DisabledGrey;
                        Logger.Info(GameOptionsData.GameSearchOptions.MapId.ToString(), "現在のMapId:IsSNROFF");
                    }
                }));
            }
        }
    }
}