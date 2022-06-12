using HarmonyLib;
using SuperNewRoles.Patch;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch
    {
        private static bool horseButtonState = HorseModeOption.enableHorseMode;
        private static Sprite horseModeOffSprite = null;
        private static Sprite horseModeOnSprite = null;

        private static void Prefix(MainMenuManager __instance)
        {
            // Horse mode stuff
            var horseModeSelectionBehavior = new ClientOptionsPatch.SelectionBehaviour("Enable Horse Mode", () => HorseModeOption.enableHorseMode = ConfigRoles.EnableHorseMode.Value = !ConfigRoles.EnableHorseMode.Value, ConfigRoles.EnableHorseMode.Value);

            var bottomTemplate = GameObject.Find("InventoryButton");
            if (bottomTemplate == null) return;
            var horseButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
            var passiveHorseButton = horseButton.GetComponent<PassiveButton>();
            var spriteHorseButton = horseButton.GetComponent<SpriteRenderer>();

            horseModeOffSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOff.png", 75f);
            horseModeOnSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOn.png", 75f);

            spriteHorseButton.sprite = horseButtonState ? horseModeOnSprite : horseModeOffSprite;

            passiveHorseButton.OnClick = new ButtonClickedEvent();

            passiveHorseButton.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                horseButtonState = horseModeSelectionBehavior.OnClick();
                if (horseButtonState)
                {
                    if (horseModeOnSprite == null) horseModeOnSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOn.png", 75f);
                    spriteHorseButton.sprite = horseModeOnSprite;
                }
                else
                {
                    if (horseModeOffSprite == null) horseModeOffSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOff.png", 75f);
                    spriteHorseButton.sprite = horseModeOffSprite;
                }
                CredentialsPatch.LogoPatch.updateSprite();
                // Avoid wrong Player Particles floating around in the background
                var particles = GameObject.FindObjectOfType<PlayerParticles>();
                if (particles != null)
                {
                    particles.pool.ReclaimAll();
                    particles.Start();
                }
            });
        }
    }

    public static class HorseModeOption
    {
        public static bool enableHorseMode = false;

        public static void clearAndReloadMapOptions()
        {
            enableHorseMode = ConfigRoles.EnableHorseMode.Value;
        }
    }
}