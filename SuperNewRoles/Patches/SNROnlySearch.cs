using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
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
    [HarmonyPatch(typeof(FilterTagsMenu), nameof(FilterTagsMenu.Open))]
    public static class FilterTagsMenuOpenPatch
    {
        public static void Prefix() => DataManager.Settings.Multiplayer.ValidGameFilterOptions.FilterTags.Add("SNR");
    }
    [HarmonyPatch(typeof(FilterTagsMenu), nameof(FilterTagsMenu.ChooseOption))]
    public static class FilterTagsMenuChooseOptionPatch
    {
        public static void Postfix(FilterTagsMenu __instance, ChatLanguageButton button, string filter)
        {
            if (__instance.targetOpts.FilterTags.Contains("SNR"))
            {
                if (filter == "SNR")
                {
                    __instance.targetOpts.FilterTags = new();
                    __instance.targetOpts.FilterTags.Add("SNR");
                    //foreach (var a in __instance.controllerSelectable.)
                    {

                    }
                }
                else
                {
                    __instance.targetOpts.FilterTags.Remove("SNR");
                }
            }
        }
    }
}