
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HelpMenusHUuManagerStartPatch
{
    public static void Postfix(HudManager __instance)
    {
        var obj = AssetManager.Instantiate("HelpButton", __instance.transform);
        obj.transform.localScale = Vector3.one * 0.072f;
        // ヘルプメニュー自体と被らないように-499
        var aspectPosition = obj.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightTop;
        aspectPosition.DistanceFromEdge = new(2.68f, 0.49f, -499f);
        aspectPosition.OnEnable();
        PassiveButton passiveButton = obj.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { obj.GetComponent<Collider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOver = new();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            HelpMenuObjectManager.ShowOrHideHelpMenu();
        }));
    }
}
