using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Patches;

public static class SNROnlySearch
{
    public static byte currentMapId;
    static KeyValueOption MapNameOption;
    public static bool IsSNRSearch = false;
    public static void FixedUpdate()
    {
        //Logger.Info(GameOptionsManager.Instance.CurrentGameOptions.MapId.ToString());
        if (AmongUsClient.Instance.AmHost)
        {
            if (CustomOptionHolder.IsSNROnlySearch.GetBool())
            {
                if (GameOptionsManager.Instance.CurrentGameOptions.MapId < 5)
                {
                    currentMapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, 6);
                    PlayerControl.LocalPlayer.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions));
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
                if (GameOptionsManager.Instance.CurrentGameOptions.MapId > 5)
                {
                    currentMapId = 0;
                    GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, currentMapId);
                    PlayerControl.LocalPlayer.RpcSyncSettings(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameOptionsManager.Instance.CurrentGameOptions));
                }
            }
        }
    }
    [HarmonyPatch(typeof(IGameOptionsExtensions), "ToggleMapFilter")]
    public static class GameOptionsDataToggleMapFilterPatch
    {
        public static bool Prefix(IGameOptions __instance, [HarmonyArgument(0)] byte mapId)
        {
            //Logger.Info(__instance.MapId.ToString(), "MapId1");
            __instance.SetByte(ByteOptionNames.MapId, (byte)(__instance.MapId ^ (byte)(1 << (int)mapId)));
            if (__instance.GetByte(ByteOptionNames.MapId) == 0)
            {
                __instance.SetByte(ByteOptionNames.MapId, (byte)(__instance.MapId ^ (byte)(1 << (int)mapId)));
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
                GameOptionsManager.Instance.currentHostOptions.SetByte(ByteOptionNames.MapId, 0);
                return;
            }
            Transform temp;
            GameObject IsSNROnlyRoomButton;
            PassiveButton button;
            (IsSNROnlyRoomButton = GameObject.Instantiate((temp = __instance.MapMenu.MapButtons[3].transform.parent).gameObject, temp.parent)).name = "IsSNROnlyRoom";
            IsSNROnlyRoomButton.transform.position = new(2.96f, 2.04f, -20);
            (IsSNROnlyRoomButtonRender = IsSNROnlyRoomButton.transform.Find("4").GetComponent<SpriteRenderer>()).sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.banner.png", 450f);
            if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(6))
            {
                foreach (MapFilterButton btn in __instance.MapMenu.MapButtons)
                    if (btn.ButtonImage.name != "IsSNROnlyRoom")
                        btn.transform.parent.gameObject.SetActive(false);
                IsSNROnlyRoomButtonRender.color = Color.white;
            }
            else IsSNROnlyRoomButtonRender.color = Palette.DisabledGrey;
            (button = IsSNROnlyRoomButton.GetComponent<PassiveButton>()).OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityAction)(() =>
            {
                OnClick();
            }));
            void OnClick()
            {
                bool IsSNROn = false;
                if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(0))
                {
                    GameOptionsManager.Instance.GameSearchOptions.ToggleMapFilter(0);
                    IsSNROn = true;
                }
                if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(1))
                {
                    GameOptionsManager.Instance.GameSearchOptions.ToggleMapFilter(1);
                    IsSNROn = true;
                }
                if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(2))
                {
                    GameOptionsManager.Instance.GameSearchOptions.ToggleMapFilter(2);
                    IsSNROn = true;
                }
                if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(4))
                    GameOptionsManager.Instance.GameSearchOptions.ToggleMapFilter(4);
                if (GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(6))
                    GameOptionsManager.Instance.GameSearchOptions.ToggleMapFilter(6);
                if (IsSNROn)
                {
                    foreach (MapFilterButton btn in __instance.MapMenu.MapButtons)
                        if (btn.ButtonImage.name != "IsSNROnlyRoom")
                            btn.transform.parent.gameObject.SetActive(false);
                    //他MODで実装する場合はここのIdを変えてください。(同じMOD同士でマッチングするのを防ぐため)
                    //後、他の部分のIdも変えてね
                    __instance.SetMap(6);
                    IsSNROnlyRoomButtonRender.color = Color.white;
                    Logger.Info(GameOptionsManager.Instance.GameSearchOptions.MapId.ToString(), "現在のMapId:IsSNROn");
                }
                else
                {
                    foreach (MapFilterButton btn in __instance.MapMenu.MapButtons)
                        if (btn.ButtonImage.name != "IsSNROnlyRoom")
                            btn.transform.parent.gameObject.SetActive(false);
                    __instance.SetMap(0);
                    __instance.SetMap(1);
                    __instance.SetMap(2);
                    if (!GameOptionsManager.Instance.GameSearchOptions.FilterContainsMap(4))
                        __instance.SetMap(4);
                    IsSNROnlyRoomButtonRender.color = Palette.DisabledGrey;
                    Logger.Info(GameOptionsManager.Instance.GameSearchOptions.MapId.ToString(), "現在のMapId:IsSNROFF");
                }
            }
        }
    }
}