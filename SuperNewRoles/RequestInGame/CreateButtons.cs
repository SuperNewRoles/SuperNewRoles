using HarmonyLib;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.RequestInGame;

public class CreateButtons
{
    public static void GenerateButtons(Transform parent, Vector3 position)
    {
        GameObject bugReportButton = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("bugReport"), parent);
        bugReportButton.transform.localPosition = position;
        bugReportButton.transform.localScale = Vector3.one * 0.15f;
        PassiveButton passiveButton = bugReportButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { bugReportButton.GetComponent<CircleCollider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            SelectButtonsMenu.GenerateButtonsMenu();
        }));
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuManagerStartPatch
    {
        public static void Postfix(MainMenuManager __instance)
        {
            GenerateButtons(__instance.transform, new(-0.87f, 1.7f, 0f));
            TextMeshPro text = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("bugReportText"), __instance.transform).GetComponent<TextMeshPro>();
            text.transform.localPosition = new(0.88f, 1.715f, 0f);
            text.transform.localScale = Vector3.one * 0.15f;
            text.alignment = TextAlignmentOptions.Left;
            text.text = ModTranslation.GetString("RequestInGameButtonText");
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public class HudManagerStartPatch
    {
        public static void Postfix(HudManager __instance)
        {
            GenerateButtons(__instance.transform, new(4.53f, 1.88f, 0f));
        }
    }
}