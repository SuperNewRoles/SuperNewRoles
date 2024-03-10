using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SuperNewRoles.Modules;

[HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
public static class RegionMenuOpenPatch
{
    private static TextBoxTMP ipField;
    private static TextBoxTMP portField;

    public static void Postfix(RegionMenu __instance)
    {
        var template = FastDestroyableSingleton<JoinGameButton>.Instance;
        var joinGameButtons = GameObject.FindObjectsOfType<JoinGameButton>();
        foreach (var t in joinGameButtons)
        {  // The correct button has a background, the other 2 dont
            if (t.GameIdText != null && t.GameIdText.Background != null)
            {
                template = t;
                break;
            }
        }
        if (template == null || template.GameIdText == null) return;

        if (ipField == null || ipField.gameObject == null)
        {
            ipField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
            ipField.gameObject.name = "IpTextBox";
            var arrow = ipField.transform.FindChild("arrowEnter");
            if (arrow == null || arrow.gameObject == null) return;
            UnityEngine.Object.DestroyImmediate(arrow.gameObject);

            ipField.transform.localPosition = new Vector3(0.225f, -1f, -100f);
            ipField.characterLimit = 30;
            ipField.AllowSymbols = true;
            ipField.ForceUppercase = false;
            ipField.SetText(ConfigRoles.Ip.Value);
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
            {
                ipField.outputText.SetText(ConfigRoles.Ip.Value);
                ipField.SetText(ConfigRoles.Ip.Value);
            })));

            ipField.ClearOnFocus = false;
            ipField.OnEnter = ipField.OnChange = new Button.ButtonClickedEvent();
            ipField.OnFocusLost = new Button.ButtonClickedEvent();
            ipField.OnChange.AddListener((UnityAction)onEnterOrIpChange);
            ipField.OnFocusLost.AddListener((UnityAction)onFocusLost);

            void onEnterOrIpChange()
            {
                ConfigRoles.Ip.Value = ipField.text;
            }

            void onFocusLost()
            {
                UpdateRegions();
                __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
            }
        }

        if (portField == null || portField.gameObject == null)
        {
            portField = UnityEngine.Object.Instantiate(template.GameIdText, __instance.transform);
            portField.gameObject.name = "PortTextBox";
            var arrow = portField.transform.FindChild("arrowEnter");
            if (arrow == null || arrow.gameObject == null) return;
            UnityEngine.Object.DestroyImmediate(arrow.gameObject);

            portField.transform.localPosition = new Vector3(0.225f, -1.75f, -100f);
            portField.characterLimit = 5;
            portField.SetText(ConfigRoles.Port.Value.ToString());
            __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
            {
                portField.outputText.SetText(ConfigRoles.Port.Value.ToString());
                portField.SetText(ConfigRoles.Port.Value.ToString());
            })));

            portField.ClearOnFocus = false;
            portField.OnEnter = portField.OnChange = new Button.ButtonClickedEvent();
            portField.OnFocusLost = new Button.ButtonClickedEvent();
            portField.OnChange.AddListener((UnityAction)onEnterOrPortFieldChange);
            portField.OnFocusLost.AddListener((UnityAction)onFocusLost);

            void onEnterOrPortFieldChange()
            {
                if (ushort.TryParse(portField.text, out ushort port))
                {
                    ConfigRoles.Port.Value = port;
                    portField.outputText.color = Color.white;
                }
                else
                {
                    portField.outputText.color = Color.red;
                }
            }

            void onFocusLost()
            {
                UpdateRegions();
                __instance.ChooseOption(ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
            }
        }
    }
    public static IRegionInfo[] defaultRegions;
    public static string SNRServerName => "<size=150%>"+SuperNewRolesPlugin.ColorModName+ "</size>\n<align=\"center\">Tokyo</align>";
    public static void UpdateRegions()
    {
        ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        var regions = new IRegionInfo[2] {
                new StaticHttpRegionInfo(SNRServerName, StringNames.NoTranslation,
                "cs.supernewroles.com", new(
                    new ServerInfo[1] {
                        new ServerInfo("http-1", "https://cs.supernewroles.com",
                        443, false)
                    })).CastFast<IRegionInfo>(),
                new StaticHttpRegionInfo("Custom", StringNames.NoTranslation,
                ConfigRoles.Ip.Value, new(
                    new ServerInfo[1] {
                        new ServerInfo("Custom", ConfigRoles.Ip.Value,
                        ConfigRoles.Port.Value, false)
                    })).CastFast<IRegionInfo>(),
            };

        IRegionInfo currentRegion = serverManager.CurrentRegion;
        Logger.Info($"Adding {regions.Length} regions");
        foreach (IRegionInfo region in regions)
        {
            if (region == null)
                Logger.Error("Could not add region","CustomServer");
            else
            {
                if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                    currentRegion = region;
                serverManager.AddOrUpdateRegion(region);
            }
        }

        // AU remembers the previous region that was set, so we need to restore it
        if (currentRegion != null)
        {
            Logger.Info("Resetting previous region");
            serverManager.SetRegion(currentRegion);
        }
    }
}