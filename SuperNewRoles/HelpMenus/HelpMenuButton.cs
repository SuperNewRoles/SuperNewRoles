
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HelpMenusHudManagerStartPatch
{
    public static GameObject helpMenuButton;
    public static void Postfix(HudManager __instance)
    {
        helpMenuButton = AssetManager.Instantiate("HelpButton", __instance.transform);
        helpMenuButton.transform.localScale = Vector3.one * 0.55f;
        // ヘルプメニュー自体と被らないように-499
        var aspectPosition = helpMenuButton.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
        aspectPosition.DistanceFromEdge = new(2.72f, 0.48f, -300.01f);
        aspectPosition.OnEnable();
        PassiveButton passiveButton = helpMenuButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { helpMenuButton.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            passiveButton.transform.Find("active").gameObject.SetActive(true);
            HelpMenuObjectManager.ShowOrHideHelpMenu();
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (HelpMenuObjectManager.fadeCoroutine == null || !HelpMenuObjectManager.fadeCoroutine.isActive)
                passiveButton.transform.Find("active").gameObject.SetActive(false);
        }));
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            passiveButton.transform.Find("active").gameObject.SetActive(true);
        }));
    }
}