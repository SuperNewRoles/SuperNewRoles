using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public class MainMenuPatch
{
    private static bool horseButtonState = HorseModeOption.enableHorseMode;
    private static Sprite horseModeOffSprite = null;
    static void hidebtn(PassiveButton btn)
    {
        GameObject.Destroy(btn.GetComponent<AspectScaledAsset>());
        btn.activeSprites = null;
        btn.inactiveSprites = null;
        btn.HeldButtonSprite = null;
        btn.transform.localScale = new(0.215f, 1, 1);
        btn.transform.FindChild("FontPlacer").gameObject.SetActive(false);
        btn.transform.FindChild("NewItem").gameObject.SetActive(false);
        btn.transform.FindChild("Inactive/Icon").gameObject.SetActive(false);
        btn.transform.FindChild("Inactive/Shine").gameObject.SetActive(false);
    }
    private static void Prefix(MainMenuManager __instance)
    {
        var bottomTemplate = __instance.shopButton;
        // Horse mode stuff
        var horseModeSelectionBehavior = new ClientModOptionsPatch.SelectionBehaviour("Enable Horse Mode", () => HorseModeOption.enableHorseMode = ConfigRoles.EnableHorseMode.Value = !ConfigRoles.EnableHorseMode.Value, ConfigRoles.EnableHorseMode.Value);

        if (bottomTemplate == null) return;
        var horseButton = Object.Instantiate(bottomTemplate, null);
        var passiveHorseButton = horseButton.GetComponent<PassiveButton>();
        var spriteHorseButton = horseButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        hidebtn(horseButton);
        horseButton.transform.localPosition = new(1.5f, -1.1f, 0);

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

        var CreditsButton = Object.Instantiate(bottomTemplate, null);
        var passiveCreditsButton = CreditsButton.GetComponent<PassiveButton>();
        var spriteCreditsButton = CreditsButton.transform.FindChild("Inactive").GetComponent<SpriteRenderer>();
        hidebtn(passiveCreditsButton);
        CreditsButton.transform.localPosition = new(2.5f, -1.1f, 0);

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