using HarmonyLib;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class DarkLightPlayerVoteAreaPatch
{
    public static readonly Color DarkColor = new(0.4f, 0.4f, 0.4f, 1f);
    public static readonly Color LightColor = new(0.95f, 0.95f, 0.95f, 1f);
    public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
    {
        SpriteRenderer darkLight = __instance.gameObject.transform.Find("DarkLight")?.GetComponent<SpriteRenderer>();
        if (darkLight != null)
        {
            darkLight.color = CustomColors.IsLighter(playerInfo) ? LightColor : DarkColor;
            return;
        }
        darkLight = new GameObject("DarkLight").AddComponent<SpriteRenderer>();
        darkLight.transform.SetParent(__instance.gameObject.transform);
        darkLight.transform.localPosition = new Vector3(1.26f, -0.2f, -100f);
        darkLight.transform.localScale = Vector3.one * 0.35f;
        darkLight.gameObject.layer = 5;
        darkLight.color = CustomColors.IsLighter(playerInfo) ? LightColor : DarkColor;
        darkLight.sprite = AssetManager.GetAsset<Sprite>("BrightORDarkIcon.png");
        darkLight.gameObject.SetActive(true);
    }
}