using System;
using HarmonyLib;
using UnityEngine;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class MainMenuPatch
{
    public static bool BeforeAprilR5() => DateTime.UtcNow <= new DateTime(2023, 3, 31, 15, 0, 0, 0, DateTimeKind.Utc);
    private static bool horseButtonState = HorseModeOption.enableHorseMode;
    private static Sprite horseModeOffSprite = null;

    private static void Prefix()
    {
        var bottomTemplate = GameObject.Find("InventoryButton");

        //FIXME:[SHRモードを一時封印] #1100 mergeをリバートしてください
        if (!BeforeAprilR5())
        {
            // Horse mode stuff
            var horseModeSelectionBehavior = new ClientModOptionsPatch.SelectionBehaviour("Enable Horse Mode", () => HorseModeOption.enableHorseMode = ConfigRoles.EnableHorseMode.Value = !ConfigRoles.EnableHorseMode.Value, ConfigRoles.EnableHorseMode.Value);

            if (bottomTemplate == null) return;
            var horseButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
            var passiveHorseButton = horseButton.GetComponent<PassiveButton>();
            var spriteHorseButton = horseButton.GetComponent<SpriteRenderer>();

            horseModeOffSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOff.png", 75f);

            spriteHorseButton.sprite = horseModeOffSprite;

            passiveHorseButton.OnClick = new ButtonClickedEvent();

            passiveHorseButton.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                horseButtonState = horseModeSelectionBehavior.OnClick();
                if (horseModeOffSprite == null) horseModeOffSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HorseModeButtonOff.png", 75f);
                spriteHorseButton.sprite = horseModeOffSprite;
                spriteHorseButton.transform.localScale *= -1;
                CredentialsPatch.LogoPatch.UpdateSprite();
                // Avoid wrong Player Particles floating around in the background
                var particles = GameObject.FindObjectOfType<PlayerParticles>();
                if (particles != null)
                {
                    particles.pool.ReclaimAll();
                    particles.Start();
                }
            });
        }


        var CreditsButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
        var passiveCreditsButton = CreditsButton.GetComponent<PassiveButton>();
        var spriteCreditsButton = CreditsButton.GetComponent<SpriteRenderer>();

        spriteCreditsButton.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreditsButton.png", 75f);

        passiveCreditsButton.OnClick = new ButtonClickedEvent();

        passiveCreditsButton.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
        {
            SuperNewRolesPlugin.Logger.LogInfo("クリック");
            if (CredentialsPatch.LogoPatch.CreditsPopup != null)
            {
                CredentialsPatch.LogoPatch.CreditsPopup.SetActive(true);
            }
        });
    }
}

public static class HorseModeOption
{
    public static bool enableHorseMode = false;

    public static void ClearAndReloadMapOptions()
    {
        enableHorseMode = ConfigRoles.EnableHorseMode.Value;
    }
}