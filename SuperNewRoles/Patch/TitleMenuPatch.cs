using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch
    {
        private static void Prefix(MainMenuManager __instance)
        {
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var buttonDiscord = UnityEngine.Object.Instantiate(template, null);
            buttonDiscord.transform.localPosition = new Vector3(buttonDiscord.transform.localPosition.x, buttonDiscord.transform.localPosition.y + 0.6f, buttonDiscord.transform.localPosition.z);

            var textDiscord = buttonDiscord.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                textDiscord.SetText("Discord");
            })));

            PassiveButton passiveButtonDiscord = buttonDiscord.GetComponent<PassiveButton>();
            SpriteRenderer buttonSpriteDiscord = buttonDiscord.GetComponent<SpriteRenderer>();

            passiveButtonDiscord.OnClick = new Button.ButtonClickedEvent();
            passiveButtonDiscord.OnClick.AddListener((System.Action)(() => Application.OpenURL("https://discord.gg/dKxdJZQgVQ")));

            Color discordColor = new Color32(88, 101, 242, byte.MaxValue);
            buttonSpriteDiscord.color = textDiscord.color = discordColor;
            passiveButtonDiscord.OnMouseOut.AddListener((System.Action)delegate
            {
                buttonSpriteDiscord.color = textDiscord.color = discordColor;
            });
        }
    }
}