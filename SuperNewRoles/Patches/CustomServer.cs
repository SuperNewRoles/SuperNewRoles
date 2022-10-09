using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {
        private static TextBoxTMP ipField;
        private static TextBoxTMP portField;

        public static void Postfix(RegionMenu __instance)
        {
            var template = DestroyableSingleton<JoinGameButton>.Instance;
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
        public static void UpdateRegions()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = defaultRegions;

            var CustomRegion = new DnsRegionInfo(ConfigRoles.Ip.Value, "Custom", StringNames.NoTranslation, ConfigRoles.Ip.Value, ConfigRoles.Port.Value, false);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.CastFast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }
    }
}